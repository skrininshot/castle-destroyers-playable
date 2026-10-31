using System;
using CastleDestroyers.CastleRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CastleDestroyers.Tests
{
    public sealed class GnawedCraterTests
    {
        private const float Radius = 8f;
        private static readonly float[] Teeth = { 1.3f, 0.7f, 1.3f, 0.7f, 1.3f, 0.7f, 1.3f, 0.7f };

        [Test]
        public void EdgeIsUnevenAndCentreIsGone()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Assert.IsTrue(CraterStamper.StampGnawed(mask, 20f, 15f, Radius, 2f, Teeth, Teeth.Length));
            Assert.AreEqual(0, mask.Solid[15 * mask.Width + 20]);

            DestructibleMask round = MaskFixture.Filled(MaskFixture.Height);
            CraterStamper.StampCircle(round, 20f, 15f, Radius, 2f, 0f, 0);
            bool differs = false;
            for (int i = 0; i < mask.Solid.Length && !differs; i++)
            {
                differs = (mask.Solid[i] >= DestructibleMask.SolidThreshold) != (round.Solid[i] >= DestructibleMask.SolidThreshold);
            }
            Assert.IsTrue(differs);
        }

        [Test]
        public void CountersStayConsistent()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            CraterStamper.StampGnawed(mask, 20f, 15f, Radius, 2f, Teeth, Teeth.Length);
            int solid = 0;
            for (int i = 0; i < mask.Solid.Length; i++)
            {
                solid += mask.Solid[i] != 0 ? 1 : 0;
            }
            Assert.AreEqual(solid, mask.SolidCount);
            Assert.IsTrue(mask.ConnectivityDirty);
        }

        [Test]
        public void StampInAirChangesNothing()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            Assert.IsFalse(CraterStamper.StampGnawed(mask, 20f, 25f, 3f, 1f, Teeth, Teeth.Length));
            Assert.AreEqual(0, mask.DirtyCount);
        }

        [Test]
        public void CrackIsWrittenOnlyOverMaterial()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            CraterStamper.PaintCrack(mask, 5f, 5f, Mathf.PI * 0.25f, 30f, 4f, 4, 0.3f, new Rng(3u));
            int cracked = 0;
            for (int i = 0; i < mask.Solid.Length; i++)
            {
                if (mask.Grass[i] != 0)
                {
                    cracked++;
                    Assert.AreNotEqual(0, mask.Solid[i]);
                }
            }
            Assert.Greater(cracked, 5);
            Assert.Greater(mask.DirtyCount, 0);
        }

        [Test]
        public void CrackDoesNotLeaveMaskBounds()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Assert.DoesNotThrow(() => CraterStamper.PaintCrack(mask, 44f, 29f, 0.5f, 80f, 6f, 4, 0.3f, new Rng(3u)));
            Assert.DoesNotThrow(() => CraterStamper.PaintCrack(mask, 1f, 1f, 3.5f, 80f, 6f, 4, 0.3f, new Rng(3u)));
        }

        [Test]
        public void StampAndCrackDoNotAllocate()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Rng rng = new Rng(5u);
            CraterStamper.StampGnawed(mask, 20f, 15f, Radius, 2f, Teeth, Teeth.Length);
            CraterStamper.PaintCrack(mask, 20f, 15f, 1f, 20f, 3f, 4, 0.3f, rng);
            long before = GC.GetAllocatedBytesForCurrentThread();
            CraterStamper.StampGnawed(mask, 30f, 10f, Radius, 2f, Teeth, Teeth.Length);
            CraterStamper.PaintCrack(mask, 30f, 10f, 2f, 20f, 3f, 4, 0.3f, rng);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class CastleMaskTests
    {
        private CombatRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Destroy();
        }

        [Test]
        public void EveryMountStandsOnMaterial()
        {
            for (int c = 0; c < rig.Cannons.TotalCannons; c++)
            {
                Side side = c < rig.Config.playerCannons ? Side.Player : Side.Enemy;
                Assert.IsTrue(rig.Castles.Body(side).Mask.IsSolidWorld(rig.Cannons.LocalX(c), rig.Cannons.LocalY(c)), "cannon " + c);
            }
        }

        [Test]
        public void BlueCastleKeepsTheGapBetweenTowers()
        {
            DestructibleMask mask = rig.Castles.Body(Side.Player).Mask;
            float wallBase = rig.Config.platformHeight;
            Assert.IsFalse(mask.IsSolidWorld(0f, wallBase + CastleShapes.BlueTowerTop - 0.5f));
            Assert.IsTrue(mask.IsSolidWorld(-2.3f, wallBase + CastleShapes.BlueTowerTop - 0.5f));
            Assert.IsTrue(mask.IsSolidWorld(2.3f, wallBase + CastleShapes.BlueTowerTop - 0.5f));
        }

        [Test]
        public void RedCastleIsTallerThanBlue()
        {
            Assert.Greater(rig.Castles.Body(Side.Enemy).Height, rig.Castles.Body(Side.Player).Height);
        }

        [Test]
        public void IntactCastleHasNoIslands()
        {
            for (int s = 0; s < 2; s++)
            {
                ConnectivitySolver solver = new ConnectivitySolver();
                solver.Init(rig.Castles.Body((Side)s).Mask, 8);
                solver.Solve();
                Assert.AreEqual(0, solver.IslandCount, "side " + s);
            }
        }

        [Test]
        public void MaskStaysCheapToUpload()
        {
            DestructibleMask red = rig.Castles.Body(Side.Enemy).Mask;
            Assert.LessOrEqual(red.Width * red.Height, 2 * rig.Config.maskTileSize * rig.Config.maskTileSize);
        }
    }

    public sealed class CastleDamageTests
    {
        private const float Step = 0.02f;
        private const float TopCutHeight = 9.2f;

        private CombatRig rig;
        private CastleBody red;
        private CastleBody blue;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            red = rig.Castles.Body(Side.Enemy);
            blue = rig.Castles.Body(Side.Player);
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
                rig.Step(Step);
            }
        }

        private float RedWorldX(float localX, float localY)
        {
            return red.WorldX(localX, localY);
        }

        private float RedWorldY(float localX, float localY)
        {
            return red.WorldY(localX, localY);
        }

        /// <summary>Перерезает башню красного замка поперёк на локальной высоте y.</summary>
        private void CutRedTower(float y, float halfWidth)
        {
            for (float x = -halfWidth; x <= halfWidth; x += 0.25f)
            {
                rig.Castles.HitWall(Side.Enemy, RedWorldX(x, y), RedWorldY(x, y), 1f);
            }
        }

        private int Alive(Side side)
        {
            int first = rig.Cannons.FirstCannon(side);
            int count = 0;
            for (int c = first; c < first + rig.Cannons.CountOf(side); c++)
            {
                count += rig.Cannons.IsAlive(c) ? 1 : 0;
            }
            return count;
        }

        [Test]
        public void RocketEatsCastleAndExplodesOnFirstMaterial()
        {
            int solid = red.Mask.SolidCount;
            rig.Rockets.Spawn(rig.Config.enemyCastleX - 8f, rig.Level.EnemyFocusY, 30f, 0f, Side.Player, 0f);
            Run(0.5f);
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
            Assert.Less(red.Mask.SolidCount, solid);
        }

        [Test]
        public void HitIsFoundThroughTiltedTransform()
        {
            red.AddSway(100f);
            for (int i = 0; i < 12; i++)
            {
                rig.Step(Step);
            }
            Assert.AreNotEqual(0f, red.Angle);
            float x = RedWorldX(0f, 8f);
            float y = RedWorldY(0f, 8f);
            float hit = red.FirstHit(x - 3f, y, x, y, rig.Config.rocketCollisionStep);
            Assert.GreaterOrEqual(hit, 0f);
            Assert.IsFalse(red.HitPlatform);
        }

        [Test]
        public void LocalAndWorldAreInverse()
        {
            red.AddSway(100f);
            Run(0.1f);
            float x = red.WorldX(1.5f, 4f);
            float y = red.WorldY(1.5f, 4f);
            Assert.AreEqual(1.5f, red.LocalX(x, y), 1e-3f);
            Assert.AreEqual(4f, red.LocalY(x, y), 1e-3f);
        }

        [Test]
        public void PlatformStopsRocketButIsNotCut()
        {
            int solid = red.Mask.SolidCount;
            float x = rig.Config.enemyCastleX - red.PlatformHalf - 1f;
            float y = red.WorldY(0f, rig.Config.platformHeight * 0.5f);
            rig.Rockets.Spawn(x, y, 30f, 0f, Side.Player, 0f);
            Run(0.3f);
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
            Assert.AreEqual(solid, red.Mask.SolidCount);
            Assert.AreEqual(1, red.PlatformHits);
        }

        [Test]
        public void PlatformCracksAppearWithHitsAndSaturate()
        {
            Assert.AreEqual(0, rig.Castles.PlatformCracks(Side.Enemy));
            for (int i = 0; i < rig.Config.platformHitsPerCrack; i++)
            {
                rig.Castles.HitPlatform(Side.Enemy, red.PivotX, red.PivotY, 1f);
            }
            Assert.AreEqual(1, rig.Castles.PlatformCracks(Side.Enemy));
            red.PlatformHits = 100000;
            Assert.AreEqual(CastleSystem.MaxPlatformCracks, rig.Castles.PlatformCracks(Side.Enemy));
        }

        [Test]
        public void BlastOverlappingHitCircleDetachesCannon()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            rig.Cannons.ApplyBlast(Side.Enemy, rig.Cannons.WorldX(cannon) + 0.2f, rig.Cannons.WorldY(cannon), 0.2f);
            Assert.IsFalse(rig.Cannons.IsAlive(cannon));
            Assert.IsTrue(rig.Cannons.IsFalling(cannon));
            Assert.AreEqual(rig.Config.enemyCannons - 1, rig.Match.EnemyAlive);
        }

        [Test]
        public void BlastNearTheMuzzleDetachesCannonToo()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            float muzzleX = rig.Cannons.WorldX(cannon) - CannonSystem.BarrelLength;
            rig.Cannons.ApplyBlast(Side.Enemy, muzzleX - 0.3f, rig.Cannons.WorldY(cannon), 0.2f);
            Assert.IsFalse(rig.Cannons.IsAlive(cannon));
        }

        [Test]
        public void BlastJustOutsideTheHitboxLeavesCannon()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            float reach = rig.Config.cannonHitRadius + 0.2f;
            rig.Cannons.ApplyBlast(Side.Enemy, rig.Cannons.WorldX(cannon) + reach + 0.05f, rig.Cannons.WorldY(cannon), 0.2f);
            Assert.IsTrue(rig.Cannons.IsAlive(cannon));
        }

        [Test]
        public void CannonDoesNotHangOnThinBridge()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            float x = rig.Cannons.LocalX(cannon);
            float y = rig.Cannons.LocalY(cannon);
            DestructibleMask mask = red.Mask;
            float reach = rig.Config.cannonSupportRadius;
            // Круг справа и слева: материал под самим креплением остаётся, вокруг почти ничего.
            CraterStamper.StampCircle(mask, mask.TexelX(x - reach), mask.TexelY(y), 5f, 1f, 0f, 0);
            CraterStamper.StampCircle(mask, mask.TexelX(x + reach), mask.TexelY(y), 5f, 1f, 0f, 0);
            CraterStamper.StampCircle(mask, mask.TexelX(x), mask.TexelY(y + reach), 5f, 1f, 0f, 0);
            CraterStamper.StampCircle(mask, mask.TexelX(x), mask.TexelY(y - reach), 5f, 1f, 0f, 0);
            Assert.IsTrue(mask.IsSolidWorld(x, y));
            rig.Step(Step);
            Assert.IsFalse(rig.Cannons.IsAlive(cannon));
        }

        [Test]
        public void DirectorLimitsPlayerLossesPerEnemySalvo()
        {
            rig.Cannons.BeginSalvo(Side.Enemy);
            Assert.IsTrue(rig.Cannons.Detach(0, 0f, 0f, 0f));
            Assert.IsFalse(rig.Cannons.Detach(1, 0f, 0f, 0f));
            rig.Cannons.BeginSalvo(Side.Enemy);
            Assert.IsTrue(rig.Cannons.Detach(1, 0f, 0f, 0f));
            Assert.IsFalse(rig.Cannons.Detach(2, 0f, 0f, 0f), "last cannon");
        }

        [Test]
        public void RescuedCannonSlidesDownToThePlatform()
        {
            int cannon = 0;
            float start = rig.Cannons.LocalY(cannon);
            rig.Cannons.DropToPlatform(cannon);
            rig.Step(Step);
            Assert.Less(rig.Cannons.LocalY(cannon), start);
            Assert.Greater(rig.Cannons.LocalY(cannon), rig.Config.platformHeight);
            for (int i = 0; i < 200; i++)
            {
                rig.Step(Step);
            }
            Assert.AreEqual(rig.Config.platformHeight, rig.Cannons.LocalY(cannon), 1e-4f);
        }

        [Test]
        public void VictoryJumpLiftsCannonsAndComesBack()
        {
            rig.Cannons.Celebrate(Side.Player, 0.1f);
            Assert.Greater(rig.Cannons.JumpY(0), 0f);
            Assert.LessOrEqual(rig.Cannons.JumpY(0), rig.Config.victoryJumpHeight);
            Assert.AreNotEqual(rig.Cannons.JumpY(0), rig.Cannons.JumpY(1), "phases differ");
            rig.Cannons.Celebrate(Side.Player, 0f);
            Assert.AreEqual(0f, rig.Cannons.JumpY(0));
        }

        [Test]
        public void ThinGapCutsTheTopOffTheCastle()
        {
            DestructibleMask mask = red.Mask;
            int row = mask.Height / 2;
            // Щель в один тексель через весь замок: визуально верх уже отделён.
            for (int x = 0; x < mask.Width; x++)
            {
                int index = row * mask.Width + x;
                if (mask.Solid[index] != 0)
                {
                    mask.ClearTexel(index, x, row);
                }
            }
            mask.MarkDirtyRect(0, row, mask.Width - 1, row);
            mask.ConnectivityDirty = true;
            for (int i = 0; i < 20; i++)
            {
                rig.Step(Step);
            }
            for (int i = (row + 2) * mask.Width; i < mask.Solid.Length; i++)
            {
                Assert.Less(mask.Solid[i], DestructibleMask.SolidThreshold, "everything above the gap falls");
            }
        }

        [Test]
        public void DirectorKeepsEnemyCannonsUntilTheLastTurn()
        {
            rig.Cannons.SparedEnemyCannons = 2;
            int first = rig.Cannons.FirstCannon(Side.Enemy);
            int count = rig.Cannons.CountOf(Side.Enemy);
            for (int c = first; c < first + count; c++)
            {
                rig.Cannons.Detach(c, 0f, 0f, 0f);
            }
            Assert.AreEqual(2, rig.Match.EnemyAlive);
            rig.Cannons.SparedEnemyCannons = 0;
            Assert.IsTrue(rig.Cannons.Detach(first + count - 1, 0f, 0f, 0f));
        }

        [Test]
        public void FarBlastLeavesCannons()
        {
            rig.Cannons.ApplyBlast(Side.Enemy, -50f, 50f, 1f);
            Assert.AreEqual(rig.Config.enemyCannons, rig.Match.EnemyAlive);
        }

        [Test]
        public void MaterialGoneUnderMountDetachesCannon()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            float x = rig.Cannons.LocalX(cannon);
            float y = rig.Cannons.LocalY(cannon);
            // Прямой вырез маски: взрывная волна здесь не участвует, остаётся только условие 2.
            CraterStamper.StampCircle(red.Mask, red.Mask.TexelX(x), red.Mask.TexelY(y), 16f, 2f, 0f, 0);
            rig.Step(Step);
            Assert.IsFalse(rig.Cannons.IsAlive(cannon));
        }

        [Test]
        public void CutTopFallsAsChunkAndTakesCannonsAlong()
        {
            int alive = rig.Match.EnemyAlive;
            // Верх центральной башни выше крыш средних: иначе кусок держится на них. Рядом первая пушка.
            CutRedTower(rig.Config.platformHeight + TopCutHeight, 1.1f);
            Run(rig.Config.connectivityInterval * 2f);
            Assert.AreEqual(1, rig.Castles.ChunksActive);
            Assert.Less(rig.Match.EnemyAlive, alive);
            Assert.IsFalse(red.Mask.IsSolidWorld(0f, rig.Config.platformHeight + TopCutHeight + 1.2f));
        }

        [Test]
        public void ChunkFallsIntoWaterAndIsRecycled()
        {
            CutRedTower(rig.Config.platformHeight + TopCutHeight, 1.1f);
            Run(4f);
            Assert.AreEqual(0, rig.Castles.ChunksActive);
            Assert.Greater(rig.Particles.ActiveCount, 0);
        }

        [Test]
        public void DetachedCannonFallsAndDisappears()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            rig.Cannons.Detach(cannon, 2f, 3f, 1f);
            Assert.AreEqual(1, rig.Cannons.FallingCount);
            Run(4f);
            Assert.IsFalse(rig.Cannons.IsFalling(cannon));
            Assert.AreEqual(0, rig.Cannons.FallingCount);
        }

        [Test]
        public void LoweredScoreDetachesCannonsFromTheEnd()
        {
            rig.Match.Destroy(Side.Enemy, 2, 0);
            Assert.AreEqual(rig.Config.enemyCannons - 2, Alive(Side.Enemy));
            int last = rig.Cannons.FirstCannon(Side.Enemy) + rig.Config.enemyCannons - 1;
            Assert.IsFalse(rig.Cannons.IsAlive(last));
        }

        [Test]
        public void DirectorKeepsLastPlayerCannon()
        {
            for (int c = 0; c < rig.Config.playerCannons; c++)
            {
                rig.Cannons.Detach(c, 0f, 0f, 0f);
            }
            Assert.AreEqual(1, rig.Match.PlayerAlive);
            Assert.AreEqual(1, Alive(Side.Player));
        }

        [Test]
        public void WithoutDirectorLastCannonIsLost()
        {
            rig.Config.directorEnabled = false;
            for (int c = 0; c < rig.Config.playerCannons; c++)
            {
                rig.Cannons.Detach(c, 0f, 0f, 0f);
            }
            Assert.AreEqual(0, rig.Match.PlayerAlive);
        }

        [Test]
        public void SavedCannonDropsToPlatformWhenItsTowerIsGone()
        {
            for (int c = 1; c < rig.Config.playerCannons; c++)
            {
                rig.Cannons.Detach(c, 0f, 0f, 0f);
            }
            float x = rig.Cannons.LocalX(0);
            float y = rig.Cannons.LocalY(0);
            DestructibleMask mask = blue.Mask;
            CraterStamper.StampCircle(mask, mask.TexelX(x), mask.TexelY(y), 16f, 2f, 0f, 0);
            for (int i = 0; i < 200; i++)
            {
                rig.Step(Step);
            }
            Assert.IsTrue(rig.Cannons.IsAlive(0));
            Assert.IsTrue(rig.Cannons.IsOnPlatform(0));
            Assert.AreEqual(rig.Config.platformHeight, rig.Cannons.LocalY(0), 1e-4f);
        }

        [Test]
        public void SwayIsLimitedAndFades()
        {
            float limit = rig.Config.swayMaxDeg * Mathf.Deg2Rad;
            for (int i = 0; i < 200; i++)
            {
                red.AddSway(100f);
                rig.Step(Step);
                Assert.LessOrEqual(Mathf.Abs(red.Sway), limit + 1e-5f);
            }
            Run(6f);
            Assert.AreEqual(0f, red.Sway, 1e-3f);
        }

        [Test]
        public void SwayPushesTopAwayFromBlast()
        {
            float x = RedWorldX(0f, 10f);
            red.AddSway(red.PivotX + 5f);
            Run(0.1f);
            Assert.Less(RedWorldX(0f, 10f), x);
        }

        [Test]
        public void CannonsFollowCastleWhenItMoves()
        {
            int cannon = rig.Cannons.FirstCannon(Side.Enemy);
            float x = rig.Cannons.WorldX(cannon);
            red.AddSway(100f);
            Run(0.1f);
            Assert.AreNotEqual(x, rig.Cannons.WorldX(cannon));
        }

        [Test]
        public void GroundGoneUnderOneTrackTiltsAndSettlesSmoothly()
        {
            float trackX = red.TrackX(1);
            float ground = red.TrackY(1);
            for (float x = trackX - 2f; x <= trackX + 2f; x += 0.5f)
            {
                rig.Terrain.Explode(x, ground - 0.5f, 1.2f);
            }
            float previous = red.TrackY(1);
            float largestStep = 0f;
            for (int i = 0; i < 100; i++)
            {
                rig.Step(Step);
                largestStep = Mathf.Max(largestStep, Mathf.Abs(red.TrackY(1) - previous));
                previous = red.TrackY(1);
            }
            Assert.Less(red.TrackY(1), ground);
            Assert.Less(red.Angle, 0f, "right side went down");
            Assert.LessOrEqual(largestStep, rig.Config.castleSettleSpeed * Step + 1e-4f);
            Assert.GreaterOrEqual(red.Angle, -rig.Config.castleMaxTiltDeg * Mathf.Deg2Rad - 0.02f);
            Assert.AreEqual(CastleState.Standing, red.State);
        }

        [Test]
        public void TrackFollowsSlope()
        {
            float trackX = red.TrackX(0);
            rig.Terrain.Explode(trackX + 1.3f, red.TrackY(0), 1f);
            Run(1f);
            Assert.AreNotEqual(0f, red.TrackAngle(0));
        }

        [Test]
        public void NoGroundUnderBothTracksSinksCastleAndAllItsCannons()
        {
            for (float x = red.TrackX(0) - 3f; x <= red.TrackX(1) + 3f; x += 0.7f)
            {
                for (float y = -1f; y <= red.TrackY(0) + 1f; y += 0.7f)
                {
                    rig.Terrain.Explode(x, y, 1.2f);
                }
            }
            Run(0.1f);
            Assert.AreEqual(CastleState.Sinking, red.State);
            Assert.AreEqual(0, rig.Match.EnemyAlive);
            Run(4f);
            Assert.AreEqual(CastleState.Gone, red.State);
            float hit = red.FirstHit(red.CenterX - 5f, 5f, red.CenterX + 5f, 5f, rig.Config.rocketCollisionStep);
            Assert.AreEqual(-1f, hit);
        }

        [Test]
        public void ScorchBurnsFarMoreStoneThanARocket()
        {
            int solid = red.Mask.SolidCount;
            rig.Castles.HitWall(Side.Enemy, red.CenterX, red.PivotY + 3f, 1f);
            int rocket = solid - red.Mask.SolidCount;
            solid = red.Mask.SolidCount;
            rig.Castles.ScorchStone(Side.Enemy, red.CenterX, red.PivotY + 5f, rig.Config.finaleBlastRadius);
            Assert.Greater(solid - red.Mask.SolidCount, rocket * 4);
        }

        [Test]
        public void VaporizeRemovesEverythingAndStopsChunks()
        {
            rig.Castles.ScorchStone(Side.Enemy, red.CenterX, red.PivotY + 3f, 1.5f);
            rig.Castles.Vaporize(Side.Enemy);
            Assert.AreEqual(CastleState.Gone, red.State);
            Assert.AreEqual(0, rig.Match.EnemyAlive);
            Run(1f);
            Assert.AreEqual(0, rig.Castles.ChunksActive);
            Assert.AreEqual(-1f, red.FirstHit(red.CenterX - 5f, red.PivotY, red.CenterX + 5f, red.PivotY, rig.Config.rocketCollisionStep));
        }

        [Test]
        public void ResetBringsVaporizedCastleBack()
        {
            rig.Castles.Vaporize(Side.Enemy);
            rig.Cannons.Clear();
            rig.Match.Reset();
            rig.Castles.Reset();
            Assert.AreEqual(CastleState.Standing, red.State);
            Assert.AreEqual(rig.Config.enemyCannons, rig.Match.EnemyAlive);
        }

        [Test]
        public void ResetRestoresCastleAndCannons()
        {
            int solid = red.Mask.SolidCount;
            CutRedTower(rig.Config.platformHeight + TopCutHeight, 1.1f);
            red.AddSway(100f);
            Run(0.5f);
            rig.Match.Reset();
            rig.Cannons.Clear();
            rig.Terrain.Reset();
            rig.Castles.Reset();
            Assert.AreEqual(solid, red.Mask.SolidCount);
            Assert.AreEqual(0f, red.Angle);
            Assert.AreEqual(0, rig.Castles.ChunksActive);
            Assert.AreEqual(rig.Config.enemyCannons, Alive(Side.Enemy));
            for (int i = 0; i < red.Mask.Grass.Length; i++)
            {
                Assert.AreEqual(0, red.Mask.Grass[i]);
            }
        }

        [Test]
        public void FirstHitDoesNotAllocate()
        {
            float y = rig.Level.EnemyFocusY;
            red.FirstHit(rig.Config.enemyCastleX - 8f, y, rig.Config.enemyCastleX, y, 0.25f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            red.FirstHit(rig.Config.enemyCastleX - 8f, y, rig.Config.enemyCastleX, y, 0.25f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class UndercarriageViewTests
    {
        private CombatRig rig;
        private UndercarriageView view;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            view = new UndercarriageView();
            view.Init(rig.Config, rig.Castles);
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
        public void TracksStandOnGround()
        {
            view.Tick(0f);
            float lowest = float.PositiveInfinity;
            Vector3[] vertices = view.Mesh.vertices;
            for (int v = 0; v < vertices.Length; v++)
            {
                if (vertices[v].x < 0f)
                {
                    lowest = Mathf.Min(lowest, vertices[v].y);
                }
            }
            Assert.AreEqual(rig.Level.CastleGround(Side.Player), lowest, 0.3f);
        }

        [Test]
        public void TickWithoutChangesDoesNotTouchMesh()
        {
            view.Tick(0f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            view.Tick(0.016f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    /// <summary>Настоящие залпы по настоящему уровню: сколько пушек теряет враг.</summary>
    public sealed class CastleScenarioTests
    {
        private const float Step = 0.02f;
        private const int MaxSteps = 900;

        private CombatRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Destroy();
        }

        private void Fire(Side side, float angle, float power)
        {
            rig.Cannons.SetAim(side, angle, power);
            rig.Cannons.BeginSalvo(side);
            for (int i = 0; i < MaxSteps; i++)
            {
                rig.Step(Step);
                if (!rig.Cannons.IsFiring && rig.Rockets.ActiveCount == 0 && rig.Boosters.PendingCopies == 0)
                {
                    return;
                }
            }
            Assert.Fail("rockets are still flying");
        }

        [Test]
        public void FirstHintCostsEnemyCannonsButDoesNotEndTheMatch()
        {
            TurnHint hint = rig.Config.GetHint(0);
            Fire(Side.Player, hint.handAngleDeg, hint.handPower01);
            Assert.Less(rig.Match.EnemyAlive, rig.Config.enemyCannons);
            Assert.Greater(rig.Match.EnemyAlive, 0);
        }

        [Test]
        public void EnemySalvoHurtsBlueCastle()
        {
            int solid = rig.Castles.Body(Side.Player).Mask.SolidCount;
            Fire(Side.Enemy, rig.Config.aiPresets[0].angleDeg, rig.Config.aiPresets[0].power01);
            Assert.Less(rig.Castles.Body(Side.Player).Mask.SolidCount, solid);
            Assert.GreaterOrEqual(rig.Match.PlayerAlive, 1, "director keeps the last cannon");
        }
    }
}
