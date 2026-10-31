#!/usr/bin/env python3
"""Растеризатор плоских SVG без внешних зависимостей, кроме Pillow (D-024: cairosvg и Inkscape не ставим).

Разбирается подмножество SVG, которого хватает для плоского арта (ТЗ §4.1):
  <g>, <rect rx>, <circle>, <ellipse>, <polygon>, <path> с командами M L H V C Q Z (абсолютные),
  <text>; атрибуты transform (translate, scale, rotate, matrix), fill, stroke, stroke-width, opacity.

Цвет задаётся HEX или токеном палитры: fill="var(--castleStone)". Токены — имена полей PaletteSet,
значения берутся из ThemePalette.cs, поэтому один SVG рисуется в обеих темах.
"""
import math
import re
import xml.etree.ElementTree as ElementTree
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

SVG_NS = "{http://www.w3.org/2000/svg}"
ROOT = Path(__file__).resolve().parent.parent
PALETTE_SOURCE = ROOT / "Assets" / "_Game" / "Scripts" / "Core" / "ThemePalette.cs"

SUPERSAMPLE = 4
CURVE_SEGMENTS = 20
CIRCLE_SEGMENTS = 96
CORNER_SEGMENTS = 12
# Rubik (OFL) установлен в системе; размытие с порогом ещё сильнее скругляет углы букв.
HEAVY_FONT = Path.home() / "AppData/Local/Microsoft/Windows/Fonts/Rubik-VariableFont_wght.ttf"
HEAVY_WEIGHT = 800
TEXT_ROUND = 0.018
TEXT_THRESHOLD = 110


def load_palettes():
    """Пресеты из ThemePalette.cs: {"Night": {поле: (r, g, b)}, "Sunset": {...}}."""
    source = PALETTE_SOURCE.read_text(encoding="utf-8")
    pattern = re.compile(r"set\.(\w+) = Hex\(0x([0-9A-Fa-f]{6})\)")

    def body(name):
        start = source.index(name)
        return source[start:source.index("\n        }", start)]

    shared = dict(pattern.findall(body("private static void FillShared")))
    result = {}
    for preset, method in (("Night", "PaletteSet CreateNight"), ("Sunset", "PaletteSet CreateSunset")):
        values = dict(shared)
        values.update(pattern.findall(body(method)))
        result[preset] = {key: tuple(bytes.fromhex(value)) for key, value in values.items()}
    return result


def multiply(a, b):
    return (
        a[0] * b[0] + a[2] * b[1],
        a[1] * b[0] + a[3] * b[1],
        a[0] * b[2] + a[2] * b[3],
        a[1] * b[2] + a[3] * b[3],
        a[0] * b[4] + a[2] * b[5] + a[4],
        a[1] * b[4] + a[3] * b[5] + a[5])


def parse_transform(text):
    matrix = (1.0, 0.0, 0.0, 1.0, 0.0, 0.0)
    for name, args in re.findall(r"(\w+)\(([^)]*)\)", text or ""):
        values = [float(value) for value in re.findall(r"-?\d*\.?\d+(?:e-?\d+)?", args)]
        if name == "translate":
            step = (1, 0, 0, 1, values[0], values[1] if len(values) > 1 else 0)
        elif name == "scale":
            step = (values[0], 0, 0, values[1] if len(values) > 1 else values[0], 0, 0)
        elif name == "rotate":
            cos = math.cos(math.radians(values[0]))
            sin = math.sin(math.radians(values[0]))
            step = (cos, sin, -sin, cos, 0, 0)
        else:
            step = tuple(values)
        matrix = multiply(matrix, step)
    return matrix


def apply(matrix, points):
    return [(matrix[0] * x + matrix[2] * y + matrix[4], matrix[1] * x + matrix[3] * y + matrix[5]) for x, y in points]


def ellipse_points(cx, cy, rx, ry):
    step = 2 * math.pi / CIRCLE_SEGMENTS
    return [(cx + rx * math.cos(i * step), cy + ry * math.sin(i * step)) for i in range(CIRCLE_SEGMENTS)]


def rounded_rect_points(x, y, w, h, r):
    r = min(r, w * 0.5, h * 0.5)
    if r <= 0:
        return [(x, y), (x + w, y), (x + w, y + h), (x, y + h)]
    points = []
    corners = ((x + w - r, y + r, -90), (x + w - r, y + h - r, 0), (x + r, y + h - r, 90), (x + r, y + r, 180))
    for cx, cy, start in corners:
        for i in range(CORNER_SEGMENTS + 1):
            angle = math.radians(start + 90.0 * i / CORNER_SEGMENTS)
            points.append((cx + r * math.cos(angle), cy + r * math.sin(angle)))
    return points


def path_contours(data):
    """Ломаные контуров path; каждая Z или M начинает новый контур."""
    tokens = re.findall(r"[MLHVCQZ]|-?\d*\.?\d+(?:e-?\d+)?", data)
    contours = []
    points = []
    index = 0
    command = None
    while index < len(tokens):
        if tokens[index].isalpha():
            command = tokens[index]
            index += 1
            if command == "Z":
                contours.append(points)
                points = []
                continue
            if command == "M" and points:
                contours.append(points)
                points = []
        count = {"M": 2, "L": 2, "H": 1, "V": 1, "C": 6, "Q": 4}[command]
        values = [float(token) for token in tokens[index:index + count]]
        index += count
        if command in "ML":
            points.append((values[0], values[1]))
        elif command == "H":
            points.append((values[0], points[-1][1]))
        elif command == "V":
            points.append((points[-1][0], values[0]))
        elif command == "Q":
            x0, y0 = points[-1]
            for i in range(1, CURVE_SEGMENTS + 1):
                t = i / CURVE_SEGMENTS
                u = 1 - t
                points.append((
                    u * u * x0 + 2 * u * t * values[0] + t * t * values[2],
                    u * u * y0 + 2 * u * t * values[1] + t * t * values[3]))
        else:
            x0, y0 = points[-1]
            for i in range(1, CURVE_SEGMENTS + 1):
                t = i / CURVE_SEGMENTS
                u = 1 - t
                points.append((
                    u * u * u * x0 + 3 * u * u * t * values[0] + 3 * u * t * t * values[2] + t * t * t * values[4],
                    u * u * u * y0 + 3 * u * u * t * values[1] + 3 * u * t * t * values[3] + t * t * t * values[5]))
    if points:
        contours.append(points)
    return contours


class Renderer:
    """Рисует дерево SVG на RGBA-холсте с запасом разрешения, потом уменьшает."""

    def __init__(self, palette, width, height, background=None):
        self.palette = palette
        self.size = (int(width), int(height))
        self.scale = SUPERSAMPLE
        size = (self.size[0] * self.scale, self.size[1] * self.scale)
        self.canvas = Image.new("RGBA", size, background or (0, 0, 0, 0))

    def color(self, value):
        if value is None or value == "none":
            return None
        match = re.match(r"var\(--(\w+)\)", value)
        if match:
            return self.palette[match.group(1)]
        return tuple(bytes.fromhex(value.lstrip("#")))

    def blend(self, mask, color, opacity):
        """Плоская заливка по маске; непрозрачная форма не смешивается с фоном по краю."""
        box = mask.getbbox()
        if box is None:
            return
        alpha = mask.crop(box)
        if opacity < 1:
            alpha = alpha.point(lambda value: int(value * opacity))
        self.canvas.paste(color + (255,), box, alpha)

    def fill_polygon(self, points, color, opacity):
        scaled = [(x * self.scale, y * self.scale) for x, y in points]
        if len(scaled) < 3:
            return
        if opacity >= 1:
            ImageDraw.Draw(self.canvas).polygon(scaled, fill=color + (255,))
            return
        mask = Image.new("L", self.canvas.size, 0)
        ImageDraw.Draw(mask).polygon(scaled, fill=255)
        self.blend(mask, color, opacity)

    def stroke_polyline(self, points, color, width, opacity):
        scaled = [(x * self.scale, y * self.scale) for x, y in points]
        pixels = max(1, int(round(width * self.scale)))
        mask = Image.new("L", self.canvas.size, 0)
        draw = ImageDraw.Draw(mask)
        draw.line(scaled, fill=255, width=pixels, joint="curve")
        radius = pixels * 0.5
        for x, y in scaled:
            draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=255)
        self.blend(mask, color, opacity)

    def draw_text(self, element, matrix, color, opacity):
        size = float(element.get("font-size")) * math.hypot(matrix[0], matrix[1])
        font_path = element.get("font-family") or str(HEAVY_FONT)
        font = ImageFont.truetype(font_path, int(round(size * self.scale)))
        if element.get("font-family") is None:
            font.set_variation_by_axes([HEAVY_WEIGHT])
        text = element.text or ""
        left, top, right, bottom = font.getbbox(text)
        margin = int(size * self.scale * 0.2)
        mask = Image.new("L", (right - left + margin * 2, bottom - top + margin * 2), 0)
        ImageDraw.Draw(mask).text((margin - left, margin - top), text, fill=255, font=font)
        if element.get("font-family") is None:
            radius = size * self.scale * TEXT_ROUND
            mask = mask.filter(ImageFilter.GaussianBlur(radius)).point(lambda v: 255 if v >= TEXT_THRESHOLD else 0)
        x, y = apply(matrix, [(float(element.get("x", 0)), float(element.get("y", 0)))])[0]
        anchor = {"middle": 0.5, "end": 1.0}.get(element.get("text-anchor"), 0.0)
        pivot_x = margin + (right - left) * anchor
        pivot_y = margin + (bottom - top) * 0.5
        angle = math.degrees(math.atan2(matrix[1], matrix[0]))
        if abs(angle) > 0.01:
            center = (mask.width * 0.5, mask.height * 0.5)
            cos = math.cos(math.radians(angle))
            sin = math.sin(math.radians(angle))
            offset_x = pivot_x - center[0]
            offset_y = pivot_y - center[1]
            mask = mask.rotate(-angle, resample=Image.Resampling.BICUBIC, expand=True)
            pivot_x = mask.width * 0.5 + offset_x * cos - offset_y * sin
            pivot_y = mask.height * 0.5 + offset_x * sin + offset_y * cos
        full = Image.new("L", self.canvas.size, 0)
        full.paste(mask, (int(x * self.scale - pivot_x), int(y * self.scale - pivot_y)))
        self.blend(full, color, opacity)

    def draw_element(self, element, matrix, opacity):
        tag = element.tag.replace(SVG_NS, "")
        if tag in ("title", "desc", "defs"):
            return
        matrix = multiply(matrix, parse_transform(element.get("transform")))
        opacity *= float(element.get("opacity", 1))
        if tag in ("g", "svg"):
            for child in element:
                self.draw_element(child, matrix, opacity)
            return
        fill = self.color(element.get("fill"))
        stroke = self.color(element.get("stroke"))
        if tag == "text":
            self.draw_text(element, matrix, fill, opacity)
            return
        if tag == "rect":
            contours = [rounded_rect_points(
                float(element.get("x", 0)), float(element.get("y", 0)),
                float(element.get("width")), float(element.get("height")), float(element.get("rx", 0)))]
        elif tag == "circle":
            r = float(element.get("r"))
            contours = [ellipse_points(float(element.get("cx", 0)), float(element.get("cy", 0)), r, r)]
        elif tag == "ellipse":
            contours = [ellipse_points(
                float(element.get("cx", 0)), float(element.get("cy", 0)),
                float(element.get("rx")), float(element.get("ry")))]
        elif tag == "polygon":
            values = [float(value) for value in re.findall(r"-?\d*\.?\d+", element.get("points"))]
            contours = [list(zip(values[::2], values[1::2]))]
        elif tag == "path":
            contours = path_contours(element.get("d"))
        else:
            raise ValueError(f"unsupported SVG element: {tag}")
        for contour in contours:
            points = apply(matrix, contour)
            if fill is not None:
                self.fill_polygon(points, fill, opacity)
            if stroke is not None:
                width = float(element.get("stroke-width", 1)) * math.hypot(matrix[0], matrix[1])
                self.stroke_polyline(points, stroke, width, opacity)

    def result(self):
        return self.canvas.resize(self.size, Image.Resampling.BOX)


def render(source, palette, background=None):
    """Рендер файла или строки SVG в RGBA-картинку размером width × height."""
    root = ElementTree.parse(source).getroot() if isinstance(source, Path) else ElementTree.fromstring(source)
    renderer = Renderer(palette, float(root.get("width")), float(root.get("height")), background)
    renderer.draw_element(root, (1.0, 0.0, 0.0, 1.0, 0.0, 0.0), 1.0)
    return renderer.result()
