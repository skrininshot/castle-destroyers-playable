using System.IO;
using System.Text;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SessionState = CastleDestroyers.Flow.SessionState;

namespace CastleDestroyers.EditorTools
{
    /// <summary>
    /// Прогоняет целые матчи на настоящей сцене без отрисовки и пишет итог каждого хода в Logs/balance.txt.
    /// Игрок стреляет по подсказкам, ИИ — как в игре. Запуск: -executeMethod CastleDestroyers.EditorTools.BalanceScan.Run
    /// </summary>
    public static class BalanceScan
    {
        private const string OutputPath = "Logs/balance.txt";
        private const float Step = 0.02f;
        private const int Matches = 12;
        private const int MaxSteps = 12000;
        private const int ScanCellsAngle = 13;
        private const int ScanCellsPower = 9;
        private const float ScanAngleFrom = 8f;
        private const float ScanAngleStep = 6f;
        private const float ScanPowerFrom = 0.45f;
        private const float ScanPowerStep = 0.07f;
        private const int TurnTwoMatches = 6;
        private const float EnemyStoneWhole = 13200f;

        public static void Run()
        {
            GameSceneBuilder.Build();
            GameBootstrap game = Open();
            StringBuilder log = new StringBuilder();
            log.Append("crater ").Append(game.config.craterRadiusCastle).Append(" hit ").Append(game.config.cannonHitRadius).Append('\n');
            for (int i = 0; i < Matches; i++)
            {
                PlayMatch(game, log);
            }
            File.WriteAllText(OutputPath, log.ToString());
            EditorApplication.Exit(0);
        }

        /// <summary>Сетки выстрелов для подбора подсказок и пресетов ИИ. Дольше матчей: несколько минут.</summary>
        public static void RunGrids()
        {
            GameBootstrap game = Open();
            StringBuilder log = new StringBuilder();
            ScanFirstShots(game, log);
            ScanSecondShots(game, log);
            ScanEnemyShots(game, log);
            File.WriteAllText(OutputPath, log.ToString());
            EditorApplication.Exit(0);
        }

        /// <summary>Только сетка второго хода.</summary>
        public static void RunSecondShots()
        {
            GameBootstrap game = Open();
            StringBuilder log = new StringBuilder();
            ScanSecondShots(game, log);
            File.WriteAllText(OutputPath, log.ToString());
            EditorApplication.Exit(0);
        }

        /// <summary>Подбор подсказки хода 2 по настоящим матчам: сетка считает не те пушки, что доживают в игре. Средние пушки / % камня / худшие пушки.</summary>
        public static void RunTurnTwoMatches()
        {
            GameBootstrap game = Open();
            StringBuilder log = new StringBuilder();
            TurnHint hint = game.config.GetHint(1);
            for (float angle = 22f; angle <= 62.5f; angle += 4f)
            {
                log.Append("angle ").Append(angle.ToString("00")).Append(':');
                for (float power = 0.5f; power <= 0.901f; power += 0.05f)
                {
                    hint.handAngleDeg = angle;
                    hint.handPower01 = power;
                    float red = 0f;
                    float stone = 0f;
                    int worst = 0;
                    for (int i = 0; i < TurnTwoMatches; i++)
                    {
                        PlayUntilTurnTwoEnds(game);
                        int alive = game.Match.EnemyAlive;
                        red += alive;
                        stone += game.Castles.Body(Side.Enemy).Mask.SolidCount;
                        worst = Mathf.Max(worst, alive);
                    }
                    log.Append(' ').Append((red / TurnTwoMatches).ToString("00.0")).Append('/').Append(Mathf.RoundToInt(100f * stone / TurnTwoMatches / EnemyStoneWhole).ToString("000")).Append('/').Append(worst.ToString("00"));
                }
                log.Append('\n');
                File.WriteAllText(OutputPath, log.ToString());
            }
            EditorApplication.Exit(0);
        }

        private static void PlayUntilTurnTwoEnds(GameBootstrap game)
        {
            game.Restart();
            SessionState last = SessionState.Boot;
            for (int steps = 0; steps < MaxSteps && !game.Flow.IsFinished; steps++)
            {
                SessionFlow flow = game.Flow;
                if (flow.State == SessionState.Aim && flow.ActiveSide == Side.Player)
                {
                    TurnHint hint = flow.CurrentHint;
                    game.Cannons.SetAim(Side.Player, hint.handAngleDeg, hint.handPower01);
                    flow.Skip();
                }
                game.Loop.Tick(Step);
                if (flow.State != last)
                {
                    if (last == SessionState.Flight && flow.PlayerTurn == 2 && flow.ActiveSide == Side.Player)
                    {
                        return;
                    }
                    last = flow.State;
                }
            }
        }

        private static GameBootstrap Open()
        {
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Single);
            GameBootstrap game = Object.FindFirstObjectByType<GameBootstrap>();
            game.Boot();
            game.Aim.Scripted = true;
            return game;
        }

        /// <summary>Матч целиком: игрок бьёт по подсказкам, потери пишутся в конце каждого хода.</summary>
        private static void PlayMatch(GameBootstrap game, StringBuilder log)
        {
            game.Restart();
            SessionState last = SessionState.Boot;
            int steps = 0;
            for (; steps < MaxSteps && !game.Flow.IsFinished; steps++)
            {
                SessionFlow flow = game.Flow;
                if (flow.State == SessionState.Aim && flow.ActiveSide == Side.Player)
                {
                    TurnHint hint = flow.CurrentHint;
                    game.Cannons.SetAim(Side.Player, hint.handAngleDeg, hint.handPower01);
                    flow.Skip();
                }
                game.Loop.Tick(Step);
                if (flow.State != last)
                {
                    if (last == SessionState.Flight)
                    {
                        log.Append("  turn ").Append(flow.PlayerTurn).Append(' ').Append(flow.ActiveSide)
                            .Append(": blue ").Append(game.Match.PlayerAlive).Append(" red ").Append(game.Match.EnemyAlive)
                            .Append(" stone b ").Append(game.Castles.Body(Side.Player).Mask.SolidCount)
                            .Append(" r ").Append(game.Castles.Body(Side.Enemy).Mask.SolidCount)
                            .Append(" honest ").Append(Mathf.RoundToInt(game.Boosters.HonestTotal)).Append(" t ").Append(flow.SessionTime.ToString("0.0")).Append('\n');
                    }
                    last = flow.State;
                }
            }
            log.Append("match ").Append(game.Flow.EndReason.ToString()).Append(" time ").Append(game.Flow.SessionTime.ToString("0.0"))
                .Append(" blue ").Append(game.Match.PlayerAlive).Append(" red ").Append(game.Match.EnemyAlive)
                .Append(" blueBody ").Append(game.Castles.Body(Side.Player).State.ToString()).Append('\n');
        }

        /// <summary>Сетка угол × сила первого выстрела игрока: сколько пушек из 12 остаётся. Рядом с подсказкой не должно быть обрыва.</summary>
        private static void ScanFirstShots(GameBootstrap game, StringBuilder log)
        {
            for (int a = 0; a < ScanCellsAngle; a++)
            {
                float angle = ScanAngleFrom + a * ScanAngleStep;
                log.Append("angle ").Append(angle.ToString("00")).Append(':');
                for (int p = 0; p < ScanCellsPower; p++)
                {
                    float power = ScanPowerFrom + p * ScanPowerStep;
                    log.Append(' ').Append(FirstShot(game, angle, power).ToString("00"));
                }
                log.Append('\n');
            }
        }

        /// <summary>Сетка второго хода: прошёл ход 1 по подсказке, у игрока на пушку меньше, ответа ИИ не было. Красные пушки / честное число / % камня красных.</summary>
        private static void ScanSecondShots(GameBootstrap game, StringBuilder log)
        {
            log.Append("second shot: red left / honest\n");
            for (float angle = 14f; angle <= 32f; angle += 2f)
            {
                log.Append("angle ").Append(angle.ToString("00")).Append(':');
                for (float power = 0.55f; power <= 1.001f; power += 0.05f)
                {
                    TurnHint hint = game.config.GetHint(0);
                    FirstShot(game, hint.handAngleDeg, hint.handPower01);
                    int stone = game.Castles.Body(Side.Enemy).Mask.SolidCount;
                    for (int i = 0; i < 16 && !(game.Flow.State == SessionState.Aim && game.Flow.ActiveSide == Side.Player); i++)
                    {
                        game.Flow.Skip();
                    }
                    game.Cannons.StopSalvo(Side.Enemy);
                    game.Cannons.Detach(2, 0f, 0f, 0f);
                    float honest = game.Boosters.HonestTotal;
                    game.Cannons.SetAim(Side.Player, angle, power);
                    game.Flow.Skip();
                    WaitForFlight(game);
                    log.Append(' ').Append(game.Match.EnemyAlive.ToString("00")).Append('/').Append(Mathf.RoundToInt(game.Boosters.HonestTotal - honest).ToString("0000"))
                        .Append('/').Append((100 * game.Castles.Body(Side.Enemy).Mask.SolidCount / stone).ToString("000"));
                }
                log.Append('\n');
            }
        }

        /// <summary>Сетка выстрела ИИ по целому уровню: сколько камня синих осталось, % / честное число.</summary>
        private static void ScanEnemyShots(GameBootstrap game, StringBuilder log)
        {
            log.Append("enemy shot: blue stone % / honest\n");
            for (float angle = 30f; angle <= 75f; angle += 5f)
            {
                log.Append("angle ").Append(angle.ToString("00")).Append(':');
                for (float power = 0.5f; power <= 0.921f; power += 0.06f)
                {
                    game.Restart();
                    int stone = game.Castles.Body(Side.Player).Mask.SolidCount;
                    for (int i = 0; i < 16 && !(game.Flow.State == SessionState.Fire && game.Flow.ActiveSide == Side.Enemy); i++)
                    {
                        game.Flow.Skip();
                    }
                    game.Cannons.StopSalvo(Side.Player);
                    game.Cannons.SetAim(Side.Enemy, angle, power);
                    WaitForFlight(game);
                    log.Append(' ').Append((100 * game.Castles.Body(Side.Player).Mask.SolidCount / stone).ToString("000")).Append('/').Append(Mathf.RoundToInt(game.Boosters.HonestTotal).ToString("0000"));
                }
                log.Append('\n');
            }
        }

        private static void WaitForFlight(GameBootstrap game)
        {
            for (int i = 0; i < MaxSteps; i++)
            {
                game.Loop.Tick(Step);
                if (game.Flow.State != SessionState.Fire && game.Flow.State != SessionState.Flight)
                {
                    return;
                }
            }
        }

        private static int FirstShot(GameBootstrap game, float angle, float power)
        {
            game.Restart();
            for (int i = 0; i < 8 && game.Flow.State != SessionState.Aim; i++)
            {
                game.Flow.Skip();
            }
            game.Cannons.SetAim(Side.Player, angle, power);
            game.Flow.Skip();
            WaitForFlight(game);
            return game.Match.EnemyAlive;
        }
    }
}
