using CastleDestroyers.CastleRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    /// <summary>Конец матча на настоящем уровне: один взрыв уносит замок проигравшего, D-077.</summary>
    public sealed class VictoryShowTests
    {
        private const float Step = 0.02f;
        private const int MaxSteps = 1000;

        private CombatRig rig;
        private VictoryShow show;
        private FinaleFx finale;
        private GameLoop loop;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            rig.CameraObject = new GameObject("Camera", typeof(Camera));
            Camera camera = rig.CameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            GameObject host = new GameObject("Finale", typeof(MeshFilter), typeof(MeshRenderer));
            host.transform.SetParent(rig.CameraObject.transform, false);
            finale = new FinaleFx();
            finale.Init(rig.Config, camera, host.GetComponent<MeshFilter>(), new Rng(5u));
            finale.ApplyPalette(ThemePalette.CreateNight());
            show = new VictoryShow();
            show.Init(rig.Config, rig.Flow, rig.Cannons, rig.Castles, rig.Explosions, rig.Shake, rig.Fx, finale, new Rng(3u));
            loop = new GameLoop();
            loop.Init(new ITickable[0], rig.Config.maxDeltaTime);
            show.Loop = loop;
        }

        [TearDown]
        public void TearDown()
        {
            rig.Destroy();
        }

        private void ToVictory()
        {
            for (int i = 0; i < 64 && rig.Flow.State != SessionState.Resolve; i++)
            {
                rig.Flow.Skip();
            }
            rig.Match.Destroy(Side.Enemy, rig.Config.enemyCannons, 0);
            rig.Flow.Skip();
            Assert.AreEqual(SessionState.Victory, rig.Flow.State);
        }

        private void Advance(float seconds)
        {
            float left = seconds;
            for (int i = 0; i < MaxSteps && left > 0f; i++)
            {
                float dt = Step * loop.TimeScale;
                rig.Flow.Tick(dt);
                show.Tick(dt);
                rig.Step(dt);
                finale.Tick(dt);
                left -= dt;
            }
        }

        [Test]
        public void NothingBlowsUpBeforeTheBlastDelay()
        {
            CastleBody red = rig.Castles.Body(Side.Enemy);
            ToVictory();
            Advance(rig.Config.finaleBlastDelay - Step * 2f);
            Assert.AreEqual(CastleState.Standing, red.State);
            Assert.IsFalse(show.Vaporized);
        }

        [Test]
        public void OneBlastLeavesNothingAndABigCrater()
        {
            CastleBody red = rig.Castles.Body(Side.Enemy);
            float ground = red.TrackY(0);
            ToVictory();
            int explosions = rig.Explosions.ActiveCount;
            Advance(rig.Config.finaleBlastDelay + Step);
            Assert.IsTrue(show.Vaporized);
            Assert.AreEqual(CastleState.Gone, red.State);
            Assert.AreEqual(explosions + 1, rig.Explosions.ActiveCount);
            Assert.IsTrue(finale.RingActive);
            Assert.IsTrue(finale.FlashActive);
            float depth = rig.Config.finaleCraterRadius - rig.Config.finaleCraterLift;
            Assert.AreEqual(ground - depth, rig.Mask.SurfaceHeight(red.CenterX), 0.5f);
            Assert.Less(rig.Mask.SurfaceHeight(red.CenterX - red.PlatformHalf), ground - 1f);
            Assert.Less(rig.Mask.SurfaceHeight(red.CenterX + red.PlatformHalf), ground - 1f);
        }

        [Test]
        public void FinalBlastOutlivesAnOrdinaryExplosion()
        {
            ToVictory();
            Advance(rig.Config.finaleBlastDelay + Step);
            Advance(rig.Config.explosionLife * 1.2f);
            Assert.Greater(rig.Explosions.ActiveCount, 0);
        }

        [Test]
        public void ShakeIsNotStretchedBySlowMotion()
        {
            ToVictory();
            Advance(rig.Config.finaleBlastDelay + Step);
            Assert.LessOrEqual(rig.Shake.Amplitude, rig.Config.finaleShake + 1e-3f);
            for (int i = 0; i * Step < rig.Config.finaleSlowTime; i++)
            {
                rig.Shake.Tick(Step * loop.TimeScale);
                show.Tick(Step * loop.TimeScale);
            }
            Assert.Less(rig.Shake.Amplitude, rig.Config.finaleShake * 0.3f);
        }

        [Test]
        public void SlowMotionEndsInRealTime()
        {
            ToVictory();
            Advance(rig.Config.finaleBlastDelay + Step);
            Assert.Less(loop.TimeScale, 1f);
            for (int i = 0; i * Step < rig.Config.finaleSlowTime + Step * 2f; i++)
            {
                show.Tick(Step * loop.TimeScale);
            }
            Assert.AreEqual(1f, loop.TimeScale);
        }

        [Test]
        public void WinnerGetsConfettiAfterTheCameraTurns()
        {
            ToVictory();
            Advance(rig.Config.victoryLoserHold - Step * 4f);
            Assert.AreEqual(0, finale.ConfettiCount);
            Advance(Step * 8f);
            Assert.Greater(finale.ConfettiCount, 0);
        }
    }
}
