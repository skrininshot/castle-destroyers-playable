using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Связывает FSM с боем: начинает залп при входе в Fire, держит Flight, пока ракеты в воздухе, и сообщает камере
    /// положение и высоту потока. Без жеста игрока стреляет по подсказке хода, последний залп усиливает Director.
    /// </summary>
    public sealed class TurnCombat : ITickable
    {
        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private RocketSystem rockets;
        private BoosterSystem boosters;
        private CameraDirector cameraDirector;
        private bool headPassed;
        private float centerHold;

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem, RocketSystem rocketSystem, BoosterSystem boosterSystem, CameraDirector director)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
            rockets = rocketSystem;
            boosters = boosterSystem;
            cameraDirector = director;
            flow.StateChanged += OnStateChanged;
        }

        public void Tick(float dt)
        {
            Side side = flow.ActiveSide;
            if (cannons.IsFiring && flow.Match.IsEliminated(side == Side.Player ? Side.Enemy : Side.Player))
            {
                cannons.StopSalvo(side);
            }
            flow.FlightBusy = cannons.IsFiring || rockets.ActiveCount > 0 || boosters.PendingCopies > 0;
            if (flow.State != SessionState.Flight || rockets.Count(side) == 0)
            {
                return;
            }
            float headX = rockets.HeadX(side);
            float facing = side == Side.Player ? 1f : -1f;
            float topY = rockets.TopY(side);
            cameraDirector.FlightHeadX = headX;
            if (topY > cameraDirector.FlightTopY)
            {
                cameraDirector.FlightTopY = topY;
            }
            if ((headX - config.centerFocus.x) * facing > 0f)
            {
                headPassed = true;
            }
            if (headPassed)
            {
                centerHold += dt;
                cameraDirector.HeadPassedCenter = centerHold >= config.cameraCenterHold;
            }
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            if (state == SessionState.Aim && side == Side.Player)
            {
                cannons.ClearAim(Side.Player);
            }
            else if (state == SessionState.Fire)
            {
                if (!cannons.HasAim(side))
                {
                    TurnHint hint = flow.CurrentHint;
                    cannons.SetAim(side, hint.handAngleDeg, hint.handPower01);
                }
                cameraDirector.HeadPassedCenter = false;
                cameraDirector.FlightHeadX = config.centerFocus.x;
                cameraDirector.FlightTopY = cannons.SalvoApexY(side);
                headPassed = false;
                centerHold = 0f;
                cannons.SetSalvoWeight(side, flow.IsFinisherTurn ? config.directorFinisherWeight : 1f);
                cannons.BeginSalvo(side);
            }
        }
    }
}
