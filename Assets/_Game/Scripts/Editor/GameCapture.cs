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
    /// Снимает кадры greybox-сцены для отчёта: прогоняет FSM до нужного состояния и рендерит камеру в PNG.
    /// Запуск без -nographics: <c>-executeMethod CastleDestroyers.EditorTools.GameCapture.CaptureAll</c>.
    /// </summary>
    public static class GameCapture
    {
        private const string OutputDir = "docs/reports/img";
        private const int PortraitWidth = 720;
        private const int PortraitHeight = 1280;
        private const int LandscapeWidth = 1280;
        private const int LandscapeHeight = 720;
        private const int DepthBits = 24;
        private const float StepSeconds = 0.02f;
        private const int MaxSteps = 20000;
        private const float SettleSeconds = 0.5f;

        private sealed class Shot
        {
            public string name;
            public SessionState state;
            public Side side;
            public float dwell;
            public bool landscape;
            public ThemePreset preset;
            public int turn;
            public bool stress;
            public bool clean;

            public Shot(string name, SessionState state, Side side, float dwell, bool landscape, ThemePreset preset, int turn, bool stress, bool clean = false)
            {
                this.clean = clean;
                this.name = name;
                this.state = state;
                this.side = side;
                this.dwell = dwell;
                this.landscape = landscape;
                this.preset = preset;
                this.turn = turn;
                this.stress = stress;
            }
        }

        [MenuItem("Tools/Castle Destroyers/Capture Greybox Screenshots")]
        public static void CaptureAll()
        {
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
            GameBootstrap game = Object.FindFirstObjectByType<GameBootstrap>();
            game.Boot();
            Directory.CreateDirectory(OutputDir);

            Shot[] shots =
            {
                new Shot("01_intro_portrait", SessionState.Intro, Side.Player, 0.8f, false, ThemePreset.Night, 0, false),
                new Shot("02_player_aim_portrait", SessionState.Aim, Side.Player, 1.0f, false, ThemePreset.Night, 1, false),
                new Shot("03_flight_center_portrait", SessionState.Flight, Side.Player, 0.6f, false, ThemePreset.Night, 1, true),
                new Shot("04_flight_target_portrait", SessionState.Flight, Side.Player, 2.4f, false, ThemePreset.Night, 1, false),
                new Shot("05_resolve_enemy_portrait", SessionState.Resolve, Side.Player, 0.5f, false, ThemePreset.Night, 1, false),
                new Shot("06_enemy_aim_portrait", SessionState.Aim, Side.Enemy, 1.0f, false, ThemePreset.Night, 1, false),
                new Shot("07_victory_loser_portrait", SessionState.Victory, Side.Player, 0.5f, false, ThemePreset.Night, 3, false),
                new Shot("08_victory_winner_portrait", SessionState.Victory, Side.Player, 1.8f, false, ThemePreset.Night, 3, false),
                new Shot("09_flight_center_landscape", SessionState.Flight, Side.Player, 0.6f, true, ThemePreset.Night, 1, true),
                new Shot("10_player_aim_landscape", SessionState.Aim, Side.Player, 1.0f, true, ThemePreset.Night, 1, false),
                new Shot("11_player_aim_sunset", SessionState.Aim, Side.Player, 1.0f, false, ThemePreset.Sunset, 1, false),
                new Shot("12_flight_center_sunset", SessionState.Flight, Side.Player, 0.6f, false, ThemePreset.Sunset, 1, true),
                new Shot("13_clean_player_castle", SessionState.Aim, Side.Player, 1.0f, false, ThemePreset.Night, 1, false, true),
                new Shot("14_clean_mountain", SessionState.Flight, Side.Player, 0.6f, false, ThemePreset.Night, 1, false, true),
                new Shot("15_clean_enemy_castle", SessionState.Resolve, Side.Player, 0.1f, false, ThemePreset.Night, 1, false, true),
                new Shot("16_clean_mountain_sunset", SessionState.Flight, Side.Player, 0.6f, false, ThemePreset.Sunset, 1, false, true),
                new Shot("17_clean_mountain_landscape", SessionState.Flight, Side.Player, 0.6f, true, ThemePreset.Night, 1, false, true)
            };

            for (int i = 0; i < shots.Length; i++)
            {
                Capture(game, shots[i]);
            }
        }

        private static void Capture(GameBootstrap game, Shot shot)
        {
            game.SetPreset(shot.preset);
            game.Restart();
            game.Loop.TimeScale = 1f;

            bool reached = RunUntil(game, shot);
            if (!reached)
            {
                Debug.LogError("[GameCapture] state not reached for " + shot.name);
                return;
            }
            if (shot.stress)
            {
                SpawnStress(game);
            }
            game.CameraDirector.Snap();
            Render(game, shot);
        }

        private static bool RunUntil(GameBootstrap game, Shot shot)
        {
            for (int step = 0; step < MaxSteps; step++)
            {
                SessionFlow flow = game.Flow;
                bool match = flow.State == shot.state && flow.ActiveSide == shot.side
                    && flow.StateTime >= shot.dwell && (shot.turn == 0 || flow.PlayerTurn == shot.turn || shot.state == SessionState.Victory);
                if (match)
                {
                    return true;
                }
                game.Loop.Tick(StepSeconds);
            }
            return false;
        }

        private static void SpawnStress(GameBootstrap game)
        {
#if GAME_DEBUG
            game.Stress.Spawn(game.config.stressRocketCount, game.Flow.ActiveSide, game.Palette);
            for (int i = 0; i < Mathf.RoundToInt(SettleSeconds / StepSeconds); i++)
            {
                game.Loop.Tick(StepSeconds);
            }
#endif
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

        private static void Render(GameBootstrap game, Shot shot)
        {
            int width = shot.landscape ? LandscapeWidth : PortraitWidth;
            int height = shot.landscape ? LandscapeHeight : PortraitHeight;
            RenderTexture target = new RenderTexture(width, height, DepthBits, RenderTextureFormat.ARGB32);
            Camera camera = game.mainCamera;
            camera.targetTexture = target;
            camera.aspect = (float)width / height;
            Canvas canvas = shot.clean ? null : AttachDebugCanvas(game, camera);
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

            string path = Path.Combine(OutputDir, "stage01_" + shot.name + ".png");
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            target.Release();
            Object.DestroyImmediate(target);
            Debug.Log("[GameCapture] " + path);
        }
    }
}
