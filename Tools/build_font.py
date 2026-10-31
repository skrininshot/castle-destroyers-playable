#!/usr/bin/env python3
"""Шрифт UI: статичный Rubik ExtraBold (OFL, D-055) с подмножеством глифов (ТЗ §3.12).

Использование: python Tools/build_font.py
Источник — Rubik, установленный в системе. Результат лежит в Assets/_Game/Art/Ui, заметка о лицензии в Art/Source/Rubik-License.txt.
"""
from pathlib import Path

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

import flat_svg

ROOT = Path(__file__).resolve().parent.parent
OUTPUT = ROOT / "Assets" / "_Game" / "Art" / "Ui" / "Rubik-Game.ttf"
GLYPHS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 +!%°.,-"


def main():
    font = TTFont(flat_svg.HEAVY_FONT)
    static = instancer.instantiateVariableFont(font, {"wght": flat_svg.HEAVY_WEIGHT})
    options = subset.Options()
    options.layout_features = []
    options.name_IDs = ["*"]
    options.notdef_outline = True
    subsetter = subset.Subsetter(options)
    subsetter.populate(text=GLYPHS)
    subsetter.subset(static)
    static.save(OUTPUT)
    print(f"{OUTPUT.relative_to(ROOT)}: {OUTPUT.stat().st_size} bytes")


if __name__ == "__main__":
    main()
