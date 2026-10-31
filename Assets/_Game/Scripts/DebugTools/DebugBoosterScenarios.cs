#if GAME_DEBUG
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;

namespace CastleDestroyers.DebugTools
{
    /// <summary>
    /// Сценарии проверки бустеров (ROADMAP, этап 4): после рестарта игрок сам стреляет по данным из debugBoosterShots.
    /// Вырезается без GAME_DEBUG.
    /// </summary>
    public sealed class DebugBoosterScenarios : ITickable
    {
        private const int NoScenario = -1;

        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private int pending = NoScenario;
        private int salvosLeft;

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
        }

        /// <summary>Запоминает сценарий: выстрел случится в первый же Aim игрока.</summary>
        public void Run(int scenario)
        {
            pending = scenario;
            salvosLeft = 0;
        }

        public void Tick(float dt)
        {
            bool playerShooting = flow.ActiveSide == Side.Player && (flow.State == SessionState.Fire || flow.State == SessionState.Flight);
            if (salvosLeft > 0 && playerShooting && !cannons.IsFiring)
            {
                salvosLeft--;
                cannons.BeginSalvo(Side.Player);
            }
            if (pending == NoScenario || flow.State != SessionState.Aim || flow.ActiveSide != Side.Player)
            {
                return;
            }
            TurnHint shot = config.debugBoosterShots[pending];
            salvosLeft = config.debugBoosterShotSalvos[pending] - 1;
            pending = NoScenario;
            cannons.SetAim(Side.Player, shot.handAngleDeg, shot.handPower01);
            flow.Skip();
        }
    }
}
#endif
