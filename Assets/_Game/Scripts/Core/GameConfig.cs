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
            new TurnHint("x20 lob", 20, true, 55f, 0.75f),
            new TurnHint("x50 straight shot", 50, true, 20f, 0.9f),
            new TurnHint("free shot", 0, false, 40f, 0.85f)
        };

        [Header("Тайминги состояний (заглушки до этапов 3 и 6)")]
        [Range(0f, 2f)]
        [Tooltip("Boot: пауза перед интро, секунды.")]
        public float bootDuration = 0.2f;
        [Range(0.2f, 5f)]
        [Tooltip("Intro: камера на замке игрока, секунды.")]
        public float introDuration = 1.5f;
        [Range(0.2f, 10f)]
        [Tooltip("Aim игрока: ожидание прицеливания до автоперехода, секунды. На этапе 3 уступает место жесту.")]
        public float playerAimDuration = 2.5f;
        [Range(0.2f, 5f)]
        [Tooltip("Aim ИИ: «раздумье» с поворотом пушек, секунды.")]
        public float aiAimDuration = 1.5f;
        [Range(0.1f, 3f)]
        [Tooltip("Fire: пауза на замке во время залпа, секунды (ТЗ §3.9: ~0.3 с до переезда).")]
        public float fireDuration = 0.6f;
        [Range(0.5f, 10f)]
        [Tooltip("Flight: полёт ракет, секунды. На этапе 3 заканчивается, когда ракет не осталось.")]
        public float flightDuration = 3f;
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

        [Header("Заглушка боя (до этапа 3)")]
        [Range(0, 14)]
        [Tooltip("Сколько пушек ИИ теряет за залп игрока.")]
        public int stubEnemyLossPerPlayerTurn = 4;
        [Range(0, 14)]
        [Tooltip("Сколько пушек игрок теряет за залп ИИ.")]
        public int stubPlayerLossPerAiTurn = 1;

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
        [Range(1f, 60f)]
        [Tooltip("Скорость ракеты при минимальной силе, м/с.")]
        public float launchSpeedMin = 12f;
        [Range(1f, 60f)]
        [Tooltip("Скорость ракеты при полной силе, м/с.")]
        public float launchSpeedMax = 32f;
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
        [Range(0f, 1f)]
        [Tooltip("Амплитуда синусоиды «змейки», метры.")]
        public float waveAmplitude = 0.35f;
        [Range(1f, 20f)]
        [Tooltip("Частота синусоиды, рад/с.")]
        public float waveFrequency = 9f;
        [Range(0f, 3f)]
        [Tooltip("Сдвиг фазы между соседними ракетами, рад.")]
        public float wavePhaseStep = 0.5f;
        [Range(0.2f, 2f)]
        [Tooltip("Длина квада ракеты, метры.")]
        public float rocketLength = 0.8f;
        [Range(0.05f, 1f)]
        [Tooltip("Ширина квада ракеты, метры.")]
        public float rocketWidth = 0.25f;
        [Range(1f, 10f)]
        [Tooltip("Время жизни ракеты в стресс-тесте, секунды.")]
        public float stressRocketLifetime = 4f;

        [Header("Бустеры (ТЗ §3.5)")]
        [Tooltip("Бустеры уровня: множитель и положение.")]
        public BoosterSpec[] boosters =
        {
            new BoosterSpec(20, 0f, 13.4f),
            new BoosterSpec(10, 0f, 8.5f),
            new BoosterSpec(50, 0f, 1.6f)
        };
        [Range(0.5f, 3f)]
        public float boosterWidth = 1f;
        [Range(1f, 4f)]
        public float boosterHeight = 2f;
        [Range(0f, 45f)]
        [Tooltip("Разброс угла копий ракет после бустера, градусы.")]
        public float boosterSpreadDeg = 12f;
        [Range(0f, 0.2f)]
        [Tooltip("Разнос копий ракет по времени, секунды.")]
        public float boosterSpawnStagger = 0.04f;
        [Range(0f, 0.1f)]
        [Tooltip("Амплитуда пульса бустера, доля масштаба.")]
        public float boosterPulse = 0.03f;

        [Header("Разрушаемость (ТЗ §3.6, §5.3)")]
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
        public float craterRadiusGround = 1.2f;
        [Range(0.05f, 1f)]
        [Tooltip("Ширина каймы кратера, метры.")]
        public float rimWidth = 0.3f;
        [Tooltip("Фестонная кромка у кратеров земли.")]
        public bool rimScallop;
        [Range(0.02f, 0.5f)]
        [Tooltip("Минимальный интервал между проверками связности, секунды.")]
        public float connectivityInterval = 0.1f;
        [Range(1, 16)]
        [Tooltip("Размер ячейки сетки связности, текселей.")]
        public int connectivityCellTexels = 4;
        [Range(1, 64)]
        [Tooltip("Островки меньше этого числа ячеек исчезают с пылью, а не падают обломком.")]
        public int minChunkCells = 6;

        [Header("Взрывы, дым, замки (ТЗ §3.7, §5.4)")]
        [Range(16, 256)]
        public int explosionPoolSize = 128;
        [Range(0.1f, 2f)]
        [Tooltip("Взрывы ближе этого радиуса в одном кадре сливаются визуально, метры.")]
        public float explosionMergeRadius = 0.5f;
        [Range(0f, 2f)]
        [Tooltip("Предельная амплитуда тряски камеры, метры.")]
        public float shakeMax = 0.6f;
        [Range(1f, 10f)]
        [Tooltip("Скорость затухания тряски, 1/с.")]
        public float shakeDecay = 4f;
        [Range(100, 4000)]
        public int smokeCapacity = 2000;
        [Range(50, 600)]
        [Tooltip("Выше этого числа ракет дым пускает только каждая k-я ракета.")]
        public int smokeLodRocketThreshold = 200;
        [Range(0f, 6f)]
        [Tooltip("Максимальный наклон от урона, градусы.")]
        public float swayMaxDeg = 3f;

        [Header("Уровень (ТЗ §3.8): greybox")]
        [Tooltip("Ширина уровня, метры.")]
        public float levelWidth = 60f;
        [Tooltip("Уровень воды по Y, метры.")]
        public float waterLevel;
        [Tooltip("Нижний край ландшафта и воды по Y, метры.")]
        public float levelBottom = -40f;
        [Tooltip("Контур ландшафта слева направо: x от центра уровня, y над водой. Вертикальные участки задаются двумя точками с одним x.")]
        public Vector2[] terrainProfile =
        {
            new Vector2(-34f, 1.6f), new Vector2(-26f, 1.8f), new Vector2(-18f, 1.8f),
            new Vector2(-14f, 0.2f), new Vector2(-11f, 0.2f),
            new Vector2(-9.5f, 1.2f), new Vector2(-8f, 3.4f), new Vector2(-6.2f, 6.6f),
            new Vector2(-4.6f, 9.6f), new Vector2(-3.2f, 11.4f), new Vector2(-1.4f, 11.8f),
            new Vector2(-0.7f, 11.6f), new Vector2(-0.7f, -3f),
            new Vector2(0.7f, -3f), new Vector2(0.7f, 11.6f),
            new Vector2(1.4f, 11.8f), new Vector2(3.2f, 11.4f), new Vector2(4.6f, 9.6f),
            new Vector2(6.2f, 6.6f), new Vector2(8f, 3.4f), new Vector2(9.5f, 1.2f),
            new Vector2(11f, 0.2f), new Vector2(14f, 0.2f),
            new Vector2(18f, 1.8f), new Vector2(26f, 1.8f), new Vector2(34f, 1.6f)
        };
        [Range(0.1f, 1f)]
        [Tooltip("Толщина цветной кромки по верху земли, метры.")]
        public float groundRimThickness = 0.4f;
        [Tooltip("X центра синего замка, метры.")]
        public float playerCastleX = -22f;
        [Tooltip("X центра красного замка, метры.")]
        public float enemyCastleX = 23f;
        [Range(0.1f, 1.5f)]
        [Tooltip("Высота гусениц, метры.")]
        public float trackHeight = 0.6f;
        [Range(0.1f, 1.5f)]
        [Tooltip("Высота деревянной платформы, метры.")]
        public float platformHeight = 0.5f;

        [Header("Камера (ТЗ §3.9)")]
        [Range(0.1f, 2f)]
        [Tooltip("Время сглаживания SmoothDamp, секунды.")]
        public float cameraSmoothTime = 0.45f;
        [Range(8f, 40f)]
        [Tooltip("Ширина кадра на своём замке, метры.")]
        public float castleFrameWidth = 16f;
        [Range(10f, 60f)]
        [Tooltip("Ширина кадра в центре уровня (гора и бустеры), метры. Самый широкий кадр.")]
        public float centerFrameWidth = 38f;
        [Range(8f, 50f)]
        [Tooltip("Ширина кадра на замке-цели, метры. Чуть уже центрального.")]
        public float targetFrameWidth = 24f;
        [Range(6f, 40f)]
        [Tooltip("Минимальная высота кадра, метры. Нужна в ландшафтной ориентации.")]
        public float minFrameHeight = 16f;
        [Range(0f, 8f)]
        [Tooltip("Сдвиг камеры к противнику, чтобы свой замок стоял чуть левее центра, метры.")]
        public float castleLeadX = 2f;
        [Range(-2f, 8f)]
        [Tooltip("Подъём точки фокуса над центром замка, метры (видно небо над ним).")]
        public float castleFocusLiftY = 2f;
        [Tooltip("Точка фокуса в центре уровня, метры.")]
        public Vector2 centerFocus = new Vector2(0f, 6f);
        [Range(0.1f, 0.9f)]
        [Tooltip("Какую долю полёта камера держит центр уровня, прежде чем переехать к цели.")]
        public float flightCenterFraction = 0.5f;

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
