using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    public sealed class TerrainProfileTests
    {
        [Test]
        public void InterpolatesLinearlyBetweenPoints()
        {
            Vector2[] profile = { new Vector2(0f, 0f), new Vector2(10f, 10f) };
            Assert.AreEqual(5f, TerrainProfile.HeightAt(profile, 5f), 1e-5f);
        }

        [Test]
        public void ClampsOutsideProfileToEdgeHeights()
        {
            Vector2[] profile = { new Vector2(0f, 2f), new Vector2(10f, 6f) };
            Assert.AreEqual(2f, TerrainProfile.HeightAt(profile, -5f));
            Assert.AreEqual(6f, TerrainProfile.HeightAt(profile, 50f));
        }

        [Test]
        public void VerticalStepReturnsUpperHeight()
        {
            Vector2[] profile = { new Vector2(0f, 0f), new Vector2(5f, 0f), new Vector2(5f, 4f), new Vector2(10f, 4f) };
            Assert.AreEqual(4f, TerrainProfile.HeightAt(profile, 5f));
            Assert.AreEqual(0f, TerrainProfile.HeightAt(profile, 4.9f), 1e-5f);
            Assert.AreEqual(4f, TerrainProfile.HeightAt(profile, 5.1f), 1e-5f);
        }

        [Test]
        public void EmptyProfileIsFlatAtZero()
        {
            Assert.AreEqual(0f, TerrainProfile.HeightAt(null, 3f));
            Assert.AreEqual(0f, TerrainProfile.HeightAt(new Vector2[0], 3f));
        }

        [Test]
        public void DefaultLevelHasMountainWithCrevasseAndHills()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            Vector2[] profile = config.terrainProfile;
            Assert.AreEqual(1.8f, TerrainProfile.HeightAt(profile, config.playerCastleX), 1e-4f);
            Assert.AreEqual(1.8f, TerrainProfile.HeightAt(profile, config.enemyCastleX), 1e-4f);
            Assert.AreEqual(11.6f, TerrainProfile.HeightAt(profile, -1.05f), 1e-4f);
            Assert.AreEqual(11.6f, TerrainProfile.HeightAt(profile, 1.05f), 1e-4f);
            Assert.Less(TerrainProfile.HeightAt(profile, 0f), config.waterLevel, "crevasse goes down to the water");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SmoothPassesThroughControlPoints()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            Vector2[] dense = TerrainProfile.Smooth(config.terrainProfile, 0.14f);
            for (int i = 0; i < config.terrainProfile.Length; i++)
            {
                Vector2 point = config.terrainProfile[i];
                bool found = false;
                for (int k = 0; k < dense.Length && !found; k++)
                {
                    found = (dense[k] - point).sqrMagnitude < 1e-6f;
                }
                Assert.IsTrue(found, "control point " + i);
            }
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SmoothDoesNotOvershootAndKeepsXOrder()
        {
            Vector2[] control = { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 5f), new Vector2(3f, 5f), new Vector2(4f, 1f) };
            Vector2[] dense = TerrainProfile.Smooth(control, 0.1f);
            for (int i = 0; i < dense.Length; i++)
            {
                Assert.GreaterOrEqual(dense[i].y, -1e-4f);
                Assert.LessOrEqual(dense[i].y, 5f + 1e-4f);
                if (i > 0)
                {
                    Assert.GreaterOrEqual(dense[i].x, dense[i - 1].x);
                }
            }
        }

        [Test]
        public void SmoothKeepsVerticalStepSharp()
        {
            Vector2[] control = { new Vector2(0f, 4f), new Vector2(2f, 4f), new Vector2(2f, -1f), new Vector2(4f, -1f) };
            Vector2[] dense = TerrainProfile.Smooth(control, 0.1f);
            Assert.AreEqual(4f, TerrainProfile.HeightAt(dense, 1.95f), 1e-4f);
            Assert.AreEqual(-1f, TerrainProfile.HeightAt(dense, 2.05f), 1e-4f);
        }

        [Test]
        public void MountainMatchesLevelSpec()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            Vector2[] dense = TerrainProfile.Smooth(config.terrainProfile, 0.14f);
            float top = float.NegativeInfinity;
            for (int i = 0; i < dense.Length; i++)
            {
                top = Mathf.Max(top, dense[i].y);
            }
            Assert.GreaterOrEqual(top, 11f);
            Assert.LessOrEqual(top, 12f);
            Assert.Greater(TerrainProfile.HeightAt(dense, -0.7f), config.waterLevel, "wall of the crevasse is above water");
            Assert.Less(TerrainProfile.HeightAt(dense, 0f), config.waterLevel);
            Object.DestroyImmediate(config);
        }
    }

    public sealed class FlatMeshBuilderTests
    {
        [Test]
        public void RoundedShapesGrowBuffersAndStayInBounds()
        {
            FlatMeshBuilder builder = new FlatMeshBuilder();
            Mesh mesh = new Mesh();
            Color32 color = new Color32(255, 255, 255, 255);
            for (int i = 0; i < 400; i++)
            {
                builder.AddRoundedRect(0f, 0f, 2f, 1f, 0.3f, color);
                builder.AddCircle(1f, 0.5f, 0.5f, color);
                builder.AddArch(0.5f, 0f, 1.5f, 1f, color);
            }
            builder.Apply(mesh);
            Assert.AreEqual(builder.VertexCount, mesh.vertexCount);
            Assert.LessOrEqual(mesh.bounds.max.x, 2f + 1e-4f);
            Assert.LessOrEqual(mesh.bounds.max.y, 1f + 1e-4f);
            Assert.GreaterOrEqual(mesh.bounds.min.x, -1e-4f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void RadiusIsClampedToHalfOfSmallerSide()
        {
            FlatMeshBuilder builder = new FlatMeshBuilder();
            Mesh mesh = new Mesh();
            builder.AddRoundedRect(0f, 0f, 2f, 1f, 5f, new Color32(255, 255, 255, 255));
            builder.Apply(mesh);
            Assert.AreEqual(1f, mesh.bounds.max.y, 1e-4f);
            Assert.AreEqual(0f, mesh.bounds.min.x, 1e-4f);
            Object.DestroyImmediate(mesh);
        }
    }

    public sealed class CameraFramingTests
    {
        [Test]
        public void PortraitIsLimitedByWidth()
        {
            float size = CameraFraming.OrthoSize(16f, 10f, 720f / 1280f);
            Assert.AreEqual(8f / 0.5625f, size, 1e-3f);
        }

        [Test]
        public void LandscapeIsLimitedByMinimumHeight()
        {
            float size = CameraFraming.OrthoSize(16f, 16f, 1280f / 720f);
            Assert.AreEqual(8f, size, 1e-4f);
        }

        [Test]
        public void WidthIsAlwaysVisible()
        {
            float[] aspects = { 0.45f, 0.5625f, 0.75f, 1f, 1.33f, 1.78f, 2.2f };
            for (int i = 0; i < aspects.Length; i++)
            {
                float size = CameraFraming.OrthoSize(38f, 16f, aspects[i]);
                Assert.GreaterOrEqual(size * 2f * aspects[i] + 1e-3f, 38f, "aspect " + aspects[i]);
                Assert.GreaterOrEqual(size * 2f + 1e-3f, 16f, "aspect " + aspects[i]);
            }
        }

        [Test]
        public void ZeroAspectDoesNotDivideByZero()
        {
            float size = CameraFraming.OrthoSize(16f, 16f, 0f);
            Assert.IsFalse(float.IsInfinity(size) || float.IsNaN(size));
        }
    }

    public sealed class GreyboxLevelTests
    {
        private GameConfig config;
        private GreyboxLevel level;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            level = new GreyboxLevel();
            level.Init(config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(level.Mesh);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void MountCountsMatchConfig()
        {
            Assert.AreEqual(config.playerCannons, level.PlayerMountX.Length);
            Assert.AreEqual(config.playerCannons, level.PlayerMountY.Length);
            Assert.AreEqual(config.enemyCannons, level.EnemyMountX.Length);
            Assert.AreEqual(config.enemyCannons, level.EnemyMountY.Length);
        }

        [Test]
        public void MountsAreAboveGroundAndOnTheirSideOfTheMountain()
        {
            for (int i = 0; i < level.PlayerMountX.Length; i++)
            {
                Assert.Less(level.PlayerMountX[i], 0f);
                Assert.Greater(level.PlayerMountY[i], level.GroundHeight(level.PlayerMountX[i]));
            }
            for (int i = 0; i < level.EnemyMountX.Length; i++)
            {
                Assert.Greater(level.EnemyMountX[i], 0f);
                Assert.Greater(level.EnemyMountY[i], level.GroundHeight(level.EnemyMountX[i]));
            }
        }

        [Test]
        public void MountsAreDistinct()
        {
            for (int i = 0; i < level.EnemyMountX.Length; i++)
            {
                for (int j = i + 1; j < level.EnemyMountX.Length; j++)
                {
                    bool same = Mathf.Approximately(level.EnemyMountX[i], level.EnemyMountX[j])
                        && Mathf.Approximately(level.EnemyMountY[i], level.EnemyMountY[j]);
                    Assert.IsFalse(same, "mounts " + i + " and " + j + " coincide");
                }
            }
        }

        [Test]
        public void BuildProducesGeometryForBothThemes()
        {
            level.Build(ThemePalette.CreateNight());
            int nightVertices = level.Mesh.vertexCount;
            Assert.Greater(nightVertices, 0);
            level.Build(ThemePalette.CreateSunset());
            Assert.AreEqual(nightVertices, level.Mesh.vertexCount);
        }

        [Test]
        public void MeshFitsSixteenBitIndices()
        {
            level.Build(ThemePalette.CreateNight());
            Assert.Less(level.Mesh.vertexCount, 65535);
        }

        [Test]
        public void RebuildIsDeterministic()
        {
            level.Build(ThemePalette.CreateNight());
            Vector3[] first = level.Mesh.vertices;
            level.Build(ThemePalette.CreateNight());
            CollectionAssert.AreEqual(first, level.Mesh.vertices);
        }

        [Test]
        public void MountTablesCoverMaximumCannonCount()
        {
            Assert.GreaterOrEqual(GreyboxCastles.BlueMounts.Length, 14 * 2);
            Assert.GreaterOrEqual(GreyboxCastles.RedMounts.Length, 14 * 2);
        }

        [Test]
        public void MountsStayOnTheCastleSilhouette()
        {
            for (int i = 0; i < GreyboxCastles.RedMounts.Length; i += 2)
            {
                Assert.LessOrEqual(Mathf.Abs(GreyboxCastles.RedMounts[i]), GreyboxCastles.RedHalfWidth, "red " + i);
                Assert.LessOrEqual(GreyboxCastles.RedMounts[i + 1], GreyboxCastles.RedTop, "red " + i);
            }
            for (int i = 0; i < GreyboxCastles.BlueMounts.Length; i += 2)
            {
                Assert.LessOrEqual(Mathf.Abs(GreyboxCastles.BlueMounts[i]), GreyboxCastles.BlueHalfWidth, "blue " + i);
                Assert.LessOrEqual(GreyboxCastles.BlueMounts[i + 1], GreyboxCastles.BlueTowerTop, "blue " + i);
            }
        }

        [Test]
        public void FocusPointsSitOnCastles()
        {
            Assert.AreEqual(config.playerCastleX, level.PlayerFocusX);
            Assert.AreEqual(config.enemyCastleX, level.EnemyFocusX);
            Assert.Greater(level.PlayerFocusY, level.GroundHeight(config.playerCastleX));
            Assert.Greater(level.EnemyFocusY, level.GroundHeight(config.enemyCastleX));
        }
    }

    public sealed class CameraDirectorTests
    {
        private GameConfig config;
        private MatchState match;
        private SessionFlow flow;
        private GreyboxLevel level;
        private CameraDirector director;
        private GameObject cameraObject;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            config.bootDuration = 1f;
            config.introDuration = 1f;
            config.playerAimDuration = 1f;
            config.aiAimDuration = 1f;
            config.fireDuration = 1f;
            config.flightDuration = 4f;
            config.resolveDuration = 1f;
            config.victoryDuration = 4f;
            config.victoryLoserHold = 1f;
            match = new MatchState();
            match.Init(config.playerCannons, config.enemyCannons);
            flow = new SessionFlow();
            flow.Init(config, match);
            level = new GreyboxLevel();
            level.Init(config);
            cameraObject = new GameObject("TestCamera", typeof(Camera));
            director = new CameraDirector();
            director.Init(cameraObject.GetComponent<Camera>(), config, level, flow);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(level.Mesh);
            Object.DestroyImmediate(config);
        }

        private CameraFrame FrameNow()
        {
            CameraFrame frame = new CameraFrame();
            director.SelectFrame(frame);
            return frame;
        }

        private void AdvanceTo(SessionState state, Side side)
        {
            for (int i = 0; i < 100 && !(flow.State == state && flow.ActiveSide == side); i++)
            {
                flow.Skip();
            }
            Assert.AreEqual(state, flow.State);
        }

        [Test]
        public void IntroLooksAtPlayerCastleWithLeadTowardEnemy()
        {
            AdvanceTo(SessionState.Intro, Side.Player);
            CameraFrame frame = FrameNow();
            Assert.AreEqual(level.PlayerFocusX + config.castleLeadX, frame.x, 1e-4f);
            Assert.AreEqual(config.castleFrameWidth, frame.width, 1e-4f);
        }

        [Test]
        public void PlayerFlightStartsAtCenterThenMovesToEnemy()
        {
            AdvanceTo(SessionState.Flight, Side.Player);
            CameraFrame early = FrameNow();
            Assert.AreEqual(config.centerFocus.x, early.x, 1e-4f);
            Assert.AreEqual(config.centerFrameWidth, early.width, 1e-4f);

            flow.Tick(flow.Duration(SessionState.Flight, Side.Player) * (config.flightCenterFraction + 0.05f));
            CameraFrame late = FrameNow();
            Assert.AreEqual(level.EnemyFocusX, late.x, 1e-4f);
            Assert.AreEqual(config.targetFrameWidth, late.width, 1e-4f);
        }

        [Test]
        public void EnemyFlightEndsOnPlayerCastle()
        {
            AdvanceTo(SessionState.Flight, Side.Enemy);
            flow.Tick(flow.Duration(SessionState.Flight, Side.Enemy) * (config.flightCenterFraction + 0.05f));
            Assert.AreEqual(level.PlayerFocusX, FrameNow().x, 1e-4f);
        }

        [Test]
        public void EnemyAimLooksAtEnemyCastle()
        {
            AdvanceTo(SessionState.Aim, Side.Enemy);
            Assert.AreEqual(level.EnemyFocusX - config.castleLeadX, FrameNow().x, 1e-4f);
        }

        [Test]
        public void CenterFrameIsWidestAndTargetFrameIsNarrowerThanCenter()
        {
            AdvanceTo(SessionState.Flight, Side.Player);
            float center = FrameNow().width;
            AdvanceTo(SessionState.Resolve, Side.Player);
            float target = FrameNow().width;
            AdvanceTo(SessionState.Aim, Side.Enemy);
            float own = FrameNow().width;
            Assert.Greater(center, target);
            Assert.Greater(target, own);
        }

        [Test]
        public void VictoryShowsLoserFirstThenWinner()
        {
            config.stubEnemyLossPerPlayerTurn = 12;
            AdvanceTo(SessionState.Victory, Side.Player);
            Assert.AreEqual(level.EnemyFocusX, FrameNow().x, 1e-4f);
            flow.Tick(config.victoryLoserHold + 0.1f);
            Assert.AreEqual(level.PlayerFocusX + config.castleLeadX, FrameNow().x, 1e-4f);
        }

        [Test]
        public void SnapPlacesCameraOnTargetImmediately()
        {
            AdvanceTo(SessionState.Resolve, Side.Player);
            director.Snap();
            Assert.AreEqual(level.EnemyFocusX, cameraObject.transform.position.x, 1e-4f);
        }

        [Test]
        public void TickMovesTowardTargetWithoutOvershootingInOneStep()
        {
            director.Snap();
            float startX = cameraObject.transform.position.x;
            AdvanceTo(SessionState.Resolve, Side.Player);
            director.Tick(0.016f);
            float x = cameraObject.transform.position.x;
            Assert.Greater(x, startX);
            Assert.Less(x, level.EnemyFocusX);
        }
    }
}
