using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>Формулы параболы (ТЗ §3.2, D-034): общие для ракет, пунктира и тестов.</summary>
    public static class Ballistics
    {
        /// <summary>Скорость вылета при силе прицела 0..1.</summary>
        public static float Speed(float power01, float speedMin, float speedMax)
        {
            return speedMin + (speedMax - speedMin) * Mathf.Clamp01(power01);
        }

        /// <summary>Когда ракета опустится до высоты targetY, секунды. Если высота недостижима — 0.</summary>
        public static float TimeToHeight(float startY, float speedY, float gravity, float targetY)
        {
            float discriminant = speedY * speedY + 2f * gravity * (startY - targetY);
            if (discriminant < 0f)
            {
                return 0f;
            }
            return (speedY + Mathf.Sqrt(discriminant)) / gravity;
        }
    }
}
