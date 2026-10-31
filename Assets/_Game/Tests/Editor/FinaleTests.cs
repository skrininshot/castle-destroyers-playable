using CastleDestroyers.CastleRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    /// <summary>Конец матча на настоящем уровне: замок проигравшего сгорает целиком, D-075.</summary>
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
        public void ChainBurnsTheCastleDownBeforeTheFinalBlast()
        {
            CastleBody red = rig.Castles.Body(Side.Enemy);
            int solid = red.Mask.SolidCount;
            ToVictory();
            Advance(rig.Config.finaleBlasts * rig.Config.finaleBlastInterval - Step * 2f);
            Assert.Less(red.Mask.SolidCount, solid * 0.75f);
            Assert.AreEqual(CastleState.Standing, red.State);
            Assert.IsFalse(show.Vaporized);
        }

        [Test]
        public void FinalBlastLeavesNothingAndCratersTheGround()
        {
            CastleBody red = rig.Castles.Body(Side.Enemy);
            float ground = red.TrackY(0);
            ToVictory();
            Advance(rig.Config.finaleBlasts * rig.Config.finaleBlastInterval + Step);
            Assert.IsTrue(show.Vaporized);
            Assert.AreEqual(CastleState.Gone, red.State);
            Assert.IsTrue(finale.RingActive);
            Assert.IsTrue(finale.FlashActive);
            Assert.Less(rig.Mask.SurfaceHeight(red.CenterX), ground);
        }

        [Test]
        public void SlowMotionEndsInRealTime()
        {
            ToVictory();
            Advance(rig.Config.finaleBlasts * rig.Config.finaleBlastInterval + Step);
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

        [Test]
        public void EnemyCanLoseEveryCannonOnAnyTurn()
        {
            Assert.AreEqual(0, rig.Config.directorEnemyKeepCannons);
            int first = rig.Cannons.FirstCannon(Side.Enemy);
            for (int c = first; c < first + rig.Cannons.CountOf(Side.Enemy); c++)
            {
                Assert.IsTrue(rig.Cannons.Detach(c, 0f, 0f, 0f));
            }
            Assert.IsTrue(rig.Match.IsEliminated(Side.Enemy));
        }
    }
}
