using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Геометрия траектории ракеты. Волна «змейки» задана длиной в метрах пути, а не временем: иначе у вершины дуги,
    /// где ракета медленнее, волны сжимаются и синусоида выглядит неровной (D-033).
    /// </summary>
    public static class RocketPath
    {
        private const float MinHorizontalSpeedSq = 0.01f;

        /// <summary>
        /// Первообразная скорости по вертикальной скорости u; horizontalSpeedSq — квадрат горизонтальной скорости.
        /// Значение для начальной скорости считается один раз при запуске ракеты.
        /// </summary>
        public static float SpeedIntegral(float horizontalSpeedSq, float u)
        {
            float a = horizontalSpeedSq < MinHorizontalSpeedSq ? MinHorizontalSpeedSq : horizontalSpeedSq;
            float root = Mathf.Sqrt(a + u * u);
            return 0.5f * (u * root + a * Mathf.Log(u + root));
        }

        /// <summary>Длина параболы от старта до момента t, метры.</summary>
        public static float ArcLength(float velX, float velY, float gravity, float t)
        {
            float horizontalSq = velX * velX;
            return (SpeedIntegral(horizontalSq, velY) - SpeedIntegral(horizontalSq, velY - gravity * t)) / gravity;
        }
    }
}
