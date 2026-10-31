using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Вид бустеров (ТЗ §3.5): спрайты атласа и поп-апы «+N» из глифов в одном меше. Форма строится при смене темы, в кадре
    /// пишутся только позиции вершин: «дыхание», сквош и цифры. Поп-ап тает размером, цвета в кадре не меняются (ТЗ §4.1).
    /// </summary>
    public sealed class BoosterView : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int PopupPasses = 2;
        private const int PopupDigits = 5;
        private const int PopupQuads = 1 + PopupDigits;
        private const int MaxPopupValue = 99999;
        private const int DecimalBase = 10;
        private const int MultiplierMid = 20;
        private const int MultiplierHigh = 50;
        private const float BoundsSize = 500f;
        private const float ShadowShare = 0.09f;

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly int[] digits = new int[PopupDigits];
        private GameConfig config;
        private BoosterSystem boosters;
        private int[] bodyStart;
        private int[] bodyEnd;
        private int[] popupStart;
        private bool[] popupShown;
        private int[] popupValue;
        private Vector3[] baseVertices;
        private Vector3[] vertices;
        private Vector2[] uvs;
        private bool uvDirty;
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
            popupValue = new int[count];
            Mesh = new Mesh();
            Mesh.name = "Boosters";
            Mesh.MarkDynamic();
        }

        public static Color32 GlassColor(PaletteSet palette, int multiplier)
        {
            if (multiplier >= MultiplierHigh)
            {
                return palette.booster50;
            }
            return multiplier >= MultiplierMid ? palette.booster20 : palette.booster10;
        }

        public static Color32 CapColor(PaletteSet palette, int multiplier)
        {
            if (multiplier >= MultiplierHigh)
            {
                return palette.booster50Cap;
            }
            return multiplier >= MultiplierMid ? palette.booster20Cap : palette.booster10Cap;
        }

        private static int BodySprite(int multiplier)
        {
            if (multiplier >= MultiplierHigh)
            {
                return ArtSprites.Booster50;
            }
            return multiplier >= MultiplierMid ? ArtSprites.Booster20 : ArtSprites.Booster10;
        }

        /// <summary>Перестраивает меш в цветах палитры. Первым вызовом создаёт буферы.</summary>
        public void ApplyPalette(PaletteSet palette)
        {
            builder.Clear();
            int count = boosters.Count;
            for (int k = 0; k < count; k++)
            {
                BoosterSpec spec = config.boosters[k];
                bodyStart[k] = builder.VertexCount;
                builder.AddSprite(BodySprite(spec.multiplier), spec.position.x, spec.position.y, config.boosterWidth / ArtBoosterWidth, 0f, 0f, config.boosterHeight / ArtBoosterHeight, Color.white);
                bodyEnd[k] = builder.VertexCount;
            }
            for (int k = 0; k < count; k++)
            {
                popupStart[k] = builder.VertexCount;
                AddPopupQuads(CapColor(palette, config.boosters[k].multiplier));
                AddPopupQuads(palette.hudText);
            }
            builder.Apply(Mesh);
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
            if (baseVertices == null)
            {
                baseVertices = new Vector3[builder.VertexCount];
                vertices = new Vector3[builder.VertexCount];
                uvs = new Vector2[builder.VertexCount];
            }
            builder.CopyVertices(baseVertices);
            builder.CopyVertices(vertices);
            builder.CopyUvs(uvs);
            System.Array.Clear(popupShown, 0, popupShown.Length);
            System.Array.Clear(popupValue, 0, popupValue.Length);
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
            if (uvDirty)
            {
                uvDirty = false;
                Mesh.uv = uvs;
            }
        }

        // Размер бустера, под который нарисован спрайт, метры.
        private const float ArtBoosterWidth = 1.5f;
        private const float ArtBoosterHeight = 3f;

        private void AddPopupQuads(Color32 color)
        {
            for (int q = 0; q < PopupQuads; q++)
            {
                builder.AddSprite(q == 0 ? ArtSprites.GlyphPlus : ArtSprites.Glyph0, 0f, 0f, 0f, color);
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
            float homeY = spec.position.y;
            float centerY = homeY + boosters.ShiftY(k);
            Vector3[] source = baseVertices;
            Vector3[] target = vertices;
            int end = bodyEnd[k];
            for (int v = bodyStart[k]; v < end; v++)
            {
                target[v].x = centerX + (source[v].x - centerX) * scaleX;
                target[v].y = centerY + (source[v].y - homeY) * scaleY;
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
                    popupValue[k] = 0;
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
            float y = boosters.CenterY(k) + config.boosterHeight * 0.5f + config.boosterPopupOffsetY + config.boosterPopupRise * age;

            int digitCount = SplitDigits(value);
            float width = TextWidth(digitCount) * height;
            float left = spec.position.x - width * 0.5f;
            int first = popupStart[k];
            int shadow = first + PopupQuads * VerticesPerQuad;
            if (popupValue[k] != value)
            {
                popupValue[k] = value;
                WriteGlyphUvs(first, digitCount);
                WriteGlyphUvs(shadow, digitCount);
                uvDirty = true;
            }
            WriteGlyphs(first, digitCount, left, y - height * ShadowShare, height);
            WriteGlyphs(shadow, digitCount, left, y, height);
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

        private static float GlyphAspect(int sprite)
        {
            return ArtSprites.Rect[sprite * VerticesPerQuad + 2];
        }

        /// <summary>Ширина строки «+digits» в долях высоты.</summary>
        private float TextWidth(int digitCount)
        {
            float width = GlyphAspect(ArtSprites.GlyphPlus);
            for (int i = 0; i < digitCount; i++)
            {
                width += GlyphAspect(ArtSprites.Glyph0 + digits[i]);
            }
            return width;
        }

        private void WriteGlyphUvs(int vertex, int digitCount)
        {
            int v = vertex + VerticesPerQuad;
            for (int i = 0; i < digitCount; i++)
            {
                int r = (ArtSprites.Glyph0 + digits[i]) * VerticesPerQuad;
                float[] source = ArtSprites.Uv;
                uvs[v].x = source[r];
                uvs[v].y = source[r + 1];
                uvs[v + 1].x = source[r + 2];
                uvs[v + 1].y = source[r + 1];
                uvs[v + 2].x = source[r + 2];
                uvs[v + 2].y = source[r + 3];
                uvs[v + 3].x = source[r];
                uvs[v + 3].y = source[r + 3];
                v += VerticesPerQuad;
            }
        }

        private void WriteGlyphs(int vertex, int digitCount, float left, float bottom, float height)
        {
            float cursor = left + GlyphAspect(ArtSprites.GlyphPlus) * height;
            SetQuad(vertex, left, bottom, cursor, bottom + height);
            int v = vertex + VerticesPerQuad;
            for (int i = 0; i < PopupDigits; i++)
            {
                if (i < digitCount)
                {
                    float next = cursor + GlyphAspect(ArtSprites.Glyph0 + digits[i]) * height;
                    SetQuad(v, cursor, bottom, next, bottom + height);
                    cursor = next;
                }
                else
                {
                    SetQuad(v, 0f, 0f, 0f, 0f);
                }
                v += VerticesPerQuad;
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
