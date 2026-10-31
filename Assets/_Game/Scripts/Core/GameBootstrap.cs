using CastleDestroyers.CameraRig;
using CastleDestroyers.CastleRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.InputRig;
using CastleDestroyers.Level;
using CastleDestroyers.Platform;
using CastleDestroyers.UI;
using UnityEngine;
using UnityEngine.UI;
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
        private static readonly int RocketBodyColor = Shader.PropertyToID("_BodyColor");
        private static readonly int RocketShadeColor = Shader.PropertyToID("_ShadeColor");

        public GameConfig config;
        public Camera mainCamera;
        [Tooltip("Слои фона от дальнего к ближнему.")]
        public MeshFilter[] backFilters;
        public MeshFilter decorFilter;
        public MeshFilter frontFilter;
        public MeshFilter waveFilter;
        public MeshFilter cannonsFilter;
        [Tooltip("Каменные части замков: синий, красный.")]
        public MeshFilter[] castleFilters;
        public MeshFilter undercarriageFilter;
        public MeshFilter particlesFilter;
        public MeshFilter smokeFilter;
        public MeshFilter rocketsFilter;
        public MeshFilter boostersFilter;
        public MeshFilter explosionsFilter;
        public MeshFilter aimDotsFilter;
        [Tooltip("Плашка POWER / ANGLE.")]
        public RectTransform aimPanel;
        public Image aimPanelBackground;
        public Text aimPowerText;
        public Text aimAngleText;
        public Text[] aimCaptions;
        [Tooltip("Родитель тайлов земли и обломков.")]
        public Transform terrainRoot;
        public Material terrainMaterial;
        public Material rocketMaterial;
        public Material castleMaterial;
        [Tooltip("Порядок отрисовки тайлов земли.")]
        public int terrainSortingOrder;
        [Tooltip("Порядок отрисовки обломков: поверх земли, но под водой.")]
        public int chunkSortingOrder;
        [Tooltip("Порядок отрисовки кусков замков: поверх замков, но под водой.")]
        public int castleChunkSortingOrder;
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
        public RocketSystem Rockets { get; private set; }
        public BoosterSystem Boosters { get; private set; }
        public CannonSystem Cannons { get; private set; }
        public CastleSystem Castles { get; private set; }
        public ExplosionSystem Explosions { get; private set; }
        public SmokeTrails Smoke { get; private set; }
        public AimInput Aim { get; private set; }
        public CameraDirector CameraDirector { get; private set; }
        public ParallaxBackground Parallax { get; private set; }
        public GameLoop Loop { get; private set; }
        public ThemePreset ActivePreset { get; private set; }
#if GAME_DEBUG
        public DebugRocketStress Stress { get; private set; }
        public DebugBlaster Blaster { get; private set; }
        public DebugBoosterScenarios Scenarios { get; private set; }
#endif

        private LevelFx levelFx;
        private MaskTileView tileView;
        private ChunkSystem chunks;
        private WaterStrip water;
        private CannonMarkers cannonView;
        private UndercarriageView undercarriageView;
        private BoosterView boosterView;
        private ScreenShake shake;
        private AimPreview aimPreview;
        private AimPanelView aimPanelView;
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
            Transform[] backLayers = new Transform[backFilters.Length];
            for (int i = 0; i < backFilters.Length; i++)
            {
                backFilters[i].sharedMesh = Level.BackLayers[i];
                backLayers[i] = backFilters[i].transform;
            }
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

            shake = new ScreenShake();
            shake.Init(config);
            Smoke = new SmokeTrails();
            Smoke.Init(config);
            smokeFilter.sharedMesh = Smoke.Mesh;
            Explosions = new ExplosionSystem();
            Explosions.Init(config, Terrain, Particles, levelFx, shake, Rng);
            explosionsFilter.sharedMesh = Explosions.Mesh;
            Boosters = new BoosterSystem();
            Castles = new CastleSystem();
            Rockets = new RocketSystem();
            Rockets.Init(config, terrainMask, Castles, Explosions, Smoke, levelFx, Boosters);
            rocketsFilter.sharedMesh = Rockets.Mesh;
            Boosters.Init(config, Rockets, Particles, Rng);
            boosterView = new BoosterView();
            boosterView.Init(config, Boosters);
            boostersFilter.sharedMesh = boosterView.Mesh;

            Cannons = new CannonSystem();
            Cannons.Init(config, Level, Match, Rockets, Particles, levelFx, Rng);
            cannonView = new CannonMarkers();
            cannonView.Init(Match, Cannons, config);
            cannonsFilter.sharedMesh = cannonView.Mesh;

            Castles.Init(config, Level, terrainMask, Cannons, levelFx, Rng, castleMaterial, castleFilters, terrainRoot, castleChunkSortingOrder);
            undercarriageView = new UndercarriageView();
            undercarriageView.Init(config, Castles);
            undercarriageFilter.sharedMesh = undercarriageView.Mesh;

            CameraDirector = new CameraDirector();
            CameraDirector.Init(mainCamera, config, Level, Flow, shake);
            Parallax = new ParallaxBackground();
            Parallax.Init(config, mainCamera, backLayers);

            TurnCombat combat = new TurnCombat();
            combat.Init(config, Flow, Cannons, Rockets, Boosters, CameraDirector);
            Aim = new AimInput();
            Aim.Init(config, Flow, Cannons, mainCamera);
            aimPreview = new AimPreview();
            aimPreview.Init(config, Aim, Cannons);
            aimDotsFilter.sharedMesh = aimPreview.Mesh;
            aimPanelView = new AimPanelView();
            aimPanelView.Init(config, Aim, Level, mainCamera, aimPanel, aimPanelBackground, aimPowerText, aimAngleText, aimCaptions);

            ActivePreset = config.theme.preset;

#if GAME_DEBUG
            Stress = new DebugRocketStress();
            Stress.Init(config, Level, Rockets);
            Blaster = new DebugBlaster();
            Blaster.Init(config, mainCamera, Terrain, Castles);
            Scenarios = new DebugBoosterScenarios();
            Scenarios.Init(config, Flow, Cannons);
            debugOverlay.Init(this);
            ITickable[] systems =
            {
                Flow, Aim, Scenarios, combat, Cannons, Stress, Boosters, Rockets, Explosions, shake, CameraDirector, Parallax, Blaster, Terrain, chunks, Castles, Decor, water,
                Particles, Smoke, cannonView, undercarriageView, boosterView, aimPreview, aimPanelView, debugOverlay
            };
#else
            if (debugRoot != null)
            {
                Destroy(debugRoot);
            }
            ITickable[] systems =
            {
                Flow, Aim, combat, Cannons, Boosters, Rockets, Explosions, shake, CameraDirector, Parallax, Terrain, chunks, Castles, Decor, water,
                Particles, Smoke, cannonView, undercarriageView, boosterView, aimPreview, aimPanelView
            };
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

        /// <summary>Начинает сессию заново: счёт, FSM, ландшафт, камера, бой.</summary>
        public void Restart()
        {
            Flow.Restart();
            Terrain.Reset();
            Decor.Reset();
            Particles.Clear();
            Rockets.Clear();
            Boosters.Clear();
            Cannons.Clear();
            Castles.Reset();
            Explosions.Clear();
            Smoke.Clear();
            shake.Clear();
            Aim.Cancel();
#if GAME_DEBUG
            Stress.SetActive(false);
#endif
            CameraDirector.HeadPassedCenter = false;
            CameraDirector.Snap();
            Parallax.Apply();
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
            cannonView.ApplyPalette(palette);
            Castles.ApplyPalette(palette);
            undercarriageView.ApplyPalette(palette);
            Rockets.ApplyPalette(palette);
            Boosters.ApplyPalette(palette);
            boosterView.ApplyPalette(palette);
            Explosions.ApplyPalette(palette);
            Smoke.ApplyPalette(palette);
            aimPreview.ApplyPalette(palette);
            aimPanelView.ApplyPalette(palette);
            rocketMaterial.SetColor(RocketBodyColor, palette.rocketBody);
            rocketMaterial.SetColor(RocketShadeColor, palette.rocketShade);
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
