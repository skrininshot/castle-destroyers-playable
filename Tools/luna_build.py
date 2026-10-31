#!/usr/bin/env python3
"""Полная Luna-сборка из командной строки. Unity должна быть закрыта, вход в Playworks Plugin выполнен.

Использование: python Tools/luna_build.py [develop|playground] [--release] [--all-themes] [--lite]

develop      сборка для браузера, LunaTemp/stage4/develop
playground   архив для Playground, LunaTemp/stage4/create-hub/_build.zip; всегда идёт как --release

--release     без GAME_DEBUG: в сборке нет debug-панели и читов
--all-themes  оставить обе темы, нужно только кнопке THEME
--lite        текстуры и звук из LITE_RULES (ТЗ §2.2)

Jake пишет задание Assets/.export-assets и ждёт экспорта, но завершение batchmode-экспорта не замечает.
Поэтому экспорт запускается через project:build, а остальные стадии — отдельными задачами.

Варианты сборки накладываются на файлы проекта только на время экспорта, исходные байты возвращаются в конце.
"""
import json
import os
import re
import shutil
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PLUGIN = ROOT.parent / "Unity Playworks Plugin" / "7.2.0"
NODE = PLUGIN / "tools" / "node" / "win64" / "node.exe"
UNITY = Path(r"C:\Program Files\Unity\Hub\Editor\6000.0.85f1\Editor\Unity.exe")
EXPORT_INSTRUCTION = ROOT / "Assets" / ".export-assets"
LUNA_CONFIG = ROOT / "luna.json"
THEME_ASSET = ROOT / "Assets" / "_Game" / "Configs" / "ThemePalette.asset"
FLAT_MATERIAL = ROOT / "Assets" / "_Game" / "Art" / "Mat_FlatColor.mat"
OUTPUT = ROOT / "LunaTemp" / "stage4"
STAGES = ("assets:bundle", "code:bundle", "package:bundle")
INSTRUCTION_TIMEOUT = 60
UNITY_TIMEOUT = 900
STAGE_TIMEOUT = 300

ART_BLOCK = rb"^  %sArt:\r?\n((?:    .*\r?\n)+)"
ART_GUID = rb"guid: ([0-9a-f]{32})"
ART_REFERENCE = rb"\{fileID: \d+, guid: [0-9a-f]{32}, type: \d+\}"
EMPTY_REFERENCE = b"{fileID: 0}"
DEBUG_DEFINE = b'"GAME_DEBUG"'
DEFINES_PATTERN = rb'"scriptingDefines": \[[^\]]*\]'
EMPTY_DEFINES = b'"scriptingDefines": []'

# Правила lite-сборки: сторона текстуры, качество pngquant, битрейт звука (ТЗ §2.2, D-063).
LITE_RULES = {"maxSide": 512, "quality": 45, "bitrate": 64}


def jake_command(*arguments):
    return [str(NODE), "--max-old-space-size=8192", "pipeline/jake.js", "-f", "pipeline/Jakefile.js", *arguments]


def wait_for_instruction():
    deadline = time.time() + INSTRUCTION_TIMEOUT
    while not EXPORT_INSTRUCTION.exists():
        if time.time() > deadline:
            raise RuntimeError("jake did not write Assets/.export-assets")
        time.sleep(1)


def art_block(data, theme):
    match = re.search(ART_BLOCK % theme, data, re.MULTILINE)
    if match is None:
        raise RuntimeError("no %sArt in ThemePalette.asset" % theme.decode())
    return match


def strip_inactive_theme(originals):
    """Убирает из палитры ссылки на неактивную тему и её атлас из материала: материал меняется в Play Mode."""
    palette = THEME_ASSET.read_bytes()
    preset = re.search(rb"^  preset: (\d)", palette, re.MULTILINE).group(1)
    active, inactive = (b"night", b"sunset") if preset == b"0" else (b"sunset", b"night")

    active_guid = re.search(ART_GUID, art_block(palette, active).group(1)).group(1)
    block = art_block(palette, inactive)
    inactive_guid = re.search(ART_GUID, block.group(1)).group(1)
    emptied = re.sub(ART_REFERENCE, EMPTY_REFERENCE, block.group(1))

    originals[THEME_ASSET] = palette
    THEME_ASSET.write_bytes(palette[:block.start(1)] + emptied + palette[block.end(1):])

    material = FLAT_MATERIAL.read_bytes()
    originals[FLAT_MATERIAL] = material
    FLAT_MATERIAL.write_bytes(material.replace(inactive_guid, active_guid))


def apply_release_defines(originals):
    data = originals.setdefault(LUNA_CONFIG, LUNA_CONFIG.read_bytes())
    LUNA_CONFIG.write_bytes(re.sub(DEFINES_PATTERN, EMPTY_DEFINES, data))


def apply_lite_rules(originals):
    originals.setdefault(LUNA_CONFIG, LUNA_CONFIG.read_bytes())
    config = json.loads(LUNA_CONFIG.read_bytes().decode("utf8"))
    rules = config["assets"]["rules"]
    rules["texture"]["default"]["maxWidth"] = LITE_RULES["maxSide"]
    rules["texture"]["default"]["maxHeight"] = LITE_RULES["maxSide"]
    rules["texture"]["default"]["quality"] = LITE_RULES["quality"]
    rules["sound"]["default"]["bitrate"] = LITE_RULES["bitrate"]
    LUNA_CONFIG.write_text(json.dumps(config, indent=4, ensure_ascii=False), encoding="utf8")


def apply_variant(release, all_themes, lite):
    originals = {}
    if not all_themes:
        strip_inactive_theme(originals)
    if release:
        apply_release_defines(originals)
    if lite:
        apply_lite_rules(originals)
    return originals


def restore(originals):
    for path, data in originals.items():
        path.write_bytes(data)


def build(target):
    environment = dict(os.environ, PROJECT_PATH=ROOT.as_posix())
    EXPORT_INSTRUCTION.unlink(missing_ok=True)

    with open(ROOT / "Logs" / "jake.log", "w", encoding="utf8") as log:
        exporter = subprocess.Popen(jake_command("project:build"), cwd=PLUGIN, env=environment, stdout=log, stderr=log)
    try:
        wait_for_instruction()
        subprocess.run(
            [str(UNITY), "-batchmode", "-nographics", "-projectPath", str(ROOT),
             "-executeMethod", "LunaCLI.Export", "-logFile", "Logs/luna_export.log"],
            cwd=ROOT, check=True, timeout=UNITY_TIMEOUT)
    finally:
        exporter.kill()
        EXPORT_INSTRUCTION.unlink(missing_ok=True)

    for stage in STAGES:
        arguments = [stage]
        if stage == "package:bundle" and target != "develop":
            arguments.append("TARGET_PLATFORM=" + target)
        subprocess.run(jake_command(*arguments), cwd=PLUGIN, env=environment, check=True, timeout=STAGE_TIMEOUT)


def main():
    arguments = sys.argv[1:]
    flags = {a for a in arguments if a.startswith("--")}
    targets = [a for a in arguments if not a.startswith("--")]
    target = targets[0] if targets else "develop"
    release = "--release" in flags or target != "develop"
    lite = "--lite" in flags

    originals = apply_variant(release, "--all-themes" in flags, lite)
    try:
        build(target)
    finally:
        restore(originals)
    if target != "develop":
        archive = OUTPUT / "create-hub" / "_build.zip"
        shutil.copy2(archive, OUTPUT / ("lite.zip" if lite else "main.zip"))
    return 0


if __name__ == "__main__":
    sys.exit(main())
