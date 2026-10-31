using CastleDestroyers.CameraRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.InputRig;

namespace CastleDestroyers.UI
{
    /// <summary>
    /// Подсказки хода игрока (ТЗ §3.1.1, §3.2): показ горы с подсвеченным бустером, рука с жестом «оттянуть», повтор руки
    /// после простоя. Рука играет числами плашки и пунктиром, но прицел не задаёт: выстрел делает только игрок или автовыстрел.
    /// </summary>
    public sealed class TurnGuide : ITickable
    {
        // Доли одного показа: нажатие, протяжка, удержание.
        private const float PressShare = 0.15f;
        private const float DragEndShare = 0.75f;

        private GameConfig config;
        private SessionFlow flow;
        private AimInput aim;
        private CannonSystem cannons;
        private CameraDirector cameraDirector;
        private TurnHint hint;
        private bool active;
        private float panLeft;
        private float waitLeft;
        private float demoTime;
        private int demosLeft;
        private float idle;

        /// <summary>Какой бустер пульсирует; 0 — никакой.</summary>
        public int HighlightMultiplier { get; private set; }
        public bool HandVisible { get; private set; }
        /// <summary>Доля одного показа руки, 0..1.</summary>
        public float HandProgress { get; private set; }
        /// <summary>Насколько протянут палец, 0..1.</summary>
        public float HandDrag { get { return DragAt(HandProgress); } }
        public TurnHint Hint { get { return hint; } }

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, AimInput aimInput, CannonSystem cannonSystem, CameraDirector director)
        {
            config = gameConfig;
            flow = sessionFlow;
            aim = aimInput;
            cannons = cannonSystem;
            cameraDirector = director;
            flow.StateChanged += OnStateChanged;
        }

        /// <summary>Протяжка на доле показа: плавный разгон и торможение.</summary>
        public static float DragAt(float progress)
        {
            float t = (progress - PressShare) / (DragEndShare - PressShare);
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return t * t * (3f - 2f * t);
        }

        public void Tick(float dt)
        {
            if (!active)
            {
                return;
            }
            if (aim.GestureActive)
            {
                StopShow();
                return;
            }
            if (panLeft > 0f)
            {
                panLeft -= dt;
                if (panLeft <= 0f)
                {
                    cameraDirector.ShowMountain = false;
                }
                return;
            }
            if (demosLeft > 0)
            {
                TickDemo(dt);
                return;
            }
            idle += dt;
            if (idle >= config.handRepeatDelay)
            {
                idle = 0f;
                demosLeft = 1;
                demoTime = 0f;
                waitLeft = 0f;
            }
        }

        private void TickDemo(float dt)
        {
            if (waitLeft > 0f)
            {
                waitLeft -= dt;
                return;
            }
            demoTime += dt;
            if (demoTime >= config.handDemoDuration)
            {
                demoTime = 0f;
                demosLeft--;
                if (demosLeft == 0)
                {
                    HideHand();
                    idle = 0f;
                    return;
                }
            }
            HandVisible = true;
            HandProgress = demoTime / config.handDemoDuration;
            float angle = hint.handAngleDeg;
            aim.SetDemo(angle, hint.handPower01 * HandDrag);
            cannons.PointAt(Side.Player, angle);
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            if (state == SessionState.Aim && side == Side.Player)
            {
                Begin();
            }
            else
            {
                active = false;
                HighlightMultiplier = 0;
                StopShow();
            }
        }

        private void Begin()
        {
            hint = flow.CurrentHint;
            active = true;
            idle = 0f;
            demoTime = 0f;
            waitLeft = config.handStartDelay;
            demosLeft = hint.showHand ? config.handDemoRepeats : 0;
            HighlightMultiplier = hint.highlightBoosterMultiplier;
            panLeft = hint.panToBooster ? config.hintPanDuration : 0f;
            cameraDirector.ShowMountain = hint.panToBooster;
        }

        /// <summary>Игрок взялся за прицел: рука и показ горы уходят, подсветка бустера остаётся до конца хода.</summary>
        private void StopShow()
        {
            panLeft = 0f;
            demosLeft = 0;
            idle = 0f;
            cameraDirector.ShowMountain = false;
            HideHand();
        }

        private void HideHand()
        {
            HandVisible = false;
            HandProgress = 0f;
            aim.ClearDemo();
        }
    }
}
