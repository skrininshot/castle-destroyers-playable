using CastleDestroyers.CameraRig;
using CastleDestroyers.CastleRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Platform;

namespace CastleDestroyers.Fx
{
    /// <summary>
    /// Конец матча (ТЗ §3.11, D-077). Один мощный взрыв уносит всё, что осталось от замка проигравшего, платформу и гусеницы,
    /// и оставляет большой кратер: вспышка, ударная волна, замедление. Когда камера перешла на победителя, его пушки подпрыгивают,
    /// над замком залпы фейерверка, у победившего игрока ещё и хлопушки конфетти.
    /// </summary>
    public sealed class VictoryShow : ITickable
    {

        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private CastleSystem castles;
        private ExplosionSystem explosions;
        private ScreenShake shake;
        private LevelFx fx;
        private FinaleFx finale;
        private Rng rng;
        private bool playing;
        private Side celebrating;
        private Side burning;
        private bool vaporized;
        private bool cheering;
        private int burstsLeft;
        private float nextBurst;
        private float slowLeft;
        private float restoreScale;

        /// <summary>Цикл игры: замедление идёт через его TimeScale. В тестах не задан.</summary>
        public GameLoop Loop { get; set; }
        /// <summary>Замок проигравшего уже уничтожен целиком.</summary>
        public bool Vaporized { get { return vaporized; } }

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem, CastleSystem castleSystem, ExplosionSystem explosionSystem,
            ScreenShake screenShake, LevelFx levelFx, FinaleFx finaleFx, Rng random)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
            castles = castleSystem;
            explosions = explosionSystem;
            shake = screenShake;
            fx = levelFx;
            finale = finaleFx;
            rng = random;
            flow.StateChanged += OnStateChanged;
        }

        public void Tick(float dt)
        {
            TickSlowMotion(dt);
            if (!playing)
            {
                return;
            }
            CastleBody body = castles.Body(burning);
            if (!vaporized && body.State != CastleState.Gone && flow.StateTime >= config.finaleBlastDelay)
            {
                Vaporize(body);
            }
            float time = flow.StateTime - config.victoryLoserHold - config.victoryCheerDelay;
            if (time <= 0f)
            {
                return;
            }
            if (!cheering)
            {
                cheering = true;
                Cheer();
            }
            cannons.Celebrate(celebrating, time);
            if (burstsLeft > 0 && time >= nextBurst)
            {
                burstsLeft--;
                nextBurst = time + config.fireworkInterval;
                Burst();
            }
        }

        private void Vaporize(CastleBody body)
        {
            vaporized = true;
            float groundY = body.PivotY - config.trackHeight;
            castles.Vaporize(burning);
            explosions.Detonate(body.CenterX, groundY + config.finaleBlastLift, config.finaleBlastPower, config.finaleBlastLife,
                groundY + config.finaleCraterLift, config.finaleCraterRadius);
            fx.Wreckage(body.CenterX, body.PivotY, body.PlatformHalf * 2f, config.finaleDebrisPieces);
            fx.Play(SoundId.Blast);
            finale.Flash();
            finale.Shockwave(body.CenterX, groundY + config.finaleBlastLift);
            shake.Add(config.finaleShake);
            StartSlowMotion();
        }

        private void Cheer()
        {
            if (celebrating == Side.Player)
            {
                fx.Play(SoundId.Victory);
                finale.ConfettiCannons();
            }
        }

        private void Burst()
        {
            CastleBody body = castles.Body(celebrating);
            float x = body.CenterX + rng.Range(-config.fireworkSpread, config.fireworkSpread);
            fx.Firework(x, body.PivotY + body.Height + config.fireworkLift);
        }

        private void StartSlowMotion()
        {
            if (Loop == null || config.finaleSlowTime <= 0f)
            {
                return;
            }
            restoreScale = Loop.TimeScale;
            Loop.TimeScale = restoreScale * config.finaleSlowScale;
            shake.Speed = 1f / config.finaleSlowScale;
            slowLeft = config.finaleSlowTime;
        }

        /// <summary>dt уже замедлен: длительность считается в реальном времени.</summary>
        private void TickSlowMotion(float dt)
        {
            if (slowLeft <= 0f)
            {
                return;
            }
            slowLeft -= dt / Loop.TimeScale;
            if (slowLeft <= 0f)
            {
                StopSlowMotion();
            }
        }

        private void StopSlowMotion()
        {
            if (slowLeft != 0f && Loop != null)
            {
                Loop.TimeScale = restoreScale;
                shake.Speed = 1f;
            }
            slowLeft = 0f;
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            bool ending = state == SessionState.Victory || state == SessionState.Defeat;
            if (ending)
            {
                celebrating = flow.Winner;
                burning = flow.Loser;
                vaporized = false;
                cheering = false;
                burstsLeft = config.fireworkBursts;
                nextBurst = 0f;
            }
            else
            {
                if (playing)
                {
                    cannons.Celebrate(celebrating, 0f);
                }
                StopSlowMotion();
            }
            playing = ending;
        }
    }
}
