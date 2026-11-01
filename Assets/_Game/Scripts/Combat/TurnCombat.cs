using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;

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
                // Защита держится и в ответный ход врага: куски замка могут отвалиться уже после конца полёта.
                if (side == Side.Player)
                {
                    bool spare = config.directorEnabled && flow.PlayerTurn <= config.directorEnemyKeepTurns && !flow.IsFinisherTurn;
                    cannons.SparedEnemyCannons = spare ? config.directorEnemyKeepCannons : 0;
                }
                NudgeBoosters(side);
                cannons.BeginSalvo(side);
            }
        }

        /// <summary>
        /// Подтяжка (D-069): если дуга залпа игрока едва проходит мимо бустера, он сдвигается под неё. Двигается ближайший
        /// к дуге бустер, иначе соседние съехались бы друг к другу. Остальные и все бустеры на ходу врага идут на своё место,
        /// под которое настроен ИИ. Что видно в кадре, не двигается.
        /// </summary>
        private void NudgeBoosters(Side side)
        {
            int nearest = -1;
            float nearestMiss = float.PositiveInfinity;
            if (side == Side.Player)
            {
                for (int k = 0; k < boosters.Count; k++)
                {
                    float miss = cannons.SalvoMissY(side, boosters.CenterX(k), boosters.HomeMinY(k), boosters.HomeMaxY(k));
                    if (Mathf.Abs(miss) < Mathf.Abs(nearestMiss))
                    {
                        nearest = k;
                        nearestMiss = miss;
                    }
                }
            }
            float halfWidth = config.boosterWidth * 0.5f;
            for (int k = 0; k < boosters.Count; k++)
            {
                float shift = k == nearest ? BoosterSystem.NudgeShift(nearestMiss, config.boosterNudgeReach, config.boosterNudgeInset) : 0f;
                float current = boosters.ShiftY(k);
                if (shift == current)
                {
                    continue;
                }
                float centerX = boosters.CenterX(k);
                float low = boosters.HomeMinY(k) + Mathf.Min(shift, current);
                float high = boosters.HomeMaxY(k) + Mathf.Max(shift, current);
                if (!cameraDirector.IsOnScreen(centerX - halfWidth, low, centerX + halfWidth, high, config.boosterNudgeScreenMargin))
                {
                    boosters.SetShiftY(k, shift);
                }
            }
        }
    }
}
