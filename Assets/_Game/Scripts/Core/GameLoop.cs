namespace CastleDestroyers.Core
{
    /// <summary>
    /// Единая точка обновления: порядок Tick задаётся массивом систем.
    /// Масштаб времени и пауза живут здесь, а не в Time.timeScale, чтобы не трогать UI.
    /// </summary>
    public sealed class GameLoop
    {
        private ITickable[] systems;
        private float maxDeltaTime;

        public float TimeScale { get; set; }
        public bool Paused { get; set; }
        /// <summary>Игровое время с учётом паузы и масштаба.</summary>
        public float GameTime { get; private set; }

        public void Init(ITickable[] orderedSystems, float maxDelta)
        {
            systems = orderedSystems;
            maxDeltaTime = maxDelta;
            TimeScale = 1f;
            Paused = false;
            GameTime = 0f;
        }

        public void Tick(float realDelta)
        {
            if (Paused)
            {
                return;
            }
            float dt = realDelta > maxDeltaTime ? maxDeltaTime : realDelta;
            dt *= TimeScale;
            GameTime += dt;
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Tick(dt);
            }
        }
    }
}
