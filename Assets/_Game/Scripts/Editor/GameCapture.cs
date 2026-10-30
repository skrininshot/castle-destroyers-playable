using System.IO;
using CastleDestroyers.Core;
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
        private const string FilePrefix = "stage02_";
        private const int PortraitWidth = 720;
        private const int PortraitHeight = 1280;
        private const int LandscapeWidth = 1280;
        private const int LandscapeHeight = 720;
        private const int DepthBits = 24;
        private const float StepSeconds = 0.02f;

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
            Directory.CreateDirectory(OutputDir);

            Shoot(game, "01_mountain_pristine", ViewMountain, false, ThemePreset.Night, true);
            Shoot(game, "02_overview_pristine", ViewAll, true, ThemePreset.Night, true);
            Shoot(game, "03_blue_hill", ViewBlue, false, ThemePreset.Night, true);

            Fresh(game, ThemePreset.Night);
            Craters(game);
            Shoot(game, "04_mountain_craters", ViewMountain, false, ThemePreset.Night, true);

            Fresh(game, ThemePreset.Night);
            Tunnel(game);
            Settle(game, 0.3f);
            Shoot(game, "05_mountain_tunnel", ViewMountain, false, ThemePreset.Night, true);

            Fresh(game, ThemePreset.Night);
            SlicePeak(game);
            Settle(game, 0.35f);
            Shoot(game, "06_chunk_falling", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 0.55f);
            Shoot(game, "07_chunk_splash", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 2.5f);
            Shoot(game, "08_after_chunk", ViewMountain, false, ThemePreset.Night, false);

            Fresh(game, ThemePreset.Sunset);
            Craters(game);
            Tunnel(game);
            Settle(game, 0.3f);
            Shoot(game, "09_sunset", ViewMountain, false, ThemePreset.Sunset, true);

            Fresh(game, ThemePreset.Night);
            Craters(game);
            SlicePeak(game);
            Settle(game, 0.35f);
            Shoot(game, "10_landscape", ViewMountain, true, ThemePreset.Night, true);

            Fresh(game, ThemePreset.Night);
            game.Terrain.Explode(game.config.enemyCastleX - 7f, 1.5f, 2.4f);
            game.Terrain.Explode(game.config.enemyCastleX - 9.5f, 1f, 1.2f);
            Settle(game, 0.3f);
            Shoot(game, "11_red_hill_craters", ViewRed, false, ThemePreset.Night, true);
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

        /// <summary>Отдельные кратеры на склонах, вершине и у воды.</summary>
        private static void Craters(GameBootstrap game)
        {
            game.Terrain.Explode(-5.4f, 4.6f, 1.2f);
            game.Terrain.Explode(-3.2f, 8.6f, 1.2f);
            game.Terrain.Explode(4.6f, 6.2f, 1.2f);
            game.Terrain.Explode(6.6f, 1.4f, 2.4f);
            game.Terrain.Explode(-1.1f, 3.4f, 0.6f);
            Settle(game, 0.3f);
        }

        /// <summary>Поток взрывов сквозь левую половину горы к расщелине.</summary>
        private static void Tunnel(GameBootstrap game)
        {
            for (float x = -8f; x <= 0f; x += 0.4f)
            {
                game.Terrain.Explode(x, 2.2f + (x + 8f) * 0.1f, 0.9f);
            }
        }

        /// <summary>Сквозной пропил отделяет вершину левой половины горы.</summary>
        private static void SlicePeak(GameBootstrap game)
        {
            for (float x = -7f; x <= 0f; x += 0.4f)
            {
                game.Terrain.Explode(x, 6f, 0.8f);
            }
        }

        private static void Shoot(GameBootstrap game, string name, int view, bool landscape, ThemePreset preset, bool clean)
        {
#if GAME_DEBUG
            game.CameraDirector.DebugView = view;
#endif
            game.CameraDirector.Snap();
            Render(game, name, landscape, clean);
            Debug.Log("[GameCapture] " + name + " preset " + preset);
        }

        /// <summary>Переключает debug-канвас на камеру, чтобы он попал в кадр (Overlay-канвас камера не рисует).</summary>
        private static Canvas AttachDebugCanvas(GameBootstrap game, Camera camera)
        {
#if GAME_DEBUG
            Canvas canvas = game.debugOverlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane + 1f;
            game.debugOverlay.ForceRefresh();
            Canvas.ForceUpdateCanvases();
            return canvas;
#else
            return null;
#endif
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
            game.CameraDirector.Snap();
            camera.Render();

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
