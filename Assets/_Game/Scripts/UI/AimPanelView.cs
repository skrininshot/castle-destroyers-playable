using CastleDestroyers.Core;
using CastleDestroyers.InputRig;
using CastleDestroyers.Level;
using UnityEngine;
using UnityEngine.UI;

namespace CastleDestroyers.UI
{
    /// <summary>
    /// Плашка POWER / ANGLE (ТЗ §3.2). Висит над замком игрока и показана во время жеста и показа подсказки.
    /// Строки чисел готовятся заранее, чтобы кадр не аллоцировал. Знак градуса — спрайт: Luna не запекает «°» в шрифт.
    /// </summary>
    public sealed class AimPanelView : ITickable
    {
        private const int MaxPercent = 100;
        private const int MaxDegrees = 90;
        private const int TwoDigits = 10;
        private const float DegreeOffsetOne = 22f;
        private const float DegreeOffsetTwo = 38f;
        private const float DegreeRaise = 14f;

        private GameConfig config;
        private AimInput aim;
        private LevelScene level;
        private Camera cam;
        private GameObject root;
        private RectTransform rect;
        private Image background;
        private Text powerText;
        private Text angleText;
        private Text[] captions;
        private Image degree;
        private string[] percentLabels;
        private string[] degreeLabels;
        private int shownPower = -1;
        private int shownAngle = -1;

        public void Init(GameConfig gameConfig, AimInput aimInput, LevelScene levelScene, Camera camera, RectTransform panel,
            Image panelBackground, Text power, Text angle, Text[] panelCaptions, Image degreeMark)
        {
            config = gameConfig;
            aim = aimInput;
            level = levelScene;
            cam = camera;
            rect = panel;
            root = panel.gameObject;
            background = panelBackground;
            powerText = power;
            angleText = angle;
            captions = panelCaptions;
            degree = degreeMark;

            percentLabels = new string[MaxPercent + 1];
            for (int i = 0; i <= MaxPercent; i++)
            {
                percentLabels[i] = i + "%";
            }
            degreeLabels = new string[MaxDegrees + 1];
            for (int i = 0; i <= MaxDegrees; i++)
            {
                degreeLabels[i] = i.ToString();
            }
            root.SetActive(false);
        }

        public void ApplyPalette(PaletteSet palette)
        {
            background.color = palette.hudText;
            powerText.color = palette.hudPanel;
            angleText.color = palette.hudPanel;
            degree.color = palette.hudPanel;
            for (int i = 0; i < captions.Length; i++)
            {
                captions[i].color = palette.hudPanel;
            }
        }

        public void Tick(float dt)
        {
            bool visible = aim.Showing;
            if (root.activeSelf != visible)
            {
                root.SetActive(visible);
            }
            if (!visible)
            {
                return;
            }

            int power = Mathf.RoundToInt(aim.ShownPower * MaxPercent);
            int angle = Mathf.RoundToInt(aim.ShownAngle);
            if (power != shownPower)
            {
                shownPower = power;
                powerText.text = percentLabels[power];
            }
            if (angle != shownAngle)
            {
                shownAngle = angle;
                angleText.text = degreeLabels[angle];
                degree.rectTransform.anchoredPosition = new Vector2(angle < TwoDigits ? DegreeOffsetOne : DegreeOffsetTwo, DegreeRaise);
            }

            // Якорь в долях экрана не зависит от режима канваса и масштаба UI.
            float worldX = level.PlayerFocusX + config.aimPanelOffset.x;
            float worldY = level.PlayerFocusY + config.aimPanelOffset.y;
            Vector3 viewport = cam.WorldToViewportPoint(new Vector3(worldX, worldY, 0f));
            Vector2 anchor = new Vector2(Mathf.Clamp01(viewport.x), Mathf.Clamp01(viewport.y));
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
        }
    }
}
