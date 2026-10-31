using CastleDestroyers.CameraRig;
using CastleDestroyers.CastleRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Platform;

namespace CastleDestroyers.Fx
{
    /// <summary>
    /// Конец матча (ТЗ §3.11, D-075). Сначала замок проигравшего сгорает цепью взрывов сверху вниз, затем финальный взрыв уносит остатки,
    /// платформу и гусеницы: вспышка, ударная волна, замедление. Когда камера перешла на победителя, его пушки подпрыгивают,
    /// над замком залпы фейерверка, у победившего игрока ещё и хлопушки конфетти.
    /// </summary>
    public sealed class VictoryShow : ITickable
    {
        private const float CenterShare = 0.5f;
        private const float ChainTop = 1f;

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
        private int blastsDone;
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
            TickBurning(flow.StateTime);
            float time = flow.StateTime - config.victoryLoserHold;
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

        private void TickBurning(float time)
        {
            CastleBody body = castles.Body(burning);
            if (vaporized || body.State == CastleState.Gone)
            {
                return;
            }
            int blasts = config.finaleBlasts;
            while (blastsDone < blasts && time >= blastsDone * config.finaleBlastInterval)
            {
                Blast(body, blastsDone);
                blastsDone++;
            }
            if (blastsDone >= blasts && time >= blasts * config.finaleBlastInterval)
            {
                Vaporize(body);
            }
        }

        /// <summary>Взрыв цепи: высота идёт сверху вниз, по ширине случайно.</summary>
        private void Blast(CastleBody body, int index)
        {
            float localY = body.Height * (ChainTop - (index + CenterShare) / config.finaleBlasts);
            float localX = rng.Range(-body.HalfWidth, body.HalfWidth);
            float x = body.WorldX(localX, localY);
            float y = body.WorldY(localX, localY);
            castles.ScorchStone(burning, x, y, config.finaleBlastRadius);
            explosions.Explode(x, y, false, config.finaleBlastWeight);
        }

        private void Vaporize(CastleBody body)
        {
            vaporized = true;
            float centerY = body.PivotY + body.Height * CenterShare;
            float groundY = body.PivotY - config.trackHeight;
            float span = body.PlatformHalf * 2f;
            castles.Vaporize(burning);
            for (int i = 0; i < config.finaleCraters; i++)
            {
                float x = body.CenterX - body.PlatformHalf + span * (i + CenterShare) / config.finaleCraters;
                explosions.Explode(x, groundY, true, config.finaleCraterWeight);
            }
            explosions.Explode(body.CenterX, centerY, false, config.finaleBlastWeight);
            fx.Wreckage(body.CenterX, body.PivotY, span, config.finaleDebrisPieces);
            fx.Play(SoundId.Blast);
            finale.Flash();
            finale.Shockwave(body.CenterX, centerY);
            shake.Add(config.shakeMax);
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
                blastsDone = 0;
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
