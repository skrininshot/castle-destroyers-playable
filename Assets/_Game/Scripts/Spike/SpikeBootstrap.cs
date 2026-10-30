using System.Text;
using CastleDestroyers.Platform;
using UnityEngine;
using UnityEngine.UI;

namespace CastleDestroyers.Spike
{
    /// <summary>Точка входа spike-сцены: связывает тесты, ввод, ресайз и HUD.</summary>
    public sealed class SpikeBootstrap : MonoBehaviour
    {
        private const float MillisecondsPerSecond = 1000f;
        private const float MouseAfterTouchGuard = 0.5f;
        private const int HudTextCapacity = 256;
        private const uint RngSeed = 2463534242u;

        public SpikeConfig config;
        public Camera mainCamera;
        public Material maskMaterial;
        public Transform maskRoot;
        public MeshFilter rocketsFilter;
        public MeshFilter smokeFilter;
        public CanvasScaler canvasScaler;
        public RectTransform buttonsPanel;
        public Text statsText;
        public Button autoCratersButton;
        public Button rocketsButton;
        public Button smokeButton;
        public Button goTestButton;
        public Button stressButton;
        public Button resetMaskButton;
        public Button ctaButton;

        private SpikeRng rng;
        private SpikeMask mask;
        private SpikeMaskView maskView;
        private SpikeRockets rockets;
        private SpikeSmoke smoke;
        private StringBuilder hudText;

        private bool autoCraters;
        private bool paused;
        private int screenWidth;
        private int screenHeight;
        private float pointerStampTimer;
        private float lastTouchTime = -1f;
        private int touchCount;

        private int ctaClicks;
        private int pauseEvents;
        private int windowFrames;
        private int windowUploads;
        private float windowTime;
        private float windowMaxDelta;
        private float windowMaskSeconds;
        private float windowRocketSeconds;
        private float windowSmokeSeconds;

        private void Start()
        {
            rng = new SpikeRng(RngSeed);
            mask = new SpikeMask();
            mask.Init(config.maskSize, config.tileSize);
            maskView = new SpikeMaskView();
            maskView.Init(mask, config, maskMaterial, maskRoot);
            rockets = new SpikeRockets();
            rockets.Init(config, rng);
            rocketsFilter.sharedMesh = rockets.Mesh;
            smoke = new SpikeSmoke();
            smoke.Init(config, rng);
            smokeFilter.sharedMesh = smoke.Mesh;
            hudText = new StringBuilder(HudTextCapacity);

            autoCratersButton.onClick.AddListener(ToggleAutoCraters);
            rocketsButton.onClick.AddListener(CycleRockets);
            smokeButton.onClick.AddListener(ToggleSmoke);
            goTestButton.onClick.AddListener(StartGoTest);
            stressButton.onClick.AddListener(StartStressTest);
            resetMaskButton.onClick.AddListener(ResetMask);
            ctaButton.onClick.AddListener(ClickCta);
            PlatformBridge.Init(OnPause, OnResume);
        }

        private void Update()
        {
            float realDelta = Time.unscaledDeltaTime;
            windowFrames++;
            windowTime += realDelta;
            if (realDelta > windowMaxDelta)
            {
                windowMaxDelta = realDelta;
            }

            ApplyLayoutIfResized();
            if (paused)
            {
                return;
            }

            float dt = Mathf.Min(realDelta, config.maxDeltaTime);
            UpdatePointer(dt);
            UpdateAutoCraters();

            float start = Time.realtimeSinceStartup;
            windowUploads += maskView.UploadDirty(config.maxTileUploadsPerFrame);
            float afterMask = Time.realtimeSinceStartup;

            if (rockets.ActiveCount > 0)
            {
                rockets.Tick(dt);
                rockets.UploadMesh();
            }
            float afterRockets = Time.realtimeSinceStartup;

            if (smoke.ActiveCount > 0)
            {
                smoke.Tick(dt);
                smoke.UploadMesh();
            }
            float afterSmoke = Time.realtimeSinceStartup;

            windowMaskSeconds += afterMask - start;
            windowRocketSeconds += afterRockets - afterMask;
            windowSmokeSeconds += afterSmoke - afterRockets;

            if (windowTime >= config.hudRefreshInterval)
            {
                RefreshHud();
            }
        }

        private void UpdateAutoCraters()
        {
            if (!autoCraters)
            {
                return;
            }
            if (mask.SolidCount < mask.Size * mask.Size * config.autoResetSolidRatio)
            {
                mask.Reset();
            }
            for (int i = 0; i < config.autoCratersPerFrame; i++)
            {
                int x = (int)rng.Range(0f, mask.Size);
                int y = (int)rng.Range(0f, mask.Size);
                mask.StampCrater(x, y, config.craterRadius, config.rimWidth);
            }
        }

        private void UpdatePointer(float dt)
        {
            float now = Time.realtimeSinceStartup;
            float x = 0f;
            float y = 0f;
            bool pressed = false;
            touchCount = Input.touchCount;
            if (touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                x = touch.position.x;
                y = touch.position.y;
                pressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                lastTouchTime = now;
            }
            else if (now - lastTouchTime > MouseAfterTouchGuard && Input.GetMouseButton(0))
            {
                Vector3 mouse = Input.mousePosition;
                x = mouse.x;
                y = mouse.y;
                pressed = true;
            }

            pointerStampTimer -= dt;
            if (!pressed || pointerStampTimer > 0f)
            {
                return;
            }
            if (RectTransformUtility.RectangleContainsScreenPoint(buttonsPanel, new Vector2(x, y), null))
            {
                return;
            }
            pointerStampTimer = config.pointerStampInterval;
            StampAtScreenPoint(x, y);
        }

        private void StampAtScreenPoint(float screenX, float screenY)
        {
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * screenWidth / screenHeight;
            float worldX = (screenX / screenWidth * 2f - 1f) * halfWidth;
            float worldY = (screenY / screenHeight * 2f - 1f) * halfHeight;
            float texelsPerUnit = mask.Size / config.worldSize;
            int maskX = (int)((worldX + config.worldSize * 0.5f) * texelsPerUnit);
            int maskY = (int)((worldY + config.worldSize * 0.5f) * texelsPerUnit);
            mask.StampCrater(maskX, maskY, config.craterRadius, config.rimWidth);
        }

        private void ApplyLayoutIfResized()
        {
            if (Screen.width == screenWidth && Screen.height == screenHeight)
            {
                return;
            }
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            float aspect = (float)screenWidth / screenHeight;
            float halfWorld = config.worldSize * 0.5f * config.fitMargin;
            bool portrait = aspect < 1f;
            mainCamera.orthographicSize = portrait ? halfWorld / aspect : halfWorld;
            canvasScaler.matchWidthOrHeight = portrait ? 0f : 1f;
        }

        private void ToggleAutoCraters()
        {
            autoCraters = !autoCraters;
        }

        private void CycleRockets()
        {
            int count = rockets.ActiveCount;
            int next = count == 0 ? config.rocketsGo : (count < config.rocketCapacity ? config.rocketCapacity : 0);
            rockets.SetActiveCount(next);
        }

        private void ToggleSmoke()
        {
            smoke.SetActiveCount(smoke.ActiveCount == 0 ? config.smokeCapacity : 0);
        }

        private void StartGoTest()
        {
            autoCraters = true;
            rockets.SetActiveCount(config.rocketsGo);
            smoke.SetActiveCount(0);
        }

        private void StartStressTest()
        {
            autoCraters = true;
            rockets.SetActiveCount(config.rocketCapacity);
            smoke.SetActiveCount(config.smokeCapacity);
        }

        private void ResetMask()
        {
            mask.Reset();
        }

        private void ClickCta()
        {
            ctaClicks++;
            PlatformBridge.InstallFullGame();
        }

        private void OnPause()
        {
            paused = true;
            pauseEvents++;
        }

        private void OnResume()
        {
            paused = false;
        }

        private void RefreshHud()
        {
            float frames = windowFrames;
            hudText.Length = 0;
            hudText.Append("FPS ").Append(Mathf.RoundToInt(frames / windowTime));
            hudText.Append(" min ").Append(Mathf.RoundToInt(1f / windowMaxDelta)).Append('\n');
            hudText.Append("rockets ").Append(rockets.ActiveCount);
            hudText.Append(" smoke ").Append(smoke.ActiveCount).Append('\n');
            hudText.Append("uploads/f*10 ").Append(Mathf.RoundToInt(windowUploads * 10f / frames));
            hudText.Append(" queue ").Append(mask.DirtyCount).Append('\n');
            AppendMilliseconds("ms mask ", windowMaskSeconds, frames);
            AppendMilliseconds(" rk ", windowRocketSeconds, frames);
            AppendMilliseconds(" sm ", windowSmokeSeconds, frames);
            hudText.Append('\n');
            hudText.Append("screen ").Append(screenWidth).Append('x').Append(screenHeight);
            hudText.Append(" touches ").Append(touchCount).Append('\n');
            hudText.Append("auto ").Append(autoCraters ? "ON" : "OFF");
            hudText.Append(" cta ").Append(ctaClicks).Append(" pause ").Append(pauseEvents);
            statsText.text = hudText.ToString();

            windowFrames = 0;
            windowUploads = 0;
            windowTime = 0f;
            windowMaxDelta = 0f;
            windowMaskSeconds = 0f;
            windowRocketSeconds = 0f;
            windowSmokeSeconds = 0f;
        }

        private void AppendMilliseconds(string label, float seconds, float frames)
        {
            hudText.Append(label).Append(Mathf.RoundToInt(seconds * MillisecondsPerSecond * 10f / frames) / 10f);
        }
    }
}
