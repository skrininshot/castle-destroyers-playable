#!/usr/bin/env python3
"""Style sheet этапа 7: образцы арта в SVG (Art/Source/Style) и сводный лист docs/reports/img/style_sheet.png.

Использование: python Tools/style_sheet.py

Образцы пишутся как обычные SVG с токенами палитры, поэтому их можно открыть в браузере и подменить.
Цвета берутся из ThemePalette.cs. Лист показывает оба пресета рядом со скриншотами Bowmasters.
"""
import math
import random
from pathlib import Path
from xml.sax.saxutils import escape

from PIL import Image, ImageDraw, ImageFont

import flat_svg

ROOT = Path(__file__).resolve().parent.parent
STYLE_DIR = ROOT / "Art" / "Source" / "Style"
REFERENCES = ROOT / "docs" / "references" / "style"
OUTPUT = ROOT / "docs" / "reports" / "img" / "style_sheet.png"


def var(name):
    return f"var(--{name})"


class Svg:
    """Накопитель фигур: координаты в единицах холста, ось Y вниз."""

    def __init__(self):
        self.parts = []

    def rect(self, x, y, w, h, fill, rx=0, opacity=1):
        self.parts.append(f'<rect x="{x:.2f}" y="{y:.2f}" width="{w:.2f}" height="{h:.2f}" rx="{rx:.2f}" fill="{fill}" opacity="{opacity}"/>')

    def circle(self, cx, cy, r, fill, opacity=1):
        self.parts.append(f'<circle cx="{cx:.2f}" cy="{cy:.2f}" r="{r:.2f}" fill="{fill}" opacity="{opacity}"/>')

    def ellipse(self, cx, cy, rx, ry, fill):
        self.parts.append(f'<ellipse cx="{cx:.2f}" cy="{cy:.2f}" rx="{rx:.2f}" ry="{ry:.2f}" fill="{fill}"/>')

    def poly(self, points, fill, opacity=1):
        text = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
        self.parts.append(f'<polygon points="{text}" fill="{fill}" opacity="{opacity}"/>')

    def path(self, data, fill):
        self.parts.append(f'<path d="{data}" fill="{fill}"/>')

    def line(self, points, stroke, width):
        data = "M" + " L".join(f"{x:.2f} {y:.2f}" for x, y in points)
        self.parts.append(f'<path d="{data}" fill="none" stroke="{stroke}" stroke-width="{width:.2f}"/>')

    def text(self, x, y, value, size, fill, anchor="middle"):
        self.parts.append(f'<text x="{x:.2f}" y="{y:.2f}" font-size="{size}" text-anchor="{anchor}" fill="{fill}">{escape(value)}</text>')

    def open(self, transform):
        self.parts.append(f'<g transform="{transform}">')

    def close(self):
        self.parts.append("</g>")

    def document(self, width, height):
        body = "\n".join(self.parts)
        return f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">\n{body}\n</svg>\n'


# ---------- Ландшафт ----------

def scene_height(x, width):
    """Контур земли макета: холм, гора с расщелиной, холм (форма уровня 1 в сжатом виде)."""
    u = x / width
    y = 0.70
    y -= 0.34 * math.exp(-((u - 0.5) / 0.135) ** 2)
    y -= 0.06 * math.exp(-((u - 0.12) / 0.11) ** 2)
    y -= 0.06 * math.exp(-((u - 0.88) / 0.11) ** 2)
    return y


def contour(height_fn, width, step=1.0):
    count = int(width / step) + 1
    return [(i * step, height_fn(i * step)) for i in range(count)]


def add_drips(svg, outline, base, seed, scale):
    """Капли кромки: вертикальные стенки, круглое дно и скруглённые впадины, как на кладбище Bowmasters."""
    rng = random.Random(seed)
    xs = [p[0] for p in outline]
    ys = [p[1] for p in outline]
    step = xs[1] - xs[0]
    x = xs[0] + rng.uniform(4.0, 16.0) * scale
    while x < xs[-1] - 12.0 * scale:
        half = rng.uniform(5.0, 7.5) * scale
        length = rng.uniform(9.0, 30.0) * scale
        fillet = 4.0 * scale
        left = min(len(xs) - 1, int((x - half - fillet - xs[0]) / step))
        right = min(len(xs) - 1, int((x + half + fillet - xs[0]) / step))
        top = max(ys[left], ys[right]) + base
        y_left = ys[left] + base
        y_right = ys[right] + base
        bottom = top + length
        svg.path(
            f"M{x - half - fillet:.2f} {y_left - 2:.2f} L{x - half - fillet:.2f} {y_left:.2f} "
            f"Q{x - half:.2f} {y_left:.2f} {x - half:.2f} {y_left + fillet:.2f} "
            f"L{x - half:.2f} {bottom - half:.2f} Q{x - half:.2f} {bottom:.2f} {x:.2f} {bottom:.2f} "
            f"Q{x + half:.2f} {bottom:.2f} {x + half:.2f} {bottom - half:.2f} "
            f"L{x + half:.2f} {y_right + fillet:.2f} Q{x + half:.2f} {y_right:.2f} {x + half + fillet:.2f} {y_right:.2f} "
            f"L{x + half + fillet:.2f} {y_right - 2:.2f} Z",
            var("groundRim"))
        x += rng.uniform(20.0, 34.0) * scale


def add_slime_ground(svg, outline, bottom, seed, scale=1.0, buried=True):
    """Масса земли, яркая «капающая» кромка и блик сверху."""
    xs = [p[0] for p in outline]
    ys = [p[1] for p in outline]
    svg.poly(outline + [(xs[-1], bottom), (xs[0], bottom)], var("ground"))
    if buried:
        add_ground_blobs(svg, outline, bottom, seed + 1, scale)
    base = 20.0 * scale
    lower = [(x, y + base) for x, y in zip(xs, ys)]
    svg.poly(outline + lower[::-1], var("groundRim"))
    add_drips(svg, outline, base, seed, scale)
    highlight = [(x, y + 7.0 * scale) for x, y in zip(xs, ys)]
    svg.poly(outline + highlight[::-1], var("groundRimLight"))


def add_scallop_ground(svg, outline, bottom, seed, scale=1.0, buried=True):
    """Масса земли, второй плоский слой вдоль контура и тёмная фестонная кромка (пустыня Bowmasters)."""
    xs = [p[0] for p in outline]
    ys = [p[1] for p in outline]
    svg.poly(outline + [(xs[-1], bottom), (xs[0], bottom)], var("ground"))
    if buried:
        add_ground_blobs(svg, outline, bottom, seed + 1, scale)
    band = [(x, y + 62.0 * scale) for x, y in zip(xs, ys)]
    svg.poly(outline + band[::-1], var("groundShade"))
    lower = [(x, y + 24.0 * scale) for x, y in zip(xs, ys)]
    rng = random.Random(seed)
    x = xs[0]
    while x < xs[-1]:
        r = rng.uniform(15.0, 20.0) * scale
        i = min(len(xs) - 1, int((x - xs[0]) / (xs[1] - xs[0])))
        if x < xs[-1] - r:
            svg.circle(x, ys[i] + 24.0 * scale, r, var("groundRim"))
        x += r * 1.72
    svg.poly(outline + lower[::-1], var("groundRim"))


def add_ground_blobs(svg, outline, bottom, seed, scale):
    """Редкие плоские круги и «закопанные» декоры: кости и камни, свои формы."""
    rng = random.Random(seed)
    xs = [p[0] for p in outline]
    for _ in range(int(xs[-1] * 0.02)):
        i = rng.randrange(len(xs))
        top = outline[i][1] + 80.0 * scale
        if top >= bottom:
            continue
        svg.circle(xs[i], rng.uniform(top, bottom), rng.uniform(5.0, 13.0) * scale, var("groundShade"))
    for _ in range(max(1, int(xs[-1] * 0.003))):
        i = rng.randrange(len(xs))
        top = outline[i][1] + 100.0 * scale
        if top >= bottom:
            continue
        add_bone(svg, xs[i], rng.uniform(top, bottom), rng.uniform(-40, 40), 15.0 * scale)


def add_bone(svg, x, y, angle, size):
    svg.open(f"translate({x:.2f} {y:.2f}) rotate({angle:.1f})")
    svg.rect(-size * 1.6, -size * 0.28, size * 3.2, size * 0.56, var("groundBuried"), size * 0.2)
    for sx in (-1.6, 1.6):
        for sy in (-0.42, 0.42):
            svg.circle(sx * size, sy * size, size * 0.46, var("groundBuried"))
    svg.close()


def add_water(svg, width, y, height):
    svg.rect(0, y, width, height, var("water"))
    wave = [(x, y + 2.5 * math.sin(x * 0.045) + 1.2 * math.sin(x * 0.11)) for x in range(0, width + 8, 8)]
    svg.poly(wave + [(width, y + 11), (0, y + 11)], var("waterRim"))
    for i in range(7):
        x = 40 + i * width / 7.0
        svg.rect(x, y + 26 + (i % 3) * 13, 70 + (i * 37) % 60, 6, var("waterHighlight"), 3)


# ---------- Фон ----------

def add_moon(svg, x, y, r):
    svg.circle(x, y, r, var("moon"))
    for dx, dy, cr in ((-0.35, -0.2, 0.26), (0.34, 0.26, 0.2), (0.12, -0.5, 0.13)):
        svg.circle(x + dx * r, y + dy * r, cr * r, var("moonShade"))
        svg.circle(x + dx * r + cr * r * 0.2, y + dy * r + cr * r * 0.18, cr * r * 0.74, var("moon"))


def add_mountain(svg, cx, base, half, height):
    """Гора из светлой шапки с фестоном и тёмного тела: ступень тона без градиента."""
    top = base - height
    svg.path(f"M{cx - half:.1f} {base:.1f} L{cx - half * 0.13:.1f} {top + height * 0.12:.1f} Q{cx:.1f} {top:.1f} {cx + half * 0.13:.1f} {top + height * 0.12:.1f} L{cx + half:.1f} {base:.1f} Z", var("backgroundFar"))
    level = top + height * 0.36
    inset = half * 0.3
    r = half * 0.075
    x = cx - inset
    svg.path(f"M{cx - half:.1f} {base:.1f} L{cx - inset - r:.1f} {level:.1f} L{cx + inset + r:.1f} {level:.1f} L{cx + half:.1f} {base:.1f} Z", var("backgroundFarShade"))
    while x < cx + inset + 1:
        svg.circle(x, level, r, var("backgroundFar"))
        x += r * 1.9


def add_forest(svg, width, base, seed):
    rng = random.Random(seed)
    x = -10.0
    while x < width + 20:
        h = rng.uniform(60, 120)
        w = h * 0.5
        svg.poly([(x - w, base), (x, base - h), (x + w, base)], var("backgroundMid"))
        x += w * rng.uniform(0.7, 1.1)
    svg.rect(0, base - 2, width, 300, var("backgroundMid"))


def add_city(svg, width, base, seed):
    rng = random.Random(seed)
    x = -10.0
    while x < width + 20:
        w = rng.uniform(60, 120)
        h = rng.uniform(90, 200)
        svg.rect(x, base - h, w, h + 300, var("backgroundMid"), 6)
        for wy in range(int(base - h + 16), int(base) - 10, 28):
            for wx in range(int(x + 12), int(x + w - 14), 24):
                if rng.random() < 0.4:
                    svg.rect(wx, wy, 11, 16, var("backgroundFar"), 2)
        x += w + rng.uniform(-6, 14)


def add_bare_tree(svg, x, base, size, seed):
    rng = random.Random(seed)
    stroke = var("backgroundNearLight")

    def branch(px, py, angle, length, width, depth):
        nx = px + math.cos(angle) * length
        ny = py - math.sin(angle) * length
        svg.line([(px, py), (nx, ny)], stroke, width)
        if depth == 0:
            return
        for sign in (-1, 1):
            branch(nx, ny, angle + sign * rng.uniform(0.35, 0.7), length * rng.uniform(0.62, 0.74), width * 0.66, depth - 1)

    branch(x, base, math.pi / 2, size * 0.34, size * 0.075, 4)


def add_near_bushes(svg, width, base, seed):
    rng = random.Random(seed)
    x = -20.0
    while x < width + 30:
        r = rng.uniform(26, 58)
        svg.circle(x, base - r * 0.3, r, var("backgroundNear"))
        x += r * rng.uniform(1.0, 2.1)
    svg.rect(0, base - 8, width, 300, var("backgroundNear"))


def add_sky_dots(svg, width, height, seed):
    rng = random.Random(seed)
    for _ in range(26):
        svg.circle(rng.uniform(0, width), rng.uniform(0, height), rng.uniform(1.4, 3.0), var("skyStar"))


def add_pill_clouds(svg, placements):
    for x, y, w in placements:
        svg.rect(x, y, w, 26, var("backgroundFar"), 13)
        svg.rect(x + w * 0.2, y - 18, w * 0.5, 26, var("backgroundFar"), 13)


def add_spruce(svg, x, base, height):
    """Ель из трёх ярусов: левая половина светлая, правая в тени, вершины скруглены."""
    svg.rect(x - height * 0.035, base - height * 0.2, height * 0.07, height * 0.2, var("platformShade"), 2)
    for level, w in ((0.0, 0.3), (0.26, 0.24), (0.5, 0.17)):
        y0 = base - height * (0.14 + level)
        top = y0 - height * (0.5 - level * 0.2)
        half = height * w
        svg.path(f"M{x - half:.1f} {y0:.1f} Q{x - half * 1.04:.1f} {y0 + height * 0.03:.1f} {x - half * 0.8:.1f} {y0 + height * 0.03:.1f} L{x:.1f} {y0 + height * 0.03:.1f} L{x:.1f} {top:.1f} Z", var("treeLight"))
        svg.path(f"M{x + half:.1f} {y0:.1f} Q{x + half * 1.04:.1f} {y0 + height * 0.03:.1f} {x + half * 0.8:.1f} {y0 + height * 0.03:.1f} L{x:.1f} {y0 + height * 0.03:.1f} L{x:.1f} {top:.1f} Z", var("treeShade"))


def add_bush(svg, x, base, r):
    svg.circle(x - r * 0.9, base - r * 0.35, r * 0.72, var("bush"))
    svg.circle(x + r * 0.95, base - r * 0.3, r * 0.65, var("bush"))
    svg.circle(x, base - r * 0.55, r, var("bush"))
    svg.circle(x - r * 0.3, base - r * 0.8, r * 0.16, var("bushLight"))
    svg.circle(x + r * 0.35, base - r * 0.42, r * 0.11, var("bushLight"))


# ---------- Замки: группа в метрах, ось Y вверх, низ стены на y = 0 ----------

def wall(svg, rng, x0, x1, y0, y1, bricks=True):
    w = x1 - x0
    h = y1 - y0
    svg.rect(x0, y0, w, h, var("castleStone"), 0.05)
    svg.rect(x1 - w * 0.26, y0, w * 0.26, h, var("castleStoneShade"), 0.05)
    if not bricks:
        return
    for _ in range(int(w * h * 0.55)):
        x = rng.uniform(x0 + 0.1, x1 - 0.55)
        y = rng.uniform(y0 + 0.1, y1 - 0.3)
        svg.rect(x, y, 0.42, 0.17, var("castleBrick"), 0.085)


def roof(svg, cx, width, base, height, light, shade, flag):
    half = width * 0.5
    apex = base + height
    svg.rect(cx - 0.04, apex - 0.1, 0.08, 0.72, var("castleCrack"), 0.04)
    svg.path(f"M{cx + 0.04:.2f} {apex + 0.6:.2f} Q{cx + 0.32:.2f} {apex + 0.66:.2f} {cx + 0.66:.2f} {apex + 0.5:.2f} Q{cx + 0.36:.2f} {apex + 0.42:.2f} {cx + 0.04:.2f} {apex + 0.22:.2f} Z", flag)
    svg.path(f"M{cx - half:.2f} {base:.2f} Q{cx - half * 0.42:.2f} {base + height * 0.3:.2f} {cx - 0.05:.2f} {apex:.2f} L{cx:.2f} {apex + 0.03:.2f} L{cx:.2f} {base:.2f} Z", light)
    svg.path(f"M{cx + half:.2f} {base:.2f} Q{cx + half * 0.42:.2f} {base + height * 0.3:.2f} {cx + 0.05:.2f} {apex:.2f} L{cx:.2f} {apex + 0.03:.2f} L{cx:.2f} {base:.2f} Z", shade)


def collar(svg, cx, wall_width, top):
    half = wall_width * 0.5 + 0.2
    svg.rect(cx - wall_width * 0.5, top - 0.42, wall_width, 0.2, var("castleStoneShade"))
    svg.rect(cx - half, top - 0.22, half * 2, 0.34, var("castleStone"), 0.15)
    svg.rect(cx - half, top - 0.22, half * 2, 0.12, var("castleStoneShade"), 0.06)


def window(svg, cx, y, width, height, lit):
    half = width * 0.5
    arch = f"M{{a:.2f}} {y:.2f} L{{a:.2f}} {y + height - half:.2f} Q{{a:.2f}} {y + height:.2f} {cx:.2f} {y + height:.2f} Q{{b:.2f}} {y + height:.2f} {{b:.2f}} {y + height - half:.2f} L{{b:.2f}} {y:.2f} Z"
    svg.path(arch.format(a=cx - half, b=cx + half), var("castleWindow"))
    if lit:
        inset = width * 0.17
        svg.path(arch.format(a=cx - half + inset, b=cx + half - inset), var("castleWindowLit"))
        svg.rect(cx - 0.025, y, 0.05, height, var("castleWindow"))
    svg.rect(cx - half - 0.1, y - 0.1, width + 0.2, 0.13, var("castleStoneShade"), 0.06)


def platform(svg, half, height):
    """Деревянная платформа: две полосы тона, доски, арки ворот."""
    svg.rect(-half, 0, half * 2, height, var("platform"), 0.3)
    svg.rect(-half, 0, half * 2, height * 0.34, var("platformShade"), 0.26)
    for i in range(-3, 4):
        svg.rect(i * half / 3.4 - 0.02, height * 0.42, 0.05, height * 0.5, var("platformShade"), 0.02)
    for cx in (-1.05, 1.05):
        svg.path(f"M{cx - 0.5:.2f} 0 L{cx - 0.5:.2f} {height * 0.4:.2f} Q{cx - 0.5:.2f} {height * 0.86:.2f} {cx:.2f} {height * 0.86:.2f} Q{cx + 0.5:.2f} {height * 0.86:.2f} {cx + 0.5:.2f} {height * 0.4:.2f} L{cx + 0.5:.2f} 0 Z", var("platformShade"))
        svg.path(f"M{cx - 0.39:.2f} 0 L{cx - 0.39:.2f} {height * 0.36:.2f} Q{cx - 0.39:.2f} {height * 0.72:.2f} {cx:.2f} {height * 0.72:.2f} Q{cx + 0.39:.2f} {height * 0.72:.2f} {cx + 0.39:.2f} {height * 0.36:.2f} L{cx + 0.39:.2f} 0 Z", var("castleWindow"))


def track(svg, cx, y, length, height):
    """Гусеница: тёмная скруглённая лента, шипы и шестерни с круглыми спицами."""
    half = length * 0.5
    r = height * 0.5
    x = cx - half + r
    while x <= cx + half - r + 0.01:
        svg.poly([(x - 0.12, y + height), (x, y + height + 0.17), (x + 0.12, y + height)], var("track"))
        svg.poly([(x - 0.12, y), (x, y - 0.17), (x + 0.12, y)], var("track"))
        x += 0.38
    svg.rect(cx - half, y, length, height, var("track"), r)
    for wx in (cx - length * 0.25, cx + length * 0.25):
        wy = y + r
        wr = height * 0.33
        svg.circle(wx, wy, wr, var("trackWheel"))
        for i in range(6):
            a = i * math.pi / 3
            svg.circle(wx + math.cos(a) * wr * 0.62, wy + math.sin(a) * wr * 0.62, wr * 0.15, var("track"))
        svg.circle(wx, wy, wr * 0.28, var("track"))


def cannon(svg, x, y, angle, side, size, mirrored=False):
    """Трёхствольная пушка: блок сзади, стволы с верхним бликом и дульным кольцом."""
    color = var(f"cannon{side}")
    shade = var(f"cannon{side}Shade")
    scale = -1 if mirrored else 1
    svg.open(f"translate({x:.2f} {y:.2f}) scale({scale * size:.3f} {size:.3f}) rotate({-angle:.1f})")
    for i, offset in enumerate((-0.2, 0.0, 0.2)):
        svg.rect(-0.25, offset - 0.09, 1.4, 0.18, shade if i == 0 else color, 0.07)
        if i == 2:
            svg.rect(0.0, offset - 0.065, 1.05, 0.05, var("rocketBody"), 0.025, 0.45)
        svg.rect(1.03, offset - 0.105, 0.2, 0.21, shade, 0.06)
    svg.rect(-0.55, -0.34, 0.74, 0.68, shade, 0.2)
    svg.rect(-0.55, -0.34, 0.74, 0.36, color, 0.18)
    svg.close()


def blue_castle(svg, with_platform=True):
    rng = random.Random(11)
    wall(svg, rng, -3.5, 3.5, 0, 3.6)
    for cx, lit in ((-2.3, True), (2.3, False)):
        wall(svg, rng, cx - 1.2, cx + 1.2, 0, 6.6)
        roof(svg, cx, 2.8, 6.6, 2.5, var("roofPlayer"), var("roofPlayerShade"), var("cannonPlayer"))
        collar(svg, cx, 2.4, 6.6)
        window(svg, cx, 4.7, 0.8, 1.15, lit)
    window(svg, 0, 1.2, 0.8, 1.15, True)
    if with_platform:
        platform(svg, 3.85, 1.0)


def red_castle(svg, with_platform=True):
    rng = random.Random(23)
    wall(svg, rng, -4.0, 4.0, 0, 2.6)
    wall(svg, rng, -1.7, 1.7, 0, 6.2)
    offsets = (-3.55, 3.55, -2.45, 2.45, -1.15, 1.15, 0.0)
    widths = (0.9, 0.9, 1.0, 1.0, 1.1, 1.1, 1.25)
    tops = (3.1, 3.1, 3.9, 3.9, 7.0, 7.0, 11.0)
    heights = (1.3, 1.3, 1.4, 1.4, 1.6, 1.6, 1.9)
    for i, cx in enumerate(offsets):
        w = widths[i]
        wall(svg, rng, cx - w * 0.5, cx + w * 0.5, 0, tops[i])
        roof(svg, cx, w + 0.4, tops[i], heights[i], var("roofEnemy"), var("roofEnemyShade"), var("cannonEnemy"))
        collar(svg, cx, w, tops[i])
        window(svg, cx, tops[i] - 1.25, 0.42, 0.72, i % 2 == 0)
    if with_platform:
        platform(svg, 4.35, 1.0)


def on_platform(svg, draw_castle, x, ground_y, scale, tilt=0.0):
    """Ставит замок с гусеницами на землю; 1 м = scale единиц холста."""
    svg.open(f"translate({x:.2f} {ground_y:.2f}) rotate({tilt:.2f}) scale({scale:.3f} {-scale:.3f})")
    track(svg, -2.45, 0.0, 2.7, 0.75)
    track(svg, 2.45, 0.0, 2.7, 0.75)
    svg.open("translate(0 0.72)")
    svg.open("translate(0 0)")
    draw_castle(svg)
    svg.close()
    svg.close()
    svg.close()


# ---------- Бой ----------

def rocket(svg, x, y, angle, side, size=1.0):
    color = var("rocketPlayer" if side == "Player" else "rocketEnemy")
    svg.open(f"translate({x:.2f} {y:.2f}) rotate({angle:.1f}) scale({size:.3f})")
    svg.path("M-34 0 Q-22 -11 -12 -6 L-12 6 Q-22 11 -34 0 Z", var("rocketTail"))
    svg.path("M-20 0 Q-16 -4 -12 -3 L-12 3 Q-16 4 -20 0 Z", var("blastMid"))
    svg.path("M-14 -7 L-24 -17 L-5 -7 Z", color)
    svg.path("M-14 7 L-24 17 L-5 7 Z", color)
    svg.rect(-14, -8, 34, 16, var("rocketBody"), 6)
    svg.rect(-14, 2, 34, 6, var("rocketShade"), 3)
    svg.path("M16 -8 Q36 -7 42 0 Q36 7 16 8 Z", color)
    svg.rect(4, -8, 6, 16, color)
    svg.close()


def smoke(svg, x, y, r):
    svg.circle(x, y, r, var("smoke"), 0.75)


def star_points(cx, cy, outer, inner, count, turn):
    points = []
    for i in range(count * 2):
        r = outer if i % 2 == 0 else inner
        a = turn + i * math.pi / count
        points.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return points


def blast(svg, x, y, size, seed=5):
    rng = random.Random(seed)
    for _ in range(9):
        a = rng.uniform(0, math.tau)
        d = size * rng.uniform(1.15, 1.6)
        svg.circle(x + math.cos(a) * d, y + math.sin(a) * d, size * rng.uniform(0.07, 0.15), var("blastEdge"))
    svg.poly(star_points(x, y, size, size * 0.72, 9, 0.2), var("blastEdge"))
    svg.circle(x, y, size * 0.72, var("blastEdge"))
    svg.poly(star_points(x, y, size * 0.76, size * 0.54, 8, 0.5), var("blastMid"))
    svg.circle(x, y, size * 0.54, var("blastMid"))
    svg.circle(x, y, size * 0.36, var("blastCore"))


def booster(svg, cx, cy, w, h, label, color, cap):
    """Стеклянный цилиндр в профиль (ТЗ §3.5): плоская полупрозрачная заливка, блики, заглушки, надпись."""
    x = cx - w * 0.5
    y = cy - h * 0.5
    cap_h = h * 0.1
    svg.rect(x, y + cap_h * 0.5, w, h - cap_h, color, 0, 0.9)
    svg.rect(x + w * 0.14, y + cap_h * 1.3, w * 0.1, h - cap_h * 2.6, "#FFFFFF", w * 0.05, 0.75)
    svg.rect(x + w * 0.76, y + cap_h * 1.3, w * 0.05, h - cap_h * 2.6, "#FFFFFF", w * 0.025, 0.5)
    svg.rect(x - w * 0.07, y, w * 1.14, cap_h, cap, cap_h * 0.5)
    svg.rect(x - w * 0.07, y + h - cap_h, w * 1.14, cap_h, cap, cap_h * 0.5)
    size = int(w * 0.46)
    svg.text(cx, cy + size * 0.06, label, size, cap)
    svg.text(cx, cy - size * 0.05, label, size, "#FFFFFF")


def add_dots(svg, x, y, angle, speed, count):
    a = math.radians(angle)
    for i in range(1, count + 1):
        t = i * 0.075
        svg.circle(x + math.cos(a) * speed * t, y - math.sin(a) * speed * t + 240 * t * t, 5.5 - i * 0.3, "#FFFFFF")


# ---------- HUD ----------

def tower_icon(svg, cx, cy, s, color):
    """Своя иконка: башня с зубцами и флажком."""
    svg.rect(cx - s * 0.42, cy - s * 0.12, s * 0.84, s * 0.72, color, s * 0.06)
    for i in (-1, 0, 1):
        svg.rect(cx + i * s * 0.32 - s * 0.11, cy - s * 0.32, s * 0.22, s * 0.3, color, s * 0.04)
    svg.rect(cx - s * 0.03, cy - s * 0.74, s * 0.06, s * 0.46, color)
    svg.poly([(cx + s * 0.03, cy - s * 0.74), (cx + s * 0.42, cy - s * 0.6), (cx + s * 0.03, cy - s * 0.44)], color)
    svg.path(f"M{cx - s * 0.16:.1f} {cy + s * 0.6:.1f} L{cx - s * 0.16:.1f} {cy + s * 0.24:.1f} Q{cx:.1f} {cy + s * 0.04:.1f} {cx + s * 0.16:.1f} {cy + s * 0.24:.1f} L{cx + s * 0.16:.1f} {cy + s * 0.6:.1f} Z", var("hudPanel"))


def crossed_rockets(svg, cx, cy, s):
    """Своя иконка: две скрещённые ракеты вместо стрел."""
    for angle in (45, 135):
        svg.open(f"translate({cx:.1f} {cy:.1f}) rotate({angle})")
        svg.rect(-s * 0.72, -s * 0.09, s * 1.3, s * 0.18, var("hudGold"), s * 0.09)
        svg.path(f"M{s * 0.5:.1f} {-s * 0.19:.1f} Q{s * 0.86:.1f} {-s * 0.14:.1f} {s * 1.0:.1f} 0 Q{s * 0.86:.1f} {s * 0.14:.1f} {s * 0.5:.1f} {s * 0.19:.1f} Z", var("hudGold"))
        svg.poly([(-s * 0.68, -s * 0.09), (-s * 0.98, -s * 0.28), (-s * 0.5, -s * 0.09)], var("hudGoldShade"))
        svg.poly([(-s * 0.68, s * 0.09), (-s * 0.98, s * 0.28), (-s * 0.5, s * 0.09)], var("hudGoldShade"))
        svg.close()


def health_bar(svg, x, y, w, h, total, alive, from_left):
    gap = 5.0
    segment = (w - gap * (total - 1)) / total
    for i in range(total):
        sx = x + i * (segment + gap)
        index = i if from_left else total - 1 - i
        svg.rect(sx, y, segment, h, var("hudTrack"), min(h, segment) * 0.42)
        if index < alive:
            svg.rect(sx, y, segment, h, var("hudHealth"), min(h, segment) * 0.42)
            svg.rect(sx, y + h * 0.56, segment, h * 0.44, var("hudHealthShade"), min(h, segment) * 0.3)


def hud(svg, cx, y, w, blue_alive, red_alive):
    h = 72
    left = cx - w * 0.5
    svg.rect(left + 16, y + 4, w - 32, h - 8, var("hudPanel"), (h - 8) * 0.5, 0.94)
    bar_w = w * 0.5 - 120
    health_bar(svg, left + 66, y + 22, bar_w, 28, 3, blue_alive, True)
    health_bar(svg, cx + 54, y + 22, bar_w, 28, 12, red_alive, False)
    crossed_rockets(svg, cx, y + h * 0.5, 26)
    for px, color in ((left + 38, "roofPlayer"), (left + w - 38, "rocketEnemy")):
        svg.circle(px, y + h * 0.5, 40, var("hudPanel"))
        svg.circle(px, y + h * 0.5, 33, var("hudTrack"))
        tower_icon(svg, px, y + h * 0.5 + 3, 30, var(color))


def aim_plate(svg, x, y):
    svg.rect(x, y, 200, 78, "#FFFFFF", 39)
    svg.text(x + 58, y + 32, "68%", 30, var("hudPanel"))
    svg.text(x + 142, y + 32, "50", 30, var("hudPanel"))
    svg.circle(x + 164, y + 21, 4.5, var("hudPanel"))
    svg.circle(x + 164, y + 21, 2, "#FFFFFF")
    svg.text(x + 58, y + 60, "POWER", 13, var("hudPanel"))
    svg.text(x + 142, y + 60, "ANGLE", 13, var("hudPanel"))


def turn_banner(svg, cx, y, label):
    w = 26 * len(label) + 40
    svg.rect(cx - w * 0.5, y, w, 58, var("hudPanel"), 29, 0.94)
    svg.text(cx, y + 29, label, 32, "#FFFFFF")


def play_button(svg, cx, y, w=300, h=84):
    svg.rect(cx - w * 0.5, y + 10, w, h, var("hudGoldShade"), h * 0.5)
    svg.rect(cx - w * 0.5, y, w, h, var("hudGold"), h * 0.5)
    svg.text(cx, y + h * 0.5, "PLAY NOW", 42, var("hudTrack"))


def hand(svg, x, y, s):
    """Рука-подсказка: своя плоская белая перчатка с тенью."""
    for dx, dy, tone in ((5, 7, "hudTrack"), (0, 0, "hudText")):
        fill = var(tone)
        svg.open(f"translate({x + dx * s:.1f} {y + dy * s:.1f}) rotate(-18) scale({s:.2f})")
        svg.rect(-11, -62, 22, 74, fill, 11)
        svg.rect(10, -20, 20, 46, fill, 10)
        svg.rect(28, -14, 18, 40, fill, 9)
        svg.rect(-28, -4, 18, 34, fill, 9)
        svg.rect(-24, 2, 66, 58, fill, 18)
        svg.close()
    svg.open(f"translate({x:.1f} {y:.1f}) rotate(-18) scale({s:.2f})")
    svg.rect(-20, 62, 58, 15, var("hudPanel"), 6)
    svg.close()


def logo(svg, cx, y, size):
    svg.open(f"translate({cx:.1f} {y:.1f}) rotate(-3)")
    for line, offset in (("CASTLE", 0), ("DESTROYERS", size * 1.02)):
        svg.text(0, offset + size * 0.07, line, size, var("hudTrack"))
        svg.text(0, offset, line, size, "#FFFFFF")
    svg.close()


# ---------- Макеты ----------

SCENE_W = 1215
SCENE_H = 682


def scene(theme):
    svg = Svg()
    night = theme == "Night"
    svg.rect(0, 0, SCENE_W, SCENE_H, var("sky"))
    if night:
        add_sky_dots(svg, SCENE_W, 330, 3)
        add_moon(svg, 830, 190, 62)
    else:
        svg.circle(840, 180, 92, var("moon"))
        svg.circle(840, 180, 70, var("moonShade"))
        add_pill_clouds(svg, ((60, 150, 170), (420, 130, 220), (1030, 130, 130)))
    add_mountain(svg, 240, 540, 280, 300)
    add_mountain(svg, 900, 540, 320, 340)
    add_mountain(svg, 590, 540, 220, 220)
    if night:
        add_forest(svg, SCENE_W, 450, 9)
    else:
        add_city(svg, SCENE_W, 460, 9)
    add_bare_tree(svg, 830, 500, 300, 4)
    add_bare_tree(svg, 345, 500, 240, 8)
    add_near_bushes(svg, SCENE_W, 470, 12)
    outline = contour(lambda x: scene_height(x, SCENE_W) * SCENE_H, SCENE_W)
    gap = (598, 640)
    left = [p for p in outline if p[0] <= gap[0]]
    right = [p for p in outline if p[0] >= gap[1]]

    ground_at = lambda x: scene_height(x, SCENE_W) * SCENE_H
    add_spruce(svg, 335, ground_at(335) + 8, 90)
    add_spruce(svg, 395, ground_at(395) + 8, 70)
    add_spruce(svg, 862, ground_at(862) + 8, 84)
    add_bush(svg, 300, ground_at(300) + 8, 26)
    add_bush(svg, 910, ground_at(910) + 8, 22)

    draw_ground = add_slime_ground if night else add_scallop_ground
    bottom = 628
    draw_ground(svg, left, bottom, 41, 0.8)
    draw_ground(svg, right, bottom, 77, 0.8)
    add_water(svg, SCENE_W, 612, 70)

    on_platform(svg, blue_castle, 150, ground_at(150) - 1, 15.5, -1)
    cannon(svg, 182, ground_at(150) - 74, 38, "Player", 15.5)
    cannon(svg, 112, ground_at(150) - 74, 38, "Player", 15.5)
    cannon(svg, 183, ground_at(150) - 128, 38, "Player", 15.5)
    on_platform(svg, red_castle, 1060, ground_at(1060) - 1, 15.5, 1)
    for mx, my in ((1048, 70), (1076, 112), (1046, 156), (1072, 200), (1010, 58), (1112, 62)):
        cannon(svg, mx, ground_at(1060) - my, 10, "Enemy", 15.5, True)

    booster(svg, 619, 150, 46, 92, "x20", var("booster20"), var("booster20Cap"))
    booster(svg, 619, 330, 46, 92, "x10", var("booster10"), var("booster10Cap"))
    booster(svg, 619, 520, 46, 92, "x50", var("booster50"), var("booster50Cap"))

    arc = [(i, 200 + i * 66, 330 - i * 42 + i * i * 3.4) for i in range(1, 9)]
    for i, x, y in arc:
        smoke(svg, x - 26, y + 10, 8 + i)
        smoke(svg, x - 42, y + 15, 6 + i * 0.7)
    for i, x, y in arc:
        rocket(svg, x, y, -30 + i * 7.5, "Player", 0.8)
    blast(svg, 1000, 330, 34, 2)
    blast(svg, 1130, 340, 22, 7)
    aim_plate(svg, 120, 158)
    add_dots(svg, 214, 300, 50, 420, 7)
    hud(svg, SCENE_W * 0.5, 16, 760, 2, 9)
    return svg.document(SCENE_W, SCENE_H)


def strip(theme):
    """Лента образцов крупно: каждый объект без окружения, на цвете неба темы."""
    width = 2360
    height = 700
    svg = Svg()
    svg.rect(0, 0, width, height, var("sky"))

    outline = [(0.0, 470.0)] + [(float(x), 470.0 + 26.0 * math.sin(x * 0.012)) for x in range(1, 760)]
    if theme == "Night":
        add_slime_ground(svg, outline, 700, 11, 1.3)
    else:
        add_scallop_ground(svg, outline, 700, 11, 1.3)
    add_spruce(svg, 120, 458, 130)
    add_bush(svg, 300, 452, 36)
    add_bone(svg, 560, 620, 22, 24)

    on_platform(svg, blue_castle, 960, 660, 44.0)
    cannon(svg, 1062, 660 - 44 * 5.5, 34, "Player", 44.0)
    cannon(svg, 860, 660 - 44 * 5.5, 34, "Player", 44.0)
    cannon(svg, 1000, 660 - 44 * 3.6, 34, "Player", 44.0)
    on_platform(svg, red_castle, 1360, 660, 34.0)
    cannon(svg, 1380, 660 - 34 * 8.2, 10, "Enemy", 34.0, True)
    cannon(svg, 1450, 660 - 34 * 3.0, 10, "Enemy", 34.0, True)
    cannon(svg, 1290, 660 - 34 * 3.0, 10, "Enemy", 34.0, True)

    for i, (label, color) in enumerate((("x10", "booster10"), ("x20", "booster20"), ("x50", "booster50"))):
        booster(svg, 1640 + i * 130, 160, 96, 196, label, var(color), var(color + "Cap"))
    rocket(svg, 1640, 340, -20, "Player", 2.0)
    rocket(svg, 1830, 340, 200, "Enemy", 2.0)
    for x, r in ((1590, 22), (1625, 30), (1668, 20)):
        svg.circle(x, 440, r, var("smoke"), 0.75)
    blast(svg, 1830, 520, 110, 5)
    blast(svg, 1640, 590, 60, 8)
    hand(svg, 2120, 560, 1.5)
    aim_plate(svg, 2020, 40)
    turn_banner(svg, 2120, 150, "YOUR TURN!")
    svg.text(2120, 248, "+600", 56, "#FFFFFF")
    play_button(svg, 2120, 300)
    return svg.document(width, height)


def portrait(theme):
    """Кадр 720×1280 как в игре: крупный красный замок и HUD (сравнение с кадром 4 ролика)."""
    svg = Svg()
    w, h = 720, 1280
    night = theme == "Night"
    svg.rect(0, 0, w, h, var("sky"))
    if night:
        add_sky_dots(svg, w, 560, 6)
        add_moon(svg, 520, 330, 88)
    else:
        svg.circle(520, 330, 110, var("moon"))
        svg.circle(520, 330, 84, var("moonShade"))
    add_mountain(svg, 150, 800, 300, 330)
    add_mountain(svg, 560, 800, 300, 430)
    if night:
        add_forest(svg, w, 730, 3)
    else:
        add_city(svg, w, 760, 3)
    add_near_bushes(svg, w, 800, 5)
    outline = contour(lambda x: 980.0 - 50.0 * math.exp(-((x - 360.0) / 300.0) ** 2) + 12.0 * math.sin(x * 0.012), w)
    add_spruce(svg, 70, 950, 110)
    add_bush(svg, 660, 950, 30)
    (add_slime_ground if night else add_scallop_ground)(svg, outline, 1160, 17, 1.0)
    add_water(svg, w, 1140, 140)
    on_platform(svg, red_castle, 360, 932, 33.0)
    for mx, my in ((380, 140), (440, 210), (300, 215), (380, 320), (380, 430), (250, 110)):
        cannon(svg, mx, 932 - my, 10, "Enemy", 33.0, True)
    hud(svg, w * 0.5, 130, 660, 3, 12)
    return svg.document(w, h)


def endcard(theme):
    svg = Svg()
    w, h = 720, 1280
    svg.rect(0, 0, w, h, var("hudTrack"))
    svg.circle(360, 700, 300, var("hudPanel"))
    logo(svg, 360, 270, 88)
    on_platform(svg, blue_castle, 360, 900, 40.0)
    play_button(svg, 360, 1020, 420, 110)
    hand(svg, 540, 1130, 1.2)
    return svg.document(w, h)


# ---------- Сводный лист ----------

def sheet_font(size, bold=False):
    name = "seguisb.ttf" if bold else "segoeui.ttf"
    return ImageFont.truetype(f"C:/Windows/Fonts/{name}", size)


def fit(image, width):
    height = int(image.height * width / image.width)
    return image.convert("RGB").resize((width, height), Image.Resampling.LANCZOS)


def hex_text(color):
    return "#%02X%02X%02X" % color


def luminance(color):
    return 0.3 * color[0] + 0.59 * color[1] + 0.11 * color[2]


def swatches(draw, x, y, palette, keys, cell):
    """Плашки цветов с именем поля и HEX."""
    w, h = cell
    for i, key in enumerate(keys):
        cx = x + (i % 11) * (w + 6)
        cy = y + (i // 11) * (h + 6)
        color = palette[key]
        draw.rounded_rectangle((cx, cy, cx + w, cy + h), 10, fill=color)
        ink = (20, 20, 40) if luminance(color) > 140 else (255, 255, 255)
        draw.text((cx + 10, cy + h - 44), key, font=sheet_font(16), fill=ink)
        draw.text((cx + 10, cy + h - 24), hex_text(color), font=sheet_font(17, True), fill=ink)


def build_sheet(palettes, renders):
    width = 2480
    margin = 40
    paper = (247, 245, 250)
    ink = (42, 30, 66)
    half = (width - margin * 3) // 2
    night_ref = fit(Image.open(REFERENCES / "bowmasters_graveyard.jpg"), half)
    city_ref = fit(Image.open(REFERENCES / "bowmasters_city.jpg"), half // 2 - 8)
    desert_ref = fit(Image.open(REFERENCES / "bowmasters_desert.jpg"), half)
    night_scene = fit(renders["scene", "Night"], half)
    sunset_scene = fit(renders["scene", "Sunset"], half)
    night_strip = fit(renders["strip", "Night"], width - margin * 2)
    sunset_strip = fit(renders["strip", "Sunset"], width - margin * 2)
    tall = 700
    portraits = [fit(renders[key], int(tall * 720 / 1280)) for key in (("portrait", "Night"), ("portrait", "Sunset"), ("endcard", "Night"))]

    keys = [
        "sky", "backgroundFar", "backgroundFarShade", "backgroundMid", "backgroundNear", "backgroundNearLight", "moon", "moonShade",
        "ground", "groundShade", "groundBuried", "groundRim", "groundRimLight", "groundCrater", "bush", "treeLight", "treeShade",
        "water", "waterHighlight", "waterRim", "roofPlayer", "roofEnemy"]
    shared = [
        "castleStone", "castleStoneShade", "castleBrick", "castleCrack", "castleWindow", "castleWindowLit", "cannonPlayer", "cannonEnemy",
        "platform", "platformShade", "track",
        "booster10", "booster20", "booster50", "rocketBody", "rocketPlayer", "rocketEnemy", "rocketTail", "blastMid", "blastEdge",
        "hudPanel", "hudTrack", "hudHealth", "hudHealthShade", "hudGold", "hudGoldShade"]
    cell = (214, 96)
    rows = lambda items: (len(items) + 10) // 11

    height = 110 + night_ref.height + 60 + desert_ref.height + 80
    height += night_strip.height + sunset_strip.height + 110
    height += tall + 110
    height += 3 * 40 + (rows(keys) * 2 + rows(shared)) * (cell[1] + 6) + 60

    sheet = Image.new("RGB", (width, height), paper)
    draw = ImageDraw.Draw(sheet)
    draw.text((margin, 28), "CASTLE DESTROYERS — style sheet", font=sheet_font(46, True), fill=ink)
    y = 100

    def caption(cx, cy, text):
        draw.text((cx, cy), text, font=sheet_font(24, True), fill=ink)

    caption(margin, y, "Референс: Bowmasters, кладбище")
    caption(margin * 2 + half, y, "Наш пресет Night")
    y += 40
    sheet.paste(night_ref, (margin, y))
    sheet.paste(night_scene, (margin * 2 + half, y))
    y += night_ref.height + 20
    caption(margin, y, "Референс: Bowmasters, пустыня")
    caption(margin * 2 + half, y, "Наш пресет Sunset")
    y += 40
    sheet.paste(desert_ref, (margin, y))
    sheet.paste(sunset_scene, (margin * 2 + half, y))
    y += desert_ref.height + 30

    caption(margin, y, "Образцы крупно, Night: земля с «капающей» кромкой, замки, бустеры, ракеты, взрыв, HUD")
    y += 40
    sheet.paste(night_strip, (margin, y))
    y += night_strip.height + 16
    caption(margin, y, "Образцы крупно, Sunset: земля с фестонной кромкой")
    y += 40
    sheet.paste(sunset_strip, (margin, y))
    y += sunset_strip.height + 30

    caption(margin, y, "Кадры 720×1280: Night, Sunset, EndCard. Шрифт на всех образцах — Rubik (OFL)")
    y += 40
    x = margin
    for image in portraits:
        sheet.paste(image, (x, y))
        x += image.width + 24
    sheet.paste(city_ref, (x + 20, y))
    draw.text((x + 20, y + city_ref.height + 14), "Референс: город, окна и дома", font=sheet_font(22), fill=ink)
    y += tall + 30

    caption(margin, y, "Палитра Night (пипетка: кладбище, город)")
    y += 40
    swatches(draw, margin, y, palettes["Night"], keys, cell)
    y += rows(keys) * (cell[1] + 6) + 10
    caption(margin, y, "Палитра Sunset (пипетка: пустыня)")
    y += 40
    swatches(draw, margin, y, palettes["Sunset"], keys, cell)
    y += rows(keys) * (cell[1] + 6) + 10
    caption(margin, y, "Общие цвета: замки, бустеры, ракеты, HUD")
    y += 40
    swatches(draw, margin, y, palettes["Night"], shared, cell)
    y += rows(shared) * (cell[1] + 6)
    sheet.crop((0, 0, width, y + 30)).save(OUTPUT, optimize=True)
    print(f"{OUTPUT.relative_to(ROOT)}: {width}x{y + 30}, {OUTPUT.stat().st_size} bytes")


def main():
    palettes = flat_svg.load_palettes()
    STYLE_DIR.mkdir(parents=True, exist_ok=True)
    renders = {}
    builders = {"scene": scene, "strip": strip, "portrait": portrait, "endcard": endcard}
    for name, builder in builders.items():
        for theme in ("Night", "Sunset"):
            if name == "endcard" and theme == "Sunset":
                continue
            document = builder(theme)
            (STYLE_DIR / f"Style_{name}_{theme}.svg").write_text(document, encoding="utf-8")
            renders[name, theme] = flat_svg.render(document, palettes[theme])
    build_sheet(palettes, renders)


if __name__ == "__main__":
    main()
