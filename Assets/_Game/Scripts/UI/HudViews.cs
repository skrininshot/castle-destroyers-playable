using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace CastleDestroyers.UI
{
    /// <summary>Плавности UI (ТЗ §4.1): появление с упругим перелётом.</summary>
    public static class UiEase
    {
        private const float BackOvershoot = 1.70158f;

        /// <summary>Ease-out-back: от 0 к 1 с перелётом за единицу.</summary>
        public static float OutBack(float t)
        {
            float clamped = t < 0f ? 0f : (t > 1f ? 1f : t);
            float u = clamped - 1f;
            return 1f + (BackOvershoot + 1f) * u * u * u + BackOvershoot * u * u;
        }
    }

    /// <summary>
    /// Полосы здоровья сторон (ТЗ §3.12): сегмент = пушка. Потерянный сегмент мигает белым и трясёт полосу.
    /// Цвета пишутся только при смене состояния сегмента.
    /// </summary>
    public sealed class HudView : ITickable
    {
        private const int SideCount = 2;
        private const byte SegmentOff = 0;
        private const byte SegmentOn = 1;
        private const byte SegmentFlash = 2;
        private const float DesignWidth = 640f;
        private const float SegmentGap = 4f;
        private const byte PanelAlpha = 240;

        private GameConfig config;
        private MatchState match;
        private RectTransform root;
        private RectTransform[] bars;
        private Image[][] segments;
        private byte[][] shown;
        private float[][] flash;
        private int[] seenAlive;
        private float[] shake;
        private Image[] panels;
        private Image[] frames;
        private Image[] frameInners;
        private Image[] icons;
        private Image crossedRockets;
        private Color32 onColor;
        private Color32 offColor;
        private Color32 flashColor;
        private float time;

        public void Init(GameConfig gameConfig, MatchState matchState, RectTransform hudRoot, RectTransform[] sideBars, Image[] playerSegments, Image[] enemySegments,
            Image[] hudPanels, Image[] hudFrames, Image[] hudFrameInners, Image[] castleIcons, Image crossed)
        {
            config = gameConfig;
            match = matchState;
            root = hudRoot;
            bars = sideBars;
            panels = hudPanels;
            frames = hudFrames;
            frameInners = hudFrameInners;
            icons = castleIcons;
            crossedRockets = crossed;
            segments = new[] { playerSegments, enemySegments };
            shown = new byte[SideCount][];
            flash = new float[SideCount][];
            seenAlive = new int[SideCount];
            shake = new float[SideCount];
            for (int s = 0; s < SideCount; s++)
            {
                int count = match.Start((Side)s);
                shown[s] = new byte[segments[s].Length];
                flash[s] = new float[segments[s].Length];
                seenAlive[s] = count;
                LayOut(segments[s], count);
            }
        }

        public void ApplyPalette(PaletteSet palette)
        {
            onColor = palette.hudHealth;
            offColor = palette.hudTrack;
            flashColor = palette.hudText;
            Color32 panel = palette.hudPanel;
            panel.a = PanelAlpha;
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].color = panel;
            }
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].color = palette.hudPanel;
            }
            for (int i = 0; i < frameInners.Length; i++)
            {
                frameInners[i].color = palette.hudTrack;
            }
            icons[(int)Side.Player].color = palette.roofPlayer;
            icons[(int)Side.Enemy].color = palette.rocketEnemy;
            crossedRockets.color = Color.white;
            for (int s = 0; s < SideCount; s++)
            {
                Paint(s, true);
            }
        }

        public void Tick(float dt)
        {
            time += dt;
            FitWidth();
            for (int s = 0; s < SideCount; s++)
            {
                TickSide(s, dt);
            }
        }

        /// <summary>Сегменты равной ширины на всю полосу; лишние выключены.</summary>
        private static void LayOut(Image[] bar, int count)
        {
            for (int i = 0; i < bar.Length; i++)
            {
                bool used = i < count;
                bar[i].gameObject.SetActive(used);
                if (used)
                {
                    RectTransform rect = bar[i].rectTransform;
                    rect.anchorMin = new Vector2((float)i / count, 0f);
                    rect.anchorMax = new Vector2((float)(i + 1) / count, 1f);
                    rect.offsetMin = new Vector2(SegmentGap * 0.5f, 0f);
                    rect.offsetMax = new Vector2(-SegmentGap * 0.5f, 0f);
                }
            }
        }

        /// <summary>На очень узком экране HUD ужимается, а не обрезается.</summary>
        private void FitWidth()
        {
            RectTransform parent = root.parent as RectTransform;
            float fit = parent.rect.width / DesignWidth;
            float scale = fit < 1f ? fit : 1f;
            if (parent.rect.width > parent.rect.height)
            {
                scale *= config.hudLandscapeScale;
            }
            if (root.localScale.x != scale)
            {
                root.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void TickSide(int s, float dt)
        {
            Side side = (Side)s;
            int alive = match.Alive(side);
            int count = match.Start(side);
            if (alive < seenAlive[s])
            {
                for (int k = alive; k < seenAlive[s]; k++)
                {
                    flash[s][SegmentIndex(s, k, count)] = config.hudFlashTime;
                }
                shake[s] = config.hudShakeTime;
            }
            seenAlive[s] = alive;

            float[] timers = flash[s];
            for (int i = 0; i < count; i++)
            {
                if (timers[i] > 0f)
                {
                    timers[i] -= dt;
                }
            }
            Paint(s, false);

            if (shake[s] > 0f || bars[s].localPosition.x != 0f)
            {
                shake[s] -= dt;
                float strength = shake[s] > 0f ? shake[s] / config.hudShakeTime : 0f;
                bars[s].localPosition = new Vector3(Mathf.Sin(time * config.hudShakeFrequency) * config.hudShakeAmplitude * strength, 0f, 0f);
            }
        }

        /// <summary>Индекс сегмента k-й по счёту пушки: синяя полоса гаснет к центру, красная — зеркально.</summary>
        private static int SegmentIndex(int s, int k, int count)
        {
            return s == (int)Side.Player ? k : count - 1 - k;
        }

        private void Paint(int s, bool force)
        {
            Side side = (Side)s;
            int alive = match.Alive(side);
            int count = match.Start(side);
            for (int k = 0; k < count; k++)
            {
                int index = SegmentIndex(s, k, count);
                byte state = flash[s][index] > 0f ? SegmentFlash : (k < alive ? SegmentOn : SegmentOff);
                if (force || shown[s][index] != state)
                {
                    shown[s][index] = state;
                    segments[s][index].color = state == SegmentFlash ? flashColor : (state == SegmentOn ? onColor : offColor);
                }
            }
        }
    }

    /// <summary>Плашка хода (ТЗ §3.1.1, §3.12): YOUR TURN! / ENEMY TURN на секунду, VICTORY! до EndCard.</summary>
    public sealed class TurnBannerView : ITickable
    {
        public const string YourTurn = "YOUR TURN!";
        public const string EnemyTurn = "ENEMY TURN";
        public const string Victory = "VICTORY!";
        public const string Defeat = "DEFEAT";

        private const byte PanelAlpha = 240;

        private GameConfig config;
        private SessionFlow flow;
        private RectTransform root;
        private GameObject rootObject;
        private Image background;
        private Text label;
        private float age;
        private float life;
        private bool waitingForEnding;

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, RectTransform bannerRoot, Image bannerBackground, Text bannerText)
        {
            config = gameConfig;
            flow = sessionFlow;
            root = bannerRoot;
            rootObject = bannerRoot.gameObject;
            background = bannerBackground;
            label = bannerText;
            flow.StateChanged += OnStateChanged;
            rootObject.SetActive(false);
        }

        public void ApplyPalette(PaletteSet palette)
        {
            Color32 panel = palette.hudPanel;
            panel.a = PanelAlpha;
            background.color = panel;
            label.color = palette.hudText;
        }

        public void Tick(float dt)
        {
            if (waitingForEnding && flow.StateTime >= config.victoryLoserHold)
            {
                waitingForEnding = false;
                Show(flow.Winner == Side.Player ? Victory : Defeat, flow.Duration(flow.State, flow.ActiveSide));
            }
            if (!rootObject.activeSelf)
            {
                return;
            }
            age += dt;
            if (age >= life)
            {
                rootObject.SetActive(false);
                return;
            }
            float scale = UiEase.OutBack(age / config.uiPopTime);
            root.localScale = new Vector3(scale, scale, 1f);
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            waitingForEnding = false;
            if (state == SessionState.Aim)
            {
                Show(side == Side.Player ? YourTurn : EnemyTurn, config.bannerDuration);
            }
            else if (state == SessionState.Victory || state == SessionState.Defeat)
            {
                waitingForEnding = true;
            }
            else if (state == SessionState.EndCard || state == SessionState.Boot)
            {
                rootObject.SetActive(false);
            }
        }

        private void Show(string text, float seconds)
        {
            label.text = text;
            age = 0f;
            life = seconds;
            root.localScale = Vector3.zero;
            rootObject.SetActive(true);
        }
    }

    /// <summary>Рука-подсказка: идёт по жесту «оттянуть» от точки handStartViewport (ТЗ §3.2). Положение задано долями экрана.</summary>
    public sealed class HandView : ITickable
    {
        private const float PopShare = 0.12f;

        private GameConfig config;
        private TurnGuide guide;
        private Camera cam;
        private RectTransform rect;
        private GameObject rootObject;
        private Image[] tinted;

        public void Init(GameConfig gameConfig, TurnGuide turnGuide, Camera camera, RectTransform handRoot, Image[] handImages)
        {
            config = gameConfig;
            guide = turnGuide;
            cam = camera;
            rect = handRoot;
            rootObject = handRoot.gameObject;
            tinted = handImages;
            rootObject.SetActive(false);
        }

        public void ApplyPalette(PaletteSet palette)
        {
            tinted[0].color = palette.hudPanel;
            tinted[1].color = palette.hudText;
        }

        public void Tick(float dt)
        {
            bool visible = guide.HandVisible;
            if (rootObject.activeSelf != visible)
            {
                rootObject.SetActive(visible);
            }
            if (!visible)
            {
                return;
            }
            float radians = guide.Hint.handAngleDeg * Mathf.Deg2Rad;
            float reach = guide.Hint.handPower01 * config.aimMaxDragScreenFraction * guide.HandDrag;
            float x = config.handStartViewport.x - Mathf.Cos(radians) * reach / cam.aspect;
            float y = config.handStartViewport.y - Mathf.Sin(radians) * reach;
            Vector2 anchor = new Vector2(x, y);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            float scale = UiEase.OutBack(guide.HandProgress / PopShare);
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }

    /// <summary>
    /// EndCard (ТЗ §3.11): плоский оверлей, логотип, замок победителя на круге, пульсирующая PLAY NOW. Тап в любом месте после
    /// короткой паузы ведёт в стор. Это единственное место, откуда вызывается CTA (ТЗ §3.1.1).
    /// </summary>
    public sealed class EndCardView : ITickable
    {
        public const string TryAgain = "TRY AGAIN IN THE FULL GAME!";

        private const float LandscapeScale = 0.62f;

        // Ландшафт: логотип, надпись и кнопка слева, замок на круге справа. Порядок как у layout в Init.
        private static readonly float[] LandscapeAnchors = { 0.3f, 0.68f, 0.72f, 0.5f, 0.72f, 0.5f, 0.3f, 0.4f, 0.3f, 0.24f };

        private readonly InputRig.PointerReader pointer = new InputRig.PointerReader();
        private GameConfig config;
        private SessionFlow flow;
        private GameObject rootObject;
        private Image dim;
        private RectTransform content;
        private RectTransform button;
        private Image winnerIcon;
        private Text caption;
        private Image buttonFace;
        private Image buttonEdge;
        private Text buttonText;
        private Image disc;
        private Sprite blueWinner;
        private Sprite redWinner;
        private RectTransform[] layout;
        private Vector2[] portraitAnchors;
        private bool landscapeLaidOut;
        private PaletteSet palette;
        private Color dimColor;
        private float time;
        private bool wasPressed;

        /// <summary>Устройство не читается (скриншоты, тесты): нажатия подаются через Feed.</summary>
        public bool Scripted { get; set; }
        /// <summary>Звук нажатия CTA. В тестах не задан.</summary>
        public Platform.GameSounds Sounds { get; set; }
        /// <summary>Сколько раз сработал CTA.</summary>
        public int CtaCount { get; private set; }

        public void Init(GameConfig gameConfig, SessionFlow sessionFlow, GameObject root, Image dimImage, RectTransform contentRoot, RectTransform ctaButton,
            Image iconImage, Text captionText, Image ctaFace, Image ctaEdge, Text ctaText, Image discImage, Sprite blueSprite, Sprite redSprite, RectTransform[] layoutRects)
        {
            config = gameConfig;
            flow = sessionFlow;
            rootObject = root;
            dim = dimImage;
            content = contentRoot;
            button = ctaButton;
            winnerIcon = iconImage;
            caption = captionText;
            buttonFace = ctaFace;
            buttonEdge = ctaEdge;
            buttonText = ctaText;
            disc = discImage;
            blueWinner = blueSprite;
            redWinner = redSprite;
            layout = layoutRects;
            portraitAnchors = new Vector2[layout.Length];
            for (int i = 0; i < layout.Length; i++)
            {
                portraitAnchors[i] = layout[i].anchorMin;
            }
            flow.StateChanged += OnStateChanged;
            rootObject.SetActive(false);
        }

        public void ApplyPalette(PaletteSet paletteSet)
        {
            palette = paletteSet;
            dimColor = palette.hudTrack;
            buttonFace.color = palette.hudGold;
            buttonEdge.color = palette.hudGoldShade;
            buttonText.color = palette.hudTrack;
            disc.color = palette.hudPanel;
            caption.color = palette.hudText;
            if (rootObject.activeSelf)
            {
                PaintWinner();
            }
        }

        public void Tick(float dt)
        {
            if (!rootObject.activeSelf)
            {
                return;
            }
            time += dt;
            ApplyOrientation();
            Color color = dimColor;
            color.a = Mathf.Clamp01(time / config.endCardFadeTime) * config.endCardDimAlpha;
            dim.color = color;
            float pop = UiEase.OutBack(time / config.uiPopTime);
            content.localScale = new Vector3(pop, pop, 1f);
            float pulse = (1f + Mathf.Sin(time * config.ctaPulseSpeed) * config.ctaPulse) * (landscapeLaidOut ? LandscapeScale : 1f);
            button.localScale = new Vector3(pulse, pulse, 1f);
            if (!Scripted)
            {
                pointer.Read();
                Feed(pointer.Pressed, pointer.Y, Screen.height);
            }
        }

        /// <summary>Раскладка под ориентацию: перестраивается только при смене, в том числе на лету.</summary>
        public void ApplyOrientation()
        {
            bool landscape = content.rect.width > content.rect.height;
            if (landscape == landscapeLaidOut)
            {
                return;
            }
            landscapeLaidOut = landscape;
            float scale = landscape ? LandscapeScale : 1f;
            for (int i = 0; i < layout.Length; i++)
            {
                Vector2 anchor = landscape ? new Vector2(LandscapeAnchors[i * 2], LandscapeAnchors[i * 2 + 1]) : portraitAnchors[i];
                layout[i].anchorMin = anchor;
                layout[i].anchorMax = anchor;
                layout[i].localScale = new Vector3(scale, scale, 1f);
            }
        }

        /// <summary>Один кадр ввода. Тап считается по новому нажатию: палец, оставшийся с игры, в стор не ведёт.</summary>
        public void Feed(bool pressed, float y, float screenHeight)
        {
            if (pressed && !wasPressed && time >= config.endCardTapDelay && CanTapAt(y, screenHeight))
            {
                CtaCount++;
                if (Sounds != null)
                {
                    Sounds.Play(Platform.SoundId.Click);
                }
                Platform.PlatformBridge.InstallFullGame();
            }
            wasPressed = pressed;
        }

        private bool CanTapAt(float y, float screenHeight)
        {
#if GAME_DEBUG
            return y >= screenHeight * config.debugBlastUiBand;
#else
            return true;
#endif
        }

        private void OnStateChanged(SessionState state, Side side)
        {
            bool show = state == SessionState.EndCard;
            rootObject.SetActive(show);
            if (show)
            {
                time = 0f;
                wasPressed = true;
                bool won = flow.HasWinner && flow.Winner == Side.Player;
                caption.text = won ? "" : TryAgain;
                PaintWinner();
            }
        }

        private void PaintWinner()
        {
            bool enemy = flow.HasWinner && flow.Winner == Side.Enemy;
            winnerIcon.sprite = enemy ? redWinner : blueWinner;
        }
    }
}
