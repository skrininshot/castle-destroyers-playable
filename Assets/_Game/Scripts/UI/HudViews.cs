using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
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
    /// Весь HUD — один меш на атласе: Luna не склеивает элементы uGUI, и 23 Image давали 23 draw call (D-071).
    /// Меш висит под верхними 10% кадра в единицах канваса. Вершины и цвета заливаются только при изменении.
    /// </summary>
    public sealed class HudView : ITickable
    {
        private const int SideCount = 2;
        private const byte SegmentOff = 0;
        private const byte SegmentOn = 1;
        private const byte SegmentFlash = 2;
        private const byte PanelAlpha = 240;
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int PillQuads = 9;
        private const int SegmentQuads = 3;
        private const int IconQuads = 7;
        private const float BoundsSize = 4000f;

        // Раскладка в единицах канваса 720×1280, начало в центре верхнего края HUD, Y вниз отрицательный.
        private const float SafeTopAnchor = 0.9f;
        private const float DesignWidth = 640f;
        private const float DesignHeight = 76f;
        private const float PanelWidth = 610f;
        private const float PanelHeight = 56f;
        private const float FrameSize = 76f;
        private const float FrameInnerSize = 62f;
        private const float IconSize = 54f;
        private const float CrossedSize = 58f;
        private const float BarWidth = 214f;
        private const float BarHeight = 26f;
        private const float BarCenterX = 128f;
        private const float SegmentGap = 4f;
        private const float PillBorder = 31f;
        private const float PillTexels = 64f;
        private const float SegmentBorder = 10f;
        private const float SegmentTexels = 32f;

        private GameConfig config;
        private MatchState match;
        private Camera cam;
        private ScreenShake cameraShake;
        private RectTransform canvas;
        private Transform meshTransform;
        private byte[][] shown;
        private float[][] flash;
        private Color32[][] painted;
        private int[] seenAlive;
        private float[] shake;
        private float[] barShift;
        private int[] firstSegmentQuad;
        private Vector3[] baseVertices;
        private Vector3[] vertices;
        private Color32[] colors;
        private Vector2[] uvs;
        private int quadCursor;
        private bool verticesDirty;
        private bool colorsDirty;
        private Color32 onColor;
        private Color32 offColor;
        private Color32 flashColor;
        private float time;

        public Mesh Mesh { get; private set; }

        public void Init(GameConfig gameConfig, MatchState matchState, Camera camera, ScreenShake screenShake, RectTransform gameCanvas, Transform hudTransform)
        {
            config = gameConfig;
            match = matchState;
            cam = camera;
            cameraShake = screenShake;
            canvas = gameCanvas;
            meshTransform = hudTransform;
            shown = new byte[SideCount][];
            flash = new float[SideCount][];
            painted = new Color32[SideCount][];
            seenAlive = new int[SideCount];
            shake = new float[SideCount];
            barShift = new float[SideCount];
            firstSegmentQuad = new int[SideCount];

            int segmentQuads = 0;
            for (int s = 0; s < SideCount; s++)
            {
                int count = match.Start((Side)s);
                shown[s] = new byte[count];
                flash[s] = new float[count];
                painted[s] = new Color32[count];
                seenAlive[s] = count;
                segmentQuads += count * SegmentQuads;
            }
            int quads = PillQuads + IconQuads + segmentQuads;
            int vertexCount = quads * VerticesPerQuad;
            baseVertices = new Vector3[vertexCount];
            vertices = new Vector3[vertexCount];
            colors = new Color32[vertexCount];
            uvs = new Vector2[vertexCount];
            BuildLayout();

            int[] triangles = new int[quads * IndicesPerQuad];
            for (int q = 0; q < quads; q++)
            {
                int v = q * VerticesPerQuad;
                int t = q * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }
            Mesh = new Mesh();
            Mesh.name = "Hud";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uvs;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
        }

        /// <summary>Сколько сегментов у стороны: по одному на пушку.</summary>
        public int SegmentCount(Side side)
        {
            return shown[(int)side].Length;
        }

        /// <summary>Текущий цвет сегмента.</summary>
        public Color32 SegmentColor(Side side, int index)
        {
            return painted[(int)side][index];
        }

        /// <summary>Смещение полосы от тряски, единицы канваса.</summary>
        public float BarShift(Side side)
        {
            return barShift[(int)side];
        }

        /// <summary>Масштаб HUD: на очень узком экране он ужимается, а не обрезается, в ландшафте меньше, чтобы не закрывать x20.</summary>
        public static float Scale(float canvasWidth, float canvasHeight, float landscapeScale)
        {
            float fit = canvasWidth / DesignWidth;
            float scale = fit < 1f ? fit : 1f;
            return canvasWidth > canvasHeight ? scale * landscapeScale : scale;
        }

        /// <summary>Нижний край HUD в долях высоты экрана.</summary>
        public static float BottomViewport(float canvasWidth, float canvasHeight, float landscapeScale)
        {
            return SafeTopAnchor - DesignHeight * Scale(canvasWidth, canvasHeight, landscapeScale) / canvasHeight;
        }

        /// <summary>Границы сегмента по X внутри HUD.</summary>
        public static void SegmentBounds(Side side, int index, int count, out float minX, out float maxX)
        {
            float left = (side == Side.Player ? -BarCenterX : BarCenterX) - BarWidth * 0.5f;
            minX = left + BarWidth * index / count + SegmentGap * 0.5f;
            maxX = left + BarWidth * (index + 1) / count - SegmentGap * 0.5f;
        }

        public void ApplyPalette(PaletteSet palette)
        {
            onColor = palette.hudHealth;
            offColor = palette.hudTrack;
            flashColor = palette.hudText;
            Color32 panel = palette.hudPanel;
            panel.a = PanelAlpha;
            FillColor(0, PillQuads, panel);
            FillColor(PillQuads, 2, palette.hudPanel);
            FillColor(PillQuads + 2, 2, palette.hudTrack);
            FillColor(PillQuads + 4, 1, palette.roofPlayer);
            FillColor(PillQuads + 5, 1, palette.rocketEnemy);
            FillColor(PillQuads + 6, 1, Color.white);
            for (int s = 0; s < SideCount; s++)
            {
                Paint(s, true);
            }
            Flush();
        }

        public void Tick(float dt)
        {
            time += dt;
            PlaceOnScreen();
            for (int s = 0; s < SideCount; s++)
            {
                TickSide(s, dt);
            }
            Flush();
        }

        private void BuildLayout()
        {
            float centerY = -DesignHeight * 0.5f;
            float frameX = DesignWidth * 0.5f - FrameSize * 0.5f;
            AddPill(0f, centerY);
            AddIcon(ArtSprites.Circle, -frameX, centerY, FrameSize);
            AddIcon(ArtSprites.Circle, frameX, centerY, FrameSize);
            AddIcon(ArtSprites.Circle, -frameX, centerY, FrameInnerSize);
            AddIcon(ArtSprites.Circle, frameX, centerY, FrameInnerSize);
            AddIcon(ArtSprites.HudTower, -frameX, centerY, IconSize);
            AddIcon(ArtSprites.HudTower, frameX, centerY, IconSize);
            AddIcon(ArtSprites.HudRockets, 0f, centerY, CrossedSize);
            for (int s = 0; s < SideCount; s++)
            {
                Side side = (Side)s;
                int count = shown[s].Length;
                firstSegmentQuad[s] = quadCursor;
                for (int i = 0; i < count; i++)
                {
                    float minX;
                    float maxX;
                    SegmentBounds(side, i, count, out minX, out maxX);
                    AddSegment(minX, maxX, centerY - BarHeight * 0.5f, centerY + BarHeight * 0.5f);
                }
            }
            for (int v = 0; v < vertices.Length; v++)
            {
                vertices[v].x = baseVertices[v].x;
                vertices[v].y = baseVertices[v].y;
            }
        }

        private void AddPill(float centerX, float centerY)
        {
            float halfWidth = PanelWidth * 0.5f;
            float halfHeight = PanelHeight * 0.5f;
            float borderX = Mathf.Min(PillBorder, halfWidth);
            float borderY = Mathf.Min(PillBorder, halfHeight);
            float[] xs = { centerX - halfWidth, centerX - halfWidth + borderX, centerX + halfWidth - borderX, centerX + halfWidth };
            float[] ys = { centerY - halfHeight, centerY - halfHeight + borderY, centerY + halfHeight - borderY, centerY + halfHeight };
            float uvBorder = PillBorder / PillTexels;
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    AddSlice(ArtSprites.HudPill, xs[column], ys[row], xs[column + 1], ys[row + 1], column, row, uvBorder, uvBorder);
                }
            }
        }

        private void AddSegment(float minX, float maxX, float minY, float maxY)
        {
            float border = Mathf.Min(SegmentBorder, (maxX - minX) * 0.5f);
            float uvBorder = SegmentBorder / SegmentTexels;
            AddSlice(ArtSprites.HudSegment, minX, minY, minX + border, maxY, 0, 1, uvBorder, 0f);
            AddSlice(ArtSprites.HudSegment, minX + border, minY, maxX - border, maxY, 1, 1, uvBorder, 0f);
            AddSlice(ArtSprites.HudSegment, maxX - border, minY, maxX, maxY, 2, 1, uvBorder, 0f);
        }

        private void AddIcon(int sprite, float centerX, float centerY, float size)
        {
            float half = size * 0.5f;
            int v = quadCursor * VerticesPerQuad;
            WriteQuad(v, centerX - half, centerY - half, centerX + half, centerY + half);
            AtlasUv.SetStrip(uvs, v, sprite);
            quadCursor++;
        }

        /// <summary>Клетка сетки 3×3 спрайта: середина растягивается, края берут поля uvBorderX и uvBorderY. Строка 1 без полей по Y — весь спрайт.</summary>
        private void AddSlice(int sprite, float minX, float minY, float maxX, float maxY, int column, int row, float uvBorderX, float uvBorderY)
        {
            int r = sprite * 4;
            float[] source = ArtSprites.Uv;
            float spanU = source[r + 2] - source[r];
            float spanV = source[r + 3] - source[r + 1];
            float left = source[r] + (column == 0 ? 0f : (column == 1 ? uvBorderX : 1f - uvBorderX)) * spanU;
            float right = source[r] + (column == 0 ? uvBorderX : (column == 1 ? 1f - uvBorderX : 1f)) * spanU;
            float bottom = source[r + 1] + (row == 0 ? 0f : (row == 1 ? uvBorderY : 1f - uvBorderY)) * spanV;
            float top = source[r + 1] + (row == 0 ? uvBorderY : (row == 1 ? 1f - uvBorderY : 1f)) * spanV;
            int v = quadCursor * VerticesPerQuad;
            WriteQuad(v, minX, minY, maxX, maxY);
            uvs[v].x = left;
            uvs[v].y = bottom;
            uvs[v + 1].x = right;
            uvs[v + 1].y = bottom;
            uvs[v + 2].x = left;
            uvs[v + 2].y = top;
            uvs[v + 3].x = right;
            uvs[v + 3].y = top;
            quadCursor++;
        }

        private void WriteQuad(int v, float minX, float minY, float maxX, float maxY)
        {
            baseVertices[v].x = minX;
            baseVertices[v].y = minY;
            baseVertices[v + 1].x = maxX;
            baseVertices[v + 1].y = minY;
            baseVertices[v + 2].x = minX;
            baseVertices[v + 2].y = maxY;
            baseVertices[v + 3].x = maxX;
            baseVertices[v + 3].y = maxY;
        }

        private void FillColor(int firstQuad, int quads, Color32 color)
        {
            int last = (firstQuad + quads) * VerticesPerQuad;
            for (int v = firstQuad * VerticesPerQuad; v < last; v++)
            {
                colors[v] = color;
            }
            colorsDirty = true;
        }

        /// <summary>Ставит HUD на кадр камеры без её тряски.</summary>
        private void PlaceOnScreen()
        {
            float height = canvas.rect.height;
            float halfHeight = cam.orthographicSize;
            float size = Scale(canvas.rect.width, height, config.hudLandscapeScale) * halfHeight * 2f / height;
            Vector3 center = cam.transform.position;
            meshTransform.position = new Vector3(center.x - cameraShake.OffsetX, center.y - cameraShake.OffsetY + halfHeight * (SafeTopAnchor * 2f - 1f), 0f);
            meshTransform.localScale = new Vector3(size, size, 1f);
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

            if (shake[s] > 0f || barShift[s] != 0f)
            {
                shake[s] -= dt;
                float strength = shake[s] > 0f ? shake[s] / config.hudShakeTime : 0f;
                barShift[s] = Mathf.Sin(time * config.hudShakeFrequency) * config.hudShakeAmplitude * strength;
                ShiftBar(s);
            }
        }

        private void ShiftBar(int s)
        {
            int first = firstSegmentQuad[s] * VerticesPerQuad;
            int last = first + shown[s].Length * SegmentQuads * VerticesPerQuad;
            float shift = barShift[s];
            for (int v = first; v < last; v++)
            {
                vertices[v].x = baseVertices[v].x + shift;
            }
            verticesDirty = true;
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
                    Color32 color = state == SegmentFlash ? flashColor : (state == SegmentOn ? onColor : offColor);
                    painted[s][index] = color;
                    FillColor(firstSegmentQuad[s] + index * SegmentQuads, SegmentQuads, color);
                }
            }
        }

        private void Flush()
        {
            if (verticesDirty)
            {
                Mesh.vertices = vertices;
                verticesDirty = false;
            }
            if (colorsDirty)
            {
                Mesh.colors32 = colors;
                colorsDirty = false;
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
#if GAME_DEBUG
        /// <summary>Нижняя доля экрана под открытой debug-панелью: тапы там не ведут в стор.</summary>
        public float UiBand { get; set; }
#endif

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
            return y >= screenHeight * UiBand && y <= screenHeight * (1f - config.debugToggleBand);
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
