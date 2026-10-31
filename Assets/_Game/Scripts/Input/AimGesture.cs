using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.InputRig
{
    /// <summary>
    /// Жест «оттянуть» (ТЗ §3.2) без обращений к устройству: развёрнутый вектор касания даёт угол, его длина — силу.
    /// Координаты экранные, длина считается в долях высоты экрана.
    /// </summary>
    public sealed class AimGesture
    {
        private GameConfig config;
        private float startX;
        private float startY;
        private float height;

        public bool Active { get; private set; }
        /// <summary>Палец вышел за мёртвую зону: отпускание даст выстрел.</summary>
        public bool Armed { get; private set; }
        public float AngleDeg { get; private set; }
        public float Power01 { get; private set; }

        public void Init(GameConfig gameConfig)
        {
            config = gameConfig;
        }

        public void Begin(float x, float y, float screenHeight)
        {
            startX = x;
            startY = y;
            height = screenHeight;
            Active = true;
            Armed = false;
        }

        public void Move(float x, float y)
        {
            float dragX = (x - startX) / height;
            float dragY = (y - startY) / height;
            float length = Mathf.Sqrt(dragX * dragX + dragY * dragY);
            Armed = length >= config.aimDeadZone;
            if (Armed)
            {
                SetDirection(-dragX, -dragY);
                Power01 = Mathf.Clamp01(length / config.aimMaxDragScreenFraction);
            }
        }

        /// <summary>Режим «направление на палец» (D-017): угол на палец, сила постоянная.</summary>
        public void Point(float directionX, float directionY)
        {
            Armed = true;
            SetDirection(directionX, directionY);
            Power01 = config.aimPointPower;
        }

        /// <summary>Завершает жест. True — нужен выстрел.</summary>
        public bool Release()
        {
            bool fire = Active && Armed;
            Cancel();
            return fire;
        }

        public void Cancel()
        {
            Active = false;
            Armed = false;
        }

        private void SetDirection(float directionX, float directionY)
        {
            AngleDeg = Mathf.Clamp(Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg, config.aimMinDeg, config.aimMaxDeg);
        }
    }
}
