using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Greybox уровня 1 (ТЗ §3.8): фон ступенями, ландшафт по контуру, вода, замки-прямоугольники,
    /// бустеры. Всё в одном статичном меше, порядок отрисовки = порядок добавления.
    /// Заменяется масками и артом на этапах 2, 5 и 7; геометрия мест (замки, бустеры) остаётся.
    /// </summary>
    public sealed class GreyboxLevel
    {
        // Размеры greybox-силуэтов, метры. Живут только до арт-пасса, поэтому не в GameConfig.
        private const float BlueHalfWidth = 3.5f;
        private const float BlueBaseHeight = 2.8f;
        private const float BlueTowerWidth = 2f;
        private const float BlueTowerHeight = 4.2f;
        private const float BlueRoofHeight = 1.8f;
        private const float RedTowerWidth = 1.3f;
        private const float RedTowerPitch = 1.4f;
        private const float RedBaseHeight = 4f;
        private const float RedRoofHeight = 1.4f;
        private const float RoofOverhang = 0.25f;
        private const float PlatformOverhang = 0.5f;
        private const float TrackWidth = 2.6f;
        private const float TrackInset = 0.2f;
        private const float StoneShadeFraction = 0.3f;
        private const int MountMaxRows = 6;
        private const float MountBottomMargin = 0.7f;
        private const float MountTopMargin = 0.7f;
        private const float MountColumnSpacing = 0.9f;
        private const float MountColumnStagger = 0.5f;
        private const int GroundSamples = 5;
        private const byte WaterAlpha = 215;
        private const byte BoosterAlpha = 200;
        private const float WaterRimThickness = 0.25f;
        private const int BoosterMultiplierMid = 20;
        private const int BoosterMultiplierHigh = 50;

        private static readonly float[] RedTowerExtraHeights = { 2.5f, 5f, 3.5f, 7f, 3.5f, 5f, 2.5f };

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private GameConfig config;

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
            Mesh = new Mesh();
            Mesh.name = "GreyboxLevel";
            Mesh.MarkDynamic();

            playerGroundY = CastleGroundY(config.playerCastleX, BlueHalfWidth);
            enemyGroundY = CastleGroundY(config.enemyCastleX, RedHalfWidth());

            float playerWallBase = playerGroundY + config.trackHeight + config.platformHeight;
            float enemyWallBase = enemyGroundY + config.trackHeight + config.platformHeight;
            PlayerFocusX = config.playerCastleX;
            PlayerFocusY = playerWallBase + (BlueBaseHeight + BlueTowerHeight) * 0.5f;
            EnemyFocusX = config.enemyCastleX;
            EnemyFocusY = enemyWallBase + (RedBaseHeight + MaxRedExtraHeight()) * 0.5f;

            PlayerMountX = new float[config.playerCannons];
            PlayerMountY = new float[config.playerCannons];
            EnemyMountX = new float[config.enemyCannons];
            EnemyMountY = new float[config.enemyCannons];
            ComputeMounts(PlayerMountX, PlayerMountY, config.playerCastleX, BlueHalfWidth, playerWallBase, BlueBaseHeight, 1f);
            ComputeMounts(EnemyMountX, EnemyMountY, config.enemyCastleX, RedHalfWidth(), enemyWallBase, RedBaseHeight, -1f);
        }

        /// <summary>Высота ландшафта в точке x, метры.</summary>
        public float GroundHeight(float x)
        {
            return TerrainProfile.HeightAt(config.terrainProfile, x);
        }

        /// <summary>Пересобирает меш в цветах палитры (при старте и смене темы).</summary>
        public void Build(PaletteSet palette)
        {
            builder.Clear();
            float extent = config.levelWidth;
            AddBackdrop(palette, extent);
            AddTerrain(palette, extent);
            AddWater(palette, extent);
            AddBlueCastle(palette);
            AddRedCastle(palette);
            AddBoosters(palette);
            builder.Apply(Mesh);
        }

        private float RedHalfWidth()
        {
            return RedTowerExtraHeights.Length * RedTowerPitch * 0.5f;
        }

        private static float MaxRedExtraHeight()
        {
            float max = 0f;
            for (int i = 0; i < RedTowerExtraHeights.Length; i++)
            {
                if (RedTowerExtraHeights[i] > max)
                {
                    max = RedTowerExtraHeights[i];
                }
            }
            return max;
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

        private void ComputeMounts(float[] mountX, float[] mountY, float centerX, float halfWidth, float wallBase, float wallHeight, float facing)
        {
            int count = mountX.Length;
            int rows = count < MountMaxRows ? count : MountMaxRows;
            float frontX = centerX + facing * halfWidth;
            int columns = (count + rows - 1) / rows;
            float span = wallHeight - MountBottomMargin - MountTopMargin;
            // Каждая следующая колонка сдвинута на пол-шага по высоте, чтобы стволы не перекрывали друг друга.
            float rowSpan = rows - 1 + MountColumnStagger * (columns - 1);
            for (int i = 0; i < count; i++)
            {
                int row = i % rows;
                int column = i / rows;
                float t = rowSpan <= 0f ? 0.5f : (row + MountColumnStagger * column) / rowSpan;
                mountX[i] = frontX - facing * column * MountColumnSpacing;
                mountY[i] = wallBase + MountBottomMargin + span * t;
            }
        }

        private void AddBackdrop(PaletteSet palette, float extent)
        {
            AddBackdropLayer(palette.backgroundFar, extent, 20f, 13f, 0.65f);
            AddBackdropLayer(palette.backgroundMid, extent, 11f, 8f, 0.8f);
            AddBackdropLayer(palette.backgroundNear, extent, 5.5f, 5f, 0.9f);
        }

        /// <summary>Ряд треугольных силуэтов от воды вверх. Высоты чередуются по детерминированному шаблону.</summary>
        private void AddBackdropLayer(Color32 color, float extent, float peakHeight, float halfWidth, float spacingFactor)
        {
            float step = halfWidth * 2f * spacingFactor;
            int count = (int)(extent * 2f / step) + 2;
            float baseY = config.waterLevel;
            for (int i = 0; i < count; i++)
            {
                float cx = -extent + i * step;
                float variation = 0.7f + 0.3f * ((i * 3) % 5) / 4f;
                float peak = baseY + peakHeight * variation;
                builder.AddTriangle(cx - halfWidth, baseY, cx + halfWidth, baseY, cx, peak, color);
            }
        }

        private void AddTerrain(PaletteSet palette, float extent)
        {
            Vector2[] profile = config.terrainProfile;
            float bottom = config.levelBottom;
            for (int i = 0; i < profile.Length - 1; i++)
            {
                float x0 = profile[i].x;
                float y0 = profile[i].y;
                float x1 = profile[i + 1].x;
                float y1 = profile[i + 1].y;
                if (x1 - x0 > 0f)
                {
                    builder.AddQuad(x0, y0, x1, y1, x1, bottom, x0, bottom, palette.ground);
                }
            }

            // Цветная кромка по верху (ТЗ §3.8): полоса толщиной groundRimThickness вглубь по нормали.
            float thickness = config.groundRimThickness;
            for (int i = 0; i < profile.Length - 1; i++)
            {
                float dx = profile[i + 1].x - profile[i].x;
                float dy = profile[i + 1].y - profile[i].y;
                float length = Mathf.Sqrt(dx * dx + dy * dy);
                if (length <= 0f)
                {
                    continue;
                }
                float nx = dy / length * thickness;
                float ny = -dx / length * thickness;
                builder.AddQuad(
                    profile[i].x, profile[i].y,
                    profile[i + 1].x, profile[i + 1].y,
                    profile[i + 1].x + nx, profile[i + 1].y + ny,
                    profile[i].x + nx, profile[i].y + ny,
                    palette.groundRim);
            }

            // Дальше профиля земля продолжается ровной полосой до края фона.
            float firstX = profile[0].x;
            float lastX = profile[profile.Length - 1].x;
            if (firstX > -extent)
            {
                builder.AddRect(-extent, bottom, firstX, profile[0].y, palette.ground);
                builder.AddRect(-extent, profile[0].y - thickness, firstX, profile[0].y, palette.groundRim);
            }
            if (lastX < extent)
            {
                float lastY = profile[profile.Length - 1].y;
                builder.AddRect(lastX, bottom, extent, lastY, palette.ground);
                builder.AddRect(lastX, lastY - thickness, extent, lastY, palette.groundRim);
            }
        }

        private void AddWater(PaletteSet palette, float extent)
        {
            Color32 water = palette.water;
            water.a = WaterAlpha;
            builder.AddRect(-extent, config.levelBottom, extent, config.waterLevel, water);
            builder.AddRect(-extent, config.waterLevel - WaterRimThickness, extent, config.waterLevel, palette.waterRim);
        }

        private void AddBlueCastle(PaletteSet palette)
        {
            float cx = config.playerCastleX;
            float trackTop = playerGroundY + config.trackHeight;
            float wallBase = trackTop + config.platformHeight;
            AddPlatformAndTracks(palette, cx, BlueHalfWidth, playerGroundY, trackTop, wallBase);

            builder.AddRect(cx - BlueHalfWidth, wallBase, cx + BlueHalfWidth, wallBase + BlueBaseHeight, palette.castleStone);
            float towerBase = wallBase + BlueBaseHeight;
            AddTower(palette, cx - BlueHalfWidth + BlueTowerWidth * 0.5f, BlueTowerWidth, wallBase, BlueBaseHeight + BlueTowerHeight, BlueRoofHeight, palette.roofPlayer);
            AddTower(palette, cx + BlueHalfWidth - BlueTowerWidth * 0.5f, BlueTowerWidth, wallBase, BlueBaseHeight + BlueTowerHeight, BlueRoofHeight, palette.roofPlayer);
            // Тень на внутренней стороне основания: проём между башнями читается как в референсе.
            builder.AddRect(cx - BlueHalfWidth + BlueTowerWidth, wallBase, cx + BlueHalfWidth - BlueTowerWidth, towerBase, palette.castleStoneShade);
        }

        private void AddRedCastle(PaletteSet palette)
        {
            float cx = config.enemyCastleX;
            float halfWidth = RedHalfWidth();
            float trackTop = enemyGroundY + config.trackHeight;
            float wallBase = trackTop + config.platformHeight;
            AddPlatformAndTracks(palette, cx, halfWidth, enemyGroundY, trackTop, wallBase);

            builder.AddRect(cx - halfWidth, wallBase, cx + halfWidth, wallBase + RedBaseHeight, palette.castleStone);
            int towers = RedTowerExtraHeights.Length;
            for (int i = 0; i < towers; i++)
            {
                float tx = cx + (i - (towers - 1) * 0.5f) * RedTowerPitch;
                AddTower(palette, tx, RedTowerWidth, wallBase, RedBaseHeight + RedTowerExtraHeights[i], RedRoofHeight, palette.roofEnemy);
            }
        }

        private void AddPlatformAndTracks(PaletteSet palette, float cx, float halfWidth, float groundY, float trackTop, float wallBase)
        {
            float trackInnerEdge = halfWidth + PlatformOverhang - TrackInset;
            builder.AddRect(cx - trackInnerEdge, groundY, cx - trackInnerEdge + TrackWidth, trackTop, palette.track);
            builder.AddRect(cx + trackInnerEdge - TrackWidth, groundY, cx + trackInnerEdge, trackTop, palette.track);
            builder.AddRect(cx - halfWidth - PlatformOverhang, trackTop, cx + halfWidth + PlatformOverhang, wallBase, palette.platform);
        }

        /// <summary>Башня: стена с тенью на правой трети и коническая крыша.</summary>
        private void AddTower(PaletteSet palette, float cx, float width, float wallBase, float height, float roofHeight, Color32 roofColor)
        {
            float half = width * 0.5f;
            float top = wallBase + height;
            builder.AddRect(cx - half, wallBase, cx + half, top, palette.castleStone);
            builder.AddRect(cx + half - width * StoneShadeFraction, wallBase, cx + half, top, palette.castleStoneShade);
            builder.AddTriangle(cx - half - RoofOverhang, top, cx + half + RoofOverhang, top, cx, top + roofHeight, roofColor);
        }

        private void AddBoosters(PaletteSet palette)
        {
            BoosterSpec[] boosters = config.boosters;
            float halfWidth = config.boosterWidth * 0.5f;
            float halfHeight = config.boosterHeight * 0.5f;
            for (int i = 0; i < boosters.Length; i++)
            {
                Color32 color = BoosterColor(palette, boosters[i].multiplier);
                color.a = BoosterAlpha;
                float x = boosters[i].position.x;
                float y = boosters[i].position.y;
                builder.AddRect(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight, color);
            }
        }

        private static Color32 BoosterColor(PaletteSet palette, int multiplier)
        {
            if (multiplier >= BoosterMultiplierHigh)
            {
                return palette.booster50;
            }
            if (multiplier >= BoosterMultiplierMid)
            {
                return palette.booster20;
            }
            return palette.booster10;
        }
    }
}
