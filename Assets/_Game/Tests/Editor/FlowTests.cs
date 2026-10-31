using System.Collections.Generic;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    public sealed class MatchStateTests
    {
        [Test]
        public void InitFillsBothSides()
        {
            MatchState match = new MatchState();
            match.Init(3, 12);
            Assert.AreEqual(3, match.PlayerAlive);
            Assert.AreEqual(12, match.EnemyAlive);
            Assert.AreEqual(1f, match.Health01(Side.Player));
            Assert.IsFalse(match.IsEliminated(Side.Enemy));
        }

        [Test]
        public void DestroyIsClampedToAliveCount()
        {
            MatchState match = new MatchState();
            match.Init(3, 12);
            Assert.AreEqual(12, match.Destroy(Side.Enemy, 50, 0));
            Assert.AreEqual(0, match.EnemyAlive);
            Assert.IsTrue(match.IsEliminated(Side.Enemy));
            Assert.AreEqual(0, match.Destroy(Side.Enemy, 1, 0));
        }

        [Test]
        public void DestroyKeepsMinimumRemaining()
        {
            MatchState match = new MatchState();
            match.Init(3, 12);
            Assert.AreEqual(2, match.Destroy(Side.Player, 10, 1));
            Assert.AreEqual(1, match.PlayerAlive);
            Assert.AreEqual(0, match.Destroy(Side.Player, 10, 1));
        }

        [Test]
        public void HealthIsShareOfAliveCannons()
        {
            MatchState match = new MatchState();
            match.Init(3, 12);
            match.Destroy(Side.Enemy, 3, 0);
            Assert.AreEqual(0.75f, match.Health01(Side.Enemy), 1e-5f);
        }

        [Test]
        public void ChangedFiresOnlyWhenCountChanges()
        {
            MatchState match = new MatchState();
            match.Init(3, 12);
            int calls = 0;
            match.Changed += () => calls++;
            match.Destroy(Side.Enemy, 0, 0);
            Assert.AreEqual(0, calls);
            match.Destroy(Side.Enemy, 2, 0);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void ResetRestoresCounts()
        {
            MatchState match = new MatchState();
            match.Init(3, 12);
            match.Destroy(Side.Enemy, 5, 0);
            match.Destroy(Side.Player, 2, 0);
            match.Reset();
            Assert.AreEqual(3, match.PlayerAlive);
            Assert.AreEqual(12, match.EnemyAlive);
        }

        [Test]
        public void EmptySideHasZeroHealth()
        {
            MatchState match = new MatchState();
            match.Init(0, 5);
            Assert.AreEqual(0f, match.Health01(Side.Player));
            Assert.IsTrue(match.IsEliminated(Side.Player));
        }
    }

    public sealed class SessionFlowTests
    {
        private GameConfig config;
        private MatchState match;
        private SessionFlow flow;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            // Все состояния по 1 с: один Tick(1) = один переход.
            config.bootDuration = 1f;
            config.introDuration = 1f;
            config.playerAimDuration = 1f;
            config.aiAimDuration = 1f;
            config.fireDuration = 1f;
            config.flightDuration = 1f;
            config.resolveDuration = 1f;
            config.victoryDuration = 1f;
            config.defeatDuration = 1f;
            config.maxSessionSeconds = 1000f;
            match = new MatchState();
            match.Init(config.playerCannons, config.enemyCannons);
            flow = new SessionFlow();
            flow.Init(config, match);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        /// <summary>Потери стороны в конце каждого хода side: бой в этих тестах не участвует.</summary>
        private void LoseOnResolve(Side side, Side loser, int count)
        {
            flow.StateChanged += (state, active) =>
            {
                if (state == SessionState.Resolve && active == side)
                {
                    match.Destroy(loser, count, 0);
                }
            };
        }

        private void RunToEnd(int maxTicks)
        {
            for (int i = 0; i < maxTicks && !flow.IsFinished; i++)
            {
                flow.Tick(1f);
            }
        }

        [Test]
        public void StartsInBootWithPlayerAndNoTurn()
        {
            Assert.AreEqual(SessionState.Boot, flow.State);
            Assert.AreEqual(Side.Player, flow.ActiveSide);
            Assert.AreEqual(0, flow.PlayerTurn);
            Assert.AreEqual(EndReason.None, flow.EndReason);
        }

        [Test]
        public void FollowsSpecOrderForFirstTurnPair()
        {
            List<string> seen = new List<string>();
            flow.StateChanged += (state, side) => seen.Add(state + ":" + side);
            for (int i = 0; i < 9; i++)
            {
                flow.Tick(1f);
            }
            string[] expected =
            {
                "Intro:Player", "Aim:Player", "Fire:Player", "Flight:Player", "Resolve:Player",
                "Aim:Enemy", "Fire:Enemy", "Flight:Enemy", "Resolve:Enemy"
            };
            CollectionAssert.AreEqual(expected, seen);
        }

        [Test]
        public void PlayerTurnCounterGrowsOnEachPlayerAim()
        {
            flow.Tick(1f);
            flow.Tick(1f);
            Assert.AreEqual(1, flow.PlayerTurn);
            for (int i = 0; i < 8; i++)
            {
                flow.Tick(1f);
            }
            Assert.AreEqual(SessionState.Aim, flow.State);
            Assert.AreEqual(Side.Player, flow.ActiveSide);
            Assert.AreEqual(2, flow.PlayerTurn);
        }

        [Test]
        public void DirectorGivesVictoryOnLastAllowedTurn()
        {
            RunToEnd(200);
            Assert.IsTrue(flow.IsFinished);
            Assert.AreEqual(EndReason.Victory, flow.EndReason);
            Assert.IsTrue(flow.HasWinner);
            Assert.AreEqual(Side.Player, flow.Winner);
            Assert.AreEqual(config.maxPlayerTurns, flow.PlayerTurn);
            Assert.AreEqual(0, match.EnemyAlive);
            Assert.GreaterOrEqual(match.PlayerAlive, 1);
        }

        [Test]
        public void VictoryComesBeforeEndCard()
        {
            List<SessionState> seen = new List<SessionState>();
            flow.StateChanged += (state, side) => seen.Add(state);
            RunToEnd(200);
            int victory = seen.IndexOf(SessionState.Victory);
            int endCard = seen.IndexOf(SessionState.EndCard);
            Assert.GreaterOrEqual(victory, 0);
            Assert.Greater(endCard, victory);
        }

        [Test]
        public void EarlyVictoryEndsBeforeLastTurn()
        {
            LoseOnResolve(Side.Player, Side.Enemy, 12);
            RunToEnd(200);
            Assert.AreEqual(EndReason.Victory, flow.EndReason);
            Assert.AreEqual(1, flow.PlayerTurn);
        }

        [Test]
        public void WithoutDirectorSurvivingEnemyMeansLimitEndCard()
        {
            config.directorEnabled = false;
            LoseOnResolve(Side.Player, Side.Enemy, 1);
            RunToEnd(200);
            Assert.AreEqual(EndReason.Limit, flow.EndReason);
            Assert.IsFalse(flow.HasWinner);
            Assert.AreEqual(config.maxPlayerTurns, flow.PlayerTurn);
        }

        [Test]
        public void WithoutDirectorLastTurnStillGetsEnemyReply()
        {
            config.directorEnabled = false;
            LoseOnResolve(Side.Player, Side.Enemy, 1);
            int enemyAims = 0;
            flow.StateChanged += (state, side) =>
            {
                if (state == SessionState.Aim && side == Side.Enemy)
                {
                    enemyAims++;
                }
            };
            RunToEnd(200);
            Assert.AreEqual(config.maxPlayerTurns, enemyAims);
        }

        [Test]
        public void WithoutDirectorPlayerCanLose()
        {
            config.directorEnabled = false;
            LoseOnResolve(Side.Enemy, Side.Player, 5);
            RunToEnd(200);
            Assert.AreEqual(EndReason.Defeat, flow.EndReason);
            Assert.AreEqual(Side.Enemy, flow.Winner);
            Assert.AreEqual(Side.Player, flow.Loser);
            Assert.AreEqual(0, match.PlayerAlive);
        }

        [Test]
        public void SessionTimeLimitForcesEndCard()
        {
            config.flightDuration = 100f;
            config.maxSessionSeconds = 20f;
            RunToEnd(100);
            Assert.AreEqual(EndReason.Limit, flow.EndReason);
            Assert.AreEqual(SessionState.EndCard, flow.State);
            Assert.GreaterOrEqual(flow.SessionTime, 20f);
            Assert.Less(flow.SessionTime, 22f);
        }

        [Test]
        public void SessionTimeLimitDoesNotCutVictoryShort()
        {
            LoseOnResolve(Side.Player, Side.Enemy, 12);
            config.victoryDuration = 5f;
            config.maxSessionSeconds = 8f;
            RunToEnd(100);
            Assert.AreEqual(EndReason.Victory, flow.EndReason);
        }

        [Test]
        public void DeadTargetEndsFlightSoonEvenIfRocketsStillFly()
        {
            while (flow.State != SessionState.Flight)
            {
                flow.Skip();
            }
            flow.FlightBusy = true;
            flow.Tick(config.flightDuration + 0.1f);
            Assert.AreEqual(SessionState.Flight, flow.State);
            match.Destroy(Side.Enemy, config.enemyCannons, 0);
            flow.Tick(config.victoryFlightCap * 0.5f);
            Assert.AreEqual(SessionState.Flight, flow.State);
            flow.Tick(config.victoryFlightCap * 0.6f);
            Assert.AreEqual(SessionState.Resolve, flow.State);
        }

        [Test]
        public void OnlyLastPlayerTurnIsFinisher()
        {
            config.maxPlayerTurns = 2;
            Assert.IsFalse(flow.IsFinisherTurn);
            while (flow.PlayerTurn < 2)
            {
                flow.Skip();
                Assert.IsFalse(flow.PlayerTurn < 2 && flow.IsFinisherTurn);
            }
            Assert.IsTrue(flow.IsFinisherTurn);
            config.directorEnabled = false;
            Assert.IsFalse(flow.IsFinisherTurn);
        }

        [Test]
        public void SkipMovesToNextStateImmediately()
        {
            flow.Skip();
            Assert.AreEqual(SessionState.Intro, flow.State);
            flow.Skip();
            Assert.AreEqual(SessionState.Aim, flow.State);
            Assert.AreEqual(0f, flow.StateTime);
        }

        [Test]
        public void SkipDoesNothingAtEndCard()
        {
            RunToEnd(200);
            flow.Skip();
            Assert.AreEqual(SessionState.EndCard, flow.State);
        }

        [Test]
        public void HugeDeltaAdvancesWithoutLooping()
        {
            flow.Tick(100f);
            Assert.AreNotEqual(SessionState.Boot, flow.State);
        }

        [Test]
        public void LeftoverTimeCarriesIntoNextState()
        {
            flow.Tick(1.4f);
            Assert.AreEqual(SessionState.Intro, flow.State);
            Assert.AreEqual(0.4f, flow.StateTime, 1e-4f);
        }

        [Test]
        public void FinishedFlowIgnoresTick()
        {
            RunToEnd(200);
            float time = flow.SessionTime;
            flow.Tick(5f);
            Assert.AreEqual(time, flow.SessionTime);
        }

        [Test]
        public void RestartResetsEverything()
        {
            RunToEnd(200);
            flow.Restart();
            Assert.AreEqual(SessionState.Boot, flow.State);
            Assert.AreEqual(0, flow.PlayerTurn);
            Assert.AreEqual(0f, flow.SessionTime);
            Assert.AreEqual(EndReason.None, flow.EndReason);
            Assert.IsFalse(flow.HasWinner);
            Assert.AreEqual(config.enemyCannons, match.EnemyAlive);
        }

        [Test]
        public void CurrentHintFollowsPlayerTurn()
        {
            flow.Tick(1f);
            flow.Tick(1f);
            Assert.AreSame(config.turnHints[0], flow.CurrentHint);
            for (int i = 0; i < 10; i++)
            {
                flow.Tick(1f);
            }
            Assert.AreEqual(2, flow.PlayerTurn);
            Assert.AreSame(config.turnHints[1], flow.CurrentHint);
        }

        [Test]
        public void FlightWaitsWhileRocketsAreBusy()
        {
            while (flow.State != SessionState.Flight)
            {
                flow.Skip();
            }
            flow.FlightBusy = true;
            flow.Tick(config.flightDuration + 1f);
            Assert.AreEqual(SessionState.Flight, flow.State);
            flow.FlightBusy = false;
            flow.Tick(0.01f);
            Assert.AreEqual(SessionState.Resolve, flow.State);
            Assert.Less(flow.StateTime, 0.01f, "waiting does not eat Resolve time");
        }

        [Test]
        public void FlightLimitReleasesStuckTurn()
        {
            while (flow.State != SessionState.Flight)
            {
                flow.Skip();
            }
            flow.FlightBusy = true;
            flow.Tick(config.flightMaxDuration + 0.1f);
            Assert.AreEqual(SessionState.Resolve, flow.State);
        }

        [Test]
        public void KeepAimingRestartsIdleTimer()
        {
            while (flow.State != SessionState.Aim)
            {
                flow.Skip();
            }
            flow.Tick(0.9f);
            flow.KeepAiming();
            flow.Tick(0.9f);
            Assert.AreEqual(SessionState.Aim, flow.State);
            flow.Tick(0.2f);
            Assert.AreEqual(SessionState.Fire, flow.State);
        }

        [Test]
        public void DurationsAreReadFromConfig()
        {
            config.playerAimDuration = 2.5f;
            config.aiAimDuration = 1.5f;
            Assert.AreEqual(2.5f, flow.Duration(SessionState.Aim, Side.Player));
            Assert.AreEqual(1.5f, flow.Duration(SessionState.Aim, Side.Enemy));
            Assert.Less(flow.Duration(SessionState.EndCard, Side.Player), 0f);
        }
    }
}
