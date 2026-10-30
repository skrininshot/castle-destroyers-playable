using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>Контур ландшафта как ломаная «x → высота». Нужен greybox-уровню, камере и посадке замков.</summary>
    public static class TerrainProfile
    {
        private const int MinSamplesPerSpan = 1;

        /// <summary>
        /// Плотный гладкий контур из опорных точек с шагом около step метров. Монотонный кубический сплайн
        /// проходит через все опорные точки и не даёт выбросов; вертикальные участки остаются острыми.
        /// Вызывается при Init, в кадре не используется.
        /// </summary>
        public static Vector2[] Smooth(Vector2[] control, float step)
        {
            int count = control.Length;
            float[] tangents = new float[count];
            ComputeTangents(control, tangents);

            int total = 1;
            for (int i = 0; i < count - 1; i++)
            {
                total += SpanSamples(control[i], control[i + 1], step);
            }

            Vector2[] dense = new Vector2[total];
            dense[0] = control[0];
            int index = 1;
            for (int i = 0; i < count - 1; i++)
            {
                int samples = SpanSamples(control[i], control[i + 1], step);
                for (int s = 1; s <= samples; s++)
                {
                    dense[index] = Hermite(control[i], control[i + 1], tangents[i], tangents[i + 1], (float)s / samples);
                    index++;
                }
            }
            return dense;
        }

        private static int SpanSamples(Vector2 from, Vector2 to, float step)
        {
            float dx = to.x - from.x;
            if (dx <= 0f)
            {
                return MinSamplesPerSpan;
            }
            float dy = to.y - from.y;
            int samples = Mathf.CeilToInt(Mathf.Sqrt(dx * dx + dy * dy) / step);
            return samples < MinSamplesPerSpan ? MinSamplesPerSpan : samples;
        }

        /// <summary>Наклоны по Фритчу — Батленду: на экстремумах ноль, иначе взвешенное гармоническое среднее.</summary>
        private static void ComputeTangents(Vector2[] control, float[] tangents)
        {
            int last = control.Length - 1;
            for (int i = 0; i <= last; i++)
            {
                bool hasLeft = i > 0 && control[i].x > control[i - 1].x;
                bool hasRight = i < last && control[i + 1].x > control[i].x;
                float left = hasLeft ? Slope(control[i - 1], control[i]) : 0f;
                float right = hasRight ? Slope(control[i], control[i + 1]) : 0f;
                if (!hasLeft || !hasRight)
                {
                    tangents[i] = hasLeft ? left : right;
                }
                else if (left * right <= 0f)
                {
                    tangents[i] = 0f;
                }
                else
                {
                    float leftWidth = control[i].x - control[i - 1].x;
                    float rightWidth = control[i + 1].x - control[i].x;
                    float weightLeft = 2f * rightWidth + leftWidth;
                    float weightRight = rightWidth + 2f * leftWidth;
                    tangents[i] = (weightLeft + weightRight) / (weightLeft / left + weightRight / right);
                }
            }
        }

        private static float Slope(Vector2 from, Vector2 to)
        {
            return (to.y - from.y) / (to.x - from.x);
        }

        private static Vector2 Hermite(Vector2 from, Vector2 to, float tangentFrom, float tangentTo, float t)
        {
            float width = to.x - from.x;
            if (width <= 0f)
            {
                return Vector2.Lerp(from, to, t);
            }
            float t2 = t * t;
            float t3 = t2 * t;
            float y = (2f * t3 - 3f * t2 + 1f) * from.y
                + (t3 - 2f * t2 + t) * width * tangentFrom
                + (3f * t2 - 2f * t3) * to.y
                + (t3 - t2) * width * tangentTo;
            return new Vector2(from.x + width * t, y);
        }

        /// <summary>
        /// Высота контура в точке x. Вне профиля — высота крайней точки.
        /// На вертикальном участке (две точки с одним x) возвращается верхняя из них.
        /// </summary>
        public static float HeightAt(Vector2[] profile, float x)
        {
            if (profile == null || profile.Length == 0)
            {
                return 0f;
            }
            if (x <= profile[0].x)
            {
                return profile[0].y;
            }
            int last = profile.Length - 1;
            if (x >= profile[last].x)
            {
                return profile[last].y;
            }

            float best = float.NegativeInfinity;
            for (int i = 0; i < last; i++)
            {
                float x0 = profile[i].x;
                float x1 = profile[i + 1].x;
                if (x < x0 || x > x1)
                {
                    continue;
                }
                float y0 = profile[i].y;
                float y1 = profile[i + 1].y;
                float height;
                if (x1 - x0 <= 0f)
                {
                    height = y0 > y1 ? y0 : y1;
                }
                else
                {
                    height = y0 + (y1 - y0) * (x - x0) / (x1 - x0);
                }
                if (height > best)
                {
                    best = height;
                }
            }
            return best;
        }
    }
}
