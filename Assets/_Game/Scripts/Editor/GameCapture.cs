using System.IO;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SessionState = CastleDestroyers.Flow.SessionState;

namespace CastleDestroyers.EditorTools
{
    /// <summary>
    /// Снимает кадры сцены для отчёта: играет матч по подсказкам и рендерит камеру вместе с UI в PNG.
    /// Запуск без -nographics: <c>-executeMethod CastleDestroyers.EditorTools.GameCapture.CaptureAll</c>.
    /// </summary>
    public static class GameCapture
    {
        private const string OutputDir = "docs/reports/img";
        private const string FilePrefix = "stage06_";
        private const int PortraitWidth = 720;
        private const int PortraitHeight = 1280;
        private const int LandscapeWidth = 1280;
        private const int LandscapeHeight = 720;
        private const int DepthBits = 24;
        private const float StepSeconds = 0.02f;

        private const int MaxSkips = 32;
        private const float MaxWaitSeconds = 12f;
        private const float MaxMatchSeconds = 90f;
        private const int CapturedCanvasOrder = 100;
        private const float HonestAtTrigger = 300f;

        // Кадр debug-камеры (CameraDirector.DebugView).
        private const int ViewRed = 3;

        [MenuItem("Tools/Castle Destroyers/Capture Stage Screenshots")]
        public static void CaptureAll()
        {
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
            GameBootstrap game = Object.FindFirstObjectByType<GameBootstrap>();
            game.Boot();
            game.Aim.Scripted = true;
            game.EndCard.Scripted = true;
            Directory.CreateDirectory(OutputDir);

            // Матч целиком, камера по состояниям. Ход 1: рука, плашка хода, навес через x20.
            Fresh(game, ThemePreset.Night);
            WaitForState(game, SessionState.Aim, Side.Player);
            Settle(game, 0.2f);
            Auto(game, "01_your_turn", false);
            Settle(game, game.config.handStartDelay + game.config.handDemoDuration * 0.55f);
            Auto(game, "02_hand_turn1", false);
            FireHint(game);
            WaitForHead(game, -3f);
            Auto(game, "03_center_over_x20", false);
            Auto(game, "04_center_landscape", true);
            WaitForState(game, SessionState.Resolve, Side.Player);
            Auto(game, "05_after_turn1_hud", false);

            // Ход ИИ: раздумье, дуга выше x20 в центральном кадре, попадание по синим.
            WaitForState(game, SessionState.Aim, Side.Enemy);
            Settle(game, 0.3f);
            Auto(game, "06_enemy_turn", false);
            WaitForState(game, SessionState.Flight, Side.Enemy);
            WaitForHead(game, 3f);
            Auto(game, "07_enemy_center_high", false);
            WaitForState(game, SessionState.Resolve, Side.Enemy);
            Auto(game, "08_blue_after_ai", false);

            // Ход 2: показ горы с подсвеченным x50, рука, выстрел в склон.
            WaitForState(game, SessionState.Aim, Side.Player);
            Settle(game, 0.6f);
            Auto(game, "09_pan_x50", false);
            Settle(game, game.config.hintPanDuration + game.config.handStartDelay + game.config.handDemoDuration * 0.6f - 0.6f);
            Auto(game, "10_hand_turn2", false);
            FireHint(game);
            WaitForHonest(game, game.Boosters.HonestTotal + HonestAtTrigger);
            Auto(game, "11_x50_avalanche", false);
            WaitForState(game, SessionState.Resolve, Side.Player);
            Auto(game, "12_after_turn2", false);
            WaitForState(game, SessionState.Resolve, Side.Enemy);

            // Ход 3 и победа.
            WaitForState(game, SessionState.Aim, Side.Player);
            FireHint(game);
            WaitForState(game, SessionState.Victory, Side.Player);
            Settle(game, 0.3f);
            Auto(game, "13_loser", false);
            Settle(game, game.config.victoryLoserHold + 0.5f);
            Auto(game, "14_victory", false);
            WaitForFinish(game);
            Settle(game, 1f);
            Auto(game, "15_endcard", false);
            Auto(game, "16_endcard_landscape", true);

            // Поражение при выключенном Director.
            Fresh(game, ThemePreset.Night);
            game.config.directorEnabled = false;
            WaitForState(game, SessionState.Aim, Side.Player);
            for (int i = 0; i < MaxSkips && !(game.Flow.State == SessionState.Resolve && game.Flow.ActiveSide == Side.Enemy); i++)
            {
                game.Flow.Skip();
            }
            game.Cannons.StopSalvo(Side.Player);
            game.Cannons.StopSalvo(Side.Enemy);
            game.Match.Destroy(Side.Player, game.Match.PlayerAlive, 0);
            WaitForFinish(game);
            Settle(game, 1f);
            Auto(game, "17_defeat", false);
            game.config.directorEnabled = true;

            // Кратер и хитбокс пушек после правок.
            Fresh(game, ThemePreset.Night);
            WaitForState(game, SessionState.Aim, Side.Player);
            FireHint(game);
            WaitForState(game, SessionState.Resolve, Side.Player);
            Shoot(game, "18_red_after_x20", ViewRed, false, ThemePreset.Night, true);

            Fresh(game, ThemePreset.Sunset);
            WaitForState(game, SessionState.Aim, Side.Player);
            Settle(game, game.config.handStartDelay + game.config.handDemoDuration * 0.55f);
            Auto(game, "19_sunset", false);
        }

        /// <summary>Кадр авто-камеры без debug-панели.</summary>
        private static void Auto(GameBootstrap game, string name, bool landscape)
        {
            Shoot(game, name, 0, landscape, game.ActivePreset, true);
        }

        /// <summary>Выстрел игрока по подсказке текущего хода.</summary>
        private static void FireHint(GameBootstrap game)
        {
            TurnHint hint = game.Flow.CurrentHint;
            game.Cannons.SetAim(Side.Player, hint.handAngleDeg, hint.handPower01);
            game.Flow.Skip();
        }

        private static void WaitForState(GameBootstrap game, SessionState state, Side side)
        {
            int steps = Mathf.RoundToInt(MaxMatchSeconds / StepSeconds);
            for (int i = 0; i < steps && !(game.Flow.State == state && game.Flow.ActiveSide == side); i++)
            {
                game.Loop.Tick(StepSeconds);
            }
        }

        private static void WaitForFinish(GameBootstrap game)
        {
            int steps = Mathf.RoundToInt(MaxMatchSeconds / StepSeconds);
            for (int i = 0; i < steps && !game.Flow.IsFinished; i++)
            {
                game.Loop.Tick(StepSeconds);
            }
        }

        /// <summary>Держит время, пока голова потока активной стороны не дойдёт до x.</summary>
        private static void WaitForHead(GameBootstrap game, float x)
        {
            int steps = Mathf.RoundToInt(MaxWaitSeconds / StepSeconds);
            Side side = game.Flow.ActiveSide;
            float facing = side == Side.Player ? 1f : -1f;
            for (int i = 0; i < steps; i++)
            {
                game.Loop.Tick(StepSeconds);
                if (game.Rockets.Count(side) > 0 && (game.Rockets.HeadX(side) - x) * facing > 0f)
                {
                    return;
                }
            }
        }

        private static void WaitForHonest(GameBootstrap game, float honest)
        {
            int steps = Mathf.RoundToInt(MaxWaitSeconds / StepSeconds);
            for (int i = 0; i < steps && game.Boosters.HonestTotal < honest; i++)
            {
                game.Loop.Tick(StepSeconds);
            }
        }

        private static void Fresh(GameBootstrap game, ThemePreset preset)
        {
            game.SetPreset(preset);
            game.Restart();
            game.Loop.TimeScale = 1f;
        }

        private static void Settle(GameBootstrap game, float seconds)
        {
            int steps = Mathf.RoundToInt(seconds / StepSeconds);
            for (int i = 0; i < steps; i++)
            {
                game.Loop.Tick(StepSeconds);
            }
        }

        private static void Shoot(GameBootstrap game, string name, int view, bool landscape, ThemePreset preset, bool clean)
        {
#if GAME_DEBUG
            game.CameraDirector.DebugView = view;
#endif
            game.Parallax.Apply();
            Render(game, name, landscape, clean);
            Debug.Log("[GameCapture] " + name + " preset " + preset);
        }

        /// <summary>Переключает debug-канвас на камеру, чтобы он попал в кадр (Overlay-канвас камера не рисует).</summary>
        private static Canvas AttachDebugCanvas(GameBootstrap game, Camera camera)
        {
#if GAME_DEBUG
            Canvas canvas = game.debugOverlay.GetComponent<Canvas>();
            AttachCanvas(canvas, camera);
            game.debugOverlay.ForceRefresh();
            Canvas.ForceUpdateCanvases();
            return canvas;
#else
            return null;
#endif
        }

        private static void AttachCanvas(Canvas canvas, Camera camera)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            // В режиме камеры канвас сортируется вместе с мешами, а в игре он overlay поверх всего.
            canvas.sortingOrder = CapturedCanvasOrder;
            canvas.planeDistance = camera.nearClipPlane + 1f;
            Canvas.ForceUpdateCanvases();
        }

        private static void Render(GameBootstrap game, string name, bool landscape, bool clean)
        {
            int width = landscape ? LandscapeWidth : PortraitWidth;
            int height = landscape ? LandscapeHeight : PortraitHeight;
            RenderTexture target = new RenderTexture(width, height, DepthBits, RenderTextureFormat.ARGB32);
            Camera camera = game.mainCamera;
            camera.targetTexture = target;
            camera.aspect = (float)width / height;
            Canvas canvas = clean ? null : AttachDebugCanvas(game, camera);
            Canvas uiCanvas = game.aimPanel.GetComponentInParent<Canvas>(true);
            AttachCanvas(uiCanvas, camera);
            game.CameraDirector.Snap();
            game.Parallax.Apply();
            game.TickUi();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;
            camera.ResetAspect();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            string path = Path.Combine(OutputDir, FilePrefix + name + ".png");
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            target.Release();
            Object.DestroyImmediate(target);
            Debug.Log("[GameCapture] " + path);
        }
    }
}
