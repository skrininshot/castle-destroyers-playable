using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Greybox уровня 1 (ТЗ §3.8, §3.10): статичные меши вокруг земли. BackMesh — фон, рисуется до неё;
    /// FrontMesh — вода, замки и бустеры, рисуется после неё и обломков. Порядок внутри меша = порядок добавления.
    /// Заменяется масками и артом на этапах 5 и 7; геометрия мест (замки, бустеры) остаётся.
    /// </summary>
    public sealed class GreyboxLevel
    {
        // Размеры декора, метры. Живут только до арт-пасса, поэтому не в GameConfig.
        private const int GroundSamples = 5;
        private const uint DecorSeed = 7321u;

        private const int StarCount = 80;
        private const float StarMinY = 10f;
        private const float StarMaxY = 40f;
        private const float StarMinSize = 0.06f;
        private const float StarMaxSize = 0.14f;
        private const float MoonX = 5.6f;
        private const float MoonY = 19.5f;
        private const float MoonRadius = 2.4f;

        private const float FarStep = 15f;
        private const float FarHalfWidth = 11f;
        private const float FarMinPeak = 15f;
        private const float FarMaxPeak = 21f;
        private const float FarTipRadius = 2f;
        private const float MidBandHeight = 3.6f;
        private const float MidStep = 1.5f;
        private const float MidMinPeak = 6f;
        private const float MidMaxPeak = 9f;
        private const float MidHalfWidth = 1.2f;
        private const float NearStep = 2.6f;
        private const float NearMinRadius = 1.4f;
        private const float NearMaxRadius = 2.5f;
        private const float NearMinY = 1.2f;
        private const float NearMaxY = 2.8f;

        private const int WaterPillCount = 110;
        private const float WaterPillMinWidth = 0.5f;
        private const float WaterPillMaxWidth = 1.8f;
        private const float WaterPillHeight = 0.11f;
        private const float WaterPillDepth = 24f;
        private const float WaterPillTop = 0.6f;

        // Кратеры на луне: смещение от центра и радиус в долях радиуса луны.
        private static readonly float[] MoonCraters = { -0.35f, 0.3f, 0.26f, 0.32f, -0.15f, 0.18f, -0.1f, -0.5f, 0.14f };

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly Rng decorRng = new Rng(DecorSeed);
        private GameConfig config;
        private DestructibleMask terrain;

        public Mesh BackMesh { get; private set; }
        public Mesh FrontMesh { get; private set; }
        public float PlayerFocusX { get; private set; }
        public float PlayerFocusY { get; private set; }
        public float EnemyFocusX { get; private set; }
        public float EnemyFocusY { get; private set; }
        public float[] PlayerMountX { get; private set; }
        public float[] PlayerMountY { get; private set; }
        public float[] EnemyMountX { get; private set; }
        public float[] EnemyMountY { get; private set; }

        private float playerGroundY;
        private float enemyGroundY;

        public void Init(GameConfig gameConfig, DestructibleMask terrainMask)
        {
            config = gameConfig;
            terrain = terrainMask;
            BackMesh = new Mesh();
            BackMesh.name = "LevelBack";
            FrontMesh = new Mesh();
            FrontMesh.name = "LevelFront";

            playerGroundY = CastleGroundY(config.playerCastleX, GreyboxCastles.BlueHalfWidth);
            enemyGroundY = CastleGroundY(config.enemyCastleX, GreyboxCastles.RedHalfWidth);

            float playerWallBase = playerGroundY + config.trackHeight + config.platformHeight;
            float enemyWallBase = enemyGroundY + config.trackHeight + config.platformHeight;
            PlayerFocusX = config.playerCastleX;
            PlayerFocusY = playerWallBase + (GreyboxCastles.BlueTowerTop + GreyboxCastles.BlueRoofHeight) * 0.5f;
            EnemyFocusX = config.enemyCastleX;
            EnemyFocusY = enemyWallBase + GreyboxCastles.RedTotalHeight() * 0.5f;

            PlayerMountX = new float[config.playerCannons];
            PlayerMountY = new float[config.playerCannons];
            EnemyMountX = new float[config.enemyCannons];
            EnemyMountY = new float[config.enemyCannons];
            ComputeMounts(PlayerMountX, PlayerMountY, GreyboxCastles.BlueMounts, config.playerCastleX, playerWallBase);
            ComputeMounts(EnemyMountX, EnemyMountY, GreyboxCastles.RedMounts, config.enemyCastleX, enemyWallBase);
        }

        /// <summary>Текущая высота ландшафта в точке x, метры.</summary>
        public float GroundHeight(float x)
        {
            return terrain.SurfaceHeight(x);
        }

        /// <summary>Пересобирает меши в цветах палитры (при старте и смене темы).</summary>
        public void Build(PaletteSet palette)
        {
            float extent = config.levelWidth;
            decorRng.Reset(DecorSeed);
            builder.Clear();
            AddBackdrop(palette, extent);
            builder.Apply(BackMesh);

            builder.Clear();
            AddWater(palette, extent);
            GreyboxCastles.DrawBlue(builder, palette, decorRng, config, config.playerCastleX, playerGroundY);
            GreyboxCastles.DrawRed(builder, palette, decorRng, config, config.enemyCastleX, enemyGroundY);
            GreyboxBoosters.Draw(builder, palette, config);
            builder.Apply(FrontMesh);
        }

        /// <summary>Самая высокая точка ландшафта под замком: гусеницы не должны висеть над склоном.</summary>
        private float CastleGroundY(float centerX, float halfWidth)
        {
            float ground = float.NegativeInfinity;
            for (int i = 0; i < GroundSamples; i++)
            {
                float t = (float)i / (GroundSamples - 1);
                float h = GroundHeight(centerX - halfWidth + 2f * halfWidth * t);
                if (h > ground)
                {
                    ground = h;
                }
            }
            return ground;
        }

        private static void ComputeMounts(float[] mountX, float[] mountY, float[] table, float centerX, float wallBase)
        {
            for (int i = 0; i < mountX.Length; i++)
            {
                mountX[i] = centerX + table[i * 2];
                mountY[i] = wallBase + table[i * 2 + 1];
            }
        }

        private void AddBackdrop(PaletteSet palette, float extent)
        {
            AddStars(palette, extent);
            AddMoon(palette);
            AddFarMountains(palette, extent);
            AddMidForest(palette, extent);
            AddNearBushes(palette, extent);
        }

        private void AddStars(PaletteSet palette, float extent)
        {
            for (int i = 0; i < StarCount; i++)
            {
                float x = decorRng.Range(-extent, extent);
                float y = decorRng.Range(StarMinY, StarMaxY);
                float size = decorRng.Range(StarMinSize, StarMaxSize);
                builder.AddQuad(x - size, y, x, y - size, x + size, y, x, y + size, palette.skyStar);
            }
        }

        private void AddMoon(PaletteSet palette)
        {
            builder.AddCircle(MoonX, MoonY, MoonRadius, palette.moon);
            for (int i = 0; i + 2 < MoonCraters.Length; i += 3)
            {
                float x = MoonX + MoonCraters[i] * MoonRadius;
                float y = MoonY + MoonCraters[i + 1] * MoonRadius;
                builder.AddCircle(x, y, MoonCraters[i + 2] * MoonRadius, palette.moonShade);
            }
        }

        /// <summary>Дальние горы: треугольники со скруглённой вершиной.</summary>
        private void AddFarMountains(PaletteSet palette, float extent)
        {
            int count = (int)(extent * 2f / FarStep) + 2;
            for (int i = 0; i < count; i++)
            {
                float cx = -extent + i * FarStep + decorRng.Range(-1f, 1f);
                float peak = config.waterLevel + decorRng.Range(FarMinPeak, FarMaxPeak);
                AddRoundedPeak(cx, config.waterLevel, FarHalfWidth, peak, FarTipRadius, palette.backgroundFar);
            }
        }

        private void AddRoundedPeak(float cx, float baseY, float halfWidth, float peak, float tipRadius, Color32 color)
        {
            float height = peak - baseY;
            float side = Mathf.Sqrt(halfWidth * halfWidth + height * height);
            float centerY = peak - tipRadius * side / halfWidth;
            float tangentX = tipRadius * height / side;
            float tangentY = centerY + tipRadius * halfWidth / side;
            builder.AddQuad(cx - halfWidth, baseY, cx + halfWidth, baseY, cx + tangentX, tangentY, cx - tangentX, tangentY, color);
            builder.AddCircle(cx, centerY, tipRadius, color);
        }

        /// <summary>Средний план: сплошная полоса и ели-треугольники над ней.</summary>
        private void AddMidForest(PaletteSet palette, float extent)
        {
            float baseY = config.waterLevel;
            builder.AddRect(-extent, baseY, extent, baseY + MidBandHeight, palette.backgroundMid);
            int count = (int)(extent * 2f / MidStep) + 1;
            for (int i = 0; i < count; i++)
            {
                float cx = -extent + i * MidStep;
                float peak = baseY + decorRng.Range(MidMinPeak, MidMaxPeak);
                float foot = baseY + MidBandHeight - 0.1f;
                builder.AddTriangle(cx - MidHalfWidth, foot, cx + MidHalfWidth, foot, cx, peak, palette.backgroundMid);
            }
        }

        /// <summary>Ближний план: круглые кусты тёмного тона.</summary>
        private void AddNearBushes(PaletteSet palette, float extent)
        {
            float baseY = config.waterLevel;
            int count = (int)(extent * 2f / NearStep) + 1;
            for (int i = 0; i < count; i++)
            {
                float cx = -extent + i * NearStep + decorRng.Range(-0.6f, 0.6f);
                builder.AddCircle(cx, baseY + decorRng.Range(NearMinY, NearMaxY), decorRng.Range(NearMinRadius, NearMaxRadius), palette.backgroundNear);
            }
            builder.AddRect(-extent, baseY, extent, baseY + NearMinY, palette.backgroundNear);
        }

        /// <summary>Вода непрозрачная: основной тон и блики-пилюли. Волнистую кромку рисует WaterStrip (ТЗ §3.8).</summary>
        private void AddWater(PaletteSet palette, float extent)
        {
            float level = config.waterLevel;
            builder.AddRect(-extent, config.levelBottom, extent, level, palette.water);
            for (int i = 0; i < WaterPillCount; i++)
            {
                float x = decorRng.Range(-extent, extent);
                float y = level - decorRng.Range(WaterPillTop, WaterPillDepth);
                float width = decorRng.Range(WaterPillMinWidth, WaterPillMaxWidth);
                builder.AddRoundedRect(x, y, x + width, y + WaterPillHeight, WaterPillHeight * 0.5f, palette.waterHighlight);
            }
        }
    }
}
