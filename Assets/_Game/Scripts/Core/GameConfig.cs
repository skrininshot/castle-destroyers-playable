using CastleDestroyers.Destruction;
using UnityEngine;

namespace CastleDestroyers.Core
{
    /// <summary>
    /// Все настраиваемые числа игры (ТЗ). Часть полей пока не используется: их читают
    /// системы следующих этапов, а значения по умолчанию взяты из ТЗ.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Castle Destroyers/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Сессия (ТЗ §3.1)")]
        [Tooltip("Палитра с пресетами Night / Sunset.")]
        public ThemePalette theme;
        [Tooltip("Seed генератора случайных чисел. Одинаковый seed даёт одинаковую сессию.")]
        public int rngSeed = 12345;
        [Range(1, 5)]
        [Tooltip("Число ходов игрока до принудительного EndCard.")]
        public int maxPlayerTurns = 3;
        [Range(20f, 180f)]
        [Tooltip("Максимальная длительность сессии, секунды. По достижении показывается EndCard.")]
        public float maxSessionSeconds = 75f;
        [Range(0.01f, 0.1f)]
        [Tooltip("Ограничение шага времени кадра, секунды. Защищает от скачков после паузы.")]
        public float maxDeltaTime = 0.05f;
        [Tooltip("Director: игрок гарантированно побеждает к последнему разрешённому ходу, ИИ не добивает игрока.")]
        public bool directorEnabled = true;
        [Tooltip("Выбор бонуса из двух карточек после первого хода (по умолчанию выключен).")]
        public bool bonusPickEnabled;
        [Tooltip("Подсказки к ходам игрока по порядку. Если ходов больше, чем подсказок, используется последняя.")]
        public TurnHint[] turnHints =
        {
            new TurnHint("x20 lob", 20, true, 56f, 0.65f),
            new TurnHint("x50 straight shot", 50, true, 20f, 0.8f),
            new TurnHint("free shot", 0, false, 40f, 0.85f)
        };

        [Header("Тайминги состояний")]
        [Range(0f, 2f)]
        [Tooltip("Boot: пауза перед интро, секунды.")]
        public float bootDuration = 0.2f;
        [Range(0.2f, 5f)]
        [Tooltip("Intro: камера на замке игрока, секунды.")]
        public float introDuration = 1.5f;
        [Range(0.2f, 30f)]
        [Tooltip("Aim игрока: простой до автовыстрела по подсказке хода, секунды (ТЗ §3.2). Жест останавливает отсчёт.")]
        public float playerAimDuration = 10f;
        [Range(0.2f, 5f)]
        [Tooltip("Aim ИИ: «раздумье» с поворотом пушек, секунды.")]
        public float aiAimDuration = 1.5f;
        [Range(0.1f, 3f)]
        [Tooltip("Fire: пауза на замке во время залпа, секунды (ТЗ §3.9: ~0.3 с до переезда).")]
        public float fireDuration = 0.6f;
        [Range(0.1f, 10f)]
        [Tooltip("Flight: минимальная длительность, секунды. Дальше состояние держится, пока ракеты в воздухе.")]
        public float flightDuration = 1f;
        [Range(2f, 30f)]
        [Tooltip("Flight: предел ожидания ракет, секунды. Страховка от зависшего хода.")]
        public float flightMaxDuration = 12f;
        [Range(0.1f, 3f)]
        [Tooltip("Resolve: задержка на разрушениях, секунды (ТЗ §3.9: ~0.7 с).")]
        public float resolveDuration = 0.7f;
        [Range(0.5f, 6f)]
        [Tooltip("Victory: от победы до EndCard, секунды (ТЗ §3.11: ~2 с).")]
        public float victoryDuration = 2.5f;
        [Range(0f, 3f)]
        [Tooltip("Victory/Defeat: сколько камера держит кадр на проигравшем замке, секунды.")]
        public float victoryLoserHold = 1f;
        [Range(0.5f, 6f)]
        [Tooltip("Defeat: от поражения до EndCard, секунды.")]
        public float defeatDuration = 2f;

        [Header("Заглушки боя (потери до этапа 5, выстрел ИИ до этапа 6)")]
        [Range(0, 14)]
        [Tooltip("Сколько пушек ИИ теряет за залп игрока.")]
        public int stubEnemyLossPerPlayerTurn = 3;
        [Range(0, 14)]
        [Tooltip("Сколько пушек игрок теряет за залп ИИ.")]
        public int stubPlayerLossPerAiTurn = 1;
        [Range(0f, 90f)]
        [Tooltip("Угол выстрела ИИ, градусы.")]
        public float stubAiAngleDeg = 62f;
        [Range(0f, 1f)]
        [Tooltip("Сила выстрела ИИ, доля от максимума.")]
        public float stubAiPower = 0.7f;

        [Header("Прицел (ТЗ §3.2)")]
        [Range(0f, 90f)]
        public float aimMinDeg = 5f;
        [Range(0f, 90f)]
        public float aimMaxDeg = 85f;
        [Range(0.1f, 1f)]
        [Tooltip("Длина жеста для полной силы, доля высоты экрана.")]
        public float aimMaxDragScreenFraction = 0.35f;
        [Range(0f, 0.2f)]
        [Tooltip("Мёртвая зона жеста, доля высоты экрана. Короткий тап не стреляет.")]
        public float aimDeadZone = 0.03f;
        [Range(30f, 720f)]
        [Tooltip("Скорость поворота пушек, градусов в секунду.")]
        public float aimTurnSpeed = 240f;
        [Range(0.1f, 1f)]
        [Tooltip("Какая доля траектории показывается пунктиром.")]
        public float aimPreviewFraction = 0.3f;
        [Range(2, 32)]
        [Tooltip("Точек в пунктире.")]
        public int aimDotCount = 12;
        [Range(0.05f, 1f)]
        [Tooltip("Диаметр первой точки пунктира, метры.")]
        public float aimDotSizeStart = 0.36f;
        [Range(0.02f, 1f)]
        [Tooltip("Диаметр последней точки пунктира, метры.")]
        public float aimDotSizeEnd = 0.14f;
        [Range(0, 255)]
        [Tooltip("Непрозрачность последней точки пунктира, 0..255.")]
        public int aimDotAlphaEnd = 90;
        [Tooltip("Плашка POWER / ANGLE: смещение от центра замка игрока, метры.")]
        public Vector2 aimPanelOffset = new Vector2(5f, 6.5f);
        [Tooltip("Режим «направление на палец» вместо «оттянуть»: сила постоянная (D-017).")]
        public bool aimPointAtFinger;
        [Range(0f, 1f)]
        [Tooltip("Сила в режиме «направление на палец», доля от максимума.")]
        public float aimPointPower = 0.6f;
        [Range(1f, 60f)]
        [Tooltip("Скорость ракеты при минимальной силе, м/с.")]
        public float launchSpeedMin = 14f;
        [Range(1f, 60f)]
        [Tooltip("Скорость ракеты при полной силе, м/с.")]
        public float launchSpeedMax = 42f;
        [Range(1f, 40f)]
        [Tooltip("Ускорение свободного падения, м/с².")]
        public float gravity = 14f;

        [Header("Пушки и ракеты (ТЗ §3.3, §3.4)")]
        [Range(1, 14)]
        [Tooltip("Пушек у игрока (синий замок).")]
        public int playerCannons = 3;
        [Range(1, 14)]
        [Tooltip("Пушек у ИИ (красный замок).")]
        public int enemyCannons = 12;
        [Range(1, 20)]
        [Tooltip("Ракет, которые выпускает каждая пушка за залп.")]
        public int rocketsPerCannon = 10;
        [Range(0.01f, 0.2f)]
        [Tooltip("Интервал между ракетами одной пушки, секунды.")]
        public float rocketInterval = 0.06f;
        [Range(0.1f, 2f)]
        [Tooltip("Хит-круг пушки, метры.")]
        public float cannonHitRadius = 0.5f;
        [Range(100, 1000)]
        [Tooltip("Жёсткий бюджет одновременно летящих ракет.")]
        public int maxActiveRockets = 600;
        [Range(100, 2000)]
        [Tooltip("Размер буфера ракет для стресс-теста.")]
        public int stressRocketCount = 1000;
        [Range(0.2f, 2f)]
        [Tooltip("Длина квада ракеты, метры.")]
        public float rocketLength = 1.1f;
        [Range(0.05f, 1f)]
        [Tooltip("Ширина квада ракеты, метры.")]
        public float rocketWidth = 0.34f;
        [Range(0.05f, 1f)]
        [Tooltip("Шаг проверки столкновений вдоль пути за кадр, метры. Меньше — точнее и дороже.")]
        public float rocketCollisionStep = 0.25f;
        [Range(0.2f, 4f)]
        [Tooltip("Ширина всплеска от ракеты, метры.")]
        public float rocketSplashWidth = 0.8f;
        [Range(0f, 1f)]
        [Tooltip("Откат ствола при выстреле, метры.")]
        public float recoilDistance = 0.3f;
        [Range(0.5f, 10f)]
        [Tooltip("Скорость возврата ствола, м/с.")]
        public float recoilReturnSpeed = 3f;
        [Range(0.1f, 2f)]
        [Tooltip("Размер вспышки у дула, метры.")]
        public float muzzleFlashSize = 0.8f;
        [Range(0.02f, 0.5f)]
        [Tooltip("Длительность вспышки у дула, секунды.")]
        public float muzzleFlashLife = 0.1f;
        [Range(5f, 85f)]
        [Tooltip("Угол выстрела в стресс-режиме, градусы. Все ракеты летят по одной дуге в гору.")]
        public float stressRocketAngleDeg = 20f;
        [Range(1f, 60f)]
        [Tooltip("Скорость ракеты в стресс-режиме, м/с.")]
        public float stressRocketSpeed = 36f;
        [Range(1, 100)]
        [Tooltip("Сколько ракет стресс-режим выпускает за кадр, пока не заполнен буфер.")]
        public int stressSpawnPerFrame = 40;

        [Header("Бустеры (ТЗ §3.5)")]
        [Tooltip("Бустеры уровня: множитель и положение.")]
        public BoosterSpec[] boosters =
        {
            new BoosterSpec(20, 0f, 33.6f),
            new BoosterSpec(10, 0f, 19f),
            new BoosterSpec(50, 0f, 13f)
        };
        [Range(0.5f, 3f)]
        public float boosterWidth = 1f;
        [Range(1f, 4f)]
        public float boosterHeight = 2f;
        [Range(0f, 45f)]
        [Tooltip("Разброс угла копий ракет после бустера, градусы.")]
        public float boosterSpreadDeg = 18f;
        [Range(0f, 0.2f)]
        [Tooltip("Разнос рядов копий по времени, секунды. В ряду по одной копии на поток.")]
        public float boosterSpawnStagger = 0.04f;
        [Range(1, 9)]
        [Tooltip("Потоков в веере после бустера. Нечётное число даёт поток по центру.")]
        public int boosterStreams = 5;
        [Range(0f, 1f)]
        [Tooltip("Насколько точка выхода копий стянута к оси бустера. 1 — все потоки расходятся из одной точки.")]
        public float boosterExitFocus = 1f;
        [Range(1f, 4f)]
        [Tooltip("Предел роста кратера у ракеты с весом, доля радиуса. Лишние копии сверх бюджета превращаются в вес.")]
        public float boosterMaxCraterScale = 2f;
        [Range(0f, 0.1f)]
        [Tooltip("Амплитуда пульса бустера, доля масштаба.")]
        public float boosterPulse = 0.03f;
        [Range(0.5f, 10f)]
        [Tooltip("Скорость пульса бустера, рад/с.")]
        public float boosterPulseSpeed = 3f;
        [Range(0f, 0.5f)]
        [Tooltip("Сквош при срабатывании: насколько корпус шире и ниже, доля размера.")]
        public float boosterSquash = 0.14f;
        [Range(1f, 20f)]
        [Tooltip("Скорость затухания сквоша, 1/с.")]
        public float boosterSquashDecay = 8f;
        [Range(0, 32)]
        [Tooltip("Плоских пуфов в одном «пыхе».")]
        public int boosterPuffCount = 12;
        [Range(0.02f, 1f)]
        [Tooltip("Минимальный интервал между «пыхами» одного бустера, секунды.")]
        public float boosterPuffInterval = 0.12f;
        [Range(0.1f, 2f)]
        [Tooltip("Размер пуфа, метры.")]
        public float boosterPuffSize = 0.7f;
        [Range(0f, 12f)]
        [Tooltip("Скорость разлёта пуфов, м/с.")]
        public float boosterPuffSpeed = 5f;
        [Range(0.1f, 2f)]
        [Tooltip("Время жизни пуфа, секунды.")]
        public float boosterPuffLife = 0.4f;
        [Range(0.2f, 2f)]
        [Tooltip("Высота цифр поп-апа «+N», метры.")]
        public float boosterPopupHeight = 0.8f;
        [Range(0f, 3f)]
        [Tooltip("Отступ поп-апа от верха бустера, метры.")]
        public float boosterPopupOffsetY = 0.4f;
        [Range(0.1f, 3f)]
        [Tooltip("Сколько поп-ап держится после последней ракеты, секунды.")]
        public float boosterPopupHold = 0.7f;
        [Range(0.05f, 1f)]
        [Tooltip("За сколько поп-ап тает, секунды.")]
        public float boosterPopupFade = 0.25f;
        [Range(0f, 6f)]
        [Tooltip("Скорость всплытия поп-апа, м/с.")]
        public float boosterPopupRise = 1.2f;

        [Header("Разрушаемость (ТЗ §3.6, §5.3)")]
        [Tooltip("Запечённая маска ландшафта. Создаётся из Level1_TerrainMask.png.")]
        public TerrainMaskData terrainMask;
        [Tooltip("Мировая точка нижнего левого текселя маски, метры.")]
        public Vector2 terrainMaskOrigin = new Vector2(-51.59375f, -3.5f);
        [Range(4, 32)]
        [Tooltip("Разрешение масок, тексели на метр.")]
        public int maskTexelsPerMeter = 16;
        [Tooltip("Размер тайла маски, текселей.")]
        public int maskTileSize = 128;
        [Range(1, 8)]
        [Tooltip("Лимит аплоадов тайлов масок за кадр.")]
        public int maxTileUploadsPerFrame = 4;
        [Range(0.2f, 4f)]
        [Tooltip("Радиус кратера в земле, метры.")]
        public float craterRadiusGround = 1.5f;
        [Range(0.05f, 1f)]
        [Tooltip("Ширина каймы кратера, метры.")]
        public float rimWidth = 0.3f;
        [Tooltip("Фестонная кромка у кратеров земли.")]
        public bool rimScallop;
        [Range(4, 40)]
        [Tooltip("Зубцов фестона на полный круг кратера.")]
        public int rimScallopTeeth = 14;
        [Range(0.02f, 0.5f)]
        [Tooltip("Глубина фестона, метры.")]
        public float rimScallopDepth = 0.12f;
        [Range(0.02f, 0.5f)]
        [Tooltip("Сглаживание внешнего края земли, доля значения маски. Меньше — резче.")]
        public float maskEdgeSoftness = 0.15f;
        [Range(0.02f, 0.5f)]
        [Tooltip("Минимальный интервал между проверками связности, секунды.")]
        public float connectivityInterval = 0.1f;
        [Range(1, 16)]
        [Tooltip("Размер ячейки сетки связности, текселей.")]
        public int connectivityCellTexels = 4;
        [Range(1, 64)]
        [Tooltip("Островки меньше этого числа ячеек исчезают с пылью, а не падают обломком.")]
        public int minChunkCells = 6;
        [Range(1, 64)]
        [Tooltip("Сколько островов обрабатывается за одну проверку связности. Остаток — на следующей.")]
        public int maxIslandsPerSolve = 16;
        [Range(1, 16)]
        [Tooltip("Обломков одновременно. Сверх лимита остров исчезает с пылью.")]
        public int chunkSlots = 6;
        [Tooltip("Размер текстуры обломка, степень двойки. Большой остров снимается с пониженным разрешением.")]
        public int chunkTextureSize = 256;
        [Range(1f, 40f)]
        [Tooltip("Ускорение падения обломков и декора, м/с².")]
        public float chunkGravity = 14f;
        [Range(0f, 6f)]
        [Tooltip("Максимальная скорость обломка вбок, м/с.")]
        public float chunkMaxDrift = 1.2f;
        [Range(0f, 180f)]
        [Tooltip("Максимальная скорость вращения обломка, градусов в секунду.")]
        public float chunkMaxSpinDeg = 35f;
        [Range(0f, 720f)]
        [Tooltip("Максимальная скорость вращения падающего дерева, градусов в секунду.")]
        public float decorMaxSpinDeg = 220f;
        [Range(0f, 1f)]
        [Tooltip("На какой глубине под основанием дерева проверяется опора, метры.")]
        public float decorAnchorDepth = 0.15f;

        [Header("Вода и частицы (ТЗ §3.8, §4.1)")]
        [Range(0f, 0.5f)]
        [Tooltip("Амплитуда волны на кромке воды, метры.")]
        public float waterWaveAmplitude = 0.09f;
        [Range(0.1f, 4f)]
        [Tooltip("Пространственная частота волны, рад/м.")]
        public float waterWaveFrequency = 0.9f;
        [Range(0f, 8f)]
        [Tooltip("Скорость бега волны, рад/с.")]
        public float waterWaveSpeed = 1.6f;
        [Range(0.1f, 2f)]
        [Tooltip("Шаг вершин волны, метры.")]
        public float waterWaveStep = 0.5f;
        [Range(0.05f, 1f)]
        [Tooltip("Толщина светлой кромки воды, метры.")]
        public float waterRimThickness = 0.22f;
        [Range(64, 2048)]
        [Tooltip("Лимит плоских частиц (пыль, всплески).")]
        public int fxParticleCapacity = 512;
        [Range(0, 32)]
        [Tooltip("Частиц пыли на один кратер.")]
        public int craterDustPuffs = 6;
        [Range(1, 64)]
        [Tooltip("Максимум частиц пыли на исчезнувший островок.")]
        public int dustMaxPuffs = 16;
        [Range(0.1f, 2f)]
        [Tooltip("Размер частицы пыли, метры.")]
        public float dustSize = 0.6f;
        [Range(0f, 8f)]
        [Tooltip("Скорость разлёта пыли, м/с.")]
        public float dustSpeed = 2.2f;
        [Range(0.1f, 2f)]
        [Tooltip("Время жизни пыли, секунды.")]
        public float dustLife = 0.6f;
        [Range(0f, 30f)]
        [Tooltip("Гравитация для комьев земли, м/с².")]
        public float dustGravity = 12f;
        [Range(1, 64)]
        [Tooltip("Базовое число капель всплеска.")]
        public int splashDrops = 8;
        [Range(0f, 8f)]
        [Tooltip("Дополнительных капель на метр ширины падающего предмета.")]
        public float splashDropsPerMeter = 3f;
        [Range(0.05f, 1f)]
        [Tooltip("Размер капли, метры.")]
        public float splashDropSize = 0.34f;
        [Range(1f, 20f)]
        [Tooltip("Начальная скорость капли вверх, м/с.")]
        public float splashSpeed = 7f;
        [Range(0f, 6f)]
        [Tooltip("Разлёт капель вбок, м/с.")]
        public float splashSpread = 1.6f;
        [Range(0.1f, 2f)]
        [Tooltip("Время жизни капли, секунды.")]
        public float splashLife = 0.8f;
        [Range(0f, 40f)]
        [Tooltip("Гравитация капель, м/с².")]
        public float splashGravity = 18f;

        [Header("Взрывы, дым, замки (ТЗ §3.7, §5.4)")]
        [Range(16, 256)]
        public int explosionPoolSize = 128;
        [Range(0.1f, 2f)]
        [Tooltip("Взрывы ближе этого радиуса в одном кадре сливаются визуально, метры.")]
        public float explosionMergeRadius = 0.5f;
        [Range(0f, 1f)]
        [Tooltip("На сколько растёт визуальный взрыв за каждый слитый с ним, доля размера.")]
        public float explosionMergeGrowth = 0.12f;
        [Range(1f, 3f)]
        [Tooltip("Предел роста слитого взрыва, доля размера.")]
        public float explosionMaxStrength = 1.6f;
        [Range(0.2f, 4f)]
        [Tooltip("Радиус внешнего круга взрыва, метры.")]
        public float explosionRadius = 1.5f;
        [Range(0.05f, 1f)]
        [Tooltip("Длительность взрыва, секунды.")]
        public float explosionLife = 0.32f;
        [Range(0, 16)]
        [Tooltip("Плоских осколков на взрыв.")]
        public int explosionShards = 4;
        [Range(0f, 20f)]
        [Tooltip("Скорость осколков, м/с.")]
        public float explosionShardSpeed = 8f;
        [Range(0.05f, 1f)]
        [Tooltip("Размер осколка, метры.")]
        public float explosionShardSize = 0.34f;
        [Range(0.1f, 2f)]
        [Tooltip("Время жизни осколка, секунды.")]
        public float explosionShardLife = 0.5f;
        [Range(0f, 2f)]
        [Tooltip("Предельная амплитуда тряски камеры, метры.")]
        public float shakeMax = 0.6f;
        [Range(1f, 10f)]
        [Tooltip("Скорость затухания тряски, 1/с.")]
        public float shakeDecay = 4f;
        [Range(0f, 0.5f)]
        [Tooltip("Импульс тряски от одного визуального взрыва, метры.")]
        public float shakePerExplosion = 0.08f;
        [Range(5f, 90f)]
        [Tooltip("Частота тряски, рад/с.")]
        public float shakeFrequency = 45f;
        [Range(100, 4000)]
        public int smokeCapacity = 2000;
        [Range(50, 600)]
        [Tooltip("Выше этого числа ракет дым пускает только каждая k-я ракета.")]
        public int smokeLodRocketThreshold = 200;
        [Range(0f, 1f)]
        [Tooltip("Насколько толще шлейф за каждую ступень LOD, доля размера.")]
        public float smokeLodGrowth = 0.15f;
        [Range(0.01f, 0.3f)]
        [Tooltip("Интервал между пуфами одной ракеты, секунды.")]
        public float smokeInterval = 0.05f;
        [Range(0.1f, 2f)]
        [Tooltip("Время жизни пуфа, секунды.")]
        public float smokeLife = 0.5f;
        [Range(0.05f, 1f)]
        [Tooltip("Диаметр нового пуфа, метры.")]
        public float smokeSizeStart = 0.24f;
        [Range(0.05f, 2f)]
        [Tooltip("Диаметр пуфа в конце жизни, метры.")]
        public float smokeSizeEnd = 0.6f;
        [Range(0f, 1f)]
        [Tooltip("Какую долю жизни пуф остаётся цветом пламени.")]
        public float smokeFlameFraction = 0.25f;
        [Range(0, 255)]
        [Tooltip("Начальная непрозрачность пуфа, 0..255.")]
        public int smokeAlpha = 230;
        [Range(0f, 6f)]
        [Tooltip("Максимальный наклон от урона, градусы.")]
        public float swayMaxDeg = 3f;

        [Header("Уровень (ТЗ §3.8): greybox")]
        [Tooltip("Ширина уровня, метры.")]
        public float levelWidth = 80f;
        [Tooltip("Уровень воды по Y, метры.")]
        public float waterLevel;
        [Tooltip("Нижний край ландшафта и воды по Y, метры.")]
        public float levelBottom = -40f;
        [Tooltip("X центра синего замка, метры.")]
        public float playerCastleX = -33.75f;
        [Tooltip("X центра красного замка, метры.")]
        public float enemyCastleX = 33.75f;
        [Range(0.1f, 1.5f)]
        [Tooltip("Высота гусениц, метры.")]
        public float trackHeight = 1.2f;
        [Range(0.1f, 1.5f)]
        [Tooltip("Высота деревянной платформы, метры.")]
        public float platformHeight = 0.8f;

        [Header("Фон (ТЗ §3.10)")]
        [Tooltip("Параллакс слоёв от дальнего к ближнему: звёзды, луна, горы, лес, кусты. 0 — слой стоит на экране, 1 — движется с уровнем.")]
        public float[] parallaxFactors = { 0f, 0.05f, 0.15f, 0.3f, 0.5f };

        [Header("Камера (ТЗ §3.9)")]
        [Range(0.1f, 2f)]
        [Tooltip("Время сглаживания SmoothDamp, секунды.")]
        public float cameraSmoothTime = 0.45f;
        [Range(8f, 40f)]
        [Tooltip("Ширина кадра на своём замке, метры.")]
        public float castleFrameWidth = 14f;
        [Range(10f, 60f)]
        [Tooltip("Ширина кадра в центре уровня (гора и бустеры), метры. Самый широкий кадр.")]
        public float centerFrameWidth = 22f;
        [Range(20f, 80f)]
        [Tooltip("Минимальная высота центрального кадра, метры: гора и x20 должны помещаться и в ландшафте.")]
        public float centerFrameMinHeight = 40f;
        [Range(8f, 50f)]
        [Tooltip("Ширина кадра на замке-цели, метры. Чуть уже центрального.")]
        public float targetFrameWidth = 16f;
        [Range(6f, 40f)]
        [Tooltip("Минимальная высота кадра, метры. Нужна в ландшафтной ориентации.")]
        public float minFrameHeight = 22f;
        [Range(0f, 8f)]
        [Tooltip("Сдвиг камеры к противнику, чтобы свой замок стоял чуть левее центра, метры.")]
        public float castleLeadX = 2f;
        [Range(-2f, 8f)]
        [Tooltip("Подъём точки фокуса над центром замка, метры (видно небо над ним).")]
        public float castleFocusLiftY = 0.5f;
        [Tooltip("Точка фокуса в центре уровня, метры.")]
        public Vector2 centerFocus = new Vector2(0f, 17f);
        [Range(0f, 1f)]
        [Tooltip("Насколько центральный кадр смещается к голове потока, доля расстояния.")]
        public float cameraHeadFollow = 0.25f;

        [Header("Прочее")]
        [Tooltip("Дождь из ролика (по умолчанию выключен).")]
        public bool rainEnabled;
        [Tooltip("Декоративный луч прожектора.")]
        public bool lightBeamEnabled;
        [Tooltip("Постоянная мини-кнопка CTA во время игры.")]
        public bool showPersistentCta;

        [Header("Debug")]
        [Range(0.05f, 1f)]
        [Tooltip("Масштаб времени при включённом замедлении в debug-панели.")]
        public float debugSlowScale = 0.25f;
        [Range(0.05f, 1f)]
        [Tooltip("Период обновления текста debug-оверлея, секунды.")]
        public float debugHudRefresh = 0.25f;
        [Tooltip("Радиусы debug-взрыва, метры. Кнопка BLAST перебирает их по кругу.")]
        public float[] debugBlastRadii = { 1.2f, 2.4f, 4f, 0.6f };
        [Range(0.02f, 0.5f)]
        [Tooltip("Период повторения взрыва при удержании пальца, секунды.")]
        public float debugBlastHoldInterval = 0.08f;
        [Range(0f, 0.5f)]
        [Tooltip("Нижняя доля экрана, где тапы не взрывают: там кнопки debug-панели.")]
        public float debugBlastUiBand = 0.26f;
        [Range(20f, 160f)]
        [Tooltip("Ширина кадра ALL на debug-камере, метры.")]
        public float debugOverviewWidth = 106f;
        [Tooltip("Сценарии кнопок SHOT: угол и сила выстрела игрока (навес x20, тоннель x10, низ x50).")]
        public TurnHint[] debugBoosterShots =
        {
            new TurnHint("shot x20", 20, false, 56f, 0.65f),
            new TurnHint("shot x10", 10, false, 36f, 0.55f),
            new TurnHint("shot x50", 50, false, 20f, 0.8f)
        };
        [Tooltip("Залпов подряд у кнопок SHOT: до x10 и x50 поток добирается только вторым залпом.")]
        public int[] debugBoosterShotSalvos = { 1, 2, 2 };

        /// <summary>Подсказка для хода игрока (индекс с нуля); за пределами массива — последняя.</summary>
        public TurnHint GetHint(int turnIndex)
        {
            if (turnHints == null || turnHints.Length == 0)
            {
                return null;
            }
            int index = turnIndex < 0 ? 0 : (turnIndex >= turnHints.Length ? turnHints.Length - 1 : turnIndex);
            return turnHints[index];
        }
    }
}
