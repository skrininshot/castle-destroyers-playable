#!/usr/bin/env python3
"""Весь арт игры: SVG-источники в Art/Source/Game, PNG по темам в Assets/_Game/Art/Theme*, атлас, таблица спрайтов для C#.

Использование: python Tools/build_art.py

SVG содержат токены палитры var(--поле), поэтому один файл рисуется в обеих темах. Источник можно подменить руками:
запуск с --keep-svg не перезаписывает файлы в Art/Source/Game, а только растеризует их.
Раскладка атласа одна на обе темы, её описывает сгенерированный Scripts/Level/ArtSprites.cs.
Формы рисуют те же функции, что и style sheet (Tools/style_sheet.py): лист и игра не разъезжаются.
"""
import math
import random
import sys
from pathlib import Path

import numpy
from PIL import Image, ImageFont

import flat_svg
import style_sheet as ss
from style_sheet import Svg, var

ROOT = Path(__file__).resolve().parent.parent
SOURCE_DIR = ROOT / "Art" / "Source" / "Game"
ART_DIR = ROOT / "Assets" / "_Game" / "Art"
UI_DIR = ART_DIR / "Ui"
SPRITES_CS = ROOT / "Assets" / "_Game" / "Scripts" / "Level" / "ArtSprites.cs"
THEMES = ("Night", "Sunset")

ATLAS_SIZE = 1024
ATLAS_PADDING = 3
BLEED_STEPS = 6
SPRITE_PPM = 64.0

CASTLE_WIDTH = 512
CASTLE_HEIGHT = 1024
CASTLE_PPM = 58.0
CASTLE_BASE_PX = 1016.0
# Совпадают с GameConfig: platformHeight и trackHeight.
PLATFORM_HEIGHT = 0.8
TRACK_HEIGHT = 1.2
TRACK_LENGTH = 2.7

BAND_WIDTH = 1024
BANDS = {
    # слой: высота в пикселях, пикселей на метр.
    "BackFar": (512, 20.0),
    "BackMid": (512, 28.0),
    "BackNear": (512, 28.0),
}
GLYPHS = "0123456789+"
GLYPH_PX = 76


class Sprite:
    """Спрайт атласа: draw(svg) рисует в пикселях холста, rect — его прямоугольник в метрах относительно точки привязки."""

    def __init__(self, name, width, height, rect, draw):
        self.name = name
        self.width = int(math.ceil(width))
        self.height = int(math.ceil(height))
        self.rect = rect
        self.draw = draw
        self.slot = None


def meters(min_x, min_y, max_x, max_y, ppm=SPRITE_PPM):
    """Размер холста и функция-обёртка: фигуры рисуются в метрах, ось Y вверх."""
    width = (max_x - min_x) * ppm
    height = (max_y - min_y) * ppm

    def wrap(draw):
        def run(svg):
            svg.open(f"translate({-min_x * ppm:.3f} {max_y * ppm:.3f}) scale({ppm:.3f} {-ppm:.3f})")
            draw(svg)
            svg.close()
        return run
    return width, height, (min_x, min_y, max_x, max_y), wrap


def metric_sprite(name, min_x, min_y, max_x, max_y, draw, ppm=SPRITE_PPM):
    width, height, rect, wrap = meters(min_x, min_y, max_x, max_y, ppm)
    return Sprite(name, width, height, rect, wrap(draw))


def pixel_sprite(name, size, half_meters, draw):
    return Sprite(name, size, size, (-half_meters, -half_meters, half_meters, half_meters), draw)


# ---------- Рисунки спрайтов ----------

def draw_cannon_body(side):
    def draw(svg):
        svg.rect(-0.55, -0.34, 0.74, 0.68, var(f"cannon{side}"), 0.22)
        svg.rect(-0.55, -0.34, 0.74, 0.3, var(f"cannon{side}Shade"), 0.2)
    return draw


def draw_cannon_barrel(side):
    def draw(svg):
        svg.rect(-0.25, -0.09, 1.48, 0.18, var(f"cannon{side}"), 0.09)
    return draw


def draw_track(svg):
    ss.track(svg, 0.0, 0.0, TRACK_LENGTH, TRACK_HEIGHT)


def draw_platform(half):
    def draw(svg):
        ss.platform(svg, half, PLATFORM_HEIGHT)
    return draw


def draw_spruce(svg):
    svg.open("translate(0.75 2.5) scale(1 1)")
    ss.add_spruce(svg, 0.0, 0.0, 2.4)
    svg.close()


def draw_bush(svg):
    ss.add_bush(svg, 0.8, 0.86, 0.5)


def draw_rock(svg):
    svg.path("M0.08 0.62 Q0.12 0.2 0.5 0.12 L0.86 0.08 Q1.18 0.18 1.22 0.62 Z", var("treeShade"))
    svg.path("M0.08 0.62 Q0.12 0.2 0.5 0.12 L0.62 0.11 Q0.74 0.34 0.6 0.62 Z", var("treeLight"))


def draw_marker(svg, theme):
    """Night: надгробие, своё. Sunset: кактус."""
    if theme == "Night":
        svg.path("M0.18 1.5 L0.18 0.6 Q0.18 0.08 0.65 0.08 Q1.12 0.08 1.12 0.6 L1.12 1.5 Z", var("backgroundNearLight"))
        svg.path("M0.72 1.5 L0.72 0.09 Q1.12 0.14 1.12 0.6 L1.12 1.5 Z", var("backgroundNear"))
        svg.rect(0.42, 0.56, 0.46, 0.11, var("backgroundNear"), 0.05)
        svg.rect(0.42, 0.8, 0.46, 0.11, var("backgroundNear"), 0.05)
    else:
        svg.rect(0.48, 0.08, 0.34, 1.42, var("treeLight"), 0.17)
        svg.rect(0.66, 0.08, 0.16, 1.42, var("treeShade"), 0.08)
        svg.rect(0.08, 0.44, 0.24, 0.5, var("treeLight"), 0.12)
        svg.rect(0.08, 0.76, 0.5, 0.18, var("treeLight"), 0.09)
        svg.rect(0.98, 0.3, 0.22, 0.46, var("treeShade"), 0.11)
        svg.rect(0.72, 0.6, 0.48, 0.16, var("treeShade"), 0.08)


def draw_moon(theme):
    def draw(svg):
        if theme == "Night":
            ss.add_moon(svg, 128, 128, 124)
        else:
            svg.circle(128, 128, 124, var("moon"))
            svg.circle(128, 128, 94, var("moonShade"))
    return draw


def draw_cloud(svg):
    """Тонкие вытянутые облака: полосы лежат над диском луны и дают ей глубину."""
    svg.rect(4, 52, 232, 14, var("backgroundHaze"), 7)
    svg.rect(44, 36, 150, 14, var("backgroundHaze"), 7)
    svg.rect(92, 20, 74, 12, var("backgroundHaze"), 6)
    svg.rect(20, 68, 120, 10, var("backgroundHaze"), 5)


def draw_booster(label, color):
    def draw(svg):
        ss.booster(svg, 55.0, 98.0, 96, 192, label, var(color), var(color + "Cap"))
    return draw


def draw_circle(svg):
    svg.circle(32, 32, 30, "#FFFFFF")


def draw_star(outer_share, inner_share, count, turn):
    def draw(svg):
        svg.poly(ss.star_points(64, 64, 62 * outer_share, 62 * inner_share, count, turn), "#FFFFFF")
        svg.circle(64, 64, 62 * inner_share, "#FFFFFF")
    return draw


def draw_spark(svg):
    svg.poly(ss.star_points(28, 28, 27, 8, 4, -math.pi / 2), "#FFFFFF")


def glyph_sprites():
    """Цифры поп-апа: белые, цвет даёт вершина. Прямоугольник в долях высоты, ширина — настоящая ширина знака."""
    font = ImageFont.truetype(str(flat_svg.HEAVY_FONT), GLYPH_PX)
    font.set_variation_by_axes([flat_svg.HEAVY_WEIGHT])
    sprites = []
    for index, char in enumerate(GLYPHS):
        advance = font.getlength(char)
        width = int(advance) + 6
        height = int(GLYPH_PX * 1.02)

        def draw(svg, value=char, center=width * 0.5):
            svg.text(center, GLYPH_PX * 0.5, value, GLYPH_PX, "#FFFFFF")
        sprite = Sprite(f"Glyph{index}", width, height, (0.0, 0.0, width / height, 1.0), draw)
        sprites.append(sprite)
    return sprites


def atlas_sprites(theme):
    sprites = [
        Sprite("White", 12, 12, (0.0, 0.0, 1.0, 1.0), lambda svg: svg.rect(0, 0, 12, 12, "#FFFFFF")),
        Sprite("Circle", 64, 64, (-1.0, -1.0, 1.0, 1.0), draw_circle),
        Sprite("StarEdge", 128, 128, (-1.0, -1.0, 1.0, 1.0), draw_star(1.0, 0.72, 9, 0.2)),
        Sprite("StarMid", 128, 128, (-1.0, -1.0, 1.0, 1.0), draw_star(1.0, 0.72, 8, 0.5)),
        Sprite("Spark", 56, 56, (-1.0, -1.0, 1.0, 1.0), draw_spark),
        metric_sprite("CannonBodyPlayer", -0.58, -0.37, 0.22, 0.37, draw_cannon_body("Player")),
        metric_sprite("CannonBodyEnemy", -0.58, -0.37, 0.22, 0.37, draw_cannon_body("Enemy")),
        metric_sprite("CannonBarrelPlayer", -0.28, -0.13, 1.26, 0.13, draw_cannon_barrel("Player")),
        metric_sprite("CannonBarrelEnemy", -0.28, -0.13, 1.26, 0.13, draw_cannon_barrel("Enemy")),
        metric_sprite("Track", -TRACK_LENGTH * 0.5 - 0.04, -0.22, TRACK_LENGTH * 0.5 + 0.04, TRACK_HEIGHT + 0.22, draw_track, 56.0),
        metric_sprite("PlatformBlue", -3.89, -0.04, 3.89, PLATFORM_HEIGHT + 0.04, draw_platform(3.85), 56.0),
        metric_sprite("PlatformRed", -4.39, -0.04, 4.39, PLATFORM_HEIGHT + 0.04, draw_platform(4.35), 56.0),
        Sprite("Spruce", 1.5 * SPRITE_PPM, 2.6 * SPRITE_PPM, (-0.75, -0.1, 0.75, 2.5), lambda svg: scaled_y_down(svg, draw_spruce)),
        Sprite("Bush", 1.6 * SPRITE_PPM, 1.0 * SPRITE_PPM, (-0.8, -0.14, 0.8, 0.86), lambda svg: scaled_y_down(svg, draw_bush)),
        Sprite("Rock", 1.3 * SPRITE_PPM, 0.7 * SPRITE_PPM, (-0.65, -0.08, 0.65, 0.62), lambda svg: scaled_y_down(svg, draw_rock)),
        Sprite("Marker", 1.3 * SPRITE_PPM, 1.6 * SPRITE_PPM, (-0.65, -0.1, 0.65, 1.5), lambda svg: scaled_y_down(svg, lambda s: draw_marker(s, theme))),
        pixel_sprite("Moon", 256, 3.6, draw_moon(theme)),
        Sprite("Cloud", 240, 84, (-3.6, -1.26, 3.6, 1.26), draw_cloud),
        Sprite("Booster10", 110, 196, (-0.859, -1.531, 0.859, 1.531), draw_booster("x10", "booster10")),
        Sprite("Booster20", 110, 196, (-0.859, -1.531, 0.859, 1.531), draw_booster("x20", "booster20")),
        Sprite("Booster50", 110, 196, (-0.859, -1.531, 0.859, 1.531), draw_booster("x50", "booster50")),
    ]
    return sprites + glyph_sprites()


def scaled_y_down(svg, draw):
    svg.open(f"scale({SPRITE_PPM:.1f})")
    draw(svg)
    svg.close()


# ---------- Текстуры целиком ----------

def castle_document(draw_castle):
    svg = Svg()
    svg.open(f"translate({CASTLE_WIDTH * 0.5} {CASTLE_BASE_PX}) scale({CASTLE_PPM} {-CASTLE_PPM})")
    draw_castle(svg, False)
    svg.close()
    return svg.document(CASTLE_WIDTH, CASTLE_HEIGHT)


def periodic(svg, width, draw):
    """Копии слева и справа: полоса тайлится без шва (ТЗ §3.10)."""
    for shift in (-width, 0, width):
        svg.open(f"translate({shift} 0)")
        draw(svg)
        svg.close()


def faceted_peak(svg, cx, base, half, height, light, dark, cap, seed):
    """Гора с гранями: светлая сторона, теневая сторона по ломаному хребту, шапка с рваным краем. Скруглённой вершины хватает на силуэт."""
    rng = random.Random(seed)
    top = base - height
    shoulder = height * 0.1
    svg.path(f"M{cx - half:.1f} {base:.1f} L{cx - half * 0.16:.1f} {top + shoulder:.1f} Q{cx:.1f} {top:.1f} {cx + half * 0.16:.1f} {top + shoulder:.1f} L{cx + half:.1f} {base:.1f} Z", light)
    ridge = [(cx, top + shoulder * 0.45)]
    y = top + shoulder
    x = cx
    while y < base:
        step = height * rng.uniform(0.09, 0.16)
        y = min(base, y + step)
        x += half * rng.uniform(-0.14, 0.2)
        ridge.append((x, y))
    svg.poly(ridge + [(cx + half, base), (cx + half * 0.16, top + shoulder)], dark)
    edge = top + height * 0.3
    tooth = half * 0.11
    points = [(cx - half * 0.16, top + shoulder), (cx + half * 0.16, top + shoulder)]
    right = cx + half * 0.3
    left = cx - half * 0.3
    points.append((right, edge))
    x = right
    drop = False
    while x > left:
        x -= tooth
        points.append((x, edge + (height * rng.uniform(0.05, 0.1) if drop else -height * 0.02)))
        drop = not drop
    points.append((left, edge))
    svg.poly(points, cap)


def fog(svg, width, y, rows, color, opacity, seed):
    """Туман: длинные полупрозрачные полосы, ступени вместо размытия."""
    rng = random.Random(seed)
    for row in range(rows):
        x = rng.uniform(-140, 40)
        while x < width + 140:
            length = rng.uniform(180, 460)
            svg.rect(x, y + row * 20 + rng.uniform(-5, 5), length, 26, color, 13, opacity)
            x += length + rng.uniform(40, 170)


def rolling_ridge(svg, width, height, y, swell, color, phase):
    """Плавный холм: период равен ширине полосы, поэтому стыкуется без шва."""
    points = []
    for x in range(0, width + 8, 8):
        u = x / width * math.tau
        points.append((x, y - swell * (math.sin(u + phase) * 0.6 + math.sin(u * 2 + phase * 1.7) * 0.4)))
    svg.poly(points + [(width, height), (0, height)], color)


def fir(svg, x, base, size, color, shade):
    """Ель со свисающими ярусами: не треугольник, а силуэт с плечами."""
    half = size * 0.3
    svg.rect(x - size * 0.03, base - size * 0.12, size * 0.06, size * 0.14, shade)
    for level in range(4):
        y = base - size * (0.1 + level * 0.2)
        w = half * (1.0 - level * 0.2)
        h = size * 0.34
        svg.path(f"M{x:.1f} {y - h:.1f} Q{x - w * 0.34:.1f} {y - h * 0.34:.1f} {x - w:.1f} {y:.1f} Q{x - w * 0.5:.1f} {y - h * 0.08:.1f} {x:.1f} {y - h * 0.02:.1f} Z", color)
        svg.path(f"M{x:.1f} {y - h:.1f} Q{x + w * 0.34:.1f} {y - h * 0.34:.1f} {x + w:.1f} {y:.1f} Q{x + w * 0.5:.1f} {y - h * 0.08:.1f} {x:.1f} {y - h * 0.02:.1f} Z", shade)


def building(svg, rng, x, base, w, h, fill, window, lit_share, roof=None):
    svg.rect(x, base - h, w, h + 60, fill, 3)
    if roof == "spire":
        svg.poly([(x - 3, base - h), (x + w * 0.5, base - h - w * 1.5), (x + w + 3, base - h)], fill)
    elif roof == "gable":
        svg.poly([(x - 4, base - h), (x + w * 0.5, base - h - w * 0.5), (x + w + 4, base - h)], fill)
    wy = base - h + 12
    while wy < base - 12:
        wx = x + 8
        while wx < x + w - 12:
            if rng.random() < lit_share:
                svg.rect(wx, wy, 7, 11, window, 1.5)
            wx += 15
        wy += 19


def town(svg, width, base, seed, fill, window, tall):
    rng = random.Random(seed)
    x = 30.0
    while x < width - 40:
        if rng.random() < 0.55:
            run = rng.randint(3, 6)
            for _ in range(run):
                w = rng.uniform(26, 48)
                h = rng.uniform(40, 96) * tall
                kind = rng.choice(("gable", "gable", "spire", None))
                building(svg, rng, x, base, w, h, fill, window, 0.34, kind)
                x += w + rng.uniform(-3, 5)
        x += rng.uniform(70, 170)


def organic_tree(svg, x, base, size, seed, color):
    """Изогнутое голое дерево: сужающиеся ветви из коротких отрезков, а не прямые палки."""
    rng = random.Random(seed)

    def branch(px, py, angle, length, width, depth):
        segments = 5
        points = [(px, py)]
        for i in range(segments):
            angle += rng.uniform(-0.2, 0.2)
            px += math.cos(angle) * length / segments
            py -= math.sin(angle) * length / segments
            points.append((px, py))
            svg.line(points[-2:], color, width * (1.0 - 0.5 * i / segments))
        if depth == 0:
            return
        count = 3 if depth > 2 else 2
        for i in range(count):
            spread = (i - (count - 1) * 0.5) * rng.uniform(0.5, 0.8)
            branch(px, py, angle + spread, length * rng.uniform(0.6, 0.76), width * 0.55, depth - 1)

    branch(x, base, math.pi * 0.5 + rng.uniform(-0.1, 0.1), size * 0.28, size * 0.07, 4)


def tombstone(svg, x, base, scale, fill, light):
    w = 24 * scale
    h = 44 * scale
    svg.path(f"M{x - w:.1f} {base:.1f} L{x - w:.1f} {base - h:.1f} Q{x:.1f} {base - h * 1.5:.1f} {x + w:.1f} {base - h:.1f} L{x + w:.1f} {base:.1f} Z", fill)
    svg.path(f"M{x - w:.1f} {base:.1f} L{x - w:.1f} {base - h:.1f} Q{x - w * 0.6:.1f} {base - h * 1.3:.1f} {x - w * 0.2:.1f} {base - h * 1.34:.1f} L{x - w * 0.2:.1f} {base:.1f} Z", light)


def cross(svg, x, base, scale, fill):
    svg.rect(x - 5 * scale, base - 70 * scale, 10 * scale, 72 * scale, fill, 3)
    svg.rect(x - 22 * scale, base - 52 * scale, 44 * scale, 10 * scale, fill, 3)


def lantern(svg, x, base, height, post, light):
    svg.rect(x - 3, base - height, 6, height, post, 2)
    for radius, opacity in ((54, 0.1), (36, 0.14), (22, 0.2)):
        svg.circle(x, base - height, radius, light, opacity)
    svg.rect(x - 8, base - height - 12, 16, 22, light, 5)


def fence(svg, x0, x1, base, fill):
    x = x0
    while x < x1:
        svg.poly([(x, base), (x, base - 40), (x + 4, base - 50), (x + 8, base - 40), (x + 8, base)], fill)
        x += 20
    svg.rect(x0 - 6, base - 34, x1 - x0 + 12, 6, fill, 2)
    svg.rect(x0 - 6, base - 16, x1 - x0 + 12, 6, fill, 2)


def cactus(svg, x, base, scale, fill):
    svg.rect(x - 9 * scale, base - 96 * scale, 18 * scale, 98 * scale, fill, 9 * scale)
    svg.rect(x - 34 * scale, base - 62 * scale, 34 * scale, 13 * scale, fill, 6 * scale)
    svg.rect(x - 34 * scale, base - 84 * scale, 13 * scale, 34 * scale, fill, 6 * scale)
    svg.rect(x, base - 44 * scale, 32 * scale, 13 * scale, fill, 6 * scale)
    svg.rect(x + 20 * scale, base - 70 * scale, 13 * scale, 38 * scale, fill, 6 * scale)


def telegraph(svg, width, base, fill):
    posts = (120, 460, 800)
    for x in posts:
        svg.rect(x - 4, base - 190, 8, 192, fill, 2)
        svg.rect(x - 34, base - 176, 68, 7, fill, 2)
    for start, end in ((120, 460), (460, 800), (800, 1144)):
        for offset in (-30, 30):
            points = []
            for i in range(21):
                t = i / 20
                x = start + (end - start) * t + offset
                points.append((x, base - 172 + 44 * 4 * t * (1 - t)))
            svg.line(points, fill, 2.5)
    svg.line([(i * 8 - 24, base - 172 + 44 * 4 * ((i * 8 + 224) / 340) * (1 - (i * 8 + 224) / 340)) for i in range(0, 19)], fill, 2.5)


def far_band(theme):
    width = BAND_WIDTH
    height, _ = BANDS["BackFar"]
    svg = Svg()

    def back(target):
        faceted_peak(target, 110, height, 250, 360, var("backgroundHaze"), var("backgroundFar"), var("backgroundHaze"), 3)
        faceted_peak(target, 640, height, 300, 410, var("backgroundHaze"), var("backgroundFar"), var("backgroundHaze"), 5)
        faceted_peak(target, 960, height, 220, 300, var("backgroundHaze"), var("backgroundFar"), var("backgroundHaze"), 7)

    def front(target):
        faceted_peak(target, 330, height, 320, 300, var("backgroundFar"), var("backgroundFarShade"), var("backgroundHaze"), 11)
        faceted_peak(target, 800, height, 280, 250, var("backgroundFar"), var("backgroundFarShade"), var("backgroundHaze"), 13)

    svg.parts.append('<g opacity="0.62">')
    periodic(svg, width, back)
    svg.parts.append('</g>')
    fog(svg, width, 268, 2, var("backgroundHaze"), 0.2, 31)
    periodic(svg, width, front)
    svg.rect(0, height - 24, width, 24, var("backgroundFarShade"))
    fog(svg, width, height - 150, 4, var("backgroundHaze"), 0.22, 37)
    return svg.document(width, height)


def mid_band(theme):
    width = BAND_WIDTH
    height, _ = BANDS["BackMid"]
    svg = Svg()
    window = var("castleWindowLit")
    if theme == "Night":
        town(svg, width, height - 140, 21, var("backgroundMid"), window, 1.2)
        rolling_ridge(svg, width, height, height - 150, 34, var("backgroundMid"), 0.7)
        rng = random.Random(9)
        x = 0.0
        while x < width:
            size = rng.uniform(70, 150)
            fir(svg, x, height - 118 + rng.uniform(-14, 14), size, var("backgroundMid"), var("backgroundNearLight"))
            x += size * rng.uniform(0.4, 1.3)
    else:
        rng = random.Random(9)
        town(svg, width, height - 120, 21, var("backgroundMid"), var("backgroundHaze"), 1.6)
        rolling_ridge(svg, width, height, height - 122, 24, var("backgroundMid"), 0.7)
    fog(svg, width, height - 96, 4, var("backgroundHaze"), 0.13, 41)
    svg.rect(0, height - 12, width, 12, var("backgroundMid"))
    return svg.document(width, height)


def near_band(theme):
    width = BAND_WIDTH
    height, _ = BANDS["BackNear"]
    svg = Svg()
    base = height - 74
    tone = var("backgroundNearLight")

    def trees(target):
        organic_tree(target, 300, base + 18, 470, 4, tone)
        organic_tree(target, 830, base + 18, 380, 8, tone)
    periodic(svg, width, trees)
    rolling_ridge(svg, width, height, base, 16, var("backgroundNear"), 2.1)
    if theme == "Night":
        fence(svg, 40, 240, base - 2, var("backgroundNearLight"))
        fence(svg, 560, 720, base + 6, var("backgroundNearLight"))
        for x, scale in ((420, 1.0), (500, 0.8), (930, 0.9)):
            tombstone(svg, x, base + 10, scale, var("backgroundNear"), var("backgroundNearLight"))
        cross(svg, 468, base + 12, 0.8, var("backgroundNearLight"))
        cross(svg, 975, base + 10, 1.0, var("backgroundNearLight"))
        lantern(svg, 352, base + 8, 74, var("backgroundNear"), var("castleWindowLit"))
        lantern(svg, 760, base + 12, 64, var("backgroundNear"), var("castleWindowLit"))
    else:
        telegraph(svg, width, base + 8, var("backgroundNear"))
        cactus(svg, 240, base + 12, 0.9, var("backgroundNearLight"))
        cactus(svg, 610, base + 10, 1.15, var("backgroundNearLight"))
        cactus(svg, 960, base + 12, 0.75, var("backgroundNearLight"))
    svg.rect(0, height - 30, width, 30, var("backgroundNear"))
    return svg.document(width, height)


# ---------- UI: белые формы красит палитра, остальное общие цвета HUD ----------

def ui_documents():
    docs = {}

    svg = Svg()
    svg.rect(0, 0, 64, 64, "#FFFFFF", 30)
    docs["UiPill"] = svg.document(64, 64)

    svg = Svg()
    svg.rect(0, 0, 32, 32, "#FFFFFF", 9)
    svg.rect(0, 18, 32, 14, "#D2D2D2", 8)
    svg.rect(0, 18, 32, 8, "#D2D2D2")
    docs["UiSegment"] = svg.document(32, 32)

    svg = Svg()
    svg.circle(256, 256, 254, "#FFFFFF")
    docs["UiCircle"] = svg.document(512, 512)

    svg = Svg()
    svg.parts.append('<circle cx="16" cy="16" r="10" fill="none" stroke="#FFFFFF" stroke-width="7"/>')
    docs["UiDegree"] = svg.document(32, 32)

    svg = Svg()
    ss.tower_icon(svg, 64, 74, 66, "#FFFFFF")
    svg.path("M53 113 L53 89 Q64 76 75 89 L75 113 Z", "#5A5A70")
    docs["UiTower"] = svg.document(128, 128)

    svg = Svg()
    ss.crossed_rockets(svg, 64, 64, 56)
    docs["UiRockets"] = svg.document(128, 128)

    svg = Svg()
    svg.open("translate(112 160) rotate(-18) scale(1.45)")
    for x, y, w, h, r in ((-11, -62, 22, 74, 11), (10, -20, 20, 46, 10), (28, -14, 18, 40, 9), (-28, -4, 18, 34, 9), (-24, 2, 66, 58, 18)):
        svg.rect(x, y, w, h, "#FFFFFF", r)
    svg.rect(-20, 62, 58, 15, var("hudPanel"), 6)
    svg.close()
    docs["UiHand"] = svg.document(256, 256)

    svg = Svg()
    ss.logo(svg, 340, 96, 96)
    docs["UiLogo"] = svg.document(680, 300)

    svg = Svg()
    ss.on_platform(svg, lambda s: draw_end_castle(s, ss.blue_castle), 256, 490, 34.0)
    docs["UiEndBlue"] = svg.document(512, 512)

    svg = Svg()
    ss.on_platform(svg, lambda s: draw_end_castle(s, ss.red_castle), 256, 490, 30.0)
    docs["UiEndRed"] = svg.document(512, 512)
    return docs


def draw_end_castle(svg, castle):
    castle(svg, True)


# ---------- Сборка ----------

def bleed(image):
    """Цвет заходит в прозрачные тексели: билинейка у края не тянет чёрный."""
    pixels = numpy.array(image, dtype=numpy.uint8)
    alpha = pixels[..., 3] > 0
    color = pixels[..., :3].astype(numpy.int32)
    for _ in range(BLEED_STEPS):
        grown = alpha.copy()
        total = numpy.zeros_like(color)
        count = numpy.zeros(alpha.shape, dtype=numpy.int32)
        for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
            shifted_alpha = numpy.roll(alpha, (dy, dx), (0, 1))
            total += numpy.roll(color, (dy, dx), (0, 1)) * shifted_alpha[..., None]
            count += shifted_alpha
            grown |= shifted_alpha
        fresh = grown & ~alpha
        color[fresh] = total[fresh] // count[fresh][..., None]
        alpha = grown
    pixels[..., :3] = color
    return Image.fromarray(pixels, "RGBA")


def save(image, path):
    path.parent.mkdir(parents=True, exist_ok=True)
    bleed(image).save(path, optimize=True)
    return path.stat().st_size


def source_text(name, build):
    """SVG из кода; при --keep-svg уже лежащий файл важнее."""
    path = SOURCE_DIR / f"{name}.svg"
    if "--keep-svg" in sys.argv and path.exists():
        return path.read_text(encoding="utf-8")
    text = build()
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")
    return text


def pack(sprites):
    """Полки по убыванию высоты. Раскладка считается один раз и одна на темы."""
    x = ATLAS_PADDING
    y = ATLAS_PADDING
    shelf = 0
    for sprite in sorted(sprites, key=lambda item: -item.height):
        if x + sprite.width + ATLAS_PADDING > ATLAS_SIZE:
            x = ATLAS_PADDING
            y += shelf + ATLAS_PADDING * 2
            shelf = 0
        if y + sprite.height + ATLAS_PADDING > ATLAS_SIZE:
            raise RuntimeError(f"atlas overflow at {sprite.name}")
        sprite.slot = (x, y)
        x += sprite.width + ATLAS_PADDING * 2
        shelf = max(shelf, sprite.height)
    return y + shelf


def build_atlas(theme, palette, layout):
    atlas = Image.new("RGBA", (ATLAS_SIZE, ATLAS_SIZE), (0, 0, 0, 0))
    sprites = atlas_sprites(theme)
    for sprite, placed in zip(sprites, layout):
        def build(item=sprite):
            svg = Svg()
            item.draw(svg)
            return svg.document(item.width, item.height)
        # Источники атласа зависят от темы только у луны и маркера.
        text = source_text(f"{sprite.name}_{theme}" if sprite.name in ("Moon", "Marker") else sprite.name, build)
        atlas.paste(flat_svg.render(text, palette), placed.slot)
    return atlas


def cs_floats(values):
    return ", ".join(f"{value:.6f}f".replace("-0.000000f", "0f") for value in values)


def write_sprites_cs(sprites):
    names = "\n".join(f"        public const int {sprite.name} = {i};" for i, sprite in enumerate(sprites))
    uv = []
    rect = []
    for sprite in sprites:
        inset = 0.5 if sprite.name != "White" else 4.0
        x, y = sprite.slot
        uv.extend(((x + inset) / ATLAS_SIZE, 1.0 - (y + sprite.height - inset) / ATLAS_SIZE,
                   (x + sprite.width - inset) / ATLAS_SIZE, 1.0 - (y + inset) / ATLAS_SIZE))
        rect.extend(sprite.rect)
    uv_lines = ",\n            ".join(cs_floats(uv[i:i + 4]) for i in range(0, len(uv), 4))
    rect_lines = ",\n            ".join(cs_floats(rect[i:i + 4]) for i in range(0, len(rect), 4))
    bands = "\n".join(
        f"        public const float {name}WidthMeters = {BAND_WIDTH / ppm:.4f}f;\n        public const float {name}HeightMeters = {height / ppm:.4f}f;"
        for name, (height, ppm) in BANDS.items())
    text = f"""// Сгенерировано Tools/build_art.py, руками не править.
namespace CastleDestroyers.Level
{{
    /// <summary>Раскладка атласа и размеры текстур: одна на обе темы.</summary>
    public static class ArtSprites
    {{
{names}
        public const int GlyphPlus = Glyph10;

        public const float CastleArtWidthMeters = {CASTLE_WIDTH / CASTLE_PPM:.5f}f;
        public const float CastleArtHeightMeters = {CASTLE_HEIGHT / CASTLE_PPM:.5f}f;
        /// <summary>Низ текстуры замка ниже низа стены на столько метров.</summary>
        public const float CastleArtBaseMeters = {(CASTLE_HEIGHT - CASTLE_BASE_PX) / CASTLE_PPM:.5f}f;
{bands}

        /// <summary>UV спрайта: minU, minV, maxU, maxV.</summary>
        public static readonly float[] Uv =
        {{
            {uv_lines}
        }};

        /// <summary>Прямоугольник спрайта относительно точки привязки: minX, minY, maxX, maxY.</summary>
        public static readonly float[] Rect =
        {{
            {rect_lines}
        }};
    }}
}}
"""
    SPRITES_CS.write_text(text, encoding="utf-8")


def main():
    palettes = flat_svg.load_palettes()
    layout = atlas_sprites("Night")
    used = pack(layout)
    print(f"atlas shelves end at y={used} of {ATLAS_SIZE}")
    write_sprites_cs(layout)

    total = {}
    for theme in THEMES:
        palette = palettes[theme]
        folder = ART_DIR / f"Theme{theme}"
        sizes = {"Atlas": save(build_atlas(theme, palette, layout), folder / f"Atlas_{theme}.png")}
        for name, castle in (("CastleBlue", ss.blue_castle), ("CastleRed", ss.red_castle)):
            text = source_text(name, lambda c=castle: castle_document(c))
            sizes[name] = save(flat_svg.render(text, palette), folder / f"{name}_{theme}.png")
        for name, band in (("BackFar", far_band), ("BackMid", mid_band), ("BackNear", near_band)):
            text = source_text(f"{name}_{theme}", lambda b=band: b(theme))
            sizes[name] = save(flat_svg.render(text, palette), folder / f"{name}_{theme}.png")
        total[theme] = sum(sizes.values())
        print(theme, {key: value // 1024 for key, value in sizes.items()}, "KB, total", total[theme] // 1024, "KB")

    ui_bytes = 0
    for name, text in ui_documents().items():
        (SOURCE_DIR / f"{name}.svg").write_text(text, encoding="utf-8")
        ui_bytes += save(flat_svg.render(text, palettes["Night"]), UI_DIR / f"{name}.png")
    print("UI", ui_bytes // 1024, "KB")


if __name__ == "__main__":
    main()
