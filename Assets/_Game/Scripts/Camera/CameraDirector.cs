using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.CameraRig
{
    /// <summary>
    /// Ортографическая камера с переездами между точками интереса по состояниям (ТЗ §3.9).
    /// Позиция и размер сглаживаются SmoothDamp, ширина кадра пересчитывается в размер с учётом пропорций экрана.
    /// </summary>
    public sealed class CameraDirector : ITickable
    {
        private const float CameraZ = -10f;

        private Camera cam;
        private Transform camTransform;
        private GameConfig config;
        private LevelScene level;
        private SessionFlow flow;
        private ScreenShake shake;
        private readonly CameraFrame target = new CameraFrame();

        private float currentX;
        private float currentY;
        private float currentSize;
        private float velocityX;
        private float velocityY;
        private float velocitySize;

#if GAME_DEBUG
        /// <summary>Debug-камера: 0 — по состояниям, дальше MTN, BLUE, RED, ALL. Нужна, чтобы взрывать землю без переездов.</summary>
        public int DebugView { get; set; }
        public const int DebugFrames = 5;
#endif

        /// <summary>Голова потока прошла центр уровня: пора ехать к цели (ТЗ §3.9).</summary>
        public bool HeadPassedCenter { get; set; }
        /// <summary>X головы потока: за ней слегка смещается центральный кадр.</summary>
        public float FlightHeadX { get; set; }
        /// <summary>Высшая точка залпа, известная на сейчас: центральный кадр раздвигается до неё и обратно не сжимается.</summary>
        public float FlightTopY { get; set; }
        /// <summary>Подсказка хода показывает гору: подсвеченный бустер не виден с замка.</summary>
        public bool ShowMountain { get; set; }

        public void Init(Camera camera, GameConfig gameConfig, LevelScene levelScene, SessionFlow sessionFlow, ScreenShake screenShake)
        {
            cam = camera;
            camTransform = camera.transform;
            config = gameConfig;
            level = levelScene;
            flow = sessionFlow;
            shake = screenShake;
            FlightHeadX = config.centerFocus.x;
            cam.orthographic = true;
            Snap();
        }

        /// <summary>Мгновенно ставит камеру в кадр текущего состояния (старт и рестарт).</summary>
        public void Snap()
        {
            SelectFrame(target);
            currentX = target.x;
            currentY = target.y;
            currentSize = CameraFraming.OrthoSize(target.width, Mathf.Max(target.minHeight, config.minFrameHeight), cam.aspect);
            velocityX = 0f;
            velocityY = 0f;
            velocitySize = 0f;
            Apply();
        }

        public void Tick(float dt)
        {
            SelectFrame(target);
            float size = CameraFraming.OrthoSize(target.width, Mathf.Max(target.minHeight, config.minFrameHeight), cam.aspect);
            float smooth = config.cameraSmoothTime;
            currentX = Mathf.SmoothDamp(currentX, target.x, ref velocityX, smooth, Mathf.Infinity, dt);
            currentY = Mathf.SmoothDamp(currentY, target.y, ref velocityY, smooth, Mathf.Infinity, dt);
            currentSize = Mathf.SmoothDamp(currentSize, size, ref velocitySize, smooth, Mathf.Infinity, dt);
            Apply();
        }

        /// <summary>Попадает ли прямоугольник мира в текущий кадр, расширенный на margin метров.</summary>
        public bool IsOnScreen(float minX, float minY, float maxX, float maxY, float margin)
        {
            float halfHeight = currentSize + margin;
            float halfWidth = currentSize * cam.aspect + margin;
            return maxX > currentX - halfWidth && minX < currentX + halfWidth && maxY > currentY - halfHeight && minY < currentY + halfHeight;
        }

        /// <summary>Кадр для текущего состояния сессии: на своём замке, в центре уровня или на замке-цели.</summary>
        public void SelectFrame(CameraFrame frame)
        {
#if GAME_DEBUG
            if (DebugView != 0)
            {
                SelectDebugFrame(frame);
                return;
            }
#endif
            Side active = flow.ActiveSide;
            switch (flow.State)
            {
                case SessionState.Flight:
                    if (HeadPassedCenter)
                    {
                        SetTargetFrame(frame, Opposite(active));
                    }
                    else
                    {
                        SetFlightFrame(frame);
                    }
                    break;
                case SessionState.Resolve:
                    SetTargetFrame(frame, Opposite(active));
                    break;
                case SessionState.Victory:
                case SessionState.Defeat:
                    // Сначала разрушенный замок проигравшего, затем победитель (ТЗ §3.9).
                    if (flow.StateTime < config.victoryLoserHold)
                    {
                        SetTargetFrame(frame, flow.Loser);
                    }
                    else
                    {
                        SetOwnFrame(frame, flow.Winner);
                    }
                    break;
                case SessionState.EndCard:
                    if (flow.HasWinner)
                    {
                        SetOwnFrame(frame, flow.Winner);
                    }
                    else
                    {
                        SetCenterFrame(frame);
                    }
                    break;
                default:
                    if (ShowMountain)
                    {
                        SetCenterFrame(frame);
                    }
                    else
                    {
                        SetOwnFrame(frame, active);
                    }
                    break;
            }
        }

#if GAME_DEBUG
        private void SelectDebugFrame(CameraFrame frame)
        {
            switch (DebugView)
            {
                case 1:
                    SetCenterFrame(frame);
                    break;
                case 2:
                    SetTargetFrame(frame, Side.Player);
                    break;
                case 3:
                    SetTargetFrame(frame, Side.Enemy);
                    break;
                default:
                    frame.Set(config.centerFocus.x, config.centerFocus.y, config.debugOverviewWidth);
                    break;
            }
        }
#endif

        private static Side Opposite(Side side)
        {
            return side == Side.Player ? Side.Enemy : Side.Player;
        }

        private void SetCenterFrame(CameraFrame frame)
        {
            frame.Set(config.centerFocus.x, config.centerFocus.y, config.centerFrameWidth);
            frame.minHeight = config.centerFrameMinHeight;
        }

        /// <summary>
        /// Кадр полёта: низ стоит на месте, верх поднимается над дугой залпа, поэтому ракеты выше бустера x20 остаются в кадре.
        /// В портрете вместе с высотой растёт и видимая ширина.
        /// </summary>
        private void SetFlightFrame(CameraFrame frame)
        {
            SetCenterFrame(frame);
            float bottom = frame.y - frame.minHeight * 0.5f;
            float height = Mathf.Min(FlightTopY + config.cameraFlightTopMargin - bottom, config.centerFrameMaxHeight);
            if (height > frame.minHeight)
            {
                frame.minHeight = height;
                frame.y = bottom + height * 0.5f;
            }
            frame.x += (FlightHeadX - frame.x) * config.cameraHeadFollow;
        }

        /// <summary>Свой замок крупно, со сдвигом к противнику: замок стоит чуть в стороне от центра кадра.</summary>
        private void SetOwnFrame(CameraFrame frame, Side side)
        {
            float lead = side == Side.Player ? config.castleLeadX : -config.castleLeadX;
            float x = (side == Side.Player ? level.PlayerFocusX : level.EnemyFocusX) + lead;
            float y = (side == Side.Player ? level.PlayerFocusY : level.EnemyFocusY) + config.castleFocusLiftY;
            frame.Set(x, y, config.castleFrameWidth);
        }

        private void SetTargetFrame(CameraFrame frame, Side side)
        {
            float x = side == Side.Player ? level.PlayerFocusX : level.EnemyFocusX;
            float y = (side == Side.Player ? level.PlayerFocusY : level.EnemyFocusY) + config.castleFocusLiftY;
            frame.Set(x, y, config.targetFrameWidth);
        }

        private void Apply()
        {
            camTransform.position = new Vector3(currentX + shake.OffsetX, currentY + shake.OffsetY, CameraZ);
            cam.orthographicSize = currentSize;
        }
    }
}
