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

        /// <summary>На сколько дуга поднимается над точкой вылета, метры.</summary>
        public static float ApexHeight(float speedY, float gravity)
        {
            return speedY > 0f ? speedY * speedY / (2f * gravity) : 0f;
        }

        /// <summary>Высота дуги через deltaX метров по горизонтали. Знак deltaX совпадает со знаком speedX, speedX не ноль.</summary>
        public static float HeightAtDistance(float startY, float speedX, float speedY, float gravity, float deltaX)
        {
            float time = deltaX / speedX;
            return startY + speedY * time - 0.5f * gravity * time * time;
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
