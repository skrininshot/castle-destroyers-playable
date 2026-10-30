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
        private GreyboxLevel level;
        private SessionFlow flow;
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

        public void Init(Camera camera, GameConfig gameConfig, GreyboxLevel greyboxLevel, SessionFlow sessionFlow)
        {
            cam = camera;
            camTransform = camera.transform;
            config = gameConfig;
            level = greyboxLevel;
            flow = sessionFlow;
            cam.orthographic = true;
            Snap();
        }

        /// <summary>Мгновенно ставит камеру в кадр текущего состояния (старт и рестарт).</summary>
        public void Snap()
        {
            SelectFrame(target);
            currentX = target.x;
            currentY = target.y;
            currentSize = CameraFraming.OrthoSize(target.width, config.minFrameHeight, cam.aspect);
            velocityX = 0f;
            velocityY = 0f;
            velocitySize = 0f;
            Apply();
        }

        public void Tick(float dt)
        {
            SelectFrame(target);
            float size = CameraFraming.OrthoSize(target.width, config.minFrameHeight, cam.aspect);
            float smooth = config.cameraSmoothTime;
            currentX = Mathf.SmoothDamp(currentX, target.x, ref velocityX, smooth, Mathf.Infinity, dt);
            currentY = Mathf.SmoothDamp(currentY, target.y, ref velocityY, smooth, Mathf.Infinity, dt);
            currentSize = Mathf.SmoothDamp(currentSize, size, ref velocitySize, smooth, Mathf.Infinity, dt);
            Apply();
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
                    bool atCenter = flow.StateTime < flow.Duration(SessionState.Flight, active) * config.flightCenterFraction;
                    if (atCenter)
                    {
                        SetCenterFrame(frame);
                    }
                    else
                    {
                        SetTargetFrame(frame, Opposite(active));
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
                    SetOwnFrame(frame, active);
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
            camTransform.position = new Vector3(currentX, currentY, CameraZ);
            cam.orthographicSize = currentSize;
        }
    }
}
