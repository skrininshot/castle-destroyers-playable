using CastleDestroyers.CameraRig;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
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
        public MeshFilter backFilter;
        public MeshFilter decorFilter;
        public MeshFilter frontFilter;
        public MeshFilter waveFilter;
        public MeshFilter cannonsFilter;
        public MeshFilter particlesFilter;
        public MeshFilter stressFilter;
        [Tooltip("Родитель тайлов земли и обломков.")]
        public Transform terrainRoot;
        public Material terrainMaterial;
        [Tooltip("Порядок отрисовки тайлов земли.")]
        public int terrainSortingOrder;
        [Tooltip("Порядок отрисовки обломков: поверх земли, но под водой.")]
        public int chunkSortingOrder;
        [Tooltip("Корень debug-UI. В сборке без GAME_DEBUG удаляется.")]
        public GameObject debugRoot;
#if GAME_DEBUG
        public DebugOverlay debugOverlay;
#endif

        public Rng Rng { get; private set; }
        public MatchState Match { get; private set; }
        public SessionFlow Flow { get; private set; }
        public GreyboxLevel Level { get; private set; }
        public TerrainSystem Terrain { get; private set; }
        public SurfaceDecor Decor { get; private set; }
        public FlatParticles Particles { get; private set; }
        public CannonMarkers Cannons { get; private set; }
        public CameraDirector CameraDirector { get; private set; }
        public GameLoop Loop { get; private set; }
        public ThemePreset ActivePreset { get; private set; }
#if GAME_DEBUG
        public DebugRocketStress Stress { get; private set; }
        public DebugBlaster Blaster { get; private set; }
#endif

        private LevelFx levelFx;
        private MaskTileView tileView;
        private ChunkSystem chunks;
        private WaterStrip water;
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

            DestructibleMask terrainMask = DestructibleMask.CreateTerrain(config);

            Level = new GreyboxLevel();
            Level.Init(config, terrainMask);
            backFilter.sharedMesh = Level.BackMesh;
            frontFilter.sharedMesh = Level.FrontMesh;

            Particles = new FlatParticles();
            Particles.Init(config.fxParticleCapacity);
            particlesFilter.sharedMesh = Particles.Mesh;
            levelFx = new LevelFx();
            levelFx.Init(config, Particles, Rng);

            tileView = new MaskTileView();
            tileView.Init(terrainMask, terrainMaterial, config.maskEdgeSoftness, terrainRoot, terrainSortingOrder);
            chunks = new ChunkSystem();
            chunks.Init(config, levelFx, Rng, terrainMaterial, terrainRoot, chunkSortingOrder);
            Terrain = new TerrainSystem();
            Terrain.Init(config, terrainMask, tileView, chunks, levelFx);

            Decor = new SurfaceDecor();
            Decor.Init(config, terrainMask, levelFx, Rng);
            decorFilter.sharedMesh = Decor.Mesh;

            water = new WaterStrip();
            water.Init(config);
            waveFilter.sharedMesh = water.Mesh;

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
            Blaster = new DebugBlaster();
            Blaster.Init(config, mainCamera, Terrain);
            debugOverlay.Init(this);
            ITickable[] systems = { Flow, CameraDirector, Blaster, Terrain, chunks, Decor, water, Particles, Cannons, Stress, debugOverlay };
#else
            if (debugRoot != null)
            {
                Destroy(debugRoot);
            }
            ITickable[] systems = { Flow, CameraDirector, Terrain, chunks, Decor, water, Particles, Cannons };
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

        /// <summary>Начинает сессию заново: счёт, FSM, ландшафт, камера, чит-ракеты.</summary>
        public void Restart()
        {
            Flow.Restart();
            Terrain.Reset();
            Decor.Reset();
            Particles.Clear();
#if GAME_DEBUG
            Stress.Clear();
#endif
            CameraDirector.Snap();
        }

        private void ApplyTheme()
        {
            PaletteSet palette = Palette;
            Level.Build(palette);
            Decor.Build(palette);
            tileView.ApplyPalette(palette);
            chunks.ApplyPalette(palette);
            water.ApplyPalette(palette);
            levelFx.ApplyPalette(palette);
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
