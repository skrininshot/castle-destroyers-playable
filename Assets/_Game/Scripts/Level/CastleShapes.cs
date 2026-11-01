using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Геометрия замков (ТЗ §3.7): размеры, точки крепления пушек и силуэты для масок. Синий — две башни и проём, красный — «собор»
    /// из 7 башен. Рисунки в Art/Source/Game повторяют эти размеры, поэтому маска совпадает с картинкой.
    /// </summary>
    public static class CastleShapes
    {
        /// <summary>Во сколько раз замки крупнее эскиза (D-077). Пушки не масштабируются. Совпадает с CASTLE_SCALE в Tools/build_art.py.</summary>
        public const float Scale = 1.3f;

        public const float BlueHalfWidth = 3.5f * Scale;
        public const float BlueBodyHeight = 3.6f * Scale;
        public const float BlueTowerWidth = 2.4f * Scale;
        public const float BlueTowerTop = 6.6f * Scale;
        public const float BlueRoofHeight = 2.5f * Scale;
        public const float RedHalfWidth = 4f * Scale;
        public const float RedTop = 11f * Scale;
        public const float RedRoofHeight = 1.9f * Scale;

        private const float RedBodyHeight = 2.6f * Scale;
        private const float RedMidHalfWidth = 1.7f * Scale;
        private const float RedMidHeight = 6.2f * Scale;

        private const float CollarOverhang = 0.2f * Scale;
        private const float CollarHeight = 0.3f * Scale;
        private const float CollarRadius = 0.1f * Scale;
        private const float SpireHeight = 0.62f * Scale;
        private const float SpireHalfWidth = 0.04f * Scale;
        private const float FlagLength = 0.66f * Scale;
        private const float FlagHeight = 0.44f * Scale;

        private const float PlatformOverhang = 0.35f * Scale;
        private const float TrackWidth = 2.7f * Scale;
        private const float TrackInset = 0.1f * Scale;
        private const int CrackSegments = 3;
        private const float CrackHalfWidth = 0.09f * Scale;
        private const float CrackDrift = 0.18f * Scale;
        private const float CrackTaper = 0.6f;

        /// <summary>Башни красного замка: смещение от центра, ширина, верх стены, высота крыши.</summary>
        private static readonly float[] RedTowerOffset = Scaled(new[] { -3.55f, 3.55f, -2.45f, 2.45f, -1.15f, 1.15f, 0f });
        private static readonly float[] RedTowerWidth = Scaled(new[] { 0.9f, 0.9f, 1f, 1f, 1.1f, 1.1f, 1.25f });
        private static readonly float[] RedTowerTop = Scaled(new[] { 3.1f, 3.1f, 3.9f, 3.9f, 7f, 7f, 11f });
        private static readonly float[] RedTowerRoof = Scaled(new[] { 1.3f, 1.3f, 1.4f, 1.4f, 1.6f, 1.6f, 1.9f });

        /// <summary>
        /// Точки крепления пушек: пары (x от центра замка, y от основания стены). Пушки стоят на стенах,
        /// как в кадрах 1 и 4; первыми идут те, что остаются живыми дольше всех.
        /// </summary>
        public static readonly float[] BlueMounts = Scaled(new[]
        {
            1.9f, 4.05f, 0.7f, 3.05f, -2.6f, 4.05f, 1.9f, 1f, -2.6f, 1f, 1.9f, 3.1f, -1f, 3.1f,
            -2.6f, 3.1f, 0.2f, 0.9f, -1f, 1.2f, 1.9f, 6.25f, -2.6f, 6.25f, 1f, 2f, -1.6f, 2.1f
        });

        public static readonly float[] RedMounts = Scaled(new[]
        {
            0.6f, 9f, 0.6f, 7.7f, 1.7f, 5.1f, -0.55f, 5.1f, 0.6f, 4f, -0.95f, 3.9f, 0.6f, 2.9f,
            -1.45f, 2.1f, 2.7f, 2.1f, -2.9f, 1.1f, 0.6f, 1.1f, 3.7f, 1.1f, 1.7f, 3.8f, -3.7f, 2.2f
        });

        private static float[] Scaled(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] *= Scale;
            }
            return values;
        }

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

        /// <summary>Силуэт синего замка; низ стены на wallBase, центр на x = 0.</summary>
        public static void DrawBlue(FlatMeshBuilder builder, float wallBase)
        {
            float towerOffset = BlueHalfWidth - BlueTowerWidth * 0.5f;
            builder.AddRect(-BlueHalfWidth, wallBase, BlueHalfWidth, wallBase + BlueBodyHeight, Color.white);
            AddTower(builder, -towerOffset, BlueTowerWidth, wallBase, wallBase + BlueTowerTop, BlueRoofHeight);
            AddTower(builder, towerOffset, BlueTowerWidth, wallBase, wallBase + BlueTowerTop, BlueRoofHeight);
        }

        /// <summary>Силуэт красного замка; низ стены на wallBase, центр на x = 0.</summary>
        public static void DrawRed(FlatMeshBuilder builder, float wallBase)
        {
            builder.AddRect(-RedHalfWidth, wallBase, RedHalfWidth, wallBase + RedBodyHeight, Color.white);
            builder.AddRect(-RedMidHalfWidth, wallBase, RedMidHalfWidth, wallBase + RedMidHeight, Color.white);
            for (int i = 0; i < RedTowerOffset.Length; i++)
            {
                AddTower(builder, RedTowerOffset[i], RedTowerWidth[i], wallBase, wallBase + RedTowerTop[i], RedTowerRoof[i]);
            }
        }

        /// <summary>Стена, воротник, конус крыши, шпиль и флажок.</summary>
        private static void AddTower(FlatMeshBuilder builder, float cx, float width, float wallBase, float top, float roofHeight)
        {
            float half = width * 0.5f;
            float roofHalf = half + CollarOverhang;
            float apex = top + roofHeight;
            builder.AddRect(cx - half, wallBase, cx + half, top, Color.white);
            builder.AddRoundedRect(cx - roofHalf, top - CollarHeight * 0.5f, cx + roofHalf, top + CollarHeight * 0.5f, CollarRadius, Color.white);
            builder.AddTriangle(cx - roofHalf, top, cx + roofHalf, top, cx, apex, Color.white);
            builder.AddRect(cx - SpireHalfWidth, apex - SpireHalfWidth, cx + SpireHalfWidth, apex + SpireHeight, Color.white);
            builder.AddTriangle(cx + SpireHalfWidth, apex + SpireHeight, cx + SpireHalfWidth, apex + SpireHeight - FlagHeight, cx + SpireHalfWidth + FlagLength, apex + SpireHeight - FlagHeight * 0.5f, Color.white);
        }

        /// <summary>Трещина-декаль платформы: плоская ломаная сверху вниз от точки x (ТЗ §3.6, кадр 12).</summary>
        public static void AddPlatformCrack(FlatMeshBuilder builder, Core.PaletteSet palette, Core.Rng rng, float x, float height)
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
    }
}
