#if GAME_DEBUG
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using UnityEngine;

namespace CastleDestroyers.DebugTools
{
    /// <summary>Клик или тап = взрыв заданного радиуса, при удержании взрывы повторяются (этап 2). Вырезается без GAME_DEBUG.</summary>
    public sealed class DebugBlaster : ITickable
    {
        private const float MouseAfterTouchGuard = 0.5f;

        private GameConfig config;
        private Camera cam;
        private TerrainSystem terrain;
        private int radiusIndex;
        private float holdTimer;
        private float lastTouchTime = -1f;
        private bool wasPressed;

        public float Radius { get { return config.debugBlastRadii[radiusIndex]; } }

        public void Init(GameConfig gameConfig, Camera camera, TerrainSystem terrainSystem)
        {
            config = gameConfig;
            cam = camera;
            terrain = terrainSystem;
        }

        public void NextRadius()
        {
            radiusIndex = (radiusIndex + 1) % config.debugBlastRadii.Length;
        }

        public void Tick(float dt)
        {
            bool pressed = false;
            float screenX = 0f;
            float screenY = 0f;
            float now = Time.unscaledTime;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                screenX = touch.position.x;
                screenY = touch.position.y;
                pressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                lastTouchTime = now;
            }
            else if (now - lastTouchTime > MouseAfterTouchGuard && Input.GetMouseButton(0))
            {
                Vector3 mouse = Input.mousePosition;
                screenX = mouse.x;
                screenY = mouse.y;
                pressed = true;
            }

            holdTimer -= dt;
            bool overUi = screenY < Screen.height * config.debugBlastUiBand;
            bool fire = pressed && !overUi && (!wasPressed || holdTimer <= 0f);
            wasPressed = pressed;
            if (fire)
            {
                holdTimer = config.debugBlastHoldInterval;
                Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenX, screenY, -cam.transform.position.z));
                terrain.Explode(world.x, world.y, Radius);
            }
        }
    }
}
#endif
