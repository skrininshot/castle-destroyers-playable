using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Силуэты замков в плоском стиле (ТЗ §3.7): синий с двумя башнями и проёмом, красный «собор» из 7 башен,
    /// под каждым платформа и две гусеницы. Формы — по кадрам 1 и 4, тона — из палитры.
    /// Размеры живут до арт-пасса этапа 7, поэтому здесь, а не в GameConfig.
    /// </summary>
    public static class GreyboxCastles
    {
        public const float BlueHalfWidth = 3.5f;
        public const float BlueBodyHeight = 3.6f;
        public const float BlueTowerWidth = 2.4f;
        public const float BlueTowerTop = 6.6f;
        public const float BlueRoofHeight = 2.5f;
        public const float RedHalfWidth = 4f;
        public const float RedTop = 11f;
        public const float RedRoofHeight = 1.9f;

        private const float RedBodyHeight = 2.6f;
        private const float RedMidHalfWidth = 1.7f;
        private const float RedMidHeight = 6.2f;

        private const float StoneShadeFraction = 0.24f;
        private const float BrickWidth = 0.36f;
        private const float BrickHeight = 0.15f;
        private const float BrickRadius = 0.05f;
        private const float BricksPerSquareMeter = 0.9f;
        private const float BrickMargin = 0.1f;
        private const float CollarOverhang = 0.2f;
        private const float CollarHeight = 0.3f;
        private const float CollarShadeHeight = 0.12f;
        private const float CollarRadius = 0.1f;
        private const float SpireHeight = 0.6f;
        private const float SpireHalfWidth = 0.03f;
        private const float FlagLength = 0.5f;
        private const float FlagHeight = 0.32f;
        private const float WindowFrame = 0.07f;
        private const float WindowInset = 0.1f;
        private const float BlueWindowWidth = 0.8f;
        private const float BlueWindowHeight = 1.1f;
        private const float BlueWindowY = 4.7f;
        private const float BlueBodyWindowY = 1.4f;
        private const float RedWindowWidth = 0.42f;
        private const float RedWindowHeight = 0.7f;

        private const float PlatformOverhang = 0.35f;
        private const float PlatformRadius = 0.32f;
        private const float PlatformShadeHeight = 0.16f;
        private const float GateWidth = 0.9f;
        private const float GateHeight = 0.6f;
        private const float GateOffset = 1.05f;
        private const float GateFrame = 0.08f;
        private const float TrackWidth = 2.7f;
        private const float TrackInset = 0.1f;
        private const float WheelOffset = 0.68f;
        private const float WheelRadiusFraction = 0.3f;
        private const float HubFraction = 0.4f;
        private const float SpikePitch = 0.42f;
        private const float SpikeHalfBase = 0.11f;
        private const float SpikeHeight = 0.17f;
        private const int CrackSegments = 3;
        private const float CrackHalfWidth = 0.09f;
        private const float CrackDrift = 0.18f;
        private const float CrackTaper = 0.6f;

        /// <summary>Башни красного замка: смещение от центра, ширина, верх стены, высота крыши, есть ли окно.</summary>
        private static readonly float[] RedTowerOffset = { -3.55f, 3.55f, -2.45f, 2.45f, -1.15f, 1.15f, 0f };
        private static readonly float[] RedTowerWidth = { 0.9f, 0.9f, 1f, 1f, 1.1f, 1.1f, 1.25f };
        private static readonly float[] RedTowerTop = { 3.1f, 3.1f, 3.9f, 3.9f, 7f, 7f, RedTop };
        private static readonly float[] RedTowerRoof = { 1.3f, 1.3f, 1.4f, 1.4f, 1.6f, 1.6f, RedRoofHeight };

        /// <summary>
        /// Точки крепления пушек: пары (x от центра замка, y от основания стены). Пушки стоят на стенах,
        /// как в кадрах 1 и 4; первыми идут те, что остаются живыми дольше всех.
        /// </summary>
        public static readonly float[] BlueMounts =
        {
            1.9f, 4.05f, 0.7f, 3.05f, -2.6f, 4.05f, 1.9f, 1f, -2.6f, 1f, 1.9f, 3.1f, -1f, 3.1f,
            -2.6f, 3.1f, 0.2f, 0.9f, -1f, 1.2f, 1.9f, 6.25f, -2.6f, 6.25f, 1f, 2f, -1.6f, 2.1f
        };

        public static readonly float[] RedMounts =
        {
            0.6f, 9f, 0.6f, 7.7f, 1.75f, 5.1f, -0.55f, 5.1f, 0.6f, 4f, -0.95f, 3.9f, 0.6f, 2.9f,
            -1.45f, 2.1f, 2.7f, 2.1f, -2.9f, 1.1f, 0.6f, 1.1f, 3.7f, 1.1f, 1.75f, 3.8f, -3.7f, 2.2f
        };

        public static float RedTotalHeight()
        {
            return RedTop + RedRoofHeight;
        }

        /// <summary>Половина ширины платформы замка.</summary>
        public static float PlatformHalfWidth(float castleHalfWidth)
        {
            return castleHalfWidth + PlatformOverhang;
        }

        /// <summary>Расстояние от центра замка до центра гусеницы.</summary>
        public static float TrackCenter(float castleHalfWidth)
        {
            return PlatformHalfWidth(castleHalfWidth) - TrackInset - TrackWidth * 0.5f;
        }

        /// <summary>Половина длины гусеницы: её крайние точки стоят на ландшафте (ТЗ §3.7).</summary>
        public static float TrackHalfLength()
        {
            return TrackWidth * 0.5f;
        }

        /// <summary>Каменная часть синего замка; низ стены на wallBase, центр на x = 0.</summary>
        public static void DrawBlue(FlatMeshBuilder builder, PaletteSet palette, Rng rng, float wallBase)
        {
            float cx = 0f;
            float towerOffset = BlueHalfWidth - BlueTowerWidth * 0.5f;
            AddWall(builder, palette, rng, cx - BlueHalfWidth, cx + BlueHalfWidth, wallBase, wallBase + BlueBodyHeight);
            AddBlueTower(builder, palette, rng, cx - towerOffset, wallBase, true);
            AddBlueTower(builder, palette, rng, cx + towerOffset, wallBase, false);
            AddWindow(builder, palette, cx, wallBase + BlueBodyWindowY, BlueWindowWidth, BlueWindowHeight, true);
        }

        /// <summary>Каменная часть красного замка; низ стены на wallBase, центр на x = 0.</summary>
        public static void DrawRed(FlatMeshBuilder builder, PaletteSet palette, Rng rng, float wallBase)
        {
            float cx = 0f;
            AddWall(builder, palette, rng, cx - RedHalfWidth, cx + RedHalfWidth, wallBase, wallBase + RedBodyHeight);
            AddWall(builder, palette, rng, cx - RedMidHalfWidth, cx + RedMidHalfWidth, wallBase, wallBase + RedMidHeight);
            // От края к центру: центральная башня рисуется последней и перекрывает средние.
            for (int i = 0; i < RedTowerOffset.Length; i++)
            {
                float tx = cx + RedTowerOffset[i];
                float width = RedTowerWidth[i];
                float top = wallBase + RedTowerTop[i];
                AddWall(builder, palette, rng, tx - width * 0.5f, tx + width * 0.5f, wallBase, top);
                AddRoof(builder, tx, width + CollarOverhang * 2f, top, RedTowerRoof[i], palette.roofEnemy, palette.roofEnemyShade, palette.cannonEnemy);
                AddCollar(builder, palette, tx, width, top);
                AddWindow(builder, palette, tx, top - RedWindowHeight - WindowInset * 4f, RedWindowWidth, RedWindowHeight, i % 2 == 0);
            }
        }

        private static void AddBlueTower(FlatMeshBuilder builder, PaletteSet palette, Rng rng, float cx, float wallBase, bool lit)
        {
            float half = BlueTowerWidth * 0.5f;
            float top = wallBase + BlueTowerTop;
            AddWall(builder, palette, rng, cx - half, cx + half, wallBase, top);
            AddRoof(builder, cx, BlueTowerWidth + CollarOverhang * 2f, top, BlueRoofHeight, palette.roofPlayer, palette.roofPlayerShade, palette.cannonPlayer);
            AddCollar(builder, palette, cx, BlueTowerWidth, top);
            AddWindow(builder, palette, cx, wallBase + BlueWindowY, BlueWindowWidth, BlueWindowHeight, lit);
        }

        /// <summary>Стена двумя тонами: основной, тень справа и редкие кирпичи на тон темнее.</summary>
        private static void AddWall(FlatMeshBuilder builder, PaletteSet palette, Rng rng, float minX, float maxX, float minY, float maxY)
        {
            float width = maxX - minX;
            float height = maxY - minY;
            builder.AddRect(minX, minY, maxX, maxY, palette.castleStone);
            builder.AddRect(maxX - width * StoneShadeFraction, minY, maxX, maxY, palette.castleStoneShade);

            int bricks = Mathf.RoundToInt(width * height * BricksPerSquareMeter);
            for (int i = 0; i < bricks; i++)
            {
                float x = rng.Range(minX + BrickMargin, maxX - BrickMargin - BrickWidth);
                float y = rng.Range(minY + BrickMargin, maxY - BrickMargin - BrickHeight);
                builder.AddRoundedRect(x, y, x + BrickWidth, y + BrickHeight, BrickRadius, palette.castleStoneShade);
            }
        }

        private static void AddCollar(FlatMeshBuilder builder, PaletteSet palette, float cx, float wallWidth, float top)
        {
            float half = wallWidth * 0.5f + CollarOverhang;
            builder.AddRoundedRect(cx - half, top - CollarHeight * 0.5f, cx + half, top + CollarHeight * 0.5f, CollarRadius, palette.castleStone);
            builder.AddRect(cx - wallWidth * 0.5f, top - CollarHeight * 0.5f - CollarShadeHeight, cx + wallWidth * 0.5f, top - CollarHeight * 0.5f, palette.castleStoneShade);
        }

        /// <summary>Конус из двух половин (светлая и теневая), шпиль и флажок в цвет стороны.</summary>
        private static void AddRoof(FlatMeshBuilder builder, float cx, float width, float baseY, float height, Color32 light, Color32 shade, Color32 flag)
        {
            float half = width * 0.5f;
            float apex = baseY + height;
            builder.AddRect(cx - SpireHalfWidth, apex - SpireHalfWidth, cx + SpireHalfWidth, apex + SpireHeight, shade);
            builder.AddTriangle(cx + SpireHalfWidth, apex + SpireHeight, cx + SpireHalfWidth, apex + SpireHeight - FlagHeight, cx + SpireHalfWidth + FlagLength, apex + SpireHeight - FlagHeight * 0.5f, flag);
            builder.AddTriangle(cx - half, baseY, cx, baseY, cx, apex, light);
            builder.AddTriangle(cx, baseY, cx + half, baseY, cx, apex, shade);
        }

        private static void AddWindow(FlatMeshBuilder builder, PaletteSet palette, float cx, float y, float width, float height, bool lit)
        {
            float half = width * 0.5f;
            builder.AddArch(cx - half - WindowFrame, y - WindowFrame, cx + half + WindowFrame, y + height + WindowFrame, palette.castleStoneShade);
            builder.AddArch(cx - half, y, cx + half, y + height, palette.castleWindow);
            if (lit)
            {
                builder.AddArch(cx - half + WindowInset, y, cx + half - WindowInset, y + height - WindowInset, palette.castleWindowLit);
            }
        }

        /// <summary>Деревянная платформа: низ на y = 0, центр на x = 0.</summary>
        public static void DrawPlatform(FlatMeshBuilder builder, PaletteSet palette, float halfWidth, float height)
        {
            float platformHalf = PlatformHalfWidth(halfWidth);
            builder.AddRoundedRect(-platformHalf, 0f, platformHalf, height, PlatformRadius, palette.platform);
            builder.AddRect(-platformHalf + PlatformRadius, 0f, platformHalf - PlatformRadius, PlatformShadeHeight, palette.platformShade);
            AddGate(builder, palette, -GateOffset, 0f);
            AddGate(builder, palette, GateOffset, 0f);
        }

        /// <summary>Гусеница: низ на y = 0, центр на x = 0.</summary>
        public static void DrawTrack(FlatMeshBuilder builder, PaletteSet palette, float height)
        {
            AddTrack(builder, palette, 0f, 0f, height);
        }

        /// <summary>Трещина-декаль платформы: плоская ломаная сверху вниз от точки x (ТЗ §3.6, кадр 12).</summary>
        public static void AddPlatformCrack(FlatMeshBuilder builder, PaletteSet palette, Rng rng, float x, float height)
        {
            float y = height;
            float half = CrackHalfWidth;
            float step = height / CrackSegments;
            for (int i = 0; i < CrackSegments; i++)
            {
                float nextX = x + rng.Range(-CrackDrift, CrackDrift);
                float nextY = y - step;
                float nextHalf = half * CrackTaper;
                builder.AddQuad(x - half, y, nextX - nextHalf, nextY, nextX + nextHalf, nextY, x + half, y, palette.castleCrack);
                x = nextX;
                y = nextY;
                half = nextHalf;
            }
        }

        private static void AddGate(FlatMeshBuilder builder, PaletteSet palette, float cx, float y)
        {
            float half = GateWidth * 0.5f;
            builder.AddArch(cx - half - GateFrame, y, cx + half + GateFrame, y + GateHeight + GateFrame, palette.platformShade);
            builder.AddArch(cx - half, y, cx + half, y + GateHeight, palette.castleWindow);
        }

        /// <summary>Гусеница: скруглённая лента с шипами и две шестерни.</summary>
        private static void AddTrack(FlatMeshBuilder builder, PaletteSet palette, float cx, float groundY, float height)
        {
            float half = TrackWidth * 0.5f;
            float top = groundY + height;
            float radius = height * 0.5f;
            int spikes = Mathf.FloorToInt((TrackWidth - radius * 2f) / SpikePitch);
            float start = cx - SpikePitch * (spikes - 1) * 0.5f;
            for (int i = 0; i < spikes; i++)
            {
                float x = start + SpikePitch * i;
                builder.AddTriangle(x - SpikeHalfBase, top, x + SpikeHalfBase, top, x, top + SpikeHeight, palette.track);
                builder.AddTriangle(x - SpikeHalfBase, groundY, x + SpikeHalfBase, groundY, x, groundY - SpikeHeight, palette.track);
            }
            builder.AddRoundedRect(cx - half, groundY, cx + half, top, radius, palette.track);

            float wheelRadius = height * WheelRadiusFraction;
            float wheelY = groundY + radius;
            AddWheel(builder, palette, cx - WheelOffset, wheelY, wheelRadius);
            AddWheel(builder, palette, cx + WheelOffset, wheelY, wheelRadius);
        }

        private static void AddWheel(FlatMeshBuilder builder, PaletteSet palette, float x, float y, float radius)
        {
            builder.AddCircle(x, y, radius, palette.trackWheel);
            builder.AddCircle(x, y, radius * HubFraction, palette.track);
        }
    }
}
