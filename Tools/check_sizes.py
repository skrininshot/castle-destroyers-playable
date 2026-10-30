#!/usr/bin/env python3
"""Проверка размеров: ассеты в Assets/_Game и итоговый HTML Luna-сборки.

Использование: python Tools/check_sizes.py [путь к html или папке сборки]
Код возврата 1 — бюджет превышен.
"""
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
GAME_ASSETS = ROOT / "Assets" / "_Game"
KB = 1024
MB = 1024 * 1024

TEXTURE_EXT = {".png", ".jpg", ".jpeg", ".tga"}
AUDIO_EXT = {".wav", ".mp3", ".ogg"}
TEXTURE_BUDGET = int(1.5 * MB)
AUDIO_BUDGET = 150 * KB
HTML_BUDGET = int(4.5 * MB)
LITE_BUDGET = int(2.8 * MB)
EMPTY_BUDGET = 2 * MB
BASE64_FACTOR = 4 / 3


def total_size(extensions):
    files = [p for p in GAME_ASSETS.rglob("*") if p.is_file() and p.suffix.lower() in extensions]
    return sum(p.stat().st_size for p in files)


def report(name, size, budget):
    ok = size <= budget
    print(f"{'OK  ' if ok else 'OVER'} {name}: {size / KB:.1f} KB / {budget / KB:.0f} KB")
    return ok


def find_html(argument):
    if argument:
        path = Path(argument)
        if path.is_file():
            return path
        candidates = list(path.rglob("*.html"))
    else:
        candidates = []
        for folder in (ROOT / "LunaTemp", ROOT / "Builds"):
            if folder.exists():
                candidates.extend(folder.rglob("*.html"))
    return max(candidates, key=lambda p: p.stat().st_size) if candidates else None


def main():
    ok = True
    texture_size = total_size(TEXTURE_EXT)
    audio_size = total_size(AUDIO_EXT)
    ok &= report("textures", texture_size, TEXTURE_BUDGET)
    ok &= report("audio", audio_size, AUDIO_BUDGET)
    print(f"     textures+audio after base64: {(texture_size + audio_size) * BASE64_FACTOR / KB:.1f} KB")

    html = find_html(sys.argv[1] if len(sys.argv) > 1 else None)
    if html is None:
        print("-    HTML not found: run a Luna build first")
    else:
        print(f"     html: {html}")
        size = html.stat().st_size
        ok &= report("html (main budget)", size, HTML_BUDGET)
        print(f"{'OK  ' if size <= LITE_BUDGET else 'INFO'} lite budget: {LITE_BUDGET / KB:.0f} KB")
        print(f"{'OK  ' if size <= EMPTY_BUDGET else 'INFO'} empty-build budget: {EMPTY_BUDGET / KB:.0f} KB")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
