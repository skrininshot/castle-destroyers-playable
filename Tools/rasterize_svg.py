#!/usr/bin/env python3
"""Растеризация масок: ландшафт уровня 1 (ТЗ §3.8) в двух вариантах кромки и ракета (ТЗ §3.4). Остальной арт — Tools/build_art.py.

Использование: python Tools/rasterize_svg.py

Каналы Level1_TerrainMask*.png, значения 0..255, нижняя строка — низ уровня:
  R — материал; G — кромка земли ступенями; B — «закопанные» декоры ступенями.
Ступени кромки (шейдер Game/Terrain): 85 широкая полоса, 170 цветная кромка, 255 блик. Ступени декоров: 128 пятна, 255 кости.
Кромка считается по столбцам: толщина идёт по вертикали от верхнего контура, поэтому на стенках расщелины она сходит на нет,
а над контуром лента заходит за материал и порог в шейдере не зависит от края. Форма общая, Night капает, Sunset даёт фестон.
Изменение PNG запекает данные в Unity (TerrainMaskBaker).

Каналы Rocket.png: R — силуэт, G — детали в цвет стороны, B — теневая полоса. Цвета задаёт палитра.
"""
import random
import re
import xml.etree.ElementTree as ElementTree
from pathlib import Path

import numpy
from PIL import Image, ImageDraw

import flat_svg

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Art" / "Source" / "Level1_TerrainMask.svg"
LEVEL_DIR = ROOT / "Assets" / "_Game" / "Art" / "Level"
OUTPUTS = {"Night": LEVEL_DIR / "Level1_TerrainMask.png", "Sunset": LEVEL_DIR / "Level1_TerrainMask_Sunset.png"}
ROCKET_SOURCE = ROOT / "Art" / "Source" / "Rocket.svg"
ROCKET_OUTPUT = LEVEL_DIR / "Rocket.png"
SVG_NS = "{http://www.w3.org/2000/svg}"

SUPERSAMPLE = 6
CURVE_SEGMENTS = 24
TEXELS_PER_METER = 16.0

BAND_LEVEL = 85
RIM_LEVEL = 170
LIGHT_LEVEL = 255
BLOB_LEVEL = 128
BONE_LEVEL = 255

# Размеры в метрах. RIM_LIFT — на сколько лента заходит выше контура.
RIM_LIFT = 0.4
# Ниже этой линии (единицы SVG) кромки нет: там вода и дно.
RIM_LOWEST_Y = 589.0
DRIP_RIM = 0.72
DRIP_LIGHT = 0.26
DRIP_SEED = 7321
DRIP_MIN_STEP = 1.1
DRIP_MAX_STEP = 2.3
DRIP_MIN_LENGTH = 0.35
DRIP_MAX_LENGTH = 1.5
DRIP_MIN_HALF = 0.2
DRIP_MAX_HALF = 0.34
DRIP_FILLET = 0.2
DRIP_MAX_SLOPE = 1.1
SCALLOP_BAND = 2.1
SCALLOP_RIM = 0.72
SCALLOP_SEED = 2203
SCALLOP_MIN_RADIUS = 0.62
SCALLOP_MAX_RADIUS = 0.86
SCALLOP_STEP = 1.72
SLOPE_PROBE = 0.4
SLOPE_MAX_FACTOR = 3.0
RIM_FADE_START = 0.7
RIM_FADE_END = 1.3
DRIP_BAND = 2.4


def flatten_path(data):
    """Точки контура и индекс, на котором кончается верхняя кривая (L-участки замыкают низ уровня)."""
    tokens = re.findall(r"[MLCZ]|-?\d+(?:\.\d+)?", data)
    points = []
    curve_end = 0
    index = 0
    while index < len(tokens):
        command = tokens[index]
        index += 1
        if command == "Z":
            continue
        count = 6 if command == "C" else 2
        values = [float(token) for token in tokens[index:index + count]]
        index += count
        if command == "C":
            x0, y0 = points[-1]
            x1, y1, x2, y2, x3, y3 = values
            for step in range(1, CURVE_SEGMENTS + 1):
                t = step / CURVE_SEGMENTS
                u = 1.0 - t
                points.append((
                    u * u * u * x0 + 3 * u * u * t * x1 + 3 * u * t * t * x2 + t * t * t * x3,
                    u * u * u * y0 + 3 * u * u * t * y1 + 3 * u * t * t * y2 + t * t * t * y3))
            curve_end = len(points)
        else:
            points.append((values[0], values[1]))
    return points, curve_end


def scaled(points):
    return [(x * SUPERSAMPLE, y * SUPERSAMPLE) for x, y in points]


def surface_rows(solid):
    """Для каждого столбца холста — строка верхнего контура; -1, где контура нет или он ушёл в воду."""
    filled = solid >= 128
    tops = numpy.argmax(filled, axis=0).astype(numpy.float64)
    tops[~filled.any(axis=0) | (tops > RIM_LOWEST_Y * SUPERSAMPLE)] = -1.0
    return tops


def surface_slope(tops, meter):
    probe = int(SLOPE_PROBE * meter)
    return numpy.abs(numpy.roll(tops, -probe) - numpy.roll(tops, probe)) / (2.0 * probe)


def slope_factor(tops, meter):
    """Широкая полоса считается по вертикали: множитель держит её ширину на склоне постоянной."""
    slope = surface_slope(tops, meter)
    return numpy.minimum(numpy.sqrt(1.0 + slope * slope), SLOPE_MAX_FACTOR)


def gentle_share(tops, meter):
    """Яркая кромка живёт на пологой земле и сходит на нет по склону: контур горы не должен читаться как обводка."""
    slope = surface_slope(tops, meter)
    return numpy.clip((RIM_FADE_END - slope) / (RIM_FADE_END - RIM_FADE_START), 0.0, 1.0)


def drip_depth(tops, meter):
    """Толщина кромки Night: лента и капли с круглым дном и скруглёнными впадинами, как на кладбище Bowmasters."""
    depth = DRIP_RIM * meter * gentle_share(tops, meter)
    columns = numpy.arange(tops.size, dtype=numpy.float64)
    rng = random.Random(DRIP_SEED)
    x = rng.uniform(DRIP_MIN_STEP, DRIP_MAX_STEP) * meter
    probe = int(0.5 * meter)
    fillet = DRIP_FILLET * meter
    while x < tops.size - probe:
        index = int(x)
        half = rng.uniform(DRIP_MIN_HALF, DRIP_MAX_HALF) * meter
        length = rng.uniform(DRIP_MIN_LENGTH, DRIP_MAX_LENGTH) * meter
        x += rng.uniform(DRIP_MIN_STEP, DRIP_MAX_STEP) * meter
        low = max(index - probe, 0)
        if tops[index] < 0 or tops[low] < 0 or tops[index + probe] < 0:
            continue
        if abs(tops[index + probe] - tops[low]) > 2 * probe * DRIP_MAX_SLOPE:
            continue
        offset = numpy.abs(columns - index)
        inside = offset <= half
        depth[inside] = numpy.maximum(depth[inside], DRIP_RIM * meter + length - half + numpy.sqrt(half * half - offset[inside] ** 2))
        shoulder = (offset > half) & (offset < half + fillet)
        gap = half + fillet - offset[shoulder]
        lifted = DRIP_RIM * meter + fillet - numpy.sqrt(fillet * fillet - gap * gap)
        depth[shoulder] = numpy.maximum(depth[shoulder], lifted)
    return depth


def scallop_depth(tops, meter):
    """Толщина фестона Sunset: ряд полукругов вниз от ленты."""
    gentle = gentle_share(tops, meter)
    depth = SCALLOP_RIM * meter * gentle
    columns = numpy.arange(tops.size, dtype=numpy.float64)
    rng = random.Random(SCALLOP_SEED)
    x = 0.0
    while x < tops.size:
        radius = rng.uniform(SCALLOP_MIN_RADIUS, SCALLOP_MAX_RADIUS) * meter
        offset = numpy.abs(columns - x)
        inside = offset < radius
        scallop = SCALLOP_RIM * meter * 0.4 + numpy.sqrt(radius * radius - offset[inside] ** 2)
        depth[inside] = numpy.maximum(depth[inside], scallop * gentle[inside])
        x += radius * SCALLOP_STEP
    return depth


def paint_band(levels, tops, depth, lift, value):
    """Ставит value от строки tops - lift до tops + depth в каждом столбце, где есть контур."""
    rows = numpy.arange(levels.shape[0], dtype=numpy.float64)[:, None]
    mask = (rows >= tops - lift) & (rows < tops + depth) & (tops >= 0)
    levels[mask] = value


def rim_levels(solid, theme):
    meter = TEXELS_PER_METER * SUPERSAMPLE
    tops = surface_rows(solid)
    levels = numpy.zeros(solid.shape, dtype=numpy.uint8)
    lift = RIM_LIFT * meter
    if theme == "Night":
        paint_band(levels, tops, DRIP_BAND * meter * slope_factor(tops, meter), lift, BAND_LEVEL)
        paint_band(levels, tops, drip_depth(tops, meter), lift, RIM_LEVEL)
        paint_band(levels, tops, DRIP_LIGHT * meter * gentle_share(tops, meter), lift, LIGHT_LEVEL)
    else:
        paint_band(levels, tops, SCALLOP_BAND * meter * slope_factor(tops, meter), lift, BAND_LEVEL)
        paint_band(levels, tops, scallop_depth(tops, meter), lift, RIM_LEVEL)
    return Image.fromarray(levels)


def clipped(canvas, solid_canvas):
    return Image.fromarray(numpy.minimum(numpy.asarray(canvas), numpy.asarray(solid_canvas)))


def downsample(canvas, size):
    return numpy.asarray(canvas.resize(size, Image.Resampling.BOX), dtype=numpy.uint8)


def rasterize_rocket():
    root = ElementTree.parse(ROCKET_SOURCE).getroot()
    size = (int(root.get("width")), int(root.get("height")))
    canvas_size = (size[0] * SUPERSAMPLE, size[1] * SUPERSAMPLE)
    layers = {}
    for name in ("body", "accent", "shade"):
        canvas = Image.new("L", canvas_size, 0)
        draw = ImageDraw.Draw(canvas)
        data = root.find(f"{SVG_NS}path[@id='{name}']").get("d")
        for contour in data.split("Z"):
            if contour.strip():
                draw.polygon(scaled(flatten_path(contour)[0]), fill=255)
        layers[name] = canvas
    channels = numpy.dstack((
        downsample(layers["body"], size),
        downsample(clipped(layers["accent"], layers["body"]), size),
        downsample(clipped(layers["shade"], layers["body"]), size)))
    Image.fromarray(channels, "RGB").save(ROCKET_OUTPUT, optimize=True)
    print(f"{ROCKET_OUTPUT.relative_to(ROOT)}: {size[0]}x{size[1]}, {ROCKET_OUTPUT.stat().st_size} bytes")


def buried_channel(root, size, canvas_size):
    """Пятна рисуются эллипсами, кости — группой #bones: она проходит через общий растеризатор плоских SVG."""
    canvas = Image.new("L", canvas_size, 0)
    draw = ImageDraw.Draw(canvas)
    for shape in root.find(f"{SVG_NS}g[@id='buried']"):
        cx = float(shape.get("cx")) * SUPERSAMPLE
        cy = float(shape.get("cy")) * SUPERSAMPLE
        rx = float(shape.get("rx")) * SUPERSAMPLE
        ry = float(shape.get("ry")) * SUPERSAMPLE
        draw.ellipse((cx - rx, cy - ry, cx + rx, cy + ry), fill=BLOB_LEVEL)
    bones = root.find(f"{SVG_NS}g[@id='bones']")
    body = ElementTree.tostring(bones, encoding="unicode")
    document = f'<svg xmlns="http://www.w3.org/2000/svg" width="{size[0]}" height="{size[1]}">{body}</svg>'
    alpha = numpy.asarray(flat_svg.render(document, {}))[..., 3]
    bone_values = (alpha.astype(numpy.uint16) * BONE_LEVEL // 255).astype(numpy.uint8)
    return numpy.maximum(downsample(canvas, size), bone_values)


def main():
    rasterize_rocket()
    root = ElementTree.parse(SOURCE).getroot()
    width = int(root.get("width"))
    height = int(root.get("height"))
    size = (width, height)
    canvas_size = (width * SUPERSAMPLE, height * SUPERSAMPLE)

    ground = root.find(f"{SVG_NS}path[@id='ground']")
    outline = flatten_path(ground.get("d"))[0]
    solid_canvas = Image.new("L", canvas_size, 0)
    ImageDraw.Draw(solid_canvas).polygon(scaled(outline), fill=255)
    solid_pixels = numpy.asarray(solid_canvas)
    solid = downsample(solid_canvas, size)
    buried = numpy.minimum(buried_channel(root, size, canvas_size), solid)

    LEVEL_DIR.mkdir(parents=True, exist_ok=True)
    for theme, output in OUTPUTS.items():
        rim = downsample(rim_levels(solid_pixels, theme), size)
        Image.fromarray(numpy.dstack((solid, rim, buried)), "RGB").save(output, optimize=True)
        print(f"{output.relative_to(ROOT)}: {width}x{height}, {output.stat().st_size} bytes")


if __name__ == "__main__":
    main()
