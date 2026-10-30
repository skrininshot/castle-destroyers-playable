#!/usr/bin/env python3
"""Полная Luna-сборка из командной строки. Unity должна быть закрыта, вход в Playworks Plugin выполнен.

Использование: python Tools/luna_build.py [develop|playground]

Jake пишет задание Assets/.export-assets и ждёт экспорта, но завершение batchmode-экспорта не замечает.
Поэтому экспорт запускается через project:build, а остальные стадии — отдельными задачами.
"""
import os
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PLUGIN = ROOT.parent / "Unity Playworks Plugin" / "7.2.0"
NODE = PLUGIN / "tools" / "node" / "win64" / "node.exe"
UNITY = Path(r"C:\Program Files\Unity\Hub\Editor\6000.0.85f1\Editor\Unity.exe")
EXPORT_INSTRUCTION = ROOT / "Assets" / ".export-assets"
STAGES = ("assets:bundle", "code:bundle", "package:bundle")
INSTRUCTION_TIMEOUT = 60
UNITY_TIMEOUT = 900
STAGE_TIMEOUT = 300


def jake_command(*arguments):
    return [str(NODE), "--max-old-space-size=8192", "pipeline/jake.js", "-f", "pipeline/Jakefile.js", *arguments]


def wait_for_instruction():
    deadline = time.time() + INSTRUCTION_TIMEOUT
    while not EXPORT_INSTRUCTION.exists():
        if time.time() > deadline:
            raise RuntimeError("jake did not write Assets/.export-assets")
        time.sleep(1)


def main():
    target = sys.argv[1] if len(sys.argv) > 1 else "develop"
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
    return 0


if __name__ == "__main__":
    sys.exit(main())
