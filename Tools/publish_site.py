#!/usr/bin/env python3
"""Публикует браузерную сборку в site/ для Cloudflare Pages. Запускать после python Tools/luna_build.py develop.

Использование: python Tools/publish_site.py

iframe.html становится index.html, первым в <head> идёт наш скрипт:
- до старта игры не даёт Luna заметить скрытую вкладку, иначе она не запускается;
- открывает магазин по CTA. В сборке develop InstallFullGame только сообщает родительскому окну, а на сайте родителя нет (D-076).
"""
import re
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
BUILD = ROOT / "LunaTemp" / "stage4" / "develop"
SITE = ROOT / "site"
CONFIG = ROOT / "Assets" / "_Game" / "Configs" / "GameConfig.asset"
HEAD = "<head>"
GUARD = (
    '<script>(function(){var s=!1,u="%s";'
    'window.addEventListener("luna:start",function(){s=!0});'
    'document.addEventListener("visibilitychange",function(e){s||e.stopImmediatePropagation()});'
    'window.addEventListener("message",function(e){var d=e.data;'
    'if(window.parent===window&&d&&d.target==="editor:pi"&&d.eventName==="cta"&&!window.open(u,"_blank"))location.href=u})'
    '})()</script>'
)


def store_url():
    return re.search(r"^  storeUrl: (.+?)\s*$", CONFIG.read_text(encoding="utf8"), re.MULTILINE).group(1)


def main():
    # Сама папка остаётся: на Windows её может держать локальный http-сервер.
    for item in SITE.iterdir():
        if item.is_dir():
            shutil.rmtree(item)
        else:
            item.unlink()
    shutil.copytree(BUILD, SITE, dirs_exist_ok=True)
    (SITE / "index.html").unlink()
    page = (SITE / "iframe.html").read_text(encoding="utf8")
    (SITE / "iframe.html").unlink()
    if HEAD not in page:
        raise RuntimeError("no <head> in iframe.html")
    (SITE / "index.html").write_text(page.replace(HEAD, HEAD + GUARD % store_url(), 1), encoding="utf8", newline="")


if __name__ == "__main__":
    main()
