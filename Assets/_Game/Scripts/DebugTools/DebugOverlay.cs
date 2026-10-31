#if GAME_DEBUG
using System.Text;
using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace CastleDestroyers.DebugTools
{
    /// <summary>
    /// Debug-оверлей: FPS, состояние FSM, ходы, счётчики и чит-кнопки (следующее состояние, замедление,
    /// стресс 1000 ракет, смена темы, рестарт, радиус взрыва, камера, сценарии бустеров). Панель открывается кнопкой DEV.
    /// Целиком вырезается из сборки без GAME_DEBUG.
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour, ITickable
    {
        private const int HudTextCapacity = 512;
        private const float PortraitMatch = 0f;
        private const float LandscapeMatch = 1f;

        public CanvasScaler canvasScaler;
        public GameObject panelRoot;
        public Button toggleButton;
        public Text statsText;
        public Button nextStateButton;
        public Button slowButton;
        public Button rocketsButton;
        public Button themeButton;
        public Button restartButton;
        public Button blastButton;
        public Button cameraButton;
        public Button shotX20Button;
        public Button shotX10Button;
        public Button shotX50Button;
        public Text blastLabel;
        public Text cameraLabel;
        public Text rocketsLabel;

        private GameBootstrap game;
        private StringBuilder text;
        private int screenWidth;
        private int screenHeight;
        private int windowFrames;
        private float windowTime;
        private float windowMaxDelta;
        private bool slow;
        private static readonly string[] CameraNames = { "CAM AUTO", "CAM MTN", "CAM BLUE", "CAM RED", "CAM ALL" };

        public void Init(GameBootstrap bootstrap)
        {
            game = bootstrap;
            text = new StringBuilder(HudTextCapacity);
            toggleButton.onClick.AddListener(OnTogglePanel);
            nextStateButton.onClick.AddListener(OnNextState);
            slowButton.onClick.AddListener(OnToggleSlow);
            rocketsButton.onClick.AddListener(OnSpawnRockets);
            themeButton.onClick.AddListener(OnToggleTheme);
            restartButton.onClick.AddListener(OnRestart);
            blastButton.onClick.AddListener(OnNextBlast);
            cameraButton.onClick.AddListener(OnNextCamera);
            shotX20Button.onClick.AddListener(OnShotX20);
            shotX10Button.onClick.AddListener(OnShotX10);
            shotX50Button.onClick.AddListener(OnShotX50);
            UpdateBlastLabel();
            UpdateStressLabel();
            SetPanelVisible(false);
        }

        /// <summary>Открытая панель занимает низ экрана: тапы там не должны доходить до прицела и CTA.</summary>
        public void SetPanelVisible(bool visible)
        {
            panelRoot.SetActive(visible);
            float band = visible ? game.config.debugBlastUiBand : 0f;
            game.Aim.UiBand = band;
            game.EndCard.UiBand = band;
            game.Blaster.UiBand = band;
        }

        public void Tick(float dt)
        {
            float realDelta = Time.unscaledDeltaTime;
            windowFrames++;
            windowTime += realDelta;
            if (realDelta > windowMaxDelta)
            {
                windowMaxDelta = realDelta;
            }
            ApplyLayoutIfResized();
            if (windowTime >= game.config.debugHudRefresh)
            {
                Refresh();
            }
        }

        private void OnTogglePanel()
        {
            SetPanelVisible(!panelRoot.activeSelf);
        }

        /// <summary>Немедленно обновляет текст (для editor-захвата, где нет реального времени кадра).</summary>
        public void ForceRefresh()
        {
            if (windowFrames == 0)
            {
                windowFrames = 1;
                windowTime = 1f / 60f;
            }
            Refresh();
        }

        private void ApplyLayoutIfResized()
        {
            if (Screen.width == screenWidth && Screen.height == screenHeight)
            {
                return;
            }
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            canvasScaler.matchWidthOrHeight = screenWidth < screenHeight ? PortraitMatch : LandscapeMatch;
        }

        private void Refresh()
        {
            GameConfig config = game.config;
            SessionFlow flow = game.Flow;
            MatchState match = game.Match;
            float fps = windowFrames / windowTime;
            float minFps = windowMaxDelta > 0f ? 1f / windowMaxDelta : 0f;

            text.Length = 0;
            text.Append("FPS ").Append(Mathf.RoundToInt(fps));
            text.Append(" min ").Append(Mathf.RoundToInt(minFps));
            text.Append("  scale ").Append(game.Loop.TimeScale.ToString("0.00")).Append('\n');

            text.Append("state ").Append(flow.State.ToString());
            text.Append(" (").Append(flow.ActiveSide.ToString()).Append(')');
            text.Append("  t ").Append(flow.StateTime.ToString("0.0"));
            text.Append('/').Append(Mathf.Max(0f, flow.Duration(flow.State, flow.ActiveSide)).ToString("0.0")).Append('\n');

            text.Append("turn ").Append(flow.PlayerTurn).Append('/').Append(config.maxPlayerTurns);
            TurnHint hint = flow.CurrentHint;
            if (hint != null && flow.PlayerTurn > 0)
            {
                text.Append("  hint: ").Append(hint.label);
            }
            text.Append('\n');

            text.Append("cannons P ").Append(match.PlayerAlive).Append('/').Append(match.PlayerStart);
            text.Append("  E ").Append(match.EnemyAlive).Append('/').Append(match.EnemyStart).Append('\n');

            text.Append("session ").Append(flow.SessionTime.ToString("0.0")).Append('/').Append(config.maxSessionSeconds.ToString("0"));
            text.Append("  end ").Append(flow.EndReason.ToString()).Append('\n');

            text.Append("tiles ").Append(game.Terrain.LastUploads).Append(" queue ").Append(game.Terrain.Mask.DirtyCount);
            text.Append("  chunks ").Append(game.Terrain.ChunksActive);
            text.Append("  fx ").Append(game.Particles.ActiveCount).Append('\n');

            text.Append("rockets ").Append(game.Rockets.ActiveCount).Append('/').Append(game.Rockets.Budget);
            text.Append("  smoke ").Append(game.Smoke.ActiveCount);
            text.Append("  boom ").Append(game.Explosions.ActiveCount).Append('\n');

            text.Append("boost honest ").Append(Mathf.RoundToInt(game.Boosters.HonestTotal));
            text.Append("  spawned ").Append(game.Boosters.SpawnedTotal).Append('\n');

            text.Append("castle chunks ").Append(game.Castles.ChunksActive);
            text.Append("  falling ").Append(game.Cannons.FallingCount);
            text.Append("  E ").Append(game.Castles.Body(Side.Enemy).State.ToString());
            text.Append(" sway ").Append((game.Castles.Body(Side.Enemy).Sway * Mathf.Rad2Deg).ToString("0.0")).Append('\n');

            text.Append("theme ").Append(game.ActivePreset.ToString());
            text.Append("  ").Append(screenWidth).Append('x').Append(screenHeight);
            statsText.text = text.ToString();

            windowFrames = 0;
            windowTime = 0f;
            windowMaxDelta = 0f;
        }

        private void OnNextState()
        {
            game.Flow.Skip();
        }

        private void OnToggleSlow()
        {
            slow = !slow;
            game.Loop.TimeScale = slow ? game.config.debugSlowScale : 1f;
        }

        private void OnSpawnRockets()
        {
            game.Stress.SetActive(!game.Stress.Active);
            UpdateStressLabel();
        }

        private void UpdateStressLabel()
        {
            rocketsLabel.text = game.Stress.Active ? "STRESS ON" : "STRESS OFF";
        }

        private void OnToggleTheme()
        {
            game.SetPreset(game.ActivePreset == ThemePreset.Night ? ThemePreset.Sunset : ThemePreset.Night);
        }

        private void OnRestart()
        {
            game.Restart();
            UpdateStressLabel();
        }

        private void OnShotX20()
        {
            RunScenario(0);
        }

        private void OnShotX10()
        {
            RunScenario(1);
        }

        private void OnShotX50()
        {
            RunScenario(2);
        }

        private void RunScenario(int scenario)
        {
            OnRestart();
            game.Scenarios.Run(scenario);
        }

        private void OnNextBlast()
        {
            game.Blaster.NextRadius();
            game.Aim.Blocked = game.Blaster.Active;
            UpdateBlastLabel();
        }

        private void UpdateBlastLabel()
        {
            blastLabel.text = game.Blaster.Active ? "BLAST " + game.Blaster.Radius.ToString("0.0") : "BLAST OFF";
        }

        private void OnNextCamera()
        {
            game.CameraDirector.DebugView = (game.CameraDirector.DebugView + 1) % CameraDirector.DebugFrames;
            cameraLabel.text = CameraNames[game.CameraDirector.DebugView];
        }
    }
}
#endif
