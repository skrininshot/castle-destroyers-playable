using CastleDestroyers.Spike;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace CastleDestroyers.EditorTools
{
    /// <summary>Собирает spike-сцену, материалы и конфиг. Запускается из меню или -executeMethod.</summary>
    public static class SpikeSceneBuilder
    {
        private const string ArtDir = "Assets/_Game/Art/Spike";
        private const string ConfigPath = "Assets/_Game/Configs/SpikeConfig.asset";
        private const string ScenePath = "Assets/_Game/Scenes/Spike.unity";
        private const int ReferenceWidth = 720;
        private const int ReferenceHeight = 1280;
        private const int SafeBottom = 130;
        private const int FontSize = 28;

        [MenuItem("Tools/Castle Destroyers/Build Spike Scene")]
        public static void Build()
        {
            ProjectSetup.SetupRenderPipeline();

            SpikeConfig config = LoadOrCreateConfig();
            Material maskMaterial = LoadOrCreateMaterial("Mat_SpikeMask", "Game/Spike/MaskCutout", 0f);
            Material rocketMaterial = LoadOrCreateMaterial("Mat_SpikeRocket", "Game/Spike/BatchedQuad", 0f);
            Material smokeMaterial = LoadOrCreateMaterial("Mat_SpikeSmoke", "Game/Spike/BatchedQuad", 1f);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();
            Transform maskRoot = new GameObject("Mask").transform;
            MeshFilter rockets = CreateBatch("Rockets", rocketMaterial);
            MeshFilter smoke = CreateBatch("Smoke", smokeMaterial);

            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text stats = CreateStats(canvasObject.transform, font);
            RectTransform panel = CreateButtonsPanel(canvasObject.transform);

            SpikeBootstrap bootstrap = new GameObject("SpikeBootstrap").AddComponent<SpikeBootstrap>();
            bootstrap.config = config;
            bootstrap.mainCamera = camera;
            bootstrap.maskMaterial = maskMaterial;
            bootstrap.maskRoot = maskRoot;
            bootstrap.rocketsFilter = rockets;
            bootstrap.smokeFilter = smoke;
            bootstrap.canvasScaler = scaler;
            bootstrap.buttonsPanel = panel;
            bootstrap.statsText = stats;
            bootstrap.goTestButton = CreateButton(panel, "GO TEST", font);
            bootstrap.stressButton = CreateButton(panel, "STRESS", font);
            bootstrap.autoCratersButton = CreateButton(panel, "AUTO CRATERS", font);
            bootstrap.rocketsButton = CreateButton(panel, "ROCKETS", font);
            bootstrap.smokeButton = CreateButton(panel, "SMOKE", font);
            bootstrap.resetMaskButton = CreateButton(panel, "RESET MASK", font);
            bootstrap.ctaButton = CreateButton(panel, "CTA", font);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        private static SpikeConfig LoadOrCreateConfig()
        {
            SpikeConfig config = AssetDatabase.LoadAssetAtPath<SpikeConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SpikeConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            return config;
        }

        private static Material LoadOrCreateMaterial(string assetName, string shaderName, float softness)
        {
            string path = ArtDir + "/" + assetName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(shaderName));
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_Softness"))
            {
                material.SetFloat("_Softness", softness);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.45f, 0.72f, 0.92f);
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            return camera;
        }

        private static MeshFilter CreateBatch(string objectName, Material material)
        {
            GameObject batch = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
            batch.GetComponent<MeshRenderer>().sharedMaterial = material;
            return batch.GetComponent<MeshFilter>();
        }

        private static Text CreateStats(Transform canvas, Font font)
        {
            GameObject statsObject = new GameObject("Stats", typeof(RectTransform), typeof(Text));
            statsObject.transform.SetParent(canvas, false);
            RectTransform rect = statsObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(70f, -340f);
            rect.offsetMax = new Vector2(-70f, -130f);
            Text text = statsObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = FontSize;
            text.color = Color.black;
            text.raycastTarget = false;
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
            rect.offsetMax = new Vector2(-20f, SafeBottom + 280f);
            GridLayoutGroup grid = panel.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(220f, 84f);
            grid.spacing = new Vector2(10f, 10f);
            grid.childAlignment = TextAnchor.LowerCenter;
            return rect;
        }

        private static Button CreateButton(Transform parent, string label, Font font)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(220f, 84f);
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
            text.fontSize = FontSize;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = label;
            return buttonObject.GetComponent<Button>();
        }
    }
}
