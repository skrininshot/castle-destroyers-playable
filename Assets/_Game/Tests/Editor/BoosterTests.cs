using System;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CastleDestroyers.Tests
{
    public sealed class BoosterMathTests
    {
        [Test]
        public void SegmentEntersRectFromLeft()
        {
            float fraction = BoosterSystem.SegmentEntry(0f, 1f, 4f, 1f, 1f, 0f, 2f, 2f);
            Assert.AreEqual(0.25f, fraction, 1e-5f);
        }

        [Test]
        public void SegmentMissesRect()
        {
            Assert.AreEqual(-1f, BoosterSystem.SegmentEntry(0f, 3f, 4f, 3f, 1f, 0f, 2f, 2f));
            Assert.AreEqual(-1f, BoosterSystem.SegmentEntry(0f, 1f, 0.5f, 1f, 1f, 0f, 2f, 2f));
        }

        [Test]
        public void VerticalSegmentEntersFromTop()
        {
            Assert.AreEqual(0.5f, BoosterSystem.SegmentEntry(1.5f, 4f, 1.5f, 0f, 1f, 0f, 2f, 2f), 1e-5f);
        }

        [Test]
        public void SegmentThroughCornerAreaCountsOnlyRealCrossing()
        {
            Assert.AreEqual(-1f, BoosterSystem.SegmentEntry(0f, 3f, 3f, 0f, 2.5f, 2f, 3f, 3f));
        }

        [Test]
        public void ExitDistanceReachesFarSide()
        {
            Assert.AreEqual(0.5f, BoosterSystem.ExitDistance(1f, 1f, 2f, 0f, 1f, 0f, 2f, 2f), 1e-5f);
            Assert.AreEqual(0.25f, BoosterSystem.ExitDistance(1.5f, 2f, 0f, -8f, 1f, 0f, 2f, 2f), 1e-5f);
        }

        [Test]
        public void CopiesAreLimitedByFreeBudgetButNeverZero()
        {
            Assert.AreEqual(20, BoosterSystem.CopyCount(20, 100));
            Assert.AreEqual(7, BoosterSystem.CopyCount(20, 7));
            Assert.AreEqual(1, BoosterSystem.CopyCount(20, 0));
            Assert.AreEqual(1, BoosterSystem.CopyCount(20, -5));
        }

        [Test]
        public void WeightKeepsTotalDamage()
        {
            int copies = BoosterSystem.CopyCount(50, 10);
            float weight = BoosterSystem.CopyWeight(50, copies, 1f);
            Assert.AreEqual(50f, copies * weight, 1e-4f);
            Assert.AreEqual(1f, BoosterSystem.CopyWeight(20, 20, 1f), 1e-5f);
        }

        [Test]
        public void CraterScaleGrowsWithAreaAndSaturates()
        {
            Assert.AreEqual(1f, BoosterSystem.CraterScale(1f, 2f));
            Assert.AreEqual(1.5f, BoosterSystem.CraterScale(2.25f, 2f), 1e-5f);
            Assert.AreEqual(2f, BoosterSystem.CraterScale(100f, 2f));
        }

        [Test]
        public void StreamsAreSymmetricAndCentreComesFirst()
        {
            Assert.AreEqual(0f, BoosterSystem.StreamAngleDeg(0, 5, 12f));
            Assert.AreEqual(3f, BoosterSystem.StreamAngleDeg(1, 5, 12f), 1e-5f);
            Assert.AreEqual(-3f, BoosterSystem.StreamAngleDeg(2, 5, 12f), 1e-5f);
            Assert.AreEqual(6f, BoosterSystem.StreamAngleDeg(3, 5, 12f), 1e-5f);
            Assert.AreEqual(-6f, BoosterSystem.StreamAngleDeg(4, 5, 12f), 1e-5f);
            Assert.AreEqual(0f, BoosterSystem.StreamAngleDeg(0, 1, 12f));
        }
    }

    public sealed class BoosterSystemTests
    {
        private const float Step = 0.02f;
        private const int X20 = 0;
        private const int X10 = 1;

        private CombatRig rig;
        private float x20Y;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            x20Y = rig.Config.boosters[X20].position.y;
        }

        [TearDown]
        public void TearDown()
        {
            rig.Destroy();
        }

        private void Run(float seconds)
        {
            int steps = Mathf.RoundToInt(seconds / Step);
            for (int i = 0; i < steps; i++)
            {
                rig.Boosters.Tick(Step);
                rig.Rockets.Tick(Step);
            }
        }

        private void SpawnIntoX20()
        {
            rig.Rockets.Spawn(-2f, x20Y, 30f, rig.Config.gravity * 0.1f, Side.Player, 0f);
        }

        [Test]
        public void RocketBecomesExactlyNCopies()
        {
            SpawnIntoX20();
            Run(0.12f);
            Assert.AreEqual(0, rig.Rockets.Count(Side.Enemy));
            Run(0.3f);
            Assert.AreEqual(20, rig.Boosters.SpawnedTotal);
            Assert.AreEqual(20, rig.Rockets.ActiveCount);
            Assert.AreEqual(0, rig.Boosters.PendingCopies);
        }

        [Test]
        public void CopiesLeaveInRowsOverStaggerTime()
        {
            SpawnIntoX20();
            Run(0.1f);
            rig.Boosters.Tick(Step);
            Assert.Less(rig.Boosters.SpawnedTotal, 20);
            Assert.Greater(rig.Boosters.PendingCopies, 0);
            Run(0.5f);
            Assert.AreEqual(0, rig.Boosters.PendingCopies);
        }

        [Test]
        public void CopiesAppearOnExitSide()
        {
            rig.Boosters.Clear();
            rig.Rockets.SpawnCopy(-2f, x20Y, 30f, 1.4f, Side.Player, 0f, 0, 1f);
            for (int i = 0; i < 40 && rig.Boosters.SpawnedTotal == 0; i++)
            {
                rig.Boosters.Tick(Step);
                rig.Rockets.Tick(Step);
            }
            Assert.Greater(rig.Boosters.SpawnedTotal, 0);
            for (int slot = 0; slot < rig.Boosters.SpawnedTotal; slot++)
            {
                Vector3 center = rig.RocketCenter(slot);
                Assert.Greater(center.x, rig.Config.boosterWidth * 0.5f - 0.01f);
            }
        }

        [Test]
        public void ExitIsFocusedOnBoosterAxis()
        {
            rig.Rockets.Spawn(-2f, x20Y + 0.8f, 30f, 0f, Side.Player, 0f);
            rig.Rockets.Tick(0.05f);
            rig.Boosters.Tick(0f);
            rig.Rockets.Tick(0.001f);
            Assert.Greater(rig.Boosters.SpawnedTotal, 0);
            Assert.AreEqual(x20Y, rig.RocketCenter(0).y, 0.1f);
        }

        [Test]
        public void SameBoosterDoesNotMultiplyItsCopy()
        {
            byte used = 1 << X20;
            rig.Rockets.SpawnCopy(-2f, x20Y, 30f, 1.4f, Side.Player, 0f, used, 1f);
            Run(0.3f);
            Assert.AreEqual(0, rig.Boosters.SpawnedTotal);
            Assert.AreEqual(1, rig.Rockets.ActiveCount);
        }

        [Test]
        public void OtherBoosterMultipliesCopy()
        {
            Vector2 x10 = rig.Config.boosters[X10].position;
            byte used = 1 << X20;
            rig.Rockets.SpawnCopy(x10.x - 0.4f, x10.y, 30f, 1.4f, Side.Player, 0f, used, 1f);
            Run(0.5f);
            Assert.AreEqual(10, rig.Boosters.SpawnedTotal);
        }

        [Test]
        public void FastRocketDoesNotTunnelThroughBooster()
        {
            rig.Rockets.Spawn(-3f, x20Y, 60f, 3.5f, Side.Player, 0f);
            for (int i = 0; i < 4; i++)
            {
                rig.Boosters.Tick(0.05f);
                rig.Rockets.Tick(0.05f);
            }
            Assert.Greater(rig.Boosters.HonestTotal, 0f);
        }

        [Test]
        public void BoosterDoesNotBlockOrBreakWhenMissed()
        {
            int solid = rig.Mask.SolidCount;
            rig.Rockets.Spawn(-5f, x20Y + rig.Config.boosterHeight, 30f, 3f, Side.Player, 0f);
            Run(0.3f);
            Assert.AreEqual(0f, rig.Boosters.HonestTotal);
            Assert.AreEqual(1, rig.Rockets.ActiveCount);
            Assert.AreEqual(solid, rig.Mask.SolidCount);
        }

        [Test]
        public void BudgetCutsCopiesPopupStaysHonest()
        {
            rig.Rockets.Budget = 8;
            SpawnIntoX20();
            Run(0.6f);
            Assert.AreEqual(8, rig.Rockets.ActiveCount);
            Assert.AreEqual(8, rig.Boosters.SpawnedTotal);
            Assert.AreEqual(20, rig.Boosters.PopupValue(X20));
            Assert.AreEqual(20f, rig.Boosters.HonestTotal, 1e-4f);
        }

        [Test]
        public void ReservedCopiesAreNotDoubleBooked()
        {
            rig.Rockets.Budget = 30;
            SpawnIntoX20();
            rig.Rockets.Spawn(-2.3f, x20Y, 30f, rig.Config.gravity * 0.1f, Side.Player, 0f);
            Run(0.6f);
            Assert.AreEqual(30, rig.Rockets.ActiveCount);
            Assert.AreEqual(30, rig.Boosters.SpawnedTotal);
            Assert.AreEqual(40, rig.Boosters.PopupValue(X20));
        }

        [Test]
        public void WeightedRocketMakesBiggerCrater()
        {
            rig.Explosions.Explode(-5f, 3f, true, 1f);
            int plain = rig.Mask.SolidCount;
            rig.Terrain.Reset();
            rig.Explosions.Tick(1f);
            rig.Explosions.Explode(-5f, 3f, true, 4f);
            Assert.Less(rig.Mask.SolidCount, plain);
        }

        [Test]
        public void PopupDisappearsAfterHoldAndFade()
        {
            SpawnIntoX20();
            Run(0.2f);
            Assert.AreEqual(20, rig.Boosters.PopupValue(X20));
            Run(rig.Config.boosterPopupHold + rig.Config.boosterPopupFade + 0.1f);
            Assert.AreEqual(0, rig.Boosters.PopupValue(X20));
        }

        [Test]
        public void TriggerSquashesAndDecays()
        {
            SpawnIntoX20();
            Run(0.12f);
            Assert.Greater(rig.Boosters.Squash(X20), 0f);
            Run(1f);
            Assert.AreEqual(0f, rig.Boosters.Squash(X20));
        }

        [Test]
        public void ClearDropsQueuedCopies()
        {
            SpawnIntoX20();
            Run(0.1f);
            rig.Rockets.Clear();
            rig.Boosters.Clear();
            Assert.AreEqual(0, rig.Boosters.PendingCopies);
            Assert.AreEqual(0, rig.Boosters.PopupValue(X20));
            Run(0.5f);
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
        }

        [Test]
        public void EmissionDoesNotAllocate()
        {
            SpawnIntoX20();
            Run(0.1f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            rig.Boosters.Tick(Step);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
            Assert.Greater(rig.Boosters.SpawnedTotal, 0);
        }
    }

    /// <summary>Настоящий залп по настоящему уровню: сценарии кнопок SHOT, включая прогрызание горы.</summary>
    public sealed class BoosterScenarioTests
    {
        private const float Step = 0.02f;
        private const int MaxSteps = 600;

        private CombatRig rig;
        private int peakRockets;
        private float farthestX;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            peakRockets = 0;
            farthestX = float.NegativeInfinity;
        }

        [TearDown]
        public void TearDown()
        {
            rig.Destroy();
        }

        private void Fire(int shot, Side side)
        {
            TurnHint hint = rig.Config.debugBoosterShots[shot];
            rig.Cannons.SetAim(side, hint.handAngleDeg, hint.handPower01);
            rig.Cannons.BeginSalvo(side);
            int salvosLeft = rig.Config.debugBoosterShotSalvos[shot] - 1;
            for (int i = 0; i < MaxSteps; i++)
            {
                if (salvosLeft > 0 && !rig.Cannons.IsFiring)
                {
                    salvosLeft--;
                    rig.Cannons.BeginSalvo(side);
                }
                rig.Step(Step);
                peakRockets = Mathf.Max(peakRockets, rig.Rockets.ActiveCount);
                if (rig.Rockets.Count(side) > 0)
                {
                    farthestX = Mathf.Max(farthestX, rig.Rockets.HeadX(side));
                }
                if (!rig.Cannons.IsFiring && rig.Rockets.ActiveCount == 0 && rig.Boosters.PendingCopies == 0)
                {
                    return;
                }
            }
            Assert.Fail("rockets are still flying");
        }

        [Test]
        public void LobThroughX20ReachesRedCastle()
        {
            Fire(0, Side.Player);
            Assert.AreEqual(600f, rig.Boosters.HonestTotal, 1e-3f);
            Assert.Greater(farthestX, rig.Config.enemyCastleX - 4f);
        }

        [Test]
        public void TunnelReachesX10AndPassesSecondHalf()
        {
            Fire(1, Side.Player);
            Assert.GreaterOrEqual(rig.Boosters.HonestTotal, 100f);
            Assert.Greater(farthestX, rig.Config.enemyCastleX - 4f);
        }

        [Test]
        public void LowShotThroughX50ReachesRedCastle()
        {
            Fire(2, Side.Player);
            Assert.GreaterOrEqual(rig.Boosters.HonestTotal, 300f);
            Assert.Greater(farthestX, rig.Config.enemyCastleX - 4f);
        }

        [Test]
        public void StubAiShotFliesOverBoosters()
        {
            rig.Cannons.SetAim(Side.Enemy, rig.Config.stubAiAngleDeg, rig.Config.stubAiPower);
            rig.Cannons.BeginSalvo(Side.Enemy);
            float nearestX = float.PositiveInfinity;
            for (int i = 0; i < MaxSteps && (rig.Cannons.IsFiring || rig.Rockets.ActiveCount > 0); i++)
            {
                rig.Cannons.Tick(Step);
                rig.Boosters.Tick(Step);
                rig.Rockets.Tick(Step);
                if (rig.Rockets.Count(Side.Enemy) > 0)
                {
                    nearestX = Mathf.Min(nearestX, rig.Rockets.HeadX(Side.Enemy));
                }
            }
            Assert.AreEqual(0f, rig.Boosters.HonestTotal);
            Assert.Less(nearestX, rig.Config.playerCastleX + 4f);
        }

        [Test]
        public void SalvoStaysInsideBudget()
        {
            for (int shot = 0; shot < rig.Config.debugBoosterShots.Length; shot++)
            {
                Fire(shot, Side.Player);
            }
            Assert.LessOrEqual(peakRockets, rig.Config.maxActiveRockets);
        }
    }

    public sealed class BoosterViewTests
    {
        private CombatRig rig;
        private BoosterView view;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            view = new BoosterView();
            view.Init(rig.Config, rig.Boosters);
            view.ApplyPalette(ThemePalette.CreateNight());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(view.Mesh);
            rig.Destroy();
        }

        [Test]
        public void ThemeChangeKeepsVertexCount()
        {
            int vertices = view.Mesh.vertexCount;
            view.ApplyPalette(ThemePalette.CreateSunset());
            Assert.AreEqual(vertices, view.Mesh.vertexCount);
        }

        [Test]
        public void BreathStaysWithinPulse()
        {
            float height = rig.Config.boosterHeight;
            for (int i = 0; i < 100; i++)
            {
                view.Tick(0.05f);
                Vector3[] vertices = view.Mesh.vertices;
                float top = float.NegativeInfinity;
                for (int v = 0; v < 4; v++)
                {
                    top = Mathf.Max(top, vertices[v].y);
                }
                float center = rig.Config.boosters[0].position.y;
                Assert.LessOrEqual(top - center, height * 0.5f * (1f + rig.Config.boosterPulse) + 1e-3f);
            }
        }

        [Test]
        public void PopupAppearsAndLeavesNoVerticesBehind()
        {
            int total = view.Mesh.vertexCount;
            view.Tick(0.016f);
            Vector3[] idle = view.Mesh.vertices;
            float y = rig.Config.boosters[0].position.y;
            rig.Rockets.Spawn(-2f, y, 30f, rig.Config.gravity * 0.1f, Side.Player, 0f);
            for (int i = 0; i < 10; i++)
            {
                rig.Boosters.Tick(0.02f);
                rig.Rockets.Tick(0.02f);
            }
            view.Tick(0.016f);
            Assert.AreEqual(20, rig.Boosters.PopupValue(0));
            Vector3[] shown = view.Mesh.vertices;
            float high = float.NegativeInfinity;
            for (int v = 0; v < total; v++)
            {
                high = Mathf.Max(high, shown[v].y);
            }
            Assert.Greater(high, y + rig.Config.boosterHeight * 0.5f + rig.Config.boosterPopupOffsetY);

            for (int i = 0; i < 100; i++)
            {
                rig.Boosters.Tick(0.02f);
            }
            view.Tick(0.016f);
            Vector3[] after = view.Mesh.vertices;
            for (int v = total - 8; v < total; v++)
            {
                Assert.AreEqual(idle[v], after[v]);
            }
        }

        [Test]
        public void TickDoesNotAllocate()
        {
            rig.Boosters.Multiply(0, 0f, rig.Config.boosters[0].position.y, 20f, 0f, Side.Player, 0, 1f);
            view.Tick(0.016f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            view.Tick(0.016f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }
}
