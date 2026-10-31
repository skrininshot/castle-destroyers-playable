using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Вид бустеров (ТЗ §3.5): один меш с корпусами и поп-апами «+N». Форма строится при смене темы, в кадре пишутся
    /// только позиции вершин: «дыхание», сквош и цифры. Поп-ап тает размером, цвета в кадре не меняются (ТЗ §4.1).
    /// </summary>
    public sealed class BoosterView : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int PopupPasses = 2;
        private const int PopupDigits = 5;
        private const int MaxPopupValue = 99999;
        private const int DecimalBase = 10;
        private const int SegmentsPerDigit = 7;
        private const int PlusQuads = 2;
        private const int PopupQuads = PlusQuads + PopupDigits * SegmentsPerDigit;
        private const float BoundsSize = 500f;
        // Пропорции цифр в долях высоты.
        private const float GlyphWidth = 0.56f;
        private const float GlyphStroke = 0.2f;
        private const float GlyphGap = 0.16f;
        private const float PlusSize = 0.6f;
        private const float ShadowShare = 0.09f;

        // Сегменты цифры на единичной высоте (minX, minY, maxX, maxY); порядок как в GreyboxBoosters.
        private static readonly float[] Segments =
        {
            0f, 1f - GlyphStroke, GlyphWidth, 1f,
            0f, 0.5f - GlyphStroke * 0.5f, GlyphStroke, 1f,
            GlyphWidth - GlyphStroke, 0.5f - GlyphStroke * 0.5f, GlyphWidth, 1f,
            0f, 0.5f - GlyphStroke * 0.5f, GlyphWidth, 0.5f + GlyphStroke * 0.5f,
            0f, 0f, GlyphStroke, 0.5f + GlyphStroke * 0.5f,
            GlyphWidth - GlyphStroke, 0f, GlyphWidth, 0.5f + GlyphStroke * 0.5f,
            0f, 0f, GlyphWidth, GlyphStroke
        };

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly int[] digits = new int[PopupDigits];
        private GameConfig config;
        private BoosterSystem boosters;
        private int[] bodyStart;
        private int[] bodyEnd;
        private int[] popupStart;
        private bool[] popupShown;
        private Vector3[] baseVertices;
        private Vector3[] vertices;
        private float time;

        public Mesh Mesh { get; private set; }
        /// <summary>Источник подсветки бустера-подсказки.</summary>
        public UI.TurnGuide Guide { get; set; }

        public void Init(GameConfig gameConfig, BoosterSystem boosterSystem)
        {
            config = gameConfig;
            boosters = boosterSystem;
            int count = boosters.Count;
            bodyStart = new int[count];
            bodyEnd = new int[count];
            popupStart = new int[count];
            popupShown = new bool[count];
            Mesh = new Mesh();
            Mesh.name = "Boosters";
            Mesh.MarkDynamic();
        }

        /// <summary>Перестраивает меш в цветах палитры. Первым вызовом создаёт буферы.</summary>
        public void ApplyPalette(PaletteSet palette)
        {
            builder.Clear();
            int count = boosters.Count;
            for (int k = 0; k < count; k++)
            {
                bodyStart[k] = builder.VertexCount;
                GreyboxBoosters.Draw(builder, palette, config, k);
                bodyEnd[k] = builder.VertexCount;
            }
            for (int k = 0; k < count; k++)
            {
                popupStart[k] = builder.VertexCount;
                AddPopupQuads(GreyboxBoosters.CapColor(palette, config.boosters[k].multiplier));
                AddPopupQuads(palette.hudText);
            }
            builder.Apply(Mesh);
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
            if (baseVertices == null)
            {
                baseVertices = new Vector3[builder.VertexCount];
                vertices = new Vector3[builder.VertexCount];
            }
            builder.CopyVertices(baseVertices);
            builder.CopyVertices(vertices);
            System.Array.Clear(popupShown, 0, popupShown.Length);
        }

        public void Tick(float dt)
        {
            time += dt;
            for (int k = 0; k < bodyStart.Length; k++)
            {
                WriteBody(k);
                WritePopup(k);
            }
            Mesh.vertices = vertices;
        }

        private void AddPopupQuads(Color32 color)
        {
            for (int q = 0; q < PopupQuads; q++)
            {
                builder.AddRect(0f, 0f, 0f, 0f, color);
            }
        }

        private void WriteBody(int k)
        {
            BoosterSpec spec = config.boosters[k];
            float breath = config.boosterPulse * Mathf.Sin(time * config.boosterPulseSpeed + k);
            if (Guide != null && spec.multiplier == Guide.HighlightMultiplier)
            {
                breath += config.boosterHighlightPulse * (0.5f + 0.5f * Mathf.Sin(time * config.boosterHighlightSpeed));
            }
            float squash = boosters.Squash(k) * config.boosterSquash;
            float scaleX = 1f + breath + squash;
            float scaleY = 1f + breath - squash;
            float centerX = spec.position.x;
            float centerY = spec.position.y;
            Vector3[] source = baseVertices;
            Vector3[] target = vertices;
            int end = bodyEnd[k];
            for (int v = bodyStart[k]; v < end; v++)
            {
                target[v].x = centerX + (source[v].x - centerX) * scaleX;
                target[v].y = centerY + (source[v].y - centerY) * scaleY;
            }
        }

        private void WritePopup(int k)
        {
            int value = boosters.PopupValue(k);
            if (value <= 0)
            {
                if (popupShown[k])
                {
                    popupShown[k] = false;
                    int quads = PopupPasses * PopupQuads;
                    for (int q = 0; q < quads; q++)
                    {
                        SetQuad(popupStart[k] + q * VerticesPerQuad, 0f, 0f, 0f, 0f);
                    }
                }
                return;
            }
            popupShown[k] = true;

            float age = boosters.PopupAge(k);
            float hold = config.boosterPopupHold;
            float scale = age <= hold ? 1f : Mathf.Clamp01(1f - (age - hold) / config.boosterPopupFade);
            float height = config.boosterPopupHeight * scale;
            BoosterSpec spec = config.boosters[k];
            float y = spec.position.y + config.boosterHeight * 0.5f + config.boosterPopupOffsetY + config.boosterPopupRise * age;

            int digitCount = SplitDigits(value);
            float width = height * (PlusSize + GlyphGap + digitCount * (GlyphWidth + GlyphGap) - GlyphGap);
            float left = spec.position.x - width * 0.5f;
            int first = popupStart[k];
            WriteGlyphs(first, digitCount, left, y - height * ShadowShare, height);
            WriteGlyphs(first + PopupQuads * VerticesPerQuad, digitCount, left, y, height);
        }

        /// <summary>Раскладывает число в digits от старшего разряда, возвращает их количество.</summary>
        private int SplitDigits(int value)
        {
            int rest = value < MaxPopupValue ? value : MaxPopupValue;
            int count = 0;
            for (int probe = rest; probe > 0; probe /= DecimalBase)
            {
                count++;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                digits[i] = rest % DecimalBase;
                rest /= DecimalBase;
            }
            return count;
        }

        private void WriteGlyphs(int vertex, int digitCount, float left, float bottom, float height)
        {
            float stroke = height * GlyphStroke;
            float plus = height * PlusSize;
            float middleY = bottom + height * 0.5f;
            float middleX = left + plus * 0.5f;
            SetQuad(vertex, left, middleY - stroke * 0.5f, left + plus, middleY + stroke * 0.5f);
            SetQuad(vertex + VerticesPerQuad, middleX - stroke * 0.5f, middleY - plus * 0.5f, middleX + stroke * 0.5f, middleY + plus * 0.5f);

            int v = vertex + PlusQuads * VerticesPerQuad;
            float cursor = left + height * (PlusSize + GlyphGap);
            for (int i = 0; i < PopupDigits; i++)
            {
                int mask = i < digitCount ? GreyboxBoosters.DigitMask(digits[i]) : 0;
                for (int s = 0; s < SegmentsPerDigit; s++)
                {
                    if ((mask & (1 << s)) != 0)
                    {
                        int r = s * VerticesPerQuad;
                        SetQuad(v, cursor + Segments[r] * height, bottom + Segments[r + 1] * height,
                            cursor + Segments[r + 2] * height, bottom + Segments[r + 3] * height);
                    }
                    else
                    {
                        SetQuad(v, 0f, 0f, 0f, 0f);
                    }
                    v += VerticesPerQuad;
                }
                cursor += height * (GlyphWidth + GlyphGap);
            }
        }

        private void SetQuad(int v, float minX, float minY, float maxX, float maxY)
        {
            Vector3[] target = vertices;
            target[v].x = minX;
            target[v].y = minY;
            target[v + 1].x = maxX;
            target[v + 1].y = minY;
            target[v + 2].x = maxX;
            target[v + 2].y = maxY;
            target[v + 3].x = minX;
            target[v + 3].y = maxY;
        }
    }
}
