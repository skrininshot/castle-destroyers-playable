using CastleDestroyers.CameraRig;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using CastleDestroyers.Platform;
using UnityEngine;
#if GAME_DEBUG
using CastleDestroyers.DebugTools;
#endif

namespace CastleDestroyers.Core
{
    /// <summary>
    /// Composition root: создаёт системы, передаёт зависимости явно и задаёт порядок Tick.
    /// Остальной код MonoBehaviour не содержит.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public GameConfig config;
        public Camera mainCamera;
        public MeshFilter levelFilter;
        public MeshFilter cannonsFilter;
        public MeshFilter stressFilter;
        [Tooltip("Корень debug-UI. В сборке без GAME_DEBUG удаляется.")]
        public GameObject debugRoot;
#if GAME_DEBUG
        public DebugOverlay debugOverlay;
#endif

        public Rng Rng { get; private set; }
        public MatchState Match { get; private set; }
        public SessionFlow Flow { get; private set; }
        public GreyboxLevel Level { get; private set; }
        public CannonMarkers Cannons { get; private set; }
        public CameraDirector CameraDirector { get; private set; }
        public GameLoop Loop { get; private set; }
        public ThemePreset ActivePreset { get; private set; }
#if GAME_DEBUG
        public DebugRocketStress Stress { get; private set; }
#endif

        private bool booted;

        private void Start()
        {
            Boot();
        }

        private void Update()
        {
            if (booted)
            {
                Loop.Tick(Time.unscaledDeltaTime);
            }
        }

        /// <summary>Создаёт все системы. Вызывается из Start и из editor-захвата скриншотов.</summary>
        public void Boot()
        {
            if (booted)
            {
                return;
            }
            Rng = new Rng((uint)config.rngSeed);
            Match = new MatchState();
            Match.Init(config.playerCannons, config.enemyCannons);

            Flow = new SessionFlow();
            Flow.Init(config, Match);
            Flow.StateChanged += OnStateChanged;

            Level = new GreyboxLevel();
            Level.Init(config);
            levelFilter.sharedMesh = Level.Mesh;

            Cannons = new CannonMarkers();
            Cannons.Init(Level, Match, config);
            cannonsFilter.sharedMesh = Cannons.Mesh;

            CameraDirector = new CameraDirector();
            CameraDirector.Init(mainCamera, config, Level, Flow);

            ActivePreset = config.theme.preset;

#if GAME_DEBUG
            Stress = new DebugRocketStress();
            Stress.Init(config, Level, Rng);
            stressFilter.sharedMesh = Stress.Mesh;
            debugOverlay.Init(this);
            ITickable[] systems = { Flow, CameraDirector, Cannons, Stress, debugOverlay };
#else
            if (debugRoot != null)
            {
                Destroy(debugRoot);
            }
            ITickable[] systems = { Flow, CameraDirector, Cannons };
#endif
            Loop = new GameLoop();
            Loop.Init(systems, config.maxDeltaTime);

            ApplyTheme();
            PlatformBridge.Init(OnPause, OnResume);
            booted = true;
        }

        /// <summary>Переключает пресет палитры Night / Sunset без изменения ассета.</summary>
        public void SetPreset(ThemePreset preset)
        {
            ActivePreset = preset;
            ApplyTheme();
        }

        public PaletteSet Palette
        {
            get { return config.theme.Get(ActivePreset); }
        }

        /// <summary>Начинает сессию заново: счёт, FSM, камера, чит-ракеты.</summary>
        public void Restart()
        {
            Flow.Restart();
#if GAME_DEBUG
            Stress.Clear();
#endif
            CameraDirector.Snap();
        }

        private void ApplyTheme()
        {
            PaletteSet palette = Palette;
            Level.Build(palette);
            Cannons.ApplyPalette(palette);
            mainCamera.backgroundColor = palette.sky;
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            if (state == SessionState.EndCard)
            {
                PlatformBridge.GameEnded();
            }
        }

        private void OnPause()
        {
            Loop.Paused = true;
        }

        private void OnResume()
        {
            Loop.Paused = false;
        }
    }
}
