using CastleDestroyers.CastleRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;

namespace CastleDestroyers.Fx
{
    /// <summary>
    /// Праздник победителя (ТЗ §3.11): пушки подпрыгивают со сдвигом фаз, над замком залпы фейерверка.
    /// Начинается, когда камера перешла с проигравшего на победителя.
    /// </summary>
    public sealed class VictoryShow : ITickable
    {
        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private CastleSystem castles;
        private LevelFx fx;
        private Rng rng;
        private bool playing;
        private Side celebrating;
        private int burstsLeft;
        private float nextBurst;

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem, CastleSystem castleSystem, LevelFx levelFx, Rng random)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
            castles = castleSystem;
            fx = levelFx;
            rng = random;
            flow.StateChanged += OnStateChanged;
        }

        public void Tick(float dt)
        {
            if (!playing)
            {
                return;
            }
            float time = flow.StateTime - config.victoryLoserHold;
            if (time <= 0f)
            {
                return;
            }
            cannons.Celebrate(celebrating, time);
            if (burstsLeft > 0 && time >= nextBurst)
            {
                burstsLeft--;
                nextBurst = time + config.fireworkInterval;
                Burst();
            }
        }

        private void Burst()
        {
            CastleBody body = castles.Body(celebrating);
            float x = body.CenterX + rng.Range(-config.fireworkSpread, config.fireworkSpread);
            fx.Firework(x, body.PivotY + body.Height + config.fireworkLift);
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            bool ending = state == SessionState.Victory || state == SessionState.Defeat;
            if (ending)
            {
                celebrating = flow.Winner;
                burstsLeft = config.fireworkBursts;
                nextBurst = 0f;
            }
            else if (playing)
            {
                cannons.Celebrate(celebrating, 0f);
            }
            playing = ending;
        }
    }
}
