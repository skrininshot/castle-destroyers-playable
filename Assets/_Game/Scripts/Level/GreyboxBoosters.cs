using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Бустеры (ТЗ §3.5): полупрозрачная колба, блики, скруглённые заглушки на тон темнее и надпись «x20».
    /// Цифры собраны из сегментов, шрифт для этого не нужен. Анимация и «пых» появятся на этапе 4.
    /// </summary>
    public static class GreyboxBoosters
    {
        private const int MultiplierMid = 20;
        private const int MultiplierHigh = 50;
        private const byte GlassAlpha = 232;
        private const byte HighlightAlpha = 110;
        private const float CapHeight = 0.17f;
        private const float CapOverhang = 0.05f;
        private const float CapRadius = 0.06f;
        private const float HighlightInset = 0.14f;
        private const float HighlightWideWidth = 0.11f;
        private const float HighlightGap = 0.08f;
        private const float HighlightThinWidth = 0.05f;

        private const float GlyphHeight = 0.46f;
        private const float GlyphWidth = 0.25f;
        private const float GlyphStroke = 0.09f;
        private const float GlyphGap = 0.06f;
        private const float MarkWidth = 0.23f;
        private const float MarkHeight = 0.31f;
        private const float ShadowOffset = 0.05f;
        private const int DecimalBase = 10;
        private const int MaxDigits = 3;

        // Биты сегментов цифры: 0 верх, 1 левый верх, 2 правый верх, 3 середина, 4 левый низ, 5 правый низ, 6 низ.
        private static readonly int[] DigitSegments = { 0x77, 0x24, 0x5D, 0x6D, 0x2E, 0x6B, 0x7B, 0x25, 0x7F, 0x6F };
        private static readonly int[] DigitsBuffer = new int[MaxDigits];

        public static void Draw(FlatMeshBuilder builder, PaletteSet palette, GameConfig config)
        {
            BoosterSpec[] boosters = config.boosters;
            float halfWidth = config.boosterWidth * 0.5f;
            float halfHeight = config.boosterHeight * 0.5f;
            for (int i = 0; i < boosters.Length; i++)
            {
                int multiplier = boosters[i].multiplier;
                DrawOne(builder, palette, multiplier, boosters[i].position.x, boosters[i].position.y, halfWidth, halfHeight);
            }
        }

        private static void DrawOne(FlatMeshBuilder builder, PaletteSet palette, int multiplier, float x, float y, float halfWidth, float halfHeight)
        {
            Color32 glass = GlassColor(palette, multiplier);
            Color32 cap = CapColor(palette, multiplier);
            glass.a = GlassAlpha;
            Color32 highlight = palette.hudText;
            highlight.a = HighlightAlpha;

            float left = x - halfWidth;
            float right = x + halfWidth;
            float bottom = y - halfHeight;
            float top = y + halfHeight;
            builder.AddRect(left, bottom, right, top, glass);

            float wideLeft = left + HighlightInset;
            float thinLeft = wideLeft + HighlightWideWidth + HighlightGap;
            builder.AddRect(wideLeft, bottom, wideLeft + HighlightWideWidth, top, highlight);
            builder.AddRect(thinLeft, bottom, thinLeft + HighlightThinWidth, top, highlight);

            builder.AddRoundedRect(left - CapOverhang, top - CapHeight, right + CapOverhang, top + CapHeight * 0.5f, CapRadius, cap);
            builder.AddRoundedRect(left - CapOverhang, bottom - CapHeight * 0.5f, right + CapOverhang, bottom + CapHeight, CapRadius, cap);

            AddLabel(builder, multiplier, x, y - ShadowOffset, cap);
            AddLabel(builder, multiplier, x, y, palette.hudText);
        }

        private static Color32 GlassColor(PaletteSet palette, int multiplier)
        {
            if (multiplier >= MultiplierHigh)
            {
                return palette.booster50;
            }
            return multiplier >= MultiplierMid ? palette.booster20 : palette.booster10;
        }

        private static Color32 CapColor(PaletteSet palette, int multiplier)
        {
            if (multiplier >= MultiplierHigh)
            {
                return palette.booster50Cap;
            }
            return multiplier >= MultiplierMid ? palette.booster20Cap : palette.booster10Cap;
        }

        /// <summary>Надпись «xN» с центром в точке (x, y).</summary>
        private static void AddLabel(FlatMeshBuilder builder, int multiplier, float x, float y, Color32 color)
        {
            int count = 0;
            int rest = multiplier;
            while (rest > 0 && count < MaxDigits)
            {
                DigitsBuffer[count] = rest % DecimalBase;
                rest /= DecimalBase;
                count++;
            }

            float width = MarkWidth + count * (GlyphWidth + GlyphGap);
            float cursor = x - width * 0.5f;
            AddMark(builder, cursor, y, color);
            cursor += MarkWidth + GlyphGap;
            for (int i = count - 1; i >= 0; i--)
            {
                AddDigit(builder, cursor, y, DigitsBuffer[i], color);
                cursor += GlyphWidth + GlyphGap;
            }
        }

        /// <summary>Знак умножения: две диагональные полосы.</summary>
        private static void AddMark(FlatMeshBuilder builder, float left, float y, Color32 color)
        {
            float right = left + MarkWidth;
            float bottom = y - GlyphHeight * 0.5f;
            float top = bottom + MarkHeight;
            float stroke = GlyphStroke * 0.5f;
            builder.AddQuad(left, bottom, left + stroke * 2f, bottom, right, top, right - stroke * 2f, top, color);
            builder.AddQuad(right - stroke * 2f, bottom, right, bottom, left + stroke * 2f, top, left, top, color);
        }

        private static void AddDigit(FlatMeshBuilder builder, float left, float y, int digit, Color32 color)
        {
            int mask = DigitSegments[digit];
            float right = left + GlyphWidth;
            float top = y + GlyphHeight * 0.5f;
            float bottom = y - GlyphHeight * 0.5f;
            float stroke = GlyphStroke;
            float halfStroke = stroke * 0.5f;
            float radius = halfStroke;
            AddSegment(builder, mask, 0, left, top - stroke, right, top, radius, color);
            AddSegment(builder, mask, 1, left, y - halfStroke, left + stroke, top, radius, color);
            AddSegment(builder, mask, 2, right - stroke, y - halfStroke, right, top, radius, color);
            AddSegment(builder, mask, 3, left, y - halfStroke, right, y + halfStroke, radius, color);
            AddSegment(builder, mask, 4, left, bottom, left + stroke, y + halfStroke, radius, color);
            AddSegment(builder, mask, 5, right - stroke, bottom, right, y + halfStroke, radius, color);
            AddSegment(builder, mask, 6, left, bottom, right, bottom + stroke, radius, color);
        }

        private static void AddSegment(FlatMeshBuilder builder, int mask, int bit, float minX, float minY, float maxX, float maxY, float radius, Color32 color)
        {
            if ((mask & (1 << bit)) != 0)
            {
                builder.AddRoundedRect(minX, minY, maxX, maxY, radius, color);
            }
        }
    }
}
