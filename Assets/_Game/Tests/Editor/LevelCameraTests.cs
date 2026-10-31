using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
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
            config = MaskFixture.LevelConfig();
            level = new GreyboxLevel();
            level.Init(config, DestructibleMask.CreateTerrain(config));
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < level.BackLayers.Length; i++)
            {
                Object.DestroyImmediate(level.BackLayers[i]);
            }
            Object.DestroyImmediate(level.FrontMesh);
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
            int nightBack = level.BackLayers[2].vertexCount;
            int nightFront = level.FrontMesh.vertexCount;
            Assert.Greater(nightBack, 0);
            Assert.Greater(nightFront, 0);
            level.Build(ThemePalette.CreateSunset());
            Assert.AreEqual(nightBack, level.BackLayers[2].vertexCount);
            Assert.AreEqual(nightFront, level.FrontMesh.vertexCount);
        }

        [Test]
        public void MeshFitsSixteenBitIndices()
        {
            level.Build(ThemePalette.CreateNight());
            for (int i = 0; i < level.BackLayers.Length; i++)
            {
                Assert.Greater(level.BackLayers[i].vertexCount, 0, "layer " + i);
                Assert.Less(level.BackLayers[i].vertexCount, 65535, "layer " + i);
            }
            Assert.Less(level.FrontMesh.vertexCount, 65535);
        }

        [Test]
        public void RebuildIsDeterministic()
        {
            level.Build(ThemePalette.CreateNight());
            Vector3[][] back = new Vector3[level.BackLayers.Length][];
            for (int i = 0; i < back.Length; i++)
            {
                back[i] = level.BackLayers[i].vertices;
            }
            Vector3[] front = level.FrontMesh.vertices;
            level.Build(ThemePalette.CreateNight());
            for (int i = 0; i < back.Length; i++)
            {
                CollectionAssert.AreEqual(back[i], level.BackLayers[i].vertices, "layer " + i);
            }
            CollectionAssert.AreEqual(front, level.FrontMesh.vertices);
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

    public sealed class ParallaxTests
    {
        private const float Reference = 9f;

        /// <summary>Где на экране окажется точка слоя: смещение от центра кадра.</summary>
        private static float ScreenOffset(float camera, float point, float factor, float zoom)
        {
            float scale = ParallaxBackground.LayerScale(zoom, factor);
            float world = ParallaxBackground.LayerOffset(camera, Reference, factor, scale) + point * scale;
            return world - camera;
        }

        [Test]
        public void FactorOneMovesWithLevel()
        {
            Assert.AreEqual(1f, ParallaxBackground.LayerScale(0.7f, 1f), 1e-5f);
            Assert.AreEqual(0f, ParallaxBackground.LayerOffset(-20f, Reference, 1f, 1f), 1e-5f);
        }

        [Test]
        public void FactorZeroStaysFixedOnScreen()
        {
            Assert.AreEqual(ScreenOffset(Reference, 5f, 0f, 1f), ScreenOffset(-20f, 5f, 0f, 1f), 1e-4f);
        }

        [Test]
        public void AtReferenceFrameLayersAreInPlace()
        {
            float[] factors = { 0f, 0.05f, 0.15f, 0.3f, 0.5f };
            for (int i = 0; i < factors.Length; i++)
            {
                Assert.AreEqual(1f, ParallaxBackground.LayerScale(1f, factors[i]), 1e-5f);
                Assert.AreEqual(0f, ParallaxBackground.LayerOffset(Reference, Reference, factors[i], 1f), 1e-5f);
            }
        }

        [Test]
        public void NearerLayerShiftsMoreOnScreen()
        {
            float far = Mathf.Abs(ScreenOffset(-20f, Reference, 0.15f, 1f));
            float near = Mathf.Abs(ScreenOffset(-20f, Reference, 0.5f, 1f));
            Assert.Greater(near, far);
        }

        [Test]
        public void FartherLayerReactsLessToZoom()
        {
            float far = ParallaxBackground.LayerScale(0.7f, 0.15f);
            float near = ParallaxBackground.LayerScale(0.7f, 0.5f);
            Assert.Less(far, near);
        }

        [Test]
        public void DefaultFactorsCoverEveryLayerAndGrowTowardViewer()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            Assert.AreEqual(GreyboxLevel.BackLayerCount, config.parallaxFactors.Length);
            for (int i = 1; i < config.parallaxFactors.Length; i++)
            {
                Assert.Greater(config.parallaxFactors[i], config.parallaxFactors[i - 1]);
            }
            Object.DestroyImmediate(config);
        }
    }

    public sealed class CameraDirectorTests
    {
        private GameConfig config;
        private MatchState match;
        private SessionFlow flow;
        private GreyboxLevel level;
        private CameraDirector director;
        private ScreenShake shake;
        private GameObject cameraObject;

        [SetUp]
        public void SetUp()
        {
            config = MaskFixture.LevelConfig();
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
            level.Init(config, DestructibleMask.CreateTerrain(config));
            cameraObject = new GameObject("TestCamera", typeof(Camera));
            director = new CameraDirector();
            shake = new ScreenShake();
            shake.Init(config);
            director.Init(cameraObject.GetComponent<Camera>(), config, level, flow, shake);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            for (int i = 0; i < level.BackLayers.Length; i++)
            {
                Object.DestroyImmediate(level.BackLayers[i]);
            }
            Object.DestroyImmediate(level.FrontMesh);
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
            director.FlightHeadX = config.centerFocus.x;
            CameraFrame early = FrameNow();
            Assert.AreEqual(config.centerFocus.x, early.x, 1e-4f);
            Assert.AreEqual(config.centerFrameWidth, early.width, 1e-4f);

            director.FlightHeadX = config.centerFocus.x - 10f;
            Assert.AreEqual(config.centerFocus.x - 10f * config.cameraHeadFollow, FrameNow().x, 1e-4f);

            director.HeadPassedCenter = true;
            CameraFrame late = FrameNow();
            Assert.AreEqual(level.EnemyFocusX, late.x, 1e-4f);
            Assert.AreEqual(config.targetFrameWidth, late.width, 1e-4f);
        }

        [Test]
        public void EnemyFlightEndsOnPlayerCastle()
        {
            AdvanceTo(SessionState.Flight, Side.Enemy);
            director.HeadPassedCenter = true;
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
            AdvanceTo(SessionState.Resolve, Side.Player);
            match.Destroy(Side.Enemy, config.enemyCannons, 0);
            AdvanceTo(SessionState.Victory, Side.Player);
            Assert.AreEqual(level.EnemyFocusX, FrameNow().x, 1e-4f);
            flow.Tick(config.victoryLoserHold + 0.1f);
            Assert.AreEqual(level.PlayerFocusX + config.castleLeadX, FrameNow().x, 1e-4f);
        }

        [Test]
        public void ShakeOffsetsCameraWithoutMovingTheTarget()
        {
            director.Snap();
            float x = cameraObject.transform.position.x;
            shake.Add(config.shakeMax);
            shake.Tick(0.02f);
            director.Snap();
            Assert.AreEqual(x + shake.OffsetX, cameraObject.transform.position.x, 1e-4f);
            Assert.AreNotEqual(0f, shake.OffsetX);
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
