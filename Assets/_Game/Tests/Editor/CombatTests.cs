using System;
using CastleDestroyers.CameraRig;
using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.InputRig;
using CastleDestroyers.Level;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CastleDestroyers.Tests
{
    /// <summary>Боевые системы на настоящем уровне, без сцены.</summary>
    internal sealed class CombatRig
    {
        public readonly GameConfig Config;
        public readonly DestructibleMask Mask;
        public readonly GreyboxLevel Level;
        public readonly MatchState Match;
        public readonly SessionFlow Flow;
        public readonly TerrainSystem Terrain;
        public readonly FlatParticles Particles;
        public readonly ScreenShake Shake;
        public readonly SmokeTrails Smoke;
        public readonly ExplosionSystem Explosions;
        public readonly RocketSystem Rockets;
        public readonly BoosterSystem Boosters;
        public readonly CannonSystem Cannons;

        private readonly GameObject root;
        private readonly Material material;

        public CombatRig(Action<GameConfig> tune = null)
        {
            Config = MaskFixture.LevelConfig();
            if (tune != null)
            {
                tune(Config);
            }
            root = new GameObject("CombatTest");
            material = new Material(Shader.Find("Game/Terrain"));
            Rng rng = new Rng(1u);
            PaletteSet palette = ThemePalette.CreateNight();

            Particles = new FlatParticles();
            Particles.Init(Config.fxParticleCapacity);
            LevelFx fx = new LevelFx();
            fx.Init(Config, Particles, rng);
            fx.ApplyPalette(palette);

            Mask = DestructibleMask.CreateTerrain(Config);
            MaskTileView view = new MaskTileView();
            view.Init(Mask, material, Config.maskEdgeSoftness, root.transform, 0);
            ChunkSystem chunks = new ChunkSystem();
            chunks.Init(Config, fx, rng, material, root.transform, 1);
            Terrain = new TerrainSystem();
            Terrain.Init(Config, Mask, view, chunks, fx);

            Level = new GreyboxLevel();
            Level.Init(Config, Mask);
            Match = new MatchState();
            Match.Init(Config.playerCannons, Config.enemyCannons);
            Flow = new SessionFlow();
            Flow.Init(Config, Match);

            Shake = new ScreenShake();
            Shake.Init(Config);
            Smoke = new SmokeTrails();
            Smoke.Init(Config);
            Smoke.ApplyPalette(palette);
            Explosions = new ExplosionSystem();
            Explosions.Init(Config, Terrain, Particles, fx, Shake, rng);
            Explosions.ApplyPalette(palette);
            Boosters = new BoosterSystem();
            Rockets = new RocketSystem();
            Rockets.Init(Config, Mask, Level, Explosions, Smoke, fx, Boosters);
            Rockets.ApplyPalette(palette);
            Boosters.Init(Config, Rockets, Particles, rng);
            Boosters.ApplyPalette(palette);
            Cannons = new CannonSystem();
            Cannons.Init(Config, Level, Match, Rockets, Particles);
            Cannons.ApplyPalette(palette);
        }

        public void Destroy()
        {
            Object.DestroyImmediate(Rockets.Mesh);
            Object.DestroyImmediate(Smoke.Mesh);
            Object.DestroyImmediate(Explosions.Mesh);
            Object.DestroyImmediate(Particles.Mesh);
            for (int i = 0; i < Level.BackLayers.Length; i++)
            {
                Object.DestroyImmediate(Level.BackLayers[i]);
            }
            Object.DestroyImmediate(Level.FrontMesh);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(Config);
        }

        public Vector3 RocketCenter(int slot)
        {
            Vector3[] vertices = Rockets.Mesh.vertices;
            int v = slot * 4;
            return (vertices[v] + vertices[v + 1] + vertices[v + 2] + vertices[v + 3]) * 0.25f;
        }
    }

    public sealed class BallisticsTests
    {
        [Test]
        public void PowerMapsToSpeedRange()
        {
            Assert.AreEqual(12f, Ballistics.Speed(0f, 12f, 32f), 1e-5f);
            Assert.AreEqual(22f, Ballistics.Speed(0.5f, 12f, 32f), 1e-5f);
            Assert.AreEqual(32f, Ballistics.Speed(2f, 12f, 32f), 1e-5f);
        }

        [Test]
        public void TimeToHeightSolvesParabola()
        {
            float t = Ballistics.TimeToHeight(5f, 10f, 14f, 0f);
            Assert.AreEqual(0f, 5f + 10f * t - 7f * t * t, 1e-3f);
            Assert.Greater(t, 0f);
        }

        [Test]
        public void UnreachableHeightGivesZero()
        {
            Assert.AreEqual(0f, Ballistics.TimeToHeight(0f, 1f, 14f, 100f));
        }
    }

    public sealed class FirstSolidTests
    {
        private DestructibleMask mask;

        [SetUp]
        public void SetUp()
        {
            mask = MaskFixture.Filled(0);
            MaskFixture.SetRect(mask, 20, 0, 20, 30, DestructibleMask.Full);
        }

        [Test]
        public void FindsOneTexelWallOnLongSegment()
        {
            float fraction = mask.FirstSolid(0f, 1f, 2.5f, 1f, 1f / MaskFixture.TexelsPerMeter);
            Assert.GreaterOrEqual(fraction, 0f);
            float hitX = 2.5f * fraction;
            Assert.AreEqual(20f / MaskFixture.TexelsPerMeter, hitX, 0.1f);
        }

        [Test]
        public void ClearSegmentGivesMinusOne()
        {
            Assert.AreEqual(-1f, mask.FirstSolid(0f, 1f, 1f, 1f, 0.05f));
        }

        [Test]
        public void SegmentOutsideMaskIsClear()
        {
            Assert.AreEqual(-1f, mask.FirstSolid(-5f, -5f, -1f, -2f, 0.1f));
        }

        [Test]
        public void DoesNotAllocate()
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            mask.FirstSolid(0f, 1f, 2.5f, 1f, 0.05f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class RocketSystemTests
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
        public void RocketFliesExactParabolaAtAnyFrameRate()
        {
            float startX = -30f;
            float startY = 25f;
            float[] steps = { 0.016f, 0.05f };
            for (int n = 0; n < steps.Length; n++)
            {
                rig.Rockets.Clear();
                rig.Rockets.Spawn(startX, startY, 8f, 4f, Side.Player, 0f);
                float time = 0f;
                for (int i = 0; i < 20; i++)
                {
                    rig.Rockets.Tick(steps[n]);
                    time += steps[n];
                    Vector3 center = rig.RocketCenter(0);
                    Assert.AreEqual(startX + 8f * time, center.x, 1e-2f);
                    Assert.AreEqual(startY + 4f * time - 0.5f * rig.Config.gravity * time * time, center.y, 1e-2f);
                }
            }
        }

        [Test]
        public void FastRocketDoesNotTunnelThroughMountain()
        {
            rig.Rockets.Spawn(-12f, 5f, 60f, 0f, Side.Player, 0f);
            for (int i = 0; i < 30 && rig.Rockets.ActiveCount > 0; i++)
            {
                rig.Rockets.Tick(0.05f);
            }
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
            Assert.AreEqual(1, rig.Explosions.ActiveCount);
            Assert.Greater(rig.Mask.DirtyCount, 0, "crater was stamped");
        }

        [Test]
        public void WaterRemovesRocketWithoutExplosion()
        {
            rig.Rockets.Spawn(-49.5f, 3f, -1f, -20f, Side.Player, 0f);
            for (int i = 0; i < 20 && rig.Rockets.ActiveCount > 0; i++)
            {
                rig.Rockets.Tick(0.05f);
            }
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
            Assert.AreEqual(0, rig.Explosions.ActiveCount);
            Assert.Greater(rig.Particles.ActiveCount, 0, "splash");
        }

        [Test]
        public void LeavingLevelRemovesRocket()
        {
            rig.Rockets.Spawn(rig.Config.levelWidth - 0.1f, 30f, 30f, 10f, Side.Player, 0f);
            rig.Rockets.Tick(0.05f);
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
            Assert.AreEqual(0, rig.Explosions.ActiveCount);
        }

        [Test]
        public void RocketsIgnoreOwnCastleAndHitEnemyCastle()
        {
            float y = rig.Level.EnemyFocusY;
            float x = rig.Config.enemyCastleX;
            rig.Rockets.Spawn(x - 0.2f, y, 1f, 0f, Side.Enemy, 0f);
            rig.Rockets.Tick(0.02f);
            Assert.AreEqual(1, rig.Rockets.ActiveCount, "enemy rocket passes its own castle");
            rig.Rockets.Spawn(x - 0.2f, y, 1f, 0f, Side.Player, 0f);
            rig.Rockets.Tick(0.02f);
            Assert.AreEqual(1, rig.Rockets.ActiveCount, "player rocket exploded");
            Assert.AreEqual(1, rig.Explosions.ActiveCount);
            Assert.AreEqual(0, rig.Mask.DirtyCount, "castle hit leaves no terrain crater");
        }

        [Test]
        public void BudgetLimitsActiveRockets()
        {
            rig.Rockets.Budget = 5;
            for (int i = 0; i < 8; i++)
            {
                rig.Rockets.Spawn(-30f, 30f, 1f, 1f, Side.Player, 0f);
            }
            Assert.AreEqual(5, rig.Rockets.ActiveCount);
            Assert.IsFalse(rig.Rockets.Spawn(-30f, 30f, 1f, 1f, Side.Player, 0f));
        }

        [Test]
        public void StressBudgetFitsBuffer()
        {
            rig.Rockets.Budget = rig.Config.stressRocketCount;
            int spawned = 0;
            for (int i = 0; i < rig.Config.stressRocketCount; i++)
            {
                spawned += rig.Rockets.Spawn(-30f, 30f, 1f, 1f, Side.Player, 0f) ? 1 : 0;
            }
            Assert.AreEqual(rig.Config.stressRocketCount, spawned);
        }

        [Test]
        public void HeadIsTheFarthestRocketOfSide()
        {
            rig.Rockets.Spawn(-20f, 30f, 2f, 0f, Side.Player, 0f);
            rig.Rockets.Spawn(-10f, 30f, 2f, 0f, Side.Player, 0f);
            rig.Rockets.Spawn(20f, 30f, -2f, 0f, Side.Enemy, 0f);
            rig.Rockets.Tick(0.1f);
            Assert.AreEqual(-9.8f, rig.Rockets.HeadX(Side.Player), 1e-3f);
            Assert.AreEqual(19.8f, rig.Rockets.HeadX(Side.Enemy), 1e-3f);
            Assert.AreEqual(2, rig.Rockets.Count(Side.Player));
            Assert.AreEqual(1, rig.Rockets.Count(Side.Enemy));
        }

        [Test]
        public void SmokeLodThinsEmittersAboveThreshold()
        {
            int rockets = rig.Config.smokeLodRocketThreshold * 2;
            rig.Rockets.Budget = rig.Config.stressRocketCount;
            for (int i = 0; i < rockets; i++)
            {
                rig.Rockets.Spawn(-30f, 60f, 1f, 0f, Side.Player, 0f);
            }
            rig.Rockets.Tick(0.01f);
            Assert.AreEqual(rockets / 3 + (rockets % 3 == 0 ? 0 : 1), rig.Smoke.ActiveCount);
        }

        [Test]
        public void ClearRemovesRocketsImmediately()
        {
            rig.Rockets.Spawn(-30f, 30f, 1f, 1f, Side.Player, 0f);
            rig.Rockets.Tick(0.1f);
            rig.Rockets.Clear();
            Assert.AreEqual(0, rig.Rockets.ActiveCount);
            Assert.AreEqual(0, rig.Rockets.Count(Side.Player));
            Assert.AreEqual(Vector3.zero, rig.Rockets.Mesh.vertices[0]);
        }

        [Test]
        public void TickDoesNotAllocate()
        {
            for (int i = 0; i < 300; i++)
            {
                rig.Rockets.Spawn(-30f, 30f, 3f, 1f, Side.Player, 0f);
            }
            rig.Rockets.Tick(0.016f);
            // Загрузка меша в редакторе копирует массивы на стороне Unity: проверяем только управляемую кучу.
            long before = GC.GetAllocatedBytesForCurrentThread();
            rig.Rockets.Tick(0.016f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class CannonSystemTests
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

        private void RunSalvo(Side side, float step)
        {
            rig.Cannons.BeginSalvo(side);
            for (int i = 0; i < 200 && rig.Cannons.IsFiring; i++)
            {
                rig.Cannons.Tick(step);
            }
            Assert.IsFalse(rig.Cannons.IsFiring);
        }

        [Test]
        public void SalvoFiresRocketsPerCannonFromEveryLiveCannon()
        {
            rig.Cannons.SetAim(Side.Player, 80f, 1f);
            RunSalvo(Side.Player, 0.016f);
            Assert.AreEqual(rig.Config.playerCannons * rig.Config.rocketsPerCannon, rig.Rockets.ActiveCount);
        }

        [Test]
        public void SlowFramesDoNotLoseRockets()
        {
            rig.Cannons.SetAim(Side.Enemy, 80f, 1f);
            RunSalvo(Side.Enemy, 0.05f);
            Assert.AreEqual(rig.Config.enemyCannons * rig.Config.rocketsPerCannon, rig.Rockets.ActiveCount);
        }

        [Test]
        public void DeadCannonsDoNotFire()
        {
            rig.Match.Destroy(Side.Enemy, rig.Config.enemyCannons - 2, 0);
            rig.Cannons.SetAim(Side.Enemy, 80f, 1f);
            RunSalvo(Side.Enemy, 0.016f);
            Assert.AreEqual(2 * rig.Config.rocketsPerCannon, rig.Rockets.ActiveCount);
        }

        [Test]
        public void BudgetCapsTheSalvo()
        {
            rig.Rockets.Budget = 7;
            rig.Cannons.SetAim(Side.Enemy, 80f, 1f);
            RunSalvo(Side.Enemy, 0.016f);
            Assert.AreEqual(7, rig.Rockets.ActiveCount);
        }

        [Test]
        public void CannonsTurnAtConfiguredSpeedAndStopOnTarget()
        {
            rig.Cannons.SetAim(Side.Player, 30f, 1f);
            rig.Cannons.Tick(0.05f);
            Assert.AreEqual(rig.Config.aimTurnSpeed * 0.05f, rig.Cannons.AngleDeg(Side.Player), 1e-3f);
            rig.Cannons.Tick(1f);
            Assert.AreEqual(30f, rig.Cannons.AngleDeg(Side.Player), 1e-3f);
        }

        [Test]
        public void ShotPushesBarrelBackAndItReturns()
        {
            rig.Cannons.SetAim(Side.Player, 80f, 1f);
            rig.Cannons.BeginSalvo(Side.Player);
            rig.Cannons.Tick(0.001f);
            float pushed = 0f;
            for (int barrel = 0; barrel < CannonSystem.BarrelCount; barrel++)
            {
                pushed = Mathf.Max(pushed, rig.Cannons.Recoil(0, barrel));
            }
            Assert.AreEqual(rig.Config.recoilDistance, pushed, 1e-4f);
            rig.Cannons.Clear();
            Assert.AreEqual(0f, rig.Cannons.Recoil(0, 1));
        }

        [Test]
        public void RocketsOfOneCannonFollowTheSameCurve()
        {
            // Один ствол на ракету из трёх: сравниваем 1-ю и 4-ю, они вылетают из одного ствола.
            rig.Cannons.SetAim(Side.Player, 80f, 1f);
            rig.Cannons.Tick(1f);
            rig.Match.Destroy(Side.Player, rig.Config.playerCannons - 1, 0);
            rig.Cannons.BeginSalvo(Side.Player);
            float step = rig.Config.rocketInterval * CannonSystem.BarrelCount;
            rig.Cannons.Tick(step + 1e-4f);
            rig.Rockets.Tick(0f);
            Vector3 earlier = rig.RocketCenter(0);
            Vector3 later = rig.RocketCenter(3);
            rig.Rockets.Tick(step);
            Assert.AreEqual(earlier.x, rig.RocketCenter(3).x, 1e-2f);
            Assert.AreEqual(earlier.y, rig.RocketCenter(3).y, 1e-2f);
            Assert.AreNotEqual(earlier.y, later.y);
        }

        [Test]
        public void MuzzleCenterFollowsAim()
        {
            rig.Cannons.SetAim(Side.Enemy, 0f, 1f);
            float flat = rig.Cannons.MuzzleCenterX(Side.Enemy);
            rig.Cannons.SetAim(Side.Enemy, 90f, 1f);
            Assert.AreEqual(flat + CannonSystem.BarrelLength, rig.Cannons.MuzzleCenterX(Side.Enemy), 1e-3f);
        }

        [Test]
        public void DefaultHintOnePassesThroughBoosterX20()
        {
            TurnHint hint = rig.Config.GetHint(0);
            rig.Cannons.SetAim(Side.Player, hint.handAngleDeg, hint.handPower01);
            float radians = hint.handAngleDeg * Mathf.Deg2Rad;
            float speed = Ballistics.Speed(hint.handPower01, rig.Config.launchSpeedMin, rig.Config.launchSpeedMax);
            float time = (0f - rig.Cannons.MuzzleCenterX(Side.Player)) / (Mathf.Cos(radians) * speed);
            float y = rig.Cannons.MuzzleCenterY(Side.Player) + Mathf.Sin(radians) * speed * time - 0.5f * rig.Config.gravity * time * time;
            BoosterSpec booster = rig.Config.boosters[0];
            Assert.AreEqual(20, booster.multiplier);
            Assert.AreEqual(booster.position.y, y, rig.Config.boosterHeight * 0.5f);
        }
    }

    public sealed class ExplosionSystemTests
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
        public void CloseExplosionsInOneFrameMergeButStampEachCrater()
        {
            int solid = rig.Mask.SolidCount;
            rig.Explosions.Explode(-5f, 3f, true);
            int afterFirst = rig.Mask.SolidCount;
            rig.Explosions.Explode(-4.7f, 3f, true);
            Assert.AreEqual(1, rig.Explosions.ActiveCount);
            Assert.AreEqual(1, rig.Explosions.MergedTotal);
            Assert.Less(afterFirst, solid);
            Assert.Less(rig.Mask.SolidCount, afterFirst, "second crater stamped");
        }

        [Test]
        public void ExplosionsInDifferentFramesDoNotMerge()
        {
            rig.Explosions.Explode(-5f, 3f, true);
            rig.Explosions.Tick(0.001f);
            rig.Explosions.Explode(-5f, 3f, true);
            Assert.AreEqual(2, rig.Explosions.ActiveCount);
        }

        [Test]
        public void FarExplosionsDoNotMerge()
        {
            rig.Explosions.Explode(-5f, 3f, true);
            rig.Explosions.Explode(5f, 3f, true);
            Assert.AreEqual(2, rig.Explosions.ActiveCount);
        }

        [Test]
        public void PoolLimitDropsVisualsButKeepsCraters()
        {
            int solid = rig.Mask.SolidCount;
            for (int i = 0; i < rig.Config.explosionPoolSize + 10; i++)
            {
                rig.Explosions.Explode(i % 2 == 0 ? -6f + i * 0.1f : 6f - i * 0.1f, i * 0.2f, true);
                rig.Explosions.Tick(0.0001f);
            }
            Assert.AreEqual(rig.Config.explosionPoolSize, rig.Explosions.ActiveCount);
            Assert.Less(rig.Mask.SolidCount, solid);
        }

        [Test]
        public void ExplosionEndsAfterLife()
        {
            rig.Explosions.Explode(-5f, 3f, false);
            rig.Explosions.Tick(rig.Config.explosionLife + 0.01f);
            Assert.AreEqual(0, rig.Explosions.ActiveCount);
        }

        [Test]
        public void PulseRisesAndFallsToZero()
        {
            Assert.AreEqual(0f, ExplosionSystem.Pulse(0f), 1e-5f);
            Assert.AreEqual(1f, ExplosionSystem.Pulse(0.3f), 1e-5f);
            Assert.AreEqual(0f, ExplosionSystem.Pulse(1f), 1e-5f);
        }

        [Test]
        public void TickDoesNotAllocate()
        {
            rig.Explosions.Explode(-5f, 3f, false);
            rig.Explosions.Tick(0.001f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            rig.Explosions.Tick(0.001f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class ScreenShakeTests
    {
        private GameConfig config;
        private ScreenShake shake;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            shake = new ScreenShake();
            shake.Init(config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ImpulsesAddUpToSaturation()
        {
            for (int i = 0; i < 1000; i++)
            {
                shake.Add(config.shakePerExplosion);
            }
            Assert.AreEqual(config.shakeMax, shake.Amplitude, 1e-5f);
        }

        [Test]
        public void ShakeDecays()
        {
            shake.Add(config.shakeMax);
            for (int i = 0; i < 300; i++)
            {
                shake.Tick(0.016f);
            }
            Assert.Less(shake.Amplitude, 0.01f);
        }

        [Test]
        public void OffsetNeverExceedsAmplitude()
        {
            shake.Add(0.5f);
            for (int i = 0; i < 100; i++)
            {
                shake.Tick(0.016f);
                Assert.LessOrEqual(Mathf.Abs(shake.OffsetX), shake.Amplitude + 1e-5f);
                Assert.LessOrEqual(Mathf.Abs(shake.OffsetY), shake.Amplitude + 1e-5f);
            }
        }
    }

    public sealed class AimGestureTests
    {
        private const float Height = 1000f;

        private GameConfig config;
        private AimGesture gesture;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            gesture = new AimGesture();
            gesture.Init(config);
            gesture.Begin(500f, 500f, Height);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void PullingBackAndDownAimsForwardAndUp()
        {
            gesture.Move(400f, 400f);
            Assert.IsTrue(gesture.Armed);
            Assert.AreEqual(45f, gesture.AngleDeg, 1e-3f);
        }

        [Test]
        public void PowerGrowsWithDragAndIsClamped()
        {
            gesture.Move(500f, 500f - config.aimMaxDragScreenFraction * Height * 0.5f);
            Assert.AreEqual(0.5f, gesture.Power01, 1e-3f);
            gesture.Move(500f, 0f);
            Assert.AreEqual(1f, gesture.Power01, 1e-3f);
        }

        [Test]
        public void AngleIsClampedToConfiguredRange()
        {
            gesture.Move(500f, 100f);
            Assert.AreEqual(config.aimMaxDeg, gesture.AngleDeg, 1e-3f);
            gesture.Move(100f, 600f);
            Assert.AreEqual(config.aimMinDeg, gesture.AngleDeg, 1e-3f);
            gesture.Move(900f, 300f);
            Assert.AreEqual(config.aimMaxDeg, gesture.AngleDeg, 1e-3f);
        }

        [Test]
        public void ShortTapInDeadZoneDoesNotFire()
        {
            gesture.Move(501f, 501f);
            Assert.IsFalse(gesture.Armed);
            Assert.IsFalse(gesture.Release());
        }

        [Test]
        public void ReturningToDeadZoneDisarms()
        {
            gesture.Move(300f, 300f);
            gesture.Move(500f, 500f);
            Assert.IsFalse(gesture.Release());
        }

        [Test]
        public void ReleaseAfterPullFiresOnce()
        {
            gesture.Move(300f, 300f);
            Assert.IsTrue(gesture.Release());
            Assert.IsFalse(gesture.Release());
        }

        [Test]
        public void PointModeUsesFingerDirectionAndFixedPower()
        {
            gesture.Point(1f, 1f);
            Assert.IsTrue(gesture.Armed);
            Assert.AreEqual(45f, gesture.AngleDeg, 1e-3f);
            Assert.AreEqual(config.aimPointPower, gesture.Power01, 1e-5f);
        }
    }

    public sealed class AimInputTests
    {
        private const float Height = 1000f;
        private const float StartY = 700f;

        private CombatRig rig;
        private AimInput aim;
        private GameObject cameraObject;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            cameraObject = new GameObject("AimCamera", typeof(Camera));
            aim = new AimInput();
            aim.Init(rig.Config, rig.Flow, rig.Cannons, cameraObject.GetComponent<Camera>());
            aim.Scripted = true;
            for (int i = 0; i < 16 && rig.Flow.State != SessionState.Aim; i++)
            {
                rig.Flow.Skip();
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            rig.Destroy();
        }

        [Test]
        public void PullAndReleaseFires()
        {
            aim.Feed(true, 500f, StartY, Height);
            aim.Feed(true, 300f, StartY - 200f, Height);
            Assert.IsTrue(aim.Aiming);
            Assert.AreEqual(SessionState.Aim, rig.Flow.State);
            aim.Feed(false, 300f, StartY - 200f, Height);
            Assert.AreEqual(SessionState.Fire, rig.Flow.State);
            Assert.IsTrue(rig.Cannons.HasAim(Side.Player));
        }

        [Test]
        public void TapDoesNotFire()
        {
            aim.Feed(true, 500f, StartY, Height);
            aim.Feed(false, 500f, StartY, Height);
            Assert.AreEqual(SessionState.Aim, rig.Flow.State);
            Assert.IsFalse(rig.Cannons.HasAim(Side.Player));
        }

        [Test]
        public void GestureDuringFlightIsIgnored()
        {
            rig.Flow.Skip();
            rig.Flow.Skip();
            Assert.AreEqual(SessionState.Flight, rig.Flow.State);
            aim.Feed(true, 500f, StartY, Height);
            aim.Feed(true, 300f, StartY - 200f, Height);
            Assert.IsFalse(aim.Aiming);
            aim.Feed(false, 300f, StartY - 200f, Height);
            Assert.AreEqual(SessionState.Flight, rig.Flow.State);
        }

        [Test]
        public void FingerHeldSinceFlightDoesNotStartGesture()
        {
            rig.Flow.Skip();
            rig.Flow.Skip();
            aim.Feed(true, 500f, StartY, Height);
            while (!(rig.Flow.State == SessionState.Aim && rig.Flow.ActiveSide == Side.Player))
            {
                rig.Flow.Skip();
            }
            aim.Feed(true, 300f, StartY - 200f, Height);
            Assert.IsFalse(aim.Aiming);
            aim.Feed(false, 300f, StartY - 200f, Height);
            Assert.AreEqual(SessionState.Aim, rig.Flow.State);
        }

        [Test]
        public void GestureStopsIdleTimer()
        {
            rig.Flow.Tick(rig.Config.playerAimDuration * 0.9f);
            aim.Feed(true, 500f, StartY, Height);
            aim.Feed(true, 500f, StartY, Height);
            rig.Flow.Tick(rig.Config.playerAimDuration * 0.9f);
            Assert.AreEqual(SessionState.Aim, rig.Flow.State);
        }

        [Test]
        public void TouchInDebugButtonBandDoesNotStartGesture()
        {
#if GAME_DEBUG
            aim.Feed(true, 500f, 10f, Height);
            aim.Feed(true, 300f, 200f, Height);
            Assert.IsFalse(aim.Aiming);
#endif
        }
    }

    public sealed class TurnCombatTests
    {
        private CombatRig rig;
        private CameraDirector director;
        private TurnCombat combat;
        private GameObject cameraObject;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            cameraObject = new GameObject("CombatCamera", typeof(Camera));
            director = new CameraDirector();
            director.Init(cameraObject.GetComponent<Camera>(), rig.Config, rig.Level, rig.Flow, rig.Shake);
            combat = new TurnCombat();
            combat.Init(rig.Config, rig.Flow, rig.Cannons, rig.Rockets, rig.Boosters, director);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            rig.Destroy();
        }

        private void AdvanceTo(SessionState state, Side side)
        {
            for (int i = 0; i < 32 && !(rig.Flow.State == state && rig.Flow.ActiveSide == side); i++)
            {
                rig.Flow.Skip();
            }
            Assert.AreEqual(state, rig.Flow.State);
        }

        [Test]
        public void IdleShotUsesTurnHint()
        {
            AdvanceTo(SessionState.Fire, Side.Player);
            rig.Cannons.Tick(1f);
            Assert.AreEqual(rig.Config.GetHint(0).handAngleDeg, rig.Cannons.AngleDeg(Side.Player), 1e-3f);
            Assert.Greater(rig.Rockets.ActiveCount, 0);
        }

        [Test]
        public void EnemyTurnsCannonsWhileAiming()
        {
            AdvanceTo(SessionState.Aim, Side.Enemy);
            rig.Cannons.Tick(1f);
            Assert.AreEqual(rig.Config.stubAiAngleDeg, rig.Cannons.AngleDeg(Side.Enemy), 1e-3f);
        }

        [Test]
        public void FlightWaitsForRockets()
        {
            AdvanceTo(SessionState.Fire, Side.Player);
            rig.Cannons.Tick(1f);
            AdvanceTo(SessionState.Flight, Side.Player);
            combat.Tick(0f);
            Assert.IsTrue(rig.Flow.FlightBusy);
            rig.Flow.Tick(rig.Config.flightDuration + 0.1f);
            Assert.AreEqual(SessionState.Flight, rig.Flow.State);

            rig.Rockets.Clear();
            combat.Tick(0f);
            Assert.IsFalse(rig.Flow.FlightBusy);
            rig.Flow.Tick(0.01f);
            Assert.AreEqual(SessionState.Resolve, rig.Flow.State);
        }

        [Test]
        public void HeadPastCenterSendsCameraToTarget()
        {
            AdvanceTo(SessionState.Flight, Side.Player);
            rig.Rockets.Spawn(rig.Config.centerFocus.x - 3f, 45f, 1f, 0f, Side.Player, 0f);
            rig.Rockets.Tick(0.01f);
            combat.Tick(0f);
            Assert.IsFalse(director.HeadPassedCenter);
            rig.Rockets.Spawn(rig.Config.centerFocus.x + 3f, 45f, 1f, 0f, Side.Player, 0f);
            rig.Rockets.Tick(0.01f);
            combat.Tick(0f);
            Assert.IsTrue(director.HeadPassedCenter);
        }
    }
}
