#if GAME_DEBUG
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.InputRig;
using UnityEngine;

namespace CastleDestroyers.DebugTools
{
    /// <summary>
    /// Клик или тап = взрыв заданного радиуса, при удержании взрывы повторяются (этап 2). По умолчанию выключен:
    /// ввод занят прицелом, кнопка BLAST перебирает выкл. и радиусы. Вырезается без GAME_DEBUG.
    /// </summary>
    public sealed class DebugBlaster : ITickable
    {
        private const int OffIndex = -1;

        private readonly PointerReader pointer = new PointerReader();

        private GameConfig config;
        private Camera cam;
        private TerrainSystem terrain;
        private int radiusIndex = OffIndex;
        private float holdTimer;
        private bool wasPressed;

        public bool Active { get { return radiusIndex != OffIndex; } }
        public float Radius { get { return Active ? config.debugBlastRadii[radiusIndex] : 0f; } }

        public void Init(GameConfig gameConfig, Camera camera, TerrainSystem terrainSystem)
        {
            config = gameConfig;
            cam = camera;
            terrain = terrainSystem;
        }

        public void NextRadius()
        {
            radiusIndex++;
            if (radiusIndex >= config.debugBlastRadii.Length)
            {
                radiusIndex = OffIndex;
            }
        }

        public void Tick(float dt)
        {
            if (!Active)
            {
                return;
            }
            pointer.Read();
            bool pressed = pointer.Pressed;
            holdTimer -= dt;
            bool overUi = pointer.Y < Screen.height * config.debugBlastUiBand;
            bool fire = pressed && !overUi && (!wasPressed || holdTimer <= 0f);
            wasPressed = pressed;
            if (fire)
            {
                holdTimer = config.debugBlastHoldInterval;
                Vector3 world = cam.ScreenToWorldPoint(new Vector3(pointer.X, pointer.Y, -cam.transform.position.z));
                terrain.Explode(world.x, world.y, Radius);
            }
        }
    }
}
#endif
