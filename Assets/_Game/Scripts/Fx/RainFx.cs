using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Fx
{
    /// <summary>Дождь из ролика (ТЗ §3.8): плоские капли из общего пула сверху кадра. Выключен флагом rainEnabled.</summary>
    public sealed class RainFx : ITickable
    {
        private const float DropLifeMargin = 1.2f;

        private GameConfig config;
        private FlatParticles particles;
        private Camera cam;
        private Rng rng;
        private Color32 color;
        private float debt;

        public void Init(GameConfig gameConfig, FlatParticles flatParticles, Camera camera, Rng random)
        {
            config = gameConfig;
            particles = flatParticles;
            cam = camera;
            rng = random;
        }

        public void ApplyPalette(PaletteSet palette)
        {
            color = palette.waterRim;
        }

        public void Tick(float dt)
        {
            if (!config.rainEnabled)
            {
                return;
            }
            debt += config.rainPerSecond * dt;
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 center = cam.transform.position;
            float life = (center.y + halfHeight - config.waterLevel) / config.rainSpeed * DropLifeMargin;
            while (debt >= 1f)
            {
                debt -= 1f;
                float x = center.x + rng.Range(-halfWidth, halfWidth);
                particles.Spawn(x, center.y + halfHeight, 0f, -config.rainSpeed, config.rainDropSize, life, 0f, color);
            }
        }
    }
}
