# Castle Destroyers — playable ad

Рекламный плеебл: дуэль замков в духе Worms, 3 хода игрока, ракеты размножаются в бустерах. Unity 6000.0 + URP, экспорт через Luna (Playworks).

Что делаем — `docs/TZ.md`, принятые решения — `docs/DECISIONS.md`, отчёты по этапам — `docs/reports/`, правила разработки — `CLAUDE.md`.

## Что нужно
- Unity **6000.0.85f1** с модулем WebGL. Другие версии не ставить: Luna их не поддерживает.
- Playworks Plugin 7.2.0 в папке `Unity Playworks Plugin/7.2.0` рядом с папкой проекта (D-008), вход в плагин выполнен.
- Python 3 с Pillow, numpy и fontTools: только для пересборки арта, звука и шрифта.

Перед запуском команд ниже закройте Unity: batchmode и открытый редактор не работают вместе.

## Запуск
Открыть `Assets/_Game/Scenes/Game.unity`, Game view 720×1280, Play. В редакторе есть debug-панель: кнопка `DEV` в левом верхнем углу.

## Тесты
```bash
"C:\Program Files\Unity\Hub\Editor\6000.0.85f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Logs/tests.xml -logFile Logs/editor.log
```
После запуска посмотрите в `Logs/editor.log` строки `error CS` и исключения.

## Сборки
Все варианты собирает один скрипт. Он меняет файлы проекта только на время экспорта и возвращает исходные байты.

| Команда | Результат |
|---|---|
| `python Tools/luna_build.py develop` | браузерная сборка с debug-панелью, `LunaTemp/stage4/develop` |
| `python Tools/luna_build.py develop --release` | то же без debug-кода, как в рекламе |
| `python Tools/luna_build.py playground` | архив для Playground, `LunaTemp/stage4/main.zip` |
| `python Tools/luna_build.py playground --lite` | урезанный архив, `LunaTemp/stage4/lite.zip` |
| `python Tools/check_sizes.py` | проверка бюджетов размера |

Проверить браузерную сборку: раздать папку `LunaTemp/stage4/develop` любым http-сервером и открыть `iframe.html`.

## Публикация
1. Загрузить `main.zip` в Luna Playground.
2. Задать настоящую ссылку Google Play. Только там: из `luna.json` в сборку ссылка не попадает (D-070).
3. Скачать HTML под нужные сети. Локально собрать их нельзя (D-013).
4. Проверить в AppLovin Playable Preview, Unity Ads Ad Testing и Google HTML5 Validator.

## Настройка
Все числа лежат в `Assets/_Game/Configs/GameConfig.asset`, тултипы в инспекторе на русском. Часть полей дублируется в Playground, секция `Balance`. Палитры — `ThemePalette.asset`, рабочая тема — `Night`.

Подсказки ходов — массив `TurnHint[]` в конфиге. Подбирать их лучше по настоящим матчам: `-executeMethod CastleDestroyers.EditorTools.BalanceScan.RunTurnTwoMatches`.

## Арт, звук, шрифт
Арт рисуется кодом и не правится руками: источники в `Art/Source`, PNG генерируются.
```bash
python Tools/rasterize_svg.py
```
```bash
python Tools/build_art.py
```
После пересборки арта или правок сцены: меню `Tools → Castle Destroyers → Build Game Scene`. Звук — `python Tools/build_sounds.py`, шрифт — `python Tools/build_font.py`. Лицензия шрифта Rubik (OFL) — `Art/Source/Rubik-License.txt`.

## Debug-код
Весь debug-код стоит за символом `GAME_DEBUG`. Символ задан только для Standalone, то есть для редактора. Luna берёт символы из `luna.json`, там он пуст: включает его `luna_build.py develop` на время сборки. В релизе скрипт также вырезает `DebugRoot` из сцены (D-070).
