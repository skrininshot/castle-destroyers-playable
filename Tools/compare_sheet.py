#!/usr/bin/env python3
"""Лист для отчёта этапа 7: кадры игры рядом со скриншотами Bowmasters и кадрами ролика.

Использование: python Tools/compare_sheet.py (после съёмки кадров GameCapture).
"""
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
IMG = ROOT / "docs" / "reports" / "img"
STYLE = ROOT / "docs" / "references" / "style"
OUTPUT = IMG / "stage07_compare.png"
WIDTH = 2000
MARGIN = 30
GAP = 24
CAPTION = 40
PAPER = (247, 245, 250)
INK = (42, 30, 66)

ROWS = (
    (("Bowmasters: кладбище", STYLE / "bowmasters_graveyard.jpg"), ("Игра: Night, ландшафт", IMG / "stage07_04_center_landscape.png")),
    (("Bowmasters: пустыня", STYLE / "bowmasters_desert.jpg"), ("Игра: EndCard, ландшафт", IMG / "stage07_16_endcard_landscape.png")),
)
PORTRAITS = (
    ("Ход 1", "stage07_02_hand_turn1.png"), ("Бустер x50", "stage07_11_x50_avalanche.png"), ("Кратеры", "stage07_12_after_turn2.png"),
    ("Победа", "stage07_14_victory.png"), ("EndCard", "stage07_15_endcard.png"), ("Sunset", "stage07_19_sunset.png"),
)


def fit_width(image, width):
    return image.convert("RGB").resize((width, int(image.height * width / image.width)), Image.Resampling.LANCZOS)


def main():
    font = ImageFont.truetype("C:/Windows/Fonts/seguisb.ttf", 26)
    half = (WIDTH - MARGIN * 2 - GAP) // 2
    rows = [[(text, fit_width(Image.open(path), half)) for text, path in row] for row in ROWS]
    count = len(PORTRAITS)
    tall_width = (WIDTH - MARGIN * 2 - GAP * (count - 1)) // count
    portraits = [(text, fit_width(Image.open(IMG / name), tall_width)) for text, name in PORTRAITS]

    height = MARGIN + sum(CAPTION + max(item[1].height for item in row) + GAP for row in rows) + CAPTION + portraits[0][1].height + MARGIN
    sheet = Image.new("RGB", (WIDTH, height), PAPER)
    draw = ImageDraw.Draw(sheet)
    y = MARGIN
    for row in rows:
        x = MARGIN
        for text, image in row:
            draw.text((x, y), text, font=font, fill=INK)
            sheet.paste(image, (x, y + CAPTION))
            x += half + GAP
        y += CAPTION + max(item[1].height for item in row) + GAP
    x = MARGIN
    for text, image in portraits:
        draw.text((x, y), text, font=font, fill=INK)
        sheet.paste(image, (x, y + CAPTION))
        x += tall_width + GAP
    sheet.save(OUTPUT, optimize=True)
    print(f"{OUTPUT.relative_to(ROOT)}: {WIDTH}x{height}, {OUTPUT.stat().st_size // 1024} KB")


if __name__ == "__main__":
    main()
