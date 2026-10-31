#!/usr/bin/env python3
"""Растеризация SVG-источников в PNG: маска ландшафта уровня 1 (ТЗ §3.8) и ракета (ТЗ §3.4).

Использование: python Tools/rasterize_svg.py

Каналы Level1_TerrainMask.png, значения — покрытие текселя 0..255:
  R — материал, G — цветная кромка земли, B — «закопанные» декоры.
Нижняя строка картинки — низ уровня. Изменение PNG запекает данные в Unity (TerrainMaskBaker).

Каналы Rocket.png: R — силуэт, G — детали в цвет стороны, B — теневая полоса. Цвета задаёт палитра.

Разбирается подмножество SVG: <path> с командами M, L, C, Z в абсолютных координатах и <ellipse>.
Этап 7 расширит скрипт на остальной арт.
"""
import random
import re
import xml.etree.ElementTree as ElementTree
from pathlib import Path

import numpy
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Art" / "Source" / "Level1_TerrainMask.svg"
OUTPUT = ROOT / "Assets" / "_Game" / "Art" / "Level" / "Level1_TerrainMask.png"
ROCKET_SOURCE = ROOT / "Art" / "Source" / "Rocket.svg"
ROCKET_OUTPUT = ROOT / "Assets" / "_Game" / "Art" / "Level" / "Rocket.png"
SVG_NS = "{http://www.w3.org/2000/svg}"

SUPERSAMPLE = 6
CURVE_SEGMENTS = 24
TEXELS_PER_METER = 16.0

# Цветная кромка: лента вдоль контура и «капли» вниз от неё. Размеры в метрах.
RIM_THICKNESS = 0.26
RIM_DRIP_SEED = 7321
RIM_DRIP_MIN_STEP = 0.45
RIM_DRIP_MAX_STEP = 0.95
RIM_DRIP_MIN_LENGTH = 0.14
RIM_DRIP_MAX_LENGTH = 0.55
RIM_DRIP_MIN_WIDTH = 0.2
RIM_DRIP_MAX_WIDTH = 0.34
# Ниже этой линии (единицы SVG) кромка не видна: там вода и дно.
RIM_LOWEST_Y = 589.0


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


def draw_rim(canvas, contour):
    """Лента вдоль контура и капли, свисающие вниз (стиль кромки земли, ТЗ §3.8)."""
    draw = ImageDraw.Draw(canvas)
    meter = TEXELS_PER_METER * SUPERSAMPLE
    band_width = int(RIM_THICKNESS * 2 * meter)
    visible = []
    for point in contour:
        if point[1] <= RIM_LOWEST_Y:
            visible.append(point)
        elif visible:
            draw.line(scaled(visible), fill=255, width=band_width, joint="curve")
            visible = []
    if visible:
        draw.line(scaled(visible), fill=255, width=band_width, joint="curve")

    rng = random.Random(RIM_DRIP_SEED)
    distance = 0.0
    next_drip = rng.uniform(RIM_DRIP_MIN_STEP, RIM_DRIP_MAX_STEP)
    for (x0, y0), (x1, y1) in zip(contour, contour[1:]):
        distance += ((x1 - x0) ** 2 + (y1 - y0) ** 2) ** 0.5 / TEXELS_PER_METER
        if distance < next_drip:
            continue
        next_drip = distance + rng.uniform(RIM_DRIP_MIN_STEP, RIM_DRIP_MAX_STEP)
        length = rng.uniform(RIM_DRIP_MIN_LENGTH, RIM_DRIP_MAX_LENGTH)
        width = rng.uniform(RIM_DRIP_MIN_WIDTH, RIM_DRIP_MAX_WIDTH)
        if y1 > RIM_LOWEST_Y:
            continue
        x = x1 * SUPERSAMPLE
        top = y1 * SUPERSAMPLE
        half = width * 0.5 * meter
        bottom = top + (RIM_THICKNESS + length) * meter
        draw.rectangle((x - half, top, x + half, bottom - half), fill=255)
        draw.ellipse((x - half, bottom - 2 * half, x + half, bottom), fill=255)


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


def main():
    rasterize_rocket()
    root = ElementTree.parse(SOURCE).getroot()
    width = int(root.get("width"))
    height = int(root.get("height"))
    size = (width, height)
    canvas_size = (width * SUPERSAMPLE, height * SUPERSAMPLE)

    ground = root.find(f"{SVG_NS}path[@id='ground']")
    outline, curve_end = flatten_path(ground.get("d"))
    solid_canvas = Image.new("L", canvas_size, 0)
    ImageDraw.Draw(solid_canvas).polygon(scaled(outline), fill=255)

    rim_canvas = Image.new("L", canvas_size, 0)
    draw_rim(rim_canvas, outline[:curve_end])

    buried_canvas = Image.new("L", canvas_size, 0)
    buried_draw = ImageDraw.Draw(buried_canvas)
    for shape in root.find(f"{SVG_NS}g[@id='buried']"):
        cx = float(shape.get("cx")) * SUPERSAMPLE
        cy = float(shape.get("cy")) * SUPERSAMPLE
        rx = float(shape.get("rx")) * SUPERSAMPLE
        ry = float(shape.get("ry")) * SUPERSAMPLE
        buried_draw.ellipse((cx - rx, cy - ry, cx + rx, cy + ry), fill=255)

    channels = numpy.dstack((
        downsample(solid_canvas, size),
        downsample(clipped(rim_canvas, solid_canvas), size),
        downsample(clipped(buried_canvas, solid_canvas), size)))
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(channels, "RGB").save(OUTPUT, optimize=True)
    print(f"{OUTPUT.relative_to(ROOT)}: {width}x{height}, {OUTPUT.stat().st_size} bytes")


if __name__ == "__main__":
    main()
