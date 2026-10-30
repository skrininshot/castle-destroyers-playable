using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>Контур ландшафта как ломаная «x → высота». Нужен greybox-уровню, камере и посадке замков.</summary>
    public static class TerrainProfile
    {
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
