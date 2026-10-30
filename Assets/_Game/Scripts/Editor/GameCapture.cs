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
        private const string FilePrefix = "stage03_";
        private const int PortraitWidth = 720;
        private const int PortraitHeight = 1280;
        private const int LandscapeWidth = 1280;
        private const int LandscapeHeight = 720;
        private const int DepthBits = 24;
        private const float StepSeconds = 0.02f;

        private const int MaxSkips = 32;
        private const float DragStartX = 500f;
        private const float DragStartY = 700f;
        private const float EnemyLobAngle = 45f;
        private const float EnemyLobPower = 0.66f;
        private const float TunnelAngle = 8f;
        private const float TunnelPower = 0.9f;

        // Кадры debug-камеры (CameraDirector.DebugView).
        private const int ViewAuto = 0;
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

            // Навес через x20: жест, залп, поток над горой.
            Fresh(game, ThemePreset.Night);
            Drag(game, game.config.GetHint(0).handAngleDeg, game.config.GetHint(0).handPower01);
            Shoot(game, "01_aim", ViewAuto, false, ThemePreset.Night, false);
            Release(game);
            Settle(game, 0.4f);
            Shoot(game, "02_salvo", ViewAuto, false, ThemePreset.Night, true);
            Settle(game, 0.7f);
            Shoot(game, "03_stream_center", ViewAuto, false, ThemePreset.Night, true);
            Settle(game, 0.3f);
            Shoot(game, "04_arc_overview", ViewAll, true, ThemePreset.Night, true);

            // Полная дуга до красного замка.
            Fresh(game, ThemePreset.Night);
            Drag(game, EnemyLobAngle, EnemyLobPower);
            Release(game);
            Settle(game, 3.2f);
            Shoot(game, "05_hits_red_castle", ViewRed, false, ThemePreset.Night, true);

            // Низкий выстрел прогрызает гору.
            Fresh(game, ThemePreset.Night);
            Drag(game, TunnelAngle, TunnelPower);
            Release(game);
            Settle(game, 1.0f);
            Shoot(game, "06_tunnel_impact", ViewMountain, false, ThemePreset.Night, true);
            Settle(game, 1.4f);
            Shoot(game, "07_tunnel_result", ViewMountain, false, ThemePreset.Night, false);

            // Ответный залп красных.
            for (int i = 0; i < MaxSkips && !(game.Flow.State == Flow.SessionState.Fire && game.Flow.ActiveSide == Side.Enemy); i++)
            {
                game.Flow.Skip();
            }
            Settle(game, 1.5f);
            Shoot(game, "08_enemy_salvo", ViewAll, true, ThemePreset.Night, true);

            Fresh(game, ThemePreset.Sunset);
            Drag(game, TunnelAngle, TunnelPower);
            Release(game);
            Settle(game, 1.0f);
            Shoot(game, "09_sunset", ViewMountain, false, ThemePreset.Sunset, true);

            Fresh(game, ThemePreset.Night);
#if GAME_DEBUG
            game.Stress.SetActive(true);
#endif
            Settle(game, 1.5f);
            Shoot(game, "10_stress", ViewMountain, false, ThemePreset.Night, false);
            Settle(game, 1.5f);
            Shoot(game, "11_stress_later", ViewMountain, false, ThemePreset.Night, false);
        }

        private static void Fresh(GameBootstrap game, ThemePreset preset)
        {
            game.SetPreset(preset);
            game.Restart();
            game.Loop.TimeScale = 1f;
        }

        /// <summary>Доводит FSM до прицела игрока и тянет палец так, чтобы получить нужные угол и силу.</summary>
        private static void Drag(GameBootstrap game, float angleDeg, float power)
        {
            for (int i = 0; i < MaxSkips && !(game.Flow.State == Flow.SessionState.Aim && game.Flow.ActiveSide == Side.Player); i++)
            {
                game.Flow.Skip();
            }
            float length = power * game.config.aimMaxDragScreenFraction * PortraitHeight;
            float radians = angleDeg * Mathf.Deg2Rad;
            game.Aim.Feed(true, DragStartX, DragStartY, PortraitHeight);
            game.Aim.Feed(true, DragStartX - Mathf.Cos(radians) * length, DragStartY - Mathf.Sin(radians) * length, PortraitHeight);
            Settle(game, 0.3f);
        }

        private static void Release(GameBootstrap game)
        {
            game.Aim.Feed(false, 0f, 0f, PortraitHeight);
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
