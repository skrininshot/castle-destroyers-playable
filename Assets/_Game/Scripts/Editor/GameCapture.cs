using System.IO;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CastleDestroyers.EditorTools
{
    /// <summary>
    /// Снимает кадры сцены для отчёта: разрушает ландшафт по сценарию и рендерит камеру в PNG.
    /// Запуск без -nographics: <c>-executeMethod CastleDestroyers.EditorTools.GameCapture.CaptureAll</c>.
    /// </summary>
    public static class GameCapture
    {
        private const string OutputDir = "docs/reports/img";
        private const string FilePrefix = "stage04_";
        private const int PortraitWidth = 720;
        private const int PortraitHeight = 1280;
        private const int LandscapeWidth = 1280;
        private const int LandscapeHeight = 720;
        private const int DepthBits = 24;
        private const float StepSeconds = 0.02f;

        private const int MaxSkips = 32;
        private const float MaxWaitSeconds = 12f;
        private const int ShotX20 = 0;
        private const int ShotX10 = 1;
        private const int ShotX50 = 2;
        private const int CopiesAtTrigger = 60;
        private const int BudgetDemo = 120;

        // Кадры debug-камеры (CameraDirector.DebugView).
        private const int ViewMountain = 1;
        private const int ViewRed = 3;
        private const int ViewAll = 4;

        [MenuItem("Tools/Castle Destroyers/Capture Stage Screenshots")]
        public static void CaptureAll()
        {
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
            GameBootstrap game = Object.FindFirstObjectByType<GameBootstrap>();
            game.Boot();
            game.Aim.Scripted = true;
            Directory.CreateDirectory(OutputDir);

            // Навес через x20: срабатывание, потоки, вся дуга, подход к красному замку.
            FireShot(game, ShotX20, ThemePreset.Night);
            Shoot(game, "01_x20_trigger", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 0.45f);
            Shoot(game, "02_x20_streams", ViewMountain, false, ThemePreset.Night, false);
            Shoot(game, "03_x20_overview", ViewAll, true, ThemePreset.Night, true);
            Settle(game, 0.55f);
            Shoot(game, "04_x20_at_red", ViewRed, false, ThemePreset.Night, true);

            // Тоннель через левую половину, x10, пролом правой половины.
            FireShot(game, ShotX10, ThemePreset.Night);
            Shoot(game, "05_x10_trigger", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 0.5f);
            Shoot(game, "06_x10_through", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 1.2f);
            Shoot(game, "07_x10_result", ViewMountain, false, ThemePreset.Night, false);

            // Низ горы, x50.
            FireShot(game, ShotX50, ThemePreset.Night);
            Shoot(game, "08_x50_trigger", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 0.4f);
            Shoot(game, "09_x50_avalanche", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 1.5f);
            Shoot(game, "10_x50_result", ViewMountain, false, ThemePreset.Night, false);

            // Бюджет: поп-ап честный, ракет меньше.
            Fresh(game, ThemePreset.Night);
            game.Rockets.Budget = BudgetDemo;
            AimShot(game, ShotX20);
            WaitForCopies(game, BudgetDemo / 2);
            Settle(game, 0.5f);
            Shoot(game, "11_budget", ViewMountain, false, ThemePreset.Night, false);
            game.Rockets.Budget = game.config.maxActiveRockets;

            // Ответный залп красных без ракет игрока в воздухе.
            Fresh(game, ThemePreset.Night);
            for (int i = 0; i < MaxSkips && !(game.Flow.State == Flow.SessionState.Aim && game.Flow.ActiveSide == Side.Enemy); i++)
            {
                game.Flow.Skip();
            }
            game.Cannons.Clear();
            game.Rockets.Clear();
            game.Cannons.SetAim(Side.Enemy, game.config.stubAiAngleDeg, game.config.stubAiPower);
            game.Flow.Skip();
            Settle(game, 2.2f);
            Debug.Log("[GameCapture] enemy salvo honest " + game.Boosters.HonestTotal + " spawned " + game.Boosters.SpawnedTotal + " rockets " + game.Rockets.ActiveCount);
            Shoot(game, "12_enemy_salvo", ViewAll, true, ThemePreset.Night, true);

            FireShot(game, ShotX20, ThemePreset.Sunset);
            Settle(game, 0.3f);
            Shoot(game, "13_sunset", ViewMountain, false, ThemePreset.Sunset, true);
        }

        /// <summary>Выстрел сценария кнопки SHOT с чистого уровня; возвращается, когда бустер уже выпускает копии.</summary>
        private static void FireShot(GameBootstrap game, int shot, ThemePreset preset)
        {
            Fresh(game, preset);
            AimShot(game, shot);
            WaitForCopies(game, CopiesAtTrigger);
        }

        private static void AimShot(GameBootstrap game, int shot)
        {
#if GAME_DEBUG
            game.Scenarios.Run(shot);
#endif
        }

        private static void WaitForCopies(GameBootstrap game, int copies)
        {
            int steps = Mathf.RoundToInt(MaxWaitSeconds / StepSeconds);
            for (int i = 0; i < steps && game.Boosters.SpawnedTotal < copies; i++)
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
            game.CameraDirector.Snap();
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
