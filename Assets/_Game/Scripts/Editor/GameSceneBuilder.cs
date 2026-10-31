using CastleDestroyers.Core;
using CastleDestroyers.Level;
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

        private const string RocketMaterialPath = "Assets/_Game/Art/Mat_Rocket.mat";
        private const string RocketShaderName = "Game/Rocket";
        private const string CastleMaterialPath = "Assets/_Game/Art/Mat_Castle.mat";
        private const string CastleShaderName = "Game/Castle";
        private const string ThemeArtFolder = "Assets/_Game/Art/Theme";

        // Порядок отрисовки: слои фона, деревья, земля, шасси, замки, их куски, обломки, вода, кромка воды, пушки, частицы, дым, ракеты, бустеры, взрывы, пунктир.
        // Замки и куски идут до воды: они в неё падают.
        private const int OrderDecor = LevelScene.BackLayerCount;
        private const int OrderTerrain = OrderDecor + 1;
        private const int OrderUndercarriage = OrderDecor + 2;
        private const int OrderCastles = OrderDecor + 3;
        private const int OrderCastleChunks = OrderDecor + 4;
        private const int OrderChunks = OrderDecor + 5;
        private const int OrderFront = OrderDecor + 6;
        private const int OrderWave = OrderDecor + 7;
        private const int OrderCannons = OrderDecor + 8;
        private const int OrderParticles = OrderDecor + 9;
        private const int OrderSmoke = OrderDecor + 10;
        private const int OrderRockets = OrderDecor + 11;
        private const int OrderBoosters = OrderDecor + 12;
        private const int OrderExplosions = OrderDecor + 13;
        private const int OrderAimDots = OrderDecor + 14;
        private const int OrderHud = OrderDecor + 15;
        private const int OrderFinale = OrderDecor + 16;

        private const int ReferenceWidth = 720;
        private const int ReferenceHeight = 1280;
        private const int SafeTop = 130;
        private const int StatsTop = 250;
        private const int DebugCanvasOrder = 10;
        private const int SafeBottom = 130;
        private const int StatsFontSize = 16;
        private const float StatsPadding = 16f;
        private const int ButtonFontSize = 17;
        private const float CameraStartSize = 8f;
        private const float StatsWidth = 500f;
        private const float StatsHeight = 250f;
        private const float AimPanelWidth = 250f;
        private const float AimPanelHeight = 110f;
        private const float AimCaptionShare = 0.3f;
        private const float AimPanelRoundness = 0.58f;
        private const int AimValueFontSize = 46;
        private const float AimDegreeSize = 15f;
        private const int AimCaptionFontSize = 18;
        private const float ScaleMatchBalanced = 0.5f;
        private const float BannerWidth = 330f;
        private const float BannerHeight = 76f;
        private const float BannerAnchorY = 0.8f;
        private const int BannerFontSize = 42;
        private const float HandSize = 200f;
        private const float HandShadowOffset = 7f;
        private const float EndLogoAnchorY = 0.83f;
        private const float EndLogoWidth = 680f;
        private const float EndLogoHeight = 300f;
        private const float EndIconAnchorY = 0.45f;
        private const float EndIconSize = 460f;
        private const float EndCaptionAnchorY = 0.235f;
        private const int EndCaptionFontSize = 44;
        private const float EndButtonAnchorY = 0.15f;
        private const float EndButtonWidth = 400f;
        private const float EndButtonHeight = 104f;
        private const float EndButtonEdge = 10f;
        private const int EndButtonFontSize = 50;
        private const float EndRestartAnchorX = 0.89f;
        private const float EndRestartSize = 104f;
        private const float EndRestartIconSize = 68f;
        private const float EndStarSize = 124f;
        private const float EndStarMiddleSize = 156f;
        private const float EndStarSideX = 0.21f;
        private const float EndStarSideY = 0.645f;
        private const float EndStarMiddleY = 0.675f;
        private const float EndHandSize = 170f;
        private const float PillPixelsPerUnit = 1f;
        private const int ButtonWidth = 128;
        private const int ButtonHeight = 56;
        private const int ButtonSpacing = 8;
        private const int ButtonRows = 3;
        private const int ToggleWidth = 150;
        private const int ToggleHeight = 52;
        private const int ToggleMargin = 8;
        private const int ToggleFontSize = 20;
        private const int SoundSources = 6;
        private static readonly string[] SoundNames = { "Shot", "ExplosionA", "ExplosionB", "Booster", "Splash", "Victory", "Click", "Blast", "Star" };

        [MenuItem("Tools/Castle Destroyers/Build Game Scene")]
        public static void Build()
        {
            ProjectSetup.SetupRenderPipeline();

            // Палитра в коде — источник и для растеризатора: ассет не должен от неё расходиться.
            ThemePalette theme = LoadOrCreate<ThemePalette>(ThemePath);
            theme.ResetPresets();
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
            config.terrainMask = TerrainMaskBaker.Bake();
            EditorUtility.SetDirty(config);
            FillThemeArt(theme.nightArt, "Night", config.terrainMask);
            FillThemeArt(theme.sunsetArt, "Sunset", AssetDatabase.LoadAssetAtPath<Destruction.TerrainMaskData>(TerrainMaskBaker.SunsetDataPath));
            EditorUtility.SetDirty(theme);
            Material material = LoadOrCreateMaterial(MaterialPath, ShaderName);
            material.SetTexture("_MainTex", theme.nightArt.atlas);
            EditorUtility.SetDirty(material);
            Material terrainMaterial = LoadOrCreateMaterial(TerrainMaterialPath, TerrainShaderName);
            Material particleMaterial = LoadOrCreateMaterial(ParticleMaterialPath, ParticleShaderName);
            Material castleMaterial = LoadOrCreateMaterial(CastleMaterialPath, CastleShaderName);
            Material rocketMaterial = LoadOrCreateMaterial(RocketMaterialPath, RocketShaderName);
            rocketMaterial.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(GameTextureImport.RocketPath));
            EditorUtility.SetDirty(rocketMaterial);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();
            MeshFilter[] back = new MeshFilter[LevelScene.BackLayerCount];
            for (int i = 0; i < back.Length; i++)
            {
                back[i] = CreateBatch("LevelBack" + i, material, i);
            }
            MeshFilter decor = CreateBatch("SurfaceDecor", material, OrderDecor);
            MeshFilter undercarriage = CreateBatch("Undercarriage", material, OrderUndercarriage);
            MeshFilter blueCastle = CreateBatch("CastleBlue", castleMaterial, OrderCastles);
            MeshFilter redCastle = CreateBatch("CastleRed", castleMaterial, OrderCastles);
            MeshFilter front = CreateBatch("LevelFront", material, OrderFront);
            MeshFilter wave = CreateBatch("WaterStrip", material, OrderWave);
            MeshFilter cannons = CreateBatch("Cannons", material, OrderCannons);
            MeshFilter particles = CreateBatch("Particles", material, OrderParticles);
            MeshFilter smoke = CreateBatch("Smoke", particleMaterial, OrderSmoke);
            MeshFilter rockets = CreateBatch("Rockets", rocketMaterial, OrderRockets);
            MeshFilter boosters = CreateBatch("Boosters", material, OrderBoosters);
            MeshFilter explosions = CreateBatch("Explosions", material, OrderExplosions);
            MeshFilter aimDots = CreateBatch("AimDots", particleMaterial, OrderAimDots);
            MeshFilter hud = CreateBatch("Hud", material, OrderHud);
            MeshFilter finale = CreateBatch("Finale", material, OrderFinale);

            GameBootstrap bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            bootstrap.config = config;
            bootstrap.mainCamera = camera;
            bootstrap.backFilters = back;
            bootstrap.decorFilter = decor;
            bootstrap.frontFilter = front;
            bootstrap.waveFilter = wave;
            bootstrap.cannonsFilter = cannons;
            bootstrap.castleFilters = new[] { blueCastle, redCastle };
            bootstrap.undercarriageFilter = undercarriage;
            bootstrap.castleMaterial = castleMaterial;
            bootstrap.castleChunkSortingOrder = OrderCastleChunks;
            bootstrap.particlesFilter = particles;
            bootstrap.smokeFilter = smoke;
            bootstrap.rocketsFilter = rockets;
            bootstrap.boostersFilter = boosters;
            bootstrap.explosionsFilter = explosions;
            bootstrap.aimDotsFilter = aimDots;
            bootstrap.hudFilter = hud;
            bootstrap.finaleFilter = finale;
            bootstrap.rocketMaterial = rocketMaterial;
            bootstrap.flatMaterial = material;
            CreateSounds(bootstrap);
            bootstrap.terrainRoot = new GameObject("Terrain").transform;
            bootstrap.terrainMaterial = terrainMaterial;
            bootstrap.terrainSortingOrder = OrderTerrain;
            bootstrap.chunkSortingOrder = OrderChunks;
            CreateGameUi(bootstrap);
#if GAME_DEBUG
            CreateDebugUi(bootstrap);
#endif

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        private static void FillThemeArt(ThemeArt art, string themeName, Destruction.TerrainMaskData terrain)
        {
            string folder = ThemeArtFolder + themeName + "/";
            art.atlas = LoadTexture(folder, "Atlas", themeName);
            art.castleBlue = LoadTexture(folder, "CastleBlue", themeName);
            art.castleRed = LoadTexture(folder, "CastleRed", themeName);
            art.backFar = LoadTexture(folder, "BackFar", themeName);
            art.backMid = LoadTexture(folder, "BackMid", themeName);
            art.backNear = LoadTexture(folder, "BackNear", themeName);
            art.terrain = terrain;
        }

        private static Texture2D LoadTexture(string folder, string name, string themeName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(folder + name + "_" + themeName + ".png");
        }

        private static void CreateSounds(GameBootstrap bootstrap)
        {
            AudioClip[] clips = new AudioClip[SoundNames.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(GameTextureImport.AudioFolder + SoundNames[i] + ".wav");
            }
            AudioSource[] sources = new AudioSource[SoundSources];
            GameObject sound = new GameObject("Sounds");
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = sound.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
            }
            bootstrap.soundClips = clips;
            bootstrap.soundSources = sources;
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
            cameraObject.AddComponent<AudioListener>();
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

        /// <summary>Игровой канвас: пока только плашка POWER / ANGLE, не перехватывает ввод.</summary>
        private static void CreateGameUi(GameBootstrap bootstrap)
        {
            GameObject canvasObject = new GameObject("GameUi", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = ScaleMatchBalanced;

            GameObject panel = new GameObject("AimPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(AimPanelWidth, AimPanelHeight);
            Image background = panel.GetComponent<Image>();
            background.raycastTarget = false;
            Sprite pill = LoadSprite(GameTextureImport.PillPath);
            background.sprite = pill;
            background.type = Image.Type.Sliced;
            background.pixelsPerUnitMultiplier = AimPanelRoundness;

            Font font = AssetDatabase.LoadAssetAtPath<Font>(GameTextureImport.FontPath);
            bootstrap.aimPanel = rect;
            bootstrap.aimPanelBackground = background;
            bootstrap.aimPowerText = CreatePanelText(rect, "Power", 0f, 0.5f, AimCaptionShare, 1f, AimValueFontSize, "0%", font);
            bootstrap.aimAngleText = CreatePanelText(rect, "Angle", 0.5f, 1f, AimCaptionShare, 1f, AimValueFontSize, "0", font);
            bootstrap.aimDegree = CreateImage(bootstrap.aimAngleText.rectTransform, "Degree", 0.5f, 0.5f, AimDegreeSize, AimDegreeSize, LoadSprite(GameTextureImport.DegreePath));
            bootstrap.aimCaptions = new[]
            {
                CreatePanelText(rect, "PowerCaption", 0f, 0.5f, 0f, AimCaptionShare, AimCaptionFontSize, "POWER", font),
                CreatePanelText(rect, "AngleCaption", 0.5f, 1f, 0f, AimCaptionShare, AimCaptionFontSize, "ANGLE", font)
            };

            Transform canvas = canvasObject.transform;
            bootstrap.gameCanvas = canvasObject.GetComponent<RectTransform>();
            CreateBanner(bootstrap, canvas, pill, font);
            CreateHand(bootstrap, canvas);
            CreateEndCard(bootstrap, canvas, pill, LoadSprite(GameTextureImport.CirclePath), font);
        }

        private static Sprite LoadSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>Прямоугольник на точке родителя (доли его размера), размер в единицах канваса.</summary>
        private static RectTransform CreateRect(Transform parent, string objectName, float anchorX, float anchorY, float width, float height)
        {
            GameObject rectObject = new GameObject(objectName, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            RectTransform rect = rectObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorX, anchorY);
            rect.anchorMax = new Vector2(anchorX, anchorY);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static Image CreateImage(Transform parent, string objectName, float anchorX, float anchorY, float width, float height, Sprite sprite)
        {
            RectTransform rect = CreateRect(parent, objectName, anchorX, anchorY, width, height);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreatePill(Transform parent, string objectName, float anchorX, float anchorY, float width, float height, Sprite pill)
        {
            Image image = CreateImage(parent, objectName, anchorX, anchorY, width, height, pill);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = PillPixelsPerUnit;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateBanner(GameBootstrap bootstrap, Transform canvas, Sprite pill, Font font)
        {
            Image background = CreatePill(canvas, "TurnBanner", 0.5f, BannerAnchorY, BannerWidth, BannerHeight, pill);
            bootstrap.bannerRoot = background.rectTransform;
            bootstrap.bannerBackground = background;
            bootstrap.bannerText = CreatePanelText(background.rectTransform, "Label", 0f, 1f, 0f, 1f, BannerFontSize, "YOUR TURN!", font);
        }

        private static void CreateHand(GameBootstrap bootstrap, Transform canvas)
        {
            Sprite hand = LoadSprite(GameTextureImport.HandPath);
            RectTransform root = CreateRect(canvas, "Hand", 0.5f, 0.5f, HandSize, HandSize);
            // Кончик пальца в точке жеста.
            root.pivot = new Vector2(0.33f, 0.71f);
            Image shadow = CreateImage(root, "Shadow", 0.5f, 0.5f, HandSize, HandSize, hand);
            shadow.rectTransform.anchoredPosition = new Vector2(HandShadowOffset, -HandShadowOffset);
            bootstrap.handRoot = root;
            bootstrap.handImages = new[] { shadow, CreateImage(root, "Glove", 0.5f, 0.5f, HandSize, HandSize, hand) };
        }

        /// <summary>EndCard поверх всего. Тап принимает весь экран, поэтому у кнопки нет своего обработчика.</summary>
        private static void CreateEndCard(GameBootstrap bootstrap, Transform canvas, Sprite pill, Sprite circle, Font font)
        {
            RectTransform root = CreateRect(canvas, "EndCard", 0.5f, 0.5f, 0f, 0f);
            Stretch(root);

            RectTransform content = CreateRect(root, "Content", 0.5f, 0.5f, 0f, 0f);
            Stretch(content);

            bootstrap.endCardBlueCastle = LoadSprite(GameTextureImport.EndBluePath);
            bootstrap.endCardRedCastle = LoadSprite(GameTextureImport.EndRedPath);
            bootstrap.endCardIcon = CreateImage(content, "WinnerCastle", 0.5f, EndIconAnchorY, EndIconSize, EndIconSize, bootstrap.endCardBlueCastle);
            Image logo = CreateImage(content, "Logo", 0.5f, EndLogoAnchorY, EndLogoWidth, EndLogoHeight, LoadSprite(GameTextureImport.LogoPath));

            RectTransform captionRect = CreateRect(content, "Caption", 0.5f, EndCaptionAnchorY, EndLogoWidth, EndCaptionFontSize * 2f);
            bootstrap.endCardCaption = CreatePanelText(captionRect, "Text", 0f, 1f, 0f, 1f, EndCaptionFontSize, "", font);

            RectTransform button = CreateRect(content, "PlayNow", 0.5f, EndButtonAnchorY, EndButtonWidth, EndButtonHeight);
            Image edge = CreatePill(button, "Edge", 0.5f, 0.5f, EndButtonWidth, EndButtonHeight, pill);
            edge.rectTransform.anchoredPosition = new Vector2(0f, -EndButtonEdge);
            Image face = CreatePill(button, "Face", 0.5f, 0.5f, EndButtonWidth, EndButtonHeight, pill);
            bootstrap.endCardButton = button;
            bootstrap.endCardButtonFace = face;
            bootstrap.endCardButtonEdge = edge;
            bootstrap.endCardButtonText = CreatePanelText(face.rectTransform, "Label", 0f, 1f, 0f, 1f, EndButtonFontSize, "PLAY NOW", font);

            RectTransform restart = CreateRect(content, "Restart", EndRestartAnchorX, EndButtonAnchorY, EndRestartSize, EndRestartSize);
            bootstrap.endCardRestart = restart;
            bootstrap.endCardRestartDisc = CreateImage(restart, "Disc", 0.5f, 0.5f, EndRestartSize, EndRestartSize, circle);
            bootstrap.endCardRestartIcon = CreateImage(restart, "Icon", 0.5f, 0.5f, EndRestartIconSize, EndRestartIconSize, LoadSprite(GameTextureImport.RestartPath));

            Sprite star = LoadSprite(GameTextureImport.StarPath);
            Image[] stars =
            {
                CreateImage(content, "StarLeft", 0.5f - EndStarSideX, EndStarSideY, EndStarSize, EndStarSize, star),
                CreateImage(content, "StarMiddle", 0.5f, EndStarMiddleY, EndStarMiddleSize, EndStarMiddleSize, star),
                CreateImage(content, "StarRight", 0.5f + EndStarSideX, EndStarSideY, EndStarSize, EndStarSize, star)
            };
            bootstrap.endCardStars = stars;

            Sprite hand = LoadSprite(GameTextureImport.HandPath);
            RectTransform handRoot = CreateRect(button, "Hand", 0.72f, 0.3f, EndHandSize, EndHandSize);
            handRoot.pivot = new Vector2(0.33f, 0.71f);
            Image handShadow = CreateImage(handRoot, "Shadow", 0.5f, 0.5f, EndHandSize, EndHandSize, hand);
            handShadow.rectTransform.anchoredPosition = new Vector2(HandShadowOffset, -HandShadowOffset);
            bootstrap.endCardHand = handRoot;
            bootstrap.endCardHandImages = new[] { handShadow, CreateImage(handRoot, "Glove", 0.5f, 0.5f, EndHandSize, EndHandSize, hand) };

            bootstrap.endCardLayout = new[] { logo.rectTransform, bootstrap.endCardIcon.rectTransform, captionRect, button, restart, stars[0].rectTransform, stars[1].rectTransform, stars[2].rectTransform };
            bootstrap.endCardRoot = root.gameObject;
            bootstrap.endCardContent = content;
        }

        private static Text CreatePanelText(RectTransform parent, string objectName, float minX, float maxX, float minY, float maxY, int fontSize, string label, Font font)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = label;
            return text;
        }

#if GAME_DEBUG
        private static void CreateDebugUi(GameBootstrap bootstrap)
        {
            GameObject canvasObject = new GameObject("DebugRoot", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            // Выше игрового канваса: RESTART должен работать и на EndCard.
            canvasObject.GetComponent<Canvas>().sortingOrder = DebugCanvasOrder;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(canvasObject.transform, false);

            Font font = AssetDatabase.LoadAssetAtPath<Font>(GameTextureImport.FontPath);
            Sprite pill = LoadSprite(GameTextureImport.PillPath);
            DebugOverlay overlay = canvasObject.AddComponent<DebugOverlay>();
            overlay.canvasScaler = scaler;
            GameObject panelRoot = CreateStretched("Panel", canvasObject.transform);
            overlay.panelRoot = panelRoot;
            overlay.statsText = CreateStats(panelRoot.transform, font, pill, overlay);
            overlay.toggleButton = CreateToggleButton(canvasObject.transform, font, overlay);

            RectTransform panel = CreateButtonsPanel(panelRoot.transform);
            overlay.nextStateButton = CreateButton(panel, "NEXT STATE", font, pill);
            overlay.slowButton = CreateButton(panel, "SLOW x0.25", font, pill);
            overlay.rocketsButton = CreateButton(panel, "STRESS OFF", font, pill);
            overlay.rocketsLabel = overlay.rocketsButton.GetComponentInChildren<Text>();
            overlay.themeButton = CreateButton(panel, "THEME", font, pill);
            overlay.themeLabel = overlay.themeButton.GetComponentInChildren<Text>();
            overlay.restartButton = CreateButton(panel, "RESTART", font, pill);
            overlay.blastButton = CreateButton(panel, "BLAST", font, pill);
            overlay.blastLabel = overlay.blastButton.GetComponentInChildren<Text>();
            overlay.cameraButton = CreateButton(panel, "CAM AUTO", font, pill);
            overlay.cameraLabel = overlay.cameraButton.GetComponentInChildren<Text>();
            overlay.shotX20Button = CreateButton(panel, "SHOT x20", font, pill);
            overlay.shotX10Button = CreateButton(panel, "SHOT x10", font, pill);
            overlay.shotX50Button = CreateButton(panel, "SHOT x50", font, pill);
            overlay.buttonImages = panel.GetComponentsInChildren<Image>();
            overlay.buttonLabels = panel.GetComponentsInChildren<Text>();

            bootstrap.debugRoot = canvasObject;
            bootstrap.debugOverlay = overlay;
        }

        private static GameObject CreateStretched(string name, Transform parent)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return root;
        }

        /// <summary>
        /// Едва заметная надпись Developer в верхнем левом углу: в Luna нижний край debug-канваса уходил за экран, а этот угол всегда на месте (D-068).
        /// Без подложки: нажатие ловит сама надпись, в кадре это один лишний вызов отрисовки.
        /// </summary>
        private static Button CreateToggleButton(Transform canvas, Font font, DebugOverlay overlay)
        {
            GameObject buttonObject = new GameObject("Developer", typeof(RectTransform), typeof(Text), typeof(Button));
            buttonObject.transform.SetParent(canvas, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;
            rect.pivot = Vector2.up;
            rect.anchoredPosition = new Vector2(ToggleMargin, -ToggleMargin);
            rect.sizeDelta = new Vector2(ToggleWidth, ToggleHeight);
            Text text = buttonObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = ToggleFontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = "Developer";
            overlay.toggleLabel = text;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = text;
            button.transition = Selectable.Transition.None;
            return button;
        }

        private static Text CreateStats(Transform canvas, Font font, Sprite pill, DebugOverlay overlay)
        {
            GameObject panel = new GameObject("StatsPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.offsetMin = new Vector2(20f, -StatsTop - StatsHeight);
            panelRect.offsetMax = new Vector2(20f + StatsWidth, -StatsTop);
            Image background = panel.GetComponent<Image>();
            background.sprite = pill;
            background.type = Image.Type.Sliced;
            background.raycastTarget = false;
            overlay.statsBackground = background;

            GameObject textObject = new GameObject("Stats", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(StatsPadding, StatsPadding);
            rect.offsetMax = new Vector2(-StatsPadding, -StatsPadding);
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = StatsFontSize;
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

        private static Button CreateButton(Transform parent, string label, Font font, Sprite pill)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            Image background = buttonObject.GetComponent<Image>();
            background.sprite = pill;
            background.type = Image.Type.Sliced;

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
