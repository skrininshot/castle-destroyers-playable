using CastleDestroyers.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
#if GAME_DEBUG
using CastleDestroyers.DebugTools;
#endif

namespace CastleDestroyers.EditorTools
{
    /// <summary>Собирает игровую сцену, конфиг, палитру и материал. Запускается из меню или -executeMethod.</summary>
    public static class GameSceneBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Game.unity";
        private const string ConfigPath = "Assets/_Game/Configs/GameConfig.asset";
        private const string ThemePath = "Assets/_Game/Configs/ThemePalette.asset";
        private const string MaterialPath = "Assets/_Game/Art/Mat_FlatColor.mat";
        private const string ShaderName = "Game/FlatColor";
        private const string TerrainMaterialPath = "Assets/_Game/Art/Mat_Terrain.mat";
        private const string TerrainShaderName = "Game/Terrain";
        private const string ParticleMaterialPath = "Assets/_Game/Art/Mat_FlatParticle.mat";
        private const string ParticleShaderName = "Game/FlatParticle";

        // Порядок отрисовки: фон, деревья, земля, обломки, вода с замками, кромка воды, пушки, частицы, чит-ракеты.
        private const int OrderBack = 0;
        private const int OrderDecor = 1;
        private const int OrderTerrain = 2;
        private const int OrderChunks = 3;
        private const int OrderFront = 4;
        private const int OrderWave = 5;
        private const int OrderCannons = 6;
        private const int OrderParticles = 7;
        private const int OrderStress = 8;

        private const int ReferenceWidth = 720;
        private const int ReferenceHeight = 1280;
        private const int SafeTop = 130;
        private const int SafeBottom = 130;
        private const int StatsFontSize = 17;
        private const int ButtonFontSize = 15;
        private const float CameraStartSize = 8f;
        private const float StatsWidth = 380f;
        private const float StatsHeight = 160f;
        private const float StatsAlpha = 0.4f;
        private const int ButtonWidth = 128;
        private const int ButtonHeight = 56;
        private const int ButtonSpacing = 8;
        private const int ButtonRows = 2;

        [MenuItem("Tools/Castle Destroyers/Build Game Scene")]
        public static void Build()
        {
            ProjectSetup.SetupRenderPipeline();

            ThemePalette theme = LoadOrCreate<ThemePalette>(ThemePath);
            GameConfig config = LoadOrCreate<GameConfig>(ConfigPath);
            if (config.theme == null)
            {
                config.theme = theme;
                EditorUtility.SetDirty(config);
            }
            if (config.terrainMask == null)
            {
                config.terrainMask = TerrainMaskBaker.Bake();
                EditorUtility.SetDirty(config);
            }
            Material material = LoadOrCreateMaterial(MaterialPath, ShaderName);
            Material terrainMaterial = LoadOrCreateMaterial(TerrainMaterialPath, TerrainShaderName);
            Material particleMaterial = LoadOrCreateMaterial(ParticleMaterialPath, ParticleShaderName);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();
            MeshFilter back = CreateBatch("LevelBack", material, OrderBack);
            MeshFilter decor = CreateBatch("SurfaceDecor", material, OrderDecor);
            MeshFilter front = CreateBatch("LevelFront", material, OrderFront);
            MeshFilter wave = CreateBatch("WaterStrip", material, OrderWave);
            MeshFilter cannons = CreateBatch("Cannons", material, OrderCannons);
            MeshFilter particles = CreateBatch("Particles", particleMaterial, OrderParticles);
            MeshFilter stress = CreateBatch("StressRockets", material, OrderStress);

            GameBootstrap bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            bootstrap.config = config;
            bootstrap.mainCamera = camera;
            bootstrap.backFilter = back;
            bootstrap.decorFilter = decor;
            bootstrap.frontFilter = front;
            bootstrap.waveFilter = wave;
            bootstrap.cannonsFilter = cannons;
            bootstrap.particlesFilter = particles;
            bootstrap.stressFilter = stress;
            bootstrap.terrainRoot = new GameObject("Terrain").transform;
            bootstrap.terrainMaterial = terrainMaterial;
            bootstrap.terrainSortingOrder = OrderTerrain;
            bootstrap.chunkSortingOrder = OrderChunks;
#if GAME_DEBUG
            CreateDebugUi(bootstrap);
#endif

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        /// <summary>Возвращает обе темы ассета к значениям из кода. Правки цветов в инспекторе будут потеряны.</summary>
        [MenuItem("Tools/Castle Destroyers/Reset Theme Palette To Defaults")]
        public static void ResetThemePalette()
        {
            ThemePalette theme = LoadOrCreate<ThemePalette>(ThemePath);
            theme.ResetPresets();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static Material LoadOrCreateMaterial(string path, string shaderName)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(shaderName));
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraStartSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            return camera;
        }

        private static MeshFilter CreateBatch(string objectName, Material material, int sortingOrder)
        {
            GameObject batch = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
            MeshRenderer renderer = batch.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            return batch.GetComponent<MeshFilter>();
        }

#if GAME_DEBUG
        private static void CreateDebugUi(GameBootstrap bootstrap)
        {
            GameObject canvasObject = new GameObject("DebugRoot", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(canvasObject.transform, false);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            DebugOverlay overlay = canvasObject.AddComponent<DebugOverlay>();
            overlay.canvasScaler = scaler;
            overlay.statsText = CreateStats(canvasObject.transform, font);

            RectTransform panel = CreateButtonsPanel(canvasObject.transform);
            overlay.nextStateButton = CreateButton(panel, "NEXT STATE", font);
            overlay.slowButton = CreateButton(panel, "SLOW x0.25", font);
            overlay.rocketsButton = CreateButton(panel, "+1000 ROCKETS", font);
            overlay.themeButton = CreateButton(panel, "THEME", font);
            overlay.restartButton = CreateButton(panel, "RESTART", font);
            overlay.blastButton = CreateButton(panel, "BLAST", font);
            overlay.blastLabel = overlay.blastButton.GetComponentInChildren<Text>();
            overlay.cameraButton = CreateButton(panel, "CAM AUTO", font);
            overlay.cameraLabel = overlay.cameraButton.GetComponentInChildren<Text>();

            bootstrap.debugRoot = canvasObject;
            bootstrap.debugOverlay = overlay;
        }

        private static Text CreateStats(Transform canvas, Font font)
        {
            GameObject panel = new GameObject("StatsPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.offsetMin = new Vector2(20f, -SafeTop - StatsHeight);
            panelRect.offsetMax = new Vector2(20f + StatsWidth, -SafeTop);
            Image background = panel.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, StatsAlpha);
            background.raycastTarget = false;

            GameObject textObject = new GameObject("Stats", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10f, 6f);
            rect.offsetMax = new Vector2(-10f, -6f);
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = StatsFontSize;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "...";
            return text;
        }

        private static RectTransform CreateButtonsPanel(Transform canvas)
        {
            GameObject panel = new GameObject("Buttons", typeof(RectTransform), typeof(GridLayoutGroup));
            panel.transform.SetParent(canvas, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(20f, SafeBottom);
            rect.offsetMax = new Vector2(-20f, SafeBottom + ButtonRows * (ButtonHeight + ButtonSpacing));
            GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(ButtonWidth, ButtonHeight);
            grid.spacing = new Vector2(ButtonSpacing, ButtonSpacing);
            grid.childAlignment = TextAnchor.LowerCenter;
            return rect;
        }

        private static Button CreateButton(Transform parent, string label, Font font)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            buttonObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.85f);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Text text = labelObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = ButtonFontSize;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = label;
            return buttonObject.GetComponent<Button>();
        }
#endif
    }
}
