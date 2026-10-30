using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Связывает FSM с боем: начинает залп при входе в Fire, держит Flight, пока ракеты в воздухе, и сообщает камере
    /// положение головы потока. Без жеста игрока стреляет по подсказке хода. Угол ИИ — заглушка до этапа 6.
    /// </summary>
    public sealed class TurnCombat : ITickable
    {
        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private RocketSystem rockets;
        private CameraDirector cameraDirector;

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem, RocketSystem rocketSystem, CameraDirector director)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
            rockets = rocketSystem;
            cameraDirector = director;
            flow.StateChanged += OnStateChanged;
        }

        public void Tick(float dt)
        {
            flow.FlightBusy = cannons.IsFiring || rockets.ActiveCount > 0;
            Side side = flow.ActiveSide;
            if (flow.State != SessionState.Flight || rockets.Count(side) == 0)
            {
                return;
            }
            float headX = rockets.HeadX(side);
            float facing = side == Side.Player ? 1f : -1f;
            cameraDirector.FlightHeadX = headX;
            if ((headX - config.centerFocus.x) * facing > 0f)
            {
                cameraDirector.HeadPassedCenter = true;
            }
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            if (state == SessionState.Aim)
            {
                if (side == Side.Enemy)
                {
                    cannons.SetAim(Side.Enemy, config.stubAiAngleDeg, config.stubAiPower);
                }
                else
                {
                    cannons.ClearAim(Side.Player);
                }
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
                cannons.BeginSalvo(side);
            }
        }
    }
}
