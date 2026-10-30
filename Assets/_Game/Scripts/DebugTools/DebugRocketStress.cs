#if GAME_DEBUG
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.DebugTools
{
    /// <summary>
    /// Стресс-режим «1000 ракет в гору»: настоящий RocketSystem держит буфер заполненным, бюджет поднят до stressRocketCount.
    /// Все ракеты идут по одной дуге из креплений синих (D-031). Вырезается без GAME_DEBUG.
    /// </summary>
    public sealed class DebugRocketStress : ITickable
    {
        private GameConfig config;
        private GreyboxLevel level;
        private RocketSystem rockets;
        private int nextMount;

        public bool Active { get; private set; }

        public void Init(GameConfig gameConfig, GreyboxLevel greyboxLevel, RocketSystem rocketSystem)
        {
            config = gameConfig;
            level = greyboxLevel;
            rockets = rocketSystem;
        }

        public void SetActive(bool active)
        {
            Active = active;
            rockets.Budget = active ? config.stressRocketCount : config.maxActiveRockets;
        }

        public void Tick(float dt)
        {
            if (!Active)
            {
                return;
            }
            float radians = config.stressRocketAngleDeg * Mathf.Deg2Rad;
            float speedX = config.stressRocketSpeed * Mathf.Cos(radians);
            float speedY = config.stressRocketSpeed * Mathf.Sin(radians);
            float[] mountX = level.PlayerMountX;
            float[] mountY = level.PlayerMountY;
            for (int n = 0; n < config.stressSpawnPerFrame; n++)
            {
                nextMount = (nextMount + 1) % mountX.Length;
                if (!rockets.Spawn(mountX[nextMount], mountY[nextMount], speedX, speedY, Side.Player, 0f))
                {
                    break;
                }
            }
        }
    }
}
#endif
