using System.IO;
using CastleDestroyers.CastleRig;
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
        private const string FilePrefix = "stage05_";
        private const int PortraitWidth = 720;
        private const int PortraitHeight = 1280;
        private const int LandscapeWidth = 1280;
        private const int LandscapeHeight = 720;
        private const int DepthBits = 24;
        private const float StepSeconds = 0.02f;

        private const int MaxSkips = 32;
        private const float MaxWaitSeconds = 12f;
        private const int ShotX20 = 0;
        private const int ShotX50 = 2;
        private const int CopiesAtTrigger = 60;

        // Кадры debug-камеры (CameraDirector.DebugView).
        private const int ViewMountain = 1;
        private const int ViewBlue = 2;
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

            // Уровень после правок: холмы выше, расщелина шире, бустеры крупнее.
            Fresh(game, ThemePreset.Night);
            Settle(game, 0.1f);
            Shoot(game, "01_level", ViewAll, true, ThemePreset.Night, true);
            Shoot(game, "02_mountain", ViewMountain, false, ThemePreset.Night, true);
            Shoot(game, "03_blue_intact", ViewBlue, false, ThemePreset.Night, true);
            Shoot(game, "04_red_intact", ViewRed, false, ThemePreset.Night, true);

            // Навес через x20: поток бьёт по красному замку, сквош, трещины, куски и пушки падают.
            FireShot(game, ShotX20, ThemePreset.Night);
            WaitForEnemyLoss(game, game.Match.EnemyStart - 1);
            Shoot(game, "05_red_first_cannon", ViewRed, false, ThemePreset.Night, true);
            Settle(game, 0.35f);
            Shoot(game, "06_red_under_fire", ViewRed, false, ThemePreset.Night, true);
            WaitWhileFlying(game);
            Shoot(game, "07_red_after_x20", ViewRed, false, ThemePreset.Night, false);

            // Второй залп по тем же руинам.
            AimRepeat(game);
            Shoot(game, "08_red_after_second", ViewRed, false, ThemePreset.Night, false);

            // Земля под правой гусеницей выбита: замок наклоняется и оседает (кадр 12).
            Fresh(game, ThemePreset.Night);
            CastleBody red = game.Castles.Body(Side.Enemy);
            float trackX = red.TrackX(1);
            float groundY = red.TrackY(1);
            for (float x = trackX - 2f; x <= trackX + 3f; x += 0.7f)
            {
                game.Terrain.Explode(x, groundY - 1f, 1.8f);
            }
            Settle(game, 1.5f);
            Shoot(game, "09_red_tilted", ViewRed, false, ThemePreset.Night, false);

            // Под обеими гусеницами пусто до воды: замок тонет, все пушки падают.
            for (float x = red.TrackX(0) - 3f; x <= red.TrackX(1) + 3f; x += 0.7f)
            {
                for (float y = -1f; y <= groundY + 1f; y += 0.7f)
                {
                    game.Terrain.Explode(x, y, 1.4f);
                }
            }
            Settle(game, 0.75f);
            Shoot(game, "10_red_sinking", ViewRed, false, ThemePreset.Night, false);

            // Ответный залп красных по синему замку.
            Fresh(game, ThemePreset.Night);
            for (int i = 0; i < MaxSkips && !(game.Flow.State == Flow.SessionState.Aim && game.Flow.ActiveSide == Side.Enemy); i++)
            {
                game.Flow.Skip();
            }
            game.Cannons.Clear();
            game.Rockets.Clear();
            game.Cannons.SetAim(Side.Enemy, game.config.stubAiAngleDeg, game.config.stubAiPower);
            game.Flow.Skip();
            Settle(game, 1f);
            WaitWhileFlying(game);
            Settle(game, 0.3f);
            Debug.Log("[GameCapture] enemy salvo player cannons " + game.Match.PlayerAlive);
            Shoot(game, "11_blue_hit", ViewBlue, false, ThemePreset.Night, false);

            FireShot(game, ShotX50, ThemePreset.Night);
            Shoot(game, "12_x50_trigger", ViewMountain, false, ThemePreset.Night, true);
            WaitWhileFlying(game);
            Shoot(game, "13_x50_result", ViewAll, true, ThemePreset.Night, false);

            FireShot(game, ShotX20, ThemePreset.Sunset);
            WaitForEnemyLoss(game, game.Match.EnemyStart - 2);
            Settle(game, 0.3f);
            Shoot(game, "14_sunset", ViewRed, false, ThemePreset.Sunset, true);
        }

        /// <summary>Держит время, пока число живых красных пушек не упадёт до alive.</summary>
        private static void WaitForEnemyLoss(GameBootstrap game, int alive)
        {
            int steps = Mathf.RoundToInt(MaxWaitSeconds / StepSeconds);
            for (int i = 0; i < steps && game.Match.EnemyAlive > alive; i++)
            {
                game.Loop.Tick(StepSeconds);
            }
        }

        private static void WaitWhileFlying(GameBootstrap game)
        {
            int steps = Mathf.RoundToInt(MaxWaitSeconds / StepSeconds);
            for (int i = 0; i < steps && (game.Rockets.ActiveCount > 0 || game.Boosters.PendingCopies > 0 || game.Cannons.IsFiring); i++)
            {
                game.Loop.Tick(StepSeconds);
            }
        }

        /// <summary>Ещё один залп игрока той же наводкой, без хода красных.</summary>
        private static void AimRepeat(GameBootstrap game)
        {
            game.Cannons.BeginSalvo(Side.Player);
            WaitWhileFlying(game);
            Settle(game, 0.4f);
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
