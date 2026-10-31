using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.CameraRig
{
    /// <summary>Тряска камеры (ТЗ §3.9): импульсы складываются, но амплитуда насыщается на shakeMax.</summary>
    public sealed class ScreenShake : ITickable
    {
        private const float SecondAxisRatio = 1.37f;

        private GameConfig config;
        private float phase;

        public float Amplitude { get; private set; }
        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }

        public void Init(GameConfig gameConfig)
        {
            config = gameConfig;
        }

        public void Add(float amount)
        {
            Amplitude = Mathf.Min(Amplitude + amount, config.shakeMax);
        }

        public void Clear()
        {
            Amplitude = 0f;
            OffsetX = 0f;
            OffsetY = 0f;
        }

        public void Tick(float dt)
        {
            Amplitude *= Mathf.Exp(-config.shakeDecay * dt);
            phase += config.shakeFrequency * dt;
            OffsetX = Amplitude * Mathf.Sin(phase);
            OffsetY = Amplitude * Mathf.Sin(phase * SecondAxisRatio);
        }
    }
}
