using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Greybox уровня 1 (ТЗ §3.8, §3.10): плоский фон, гладкий ландшафт с фестонной кромкой, вода, замки, бустеры.
    /// Всё в одном статичном меше, порядок отрисовки = порядок добавления.
    /// Заменяется масками и артом на этапах 2, 5 и 7; геометрия мест (замки, бустеры) остаётся.
    /// </summary>
    public sealed class GreyboxLevel
    {
        // Размеры декора, метры. Живут только до арт-пасса, поэтому не в GameConfig.
        private const int GroundSamples = 5;
        private const float ProfileStep = 0.14f;
        private const float RimScallopPitch = 0.7f;
        private const float RimMinDepth = 0.55f;
        private const float RimScallopDepth = 0.75f;

        private const uint DecorSeed = 7321u;
        private const int BuriedAttempts = 260;
        private const float BuriedRange = 44f;
        private const float BuriedMaxHeight = 12f;
        private const float BuriedMinRadius = 0.2f;
        private const float BuriedMaxRadius = 0.5f;
        private const float BuriedFlatten = 0.6f;
        private const float BuriedDepth = 0.9f;
        private const float BuriedAboveWater = 0.3f;

        private const float TreeSink = 0.15f;
        private const float TreeTilt = 0.2f;
        private const float TreeMinHeight = 1.5f;
        private const float TreeMaxHeight = 2.4f;
        private const float TreeHalfWidthFraction = 0.36f;
        private const float TrunkHeightFraction = 0.16f;
        private const float TrunkWidthFraction = 0.16f;
        private const float SlopeProbe = 0.2f;
        private const int TreeTiers = 3;
        private const float TierOverlap = 0.45f;

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

        private const float WaterRimThickness = 0.22f;
        private const float WaveStep = 0.5f;
        private const float WaveAmplitude = 0.09f;
        private const float WaveFrequency = 0.9f;
        private const int WaterPillCount = 110;
        private const float WaterPillMinWidth = 0.5f;
        private const float WaterPillMaxWidth = 1.8f;
        private const float WaterPillHeight = 0.11f;
        private const float WaterPillDepth = 24f;
        private const float WaterPillTop = 0.6f;

        // Кратеры на луне: смещение от центра и радиус в долях радиуса луны.
        private static readonly float[] MoonCraters = { -0.35f, 0.3f, 0.26f, 0.32f, -0.15f, 0.18f, -0.1f, -0.5f, 0.14f };

        // Деревья на склонах и у подножий: X ёлки, метры.
        private static readonly float[] TreeX =
        {
            -16.4f, -15.1f, -13.3f, -11.6f, -9.9f, -8.6f, -7.1f, -5.9f, -4.6f, -3.3f, -2.1f,
            2.3f, 3.6f, 5f, 6.4f, 7.8f, 9.2f, 10.8f, 12.6f, 14.8f, 16f
        };

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly Rng decorRng = new Rng(DecorSeed);
        private GameConfig config;
        private Vector2[] profile;

        public Mesh Mesh { get; private set; }
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

        public void Init(GameConfig gameConfig)
        {
            config = gameConfig;
            profile = TerrainProfile.Smooth(config.terrainProfile, ProfileStep);
            Mesh = new Mesh();
            Mesh.name = "GreyboxLevel";
            Mesh.MarkDynamic();

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

        /// <summary>Высота ландшафта в точке x, метры.</summary>
        public float GroundHeight(float x)
        {
            return TerrainProfile.HeightAt(profile, x);
        }

        /// <summary>Пересобирает меш в цветах палитры (при старте и смене темы).</summary>
        public void Build(PaletteSet palette)
        {
            builder.Clear();
            decorRng.Reset(DecorSeed);
            float extent = config.levelWidth;
            AddBackdrop(palette, extent);
            AddTrees(palette);
            AddTerrain(palette);
            AddWater(palette, extent);
            GreyboxCastles.DrawBlue(builder, palette, decorRng, config, config.playerCastleX, playerGroundY);
            GreyboxCastles.DrawRed(builder, palette, decorRng, config, config.enemyCastleX, enemyGroundY);
            GreyboxBoosters.Draw(builder, palette, config);
            builder.Apply(Mesh);
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

        /// <summary>Ёлки на склонах. Рисуются до земли, поэтому их основание скрыто в склоне.</summary>
        private void AddTrees(PaletteSet palette)
        {
            for (int i = 0; i < TreeX.Length; i++)
            {
                float x = TreeX[i];
                float slope = (GroundHeight(x + SlopeProbe) - GroundHeight(x - SlopeProbe)) / (SlopeProbe * 2f);
                float length = Mathf.Sqrt(1f + slope * slope);
                // Ось дерева — между вертикалью и нормалью склона.
                float upX = -slope / length * TreeTilt;
                float upY = 1f / length * TreeTilt + (1f - TreeTilt);
                float upLength = Mathf.Sqrt(upX * upX + upY * upY);
                upX /= upLength;
                upY /= upLength;
                AddFir(x - upX * TreeSink, GroundHeight(x) - upY * TreeSink, upX, upY, decorRng.Range(TreeMinHeight, TreeMaxHeight), palette);
            }
        }

        /// <summary>Ель из ярусов-треугольников: левая половина светлая, правая теневая.</summary>
        private void AddFir(float x, float y, float upX, float upY, float height, PaletteSet palette)
        {
            float sideX = upY;
            float sideY = -upX;
            float halfWidth = height * TreeHalfWidthFraction;
            float trunkHalf = halfWidth * TrunkWidthFraction;
            float trunkTop = height * TrunkHeightFraction;
            builder.AddQuad(
                x - sideX * trunkHalf, y - sideY * trunkHalf,
                x + sideX * trunkHalf, y + sideY * trunkHalf,
                x + sideX * trunkHalf + upX * trunkTop, y + sideY * trunkHalf + upY * trunkTop,
                x - sideX * trunkHalf + upX * trunkTop, y - sideY * trunkHalf + upY * trunkTop,
                palette.treeShade);

            float tierHeight = (height - trunkTop) / (1f + (TreeTiers - 1) * (1f - TierOverlap));
            for (int tier = 0; tier < TreeTiers; tier++)
            {
                float bottom = trunkTop + tier * tierHeight * (1f - TierOverlap);
                float top = bottom + tierHeight;
                float half = halfWidth * (TreeTiers - tier + 1) / (TreeTiers + 1);
                float baseX = x + upX * bottom;
                float baseY = y + upY * bottom;
                float apexX = x + upX * top;
                float apexY = y + upY * top;
                builder.AddTriangle(baseX - sideX * half, baseY - sideY * half, baseX, baseY, apexX, apexY, palette.treeLight);
                builder.AddTriangle(baseX, baseY, baseX + sideX * half, baseY + sideY * half, apexX, apexY, palette.treeShade);
            }
        }

        private void AddTerrain(PaletteSet palette)
        {
            float bottom = config.levelBottom;
            for (int i = 0; i < profile.Length - 1; i++)
            {
                float x0 = profile[i].x;
                float x1 = profile[i + 1].x;
                if (x1 - x0 > 0f)
                {
                    builder.AddQuad(x0, profile[i].y, x1, profile[i + 1].y, x1, bottom, x0, bottom, palette.ground);
                }
            }
            AddBuriedDecor(palette);
            AddGroundRim(palette);
        }

        /// <summary>Редкие «закопанные» камни на тон темнее, чтобы большая масса не была пустой (ТЗ §3.8).</summary>
        private void AddBuriedDecor(PaletteSet palette)
        {
            for (int i = 0; i < BuriedAttempts; i++)
            {
                float x = decorRng.Range(-BuriedRange, BuriedRange);
                float y = decorRng.Range(config.waterLevel + BuriedAboveWater, BuriedMaxHeight);
                float radius = decorRng.Range(BuriedMinRadius, BuriedMaxRadius);
                bool inside = y + BuriedDepth < GroundHeight(x - radius) && y + BuriedDepth < GroundHeight(x + radius);
                if (inside)
                {
                    builder.AddEllipse(x, y, radius, radius * BuriedFlatten, palette.groundShade);
                }
            }
        }

        /// <summary>
        /// Цветная кромка (ТЗ §3.8): непрерывная лента вдоль контура. Соседние сегменты делят вершины,
        /// поэтому на изгибах нет щелей, а глубина фестоном меняется вдоль контура.
        /// </summary>
        private void AddGroundRim(PaletteSet palette)
        {
            float thickness = config.groundRimThickness;
            float distance = 0f;
            float innerX0 = 0f;
            float innerY0 = 0f;
            RimInner(0, distance, thickness, ref innerX0, ref innerY0);
            for (int i = 0; i < profile.Length - 1; i++)
            {
                float dx = profile[i + 1].x - profile[i].x;
                float dy = profile[i + 1].y - profile[i].y;
                distance += Mathf.Sqrt(dx * dx + dy * dy);
                float innerX1 = 0f;
                float innerY1 = 0f;
                RimInner(i + 1, distance, thickness, ref innerX1, ref innerY1);
                builder.AddQuad(profile[i].x, profile[i].y, profile[i + 1].x, profile[i + 1].y, innerX1, innerY1, innerX0, innerY0, palette.groundRim);
                innerX0 = innerX1;
                innerY0 = innerY1;
            }
        }

        /// <summary>Внутренняя вершина кромки: точка контура, сдвинутая вглубь по усреднённой нормали.</summary>
        private void RimInner(int index, float distance, float thickness, ref float x, ref float y)
        {
            int prev = index > 0 ? index - 1 : index;
            int next = index < profile.Length - 1 ? index + 1 : index;
            float dx = profile[next].x - profile[prev].x;
            float dy = profile[next].y - profile[prev].y;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            float scallop = Mathf.Abs(Mathf.Sin(Mathf.PI * distance / RimScallopPitch));
            float depth = thickness * (RimMinDepth + RimScallopDepth * scallop);
            x = profile[index].x + dy / length * depth;
            y = profile[index].y - dx / length * depth;
        }

        /// <summary>Вода непрозрачная: основной тон, блики-пилюли и светлая волнистая кромка (ТЗ §3.8).</summary>
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

            int steps = (int)(extent * 2f / WaveStep);
            float bottom = level - WaterRimThickness;
            for (int i = 0; i < steps; i++)
            {
                float x0 = -extent + i * WaveStep;
                float x1 = x0 + WaveStep;
                float y0 = level + WaveAmplitude * Mathf.Sin(x0 * WaveFrequency);
                float y1 = level + WaveAmplitude * Mathf.Sin(x1 * WaveFrequency);
                builder.AddQuad(x0, bottom, x1, bottom, x1, y1, x0, y0, palette.waterRim);
            }
        }
    }
}
