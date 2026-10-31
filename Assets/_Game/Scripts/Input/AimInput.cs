using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;

namespace CastleDestroyers.InputRig
{
    /// <summary>
    /// Ввод прицела игрока: жест работает только в его фазе Aim, отпускание даёт выстрел.
    /// Новый жест начинается с нового нажатия, поэтому палец, зажатый во время полёта, не считается.
    /// </summary>
    public sealed class AimInput : ITickable
    {
        private readonly AimGesture gesture = new AimGesture();
        private readonly PointerReader pointer = new PointerReader();

        private GameConfig config;
        private SessionFlow flow;
        private CannonSystem cannons;
        private Camera cam;
        private bool wasPressed;
        private bool demo;
        private float demoAngle;
        private float demoPower;

        /// <summary>Показывать пунктир и плашку.</summary>
        public bool Aiming { get { return gesture.Active && gesture.Armed; } }
        public float AngleDeg { get { return gesture.AngleDeg; } }
        public float Power01 { get { return gesture.Power01; } }
        /// <summary>Палец на экране: подсказка уступает игроку, даже если жест ещё не вышел из мёртвой зоны.</summary>
        public bool GestureActive { get { return gesture.Active; } }
        /// <summary>Показывать пунктир и плашку: жест игрока или показ подсказки.</summary>
        public bool Showing { get { return Aiming || demo; } }
        public float ShownAngle { get { return demo ? demoAngle : gesture.AngleDeg; } }
        public float ShownPower { get { return demo ? demoPower : gesture.Power01; } }
        /// <summary>Устройство не читается: нажатия подаются через Feed (скриншоты, тесты).</summary>
        public bool Scripted { get; set; }
#if GAME_DEBUG
        /// <summary>Debug-взрывы забрали ввод.</summary>
        public bool Blocked { get; set; }
#endif

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, CannonSystem cannonSystem, Camera camera)
        {
            config = gameConfig;
            flow = sessionFlow;
            cannons = cannonSystem;
            cam = camera;
            gesture.Init(gameConfig);
        }

        public void Tick(float dt)
        {
            if (Scripted)
            {
                return;
            }
            pointer.Read();
            Feed(pointer.Pressed, pointer.X, pointer.Y, Screen.height);
        }

        /// <summary>Подсказка показывает прицел: числа и пунктир видны, но выстрела это не даёт.</summary>
        public void SetDemo(float angleDeg, float power01)
        {
            demo = true;
            demoAngle = angleDeg;
            demoPower = power01;
        }

        public void ClearDemo()
        {
            demo = false;
        }

        public void Cancel()
        {
            gesture.Cancel();
            wasPressed = false;
        }

        /// <summary>Один кадр ввода: нажат ли палец и где он на экране.</summary>
        public void Feed(bool pressed, float x, float y, float screenHeight)
        {
            bool canAim = flow.State == SessionState.Aim && flow.ActiveSide == Side.Player;
            if (!canAim)
            {
                gesture.Cancel();
            }
            else if (pressed)
            {
                if (!gesture.Active && !wasPressed && CanStartAt(y, screenHeight))
                {
                    gesture.Begin(x, y, screenHeight);
                }
                if (gesture.Active)
                {
                    Track(x, y);
                }
            }
            else if (gesture.Active)
            {
                if (gesture.Release())
                {
                    flow.Skip();
                }
                else
                {
                    cannons.ClearAim(Side.Player);
                }
            }
            wasPressed = pressed;
        }

        private void Track(float x, float y)
        {
            if (config.aimPointAtFinger)
            {
                Vector3 world = cam.ScreenToWorldPoint(new Vector3(x, y, -cam.transform.position.z));
                gesture.Point(world.x - cannons.MuzzleCenterX(Side.Player), world.y - cannons.MuzzleCenterY(Side.Player));
            }
            else
            {
                gesture.Move(x, y);
            }
            flow.KeepAiming();
            if (gesture.Armed)
            {
                cannons.SetAim(Side.Player, gesture.AngleDeg, gesture.Power01);
            }
        }

        private bool CanStartAt(float y, float screenHeight)
        {
#if GAME_DEBUG
            if (Blocked || y < screenHeight * config.debugBlastUiBand)
            {
                return false;
            }
#endif
            return true;
        }
    }
}
