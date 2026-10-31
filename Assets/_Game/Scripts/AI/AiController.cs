using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;

namespace CastleDestroyers.AI
{
    /// <summary>
    /// ИИ (ТЗ §5.2): на своём ходу берёт выстрел из пресетов, добавляет шум сложности и «думает»: пушки сначала
    /// идут на ложный угол и лишь затем на настоящий. Пресет на ход задан порядком aiPresetOrder.
    /// </summary>
    public sealed class AiController : ITickable
    {
        private const float DecoySideChance = 0.5f;

        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private Rng rng;
        private float finalAngle;
        private float finalPower;
        private bool settled = true;

        /// <summary>Выстрел ИИ на этот ход, градусы.</summary>
        public float FinalAngle { get { return finalAngle; } }
        public float FinalPower { get { return finalPower; } }

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem, Rng random)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
            rng = random;
            flow.StateChanged += OnStateChanged;
        }

        /// <summary>Пресет хода ИИ; turn считается с единицы, порядок идёт по кругу.</summary>
        public static AiShotPreset PickPreset(GameConfig gameConfig, int turn)
        {
            int[] order = gameConfig.aiPresetOrder;
            int step = turn < 1 ? 0 : turn - 1;
            return gameConfig.aiPresets[order[step % order.Length]];
        }

        public void Tick(float dt)
        {
            if (!settled && flow.State == SessionState.Aim && flow.ActiveSide == Side.Enemy
                && flow.StateTime >= config.aiAimDuration * config.aiDecoyShare)
            {
                Settle();
            }
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            if (side != Side.Enemy)
            {
                return;
            }
            if (state == SessionState.Aim)
            {
                Choose();
            }
            else if (state == SessionState.Fire && !settled)
            {
                Settle();
            }
        }

        private void Choose()
        {
            AiShotPreset preset = PickPreset(config, flow.PlayerTurn);
            finalAngle = preset.angleDeg + rng.Range(-config.aiAngleNoiseDeg, config.aiAngleNoiseDeg);
            finalPower = preset.power01 + rng.Range(-config.aiPowerNoise, config.aiPowerNoise);
            finalAngle = finalAngle < config.aimMinDeg ? config.aimMinDeg : (finalAngle > config.aimMaxDeg ? config.aimMaxDeg : finalAngle);
            finalPower = finalPower < 0f ? 0f : (finalPower > 1f ? 1f : finalPower);
            float decoy = rng.Chance(DecoySideChance) ? finalAngle + config.aiDecoyDeg : finalAngle - config.aiDecoyDeg;
            cannons.SetAim(Side.Enemy, decoy < config.aimMinDeg ? finalAngle + config.aiDecoyDeg : decoy, finalPower);
            settled = false;
        }

        private void Settle()
        {
            cannons.SetAim(Side.Enemy, finalAngle, finalPower);
            settled = true;
        }
    }
}
