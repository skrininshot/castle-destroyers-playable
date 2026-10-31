using System;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CastleDestroyers.Tests
{
    /// <summary>Общие заготовки: маленькая синтетическая маска и конфиг с настоящим запечённым уровнем.</summary>
    internal static class MaskFixture
    {
        public const string LevelDataPath = "Assets/_Game/Configs/Level1_Terrain.asset";
        public const int TileSize = 16;
        public const int CellSize = 4;
        public const int Width = 46;
        public const int Height = 31;
        public const float TexelsPerMeter = 16f;

        public static DestructibleMask Filled(int groundRows)
        {
            byte[] solid = new byte[Width * Height];
            for (int y = 0; y < groundRows; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    solid[y * Width + x] = DestructibleMask.Full;
                }
            }
            DestructibleMask mask = new DestructibleMask();
            mask.Init(Width, Height, TileSize, CellSize, TexelsPerMeter, 0f, 0f, solid, new byte[solid.Length], new byte[solid.Length]);
            DrainDirty(mask);
            return mask;
        }

        public static void SetRect(DestructibleMask mask, int minX, int minY, int maxX, int maxY, byte value)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int index = y * mask.Width + x;
                    bool had = mask.Solid[index] != 0;
                    if (value == 0 && had)
                    {
                        mask.ClearTexel(index, x, y);
                    }
                    else if (value != 0 && !had)
                    {
                        mask.Solid[index] = value;
                        mask.CellCounts[y / mask.CellSize * mask.CellsX + x / mask.CellSize]++;
                    }
                }
            }
        }

        public static int DrainDirty(DestructibleMask mask)
        {
            int count = 0;
            while (mask.PopDirtyTile() >= 0)
            {
                count++;
            }
            return count;
        }

        public static GameConfig LevelConfig()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            config.terrainMask = AssetDatabase.LoadAssetAtPath<TerrainMaskData>(LevelDataPath);
            Assert.IsNotNull(config.terrainMask, "baked level data");
            return config;
        }
    }

    public sealed class MaskRleTests
    {
        [Test]
        public void RoundTripKeepsEveryByte()
        {
            byte[] data = { 0, 0, 0, 255, 255, 7, 0, 0, 128 };
            byte[] decoded = new byte[data.Length];
            MaskRle.Decode(MaskRle.Encode(data), decoded);
            CollectionAssert.AreEqual(data, decoded);
        }

        [Test]
        public void LongRunsTakeOneEntry()
        {
            Assert.AreEqual(2, MaskRle.Encode(new byte[100000]).Length + 1);
        }

        [Test]
        public void EmptyDataHasNoRuns()
        {
            Assert.AreEqual(0, MaskRle.Encode(new byte[0]).Length);
        }
    }

    public sealed class DestructibleMaskTests
    {
        [Test]
        public void NeighbourTilesShareOneTexelColumn()
        {
            DestructibleMask mask = MaskFixture.Filled(20);
            Assert.AreEqual(15, mask.TileStride);
            Assert.AreEqual(3, mask.TilesX);
            Assert.AreEqual(2, mask.TilesY);
        }

        [Test]
        public void ChangeOnSharedColumnDirtiesBothTiles()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            mask.MarkDirtyRect(15, 2, 15, 2);
            Assert.AreEqual(2, mask.DirtyCount);
            Assert.AreEqual(0, mask.PopDirtyTile());
            Assert.AreEqual(1, mask.PopDirtyTile());
        }

        [Test]
        public void ChangeInsideTileDirtiesOnlyIt()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            mask.MarkDirtyRect(20, 3, 22, 4);
            Assert.AreEqual(1, mask.DirtyCount);
            Assert.AreEqual(1, mask.PopDirtyTile());
        }

        [Test]
        public void TileIsQueuedOnce()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            mask.MarkDirtyRect(20, 3, 20, 3);
            mask.MarkDirtyRect(21, 3, 21, 3);
            Assert.AreEqual(1, mask.DirtyCount);
        }

        [Test]
        public void ChangeOnLastColumnStaysInBounds()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            mask.MarkDirtyRect(MaskFixture.Width - 1, MaskFixture.Height - 1, MaskFixture.Width - 1, MaskFixture.Height - 1);
            Assert.AreEqual(1, mask.DirtyCount);
            Assert.AreEqual(5, mask.PopDirtyTile());
        }

        [Test]
        public void ResetRestoresMaterialAndCounters()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            int solid = mask.SolidCount;
            int cellCount = mask.CellCounts[0];
            MaskFixture.SetRect(mask, 0, 0, 10, 9, 0);
            mask.Reset();
            Assert.AreEqual(solid, mask.SolidCount);
            Assert.AreEqual(cellCount, mask.CellCounts[0]);
            Assert.AreEqual(DestructibleMask.Full, mask.Solid[0]);
            Assert.AreEqual(6, mask.DirtyCount);
        }

        [Test]
        public void WorldQueriesUseOriginAndScale()
        {
            byte[] solid = new byte[MaskFixture.Width * MaskFixture.Height];
            solid[3 * MaskFixture.Width + 16] = DestructibleMask.Full;
            DestructibleMask mask = new DestructibleMask();
            mask.Init(MaskFixture.Width, MaskFixture.Height, MaskFixture.TileSize, MaskFixture.CellSize, 16f, -1f, 2f, solid, new byte[solid.Length], new byte[solid.Length]);
            Assert.IsTrue(mask.IsSolidWorld(0f, 2f + 3f / 16f));
            Assert.IsFalse(mask.IsSolidWorld(0.5f, 2f + 3f / 16f));
            Assert.AreEqual(2f + 3f / 16f, mask.SurfaceHeight(0f), 1e-4f);
        }

        [Test]
        public void OutsideMaskThereIsNoMaterial()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            Assert.IsFalse(mask.IsSolidWorld(-1f, 0.1f));
            Assert.IsFalse(mask.IsSolidWorld(100f, 0.1f));
            Assert.AreEqual(0f, mask.SurfaceHeight(-1f));
        }

        [Test]
        public void PartialCoverageIsNotSolid()
        {
            DestructibleMask mask = MaskFixture.Filled(2);
            mask.Solid[5] = DestructibleMask.SolidThreshold - 1;
            Assert.IsFalse(mask.IsSolidWorld(5f / 16f, 0f));
        }
    }

    public sealed class CraterStamperTests
    {
        private const float Radius = 5f;
        private const float Rim = 3f;

        private static bool Stamp(DestructibleMask mask, float x, float y)
        {
            return CraterStamper.StampCircle(mask, x, y, Radius, Rim, 0f, 0);
        }

        [Test]
        public void RemovesCircleAndKeepsTheRest()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Assert.IsTrue(Stamp(mask, 20f, 15f));
            Assert.AreEqual(0, mask.Solid[15 * mask.Width + 20]);
            Assert.AreEqual(0, mask.Solid[15 * mask.Width + 23]);
            Assert.AreEqual(DestructibleMask.Full, mask.Solid[15 * mask.Width + 40]);
        }

        [Test]
        public void RimIsFlatRingAroundCrater()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Stamp(mask, 20f, 15f);
            Assert.AreEqual(DestructibleMask.Full, mask.Shade[15 * mask.Width + 27], "inside the rim");
            Assert.AreEqual(0, mask.Shade[15 * mask.Width + 30], "past the rim");
        }

        [Test]
        public void CraterEdgeHasPartialCoverage()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            CraterStamper.StampCircle(mask, 20f, 15f, 5.25f, Rim, 0f, 0);
            byte edge = mask.Solid[15 * mask.Width + 25];
            Assert.Greater(edge, (byte)0);
            Assert.Less(edge, DestructibleMask.Full);
        }

        [Test]
        public void SolidCountMatchesRemovedTexels()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Stamp(mask, 20f, 15f);
            int solid = 0;
            for (int i = 0; i < mask.Solid.Length; i++)
            {
                solid += mask.Solid[i] != 0 ? 1 : 0;
            }
            Assert.AreEqual(solid, mask.SolidCount);
            int cells = 0;
            for (int i = 0; i < mask.CellCounts.Length; i++)
            {
                cells += mask.CellCounts[i];
            }
            Assert.AreEqual(solid, cells);
        }

        [Test]
        public void StampInAirChangesNothing()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            Assert.IsFalse(Stamp(mask, 20f, 25f));
            Assert.AreEqual(0, mask.DirtyCount);
            Assert.IsFalse(mask.ConnectivityDirty);
        }

        [Test]
        public void StampOutsideMaskIsIgnored()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Assert.IsFalse(Stamp(mask, -100f, 500f));
            Assert.DoesNotThrow(() => Stamp(mask, 0f, 0f));
            Assert.DoesNotThrow(() => Stamp(mask, MaskFixture.Width - 1, MaskFixture.Height - 1));
        }

        [Test]
        public void StampMarksConnectivityAndDirtyTiles()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Stamp(mask, 20f, 15f);
            Assert.IsTrue(mask.ConnectivityDirty);
            Assert.Greater(mask.DirtyCount, 0);
        }

        [Test]
        public void ScallopMakesRimUneven()
        {
            DestructibleMask plain = MaskFixture.Filled(MaskFixture.Height);
            DestructibleMask scalloped = MaskFixture.Filled(MaskFixture.Height);
            Stamp(plain, 20f, 15f);
            CraterStamper.StampCircle(scalloped, 20f, 15f, Radius, Rim, 2f, 8);
            bool differs = false;
            for (int i = 0; i < plain.Shade.Length && !differs; i++)
            {
                differs = plain.Shade[i] != scalloped.Shade[i];
            }
            Assert.IsTrue(differs);
        }

        [Test]
        public void StampDoesNotAllocate()
        {
            DestructibleMask mask = MaskFixture.Filled(MaskFixture.Height);
            Stamp(mask, 20f, 15f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            Stamp(mask, 30f, 10f);
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class ConnectivitySolverTests
    {
        private const int VisibleLoad = 13;

        private static ConnectivitySolver Solver(DestructibleMask mask, int maxIslands = 8)
        {
            ConnectivitySolver solver = new ConnectivitySolver();
            solver.Init(mask, maxIslands);
            return solver;
        }

        private static ConnectivitySolver VisibleSolver(DestructibleMask mask)
        {
            ConnectivitySolver solver = new ConnectivitySolver();
            solver.Init(mask, 8, VisibleLoad);
            return solver;
        }

        /// <summary>Столб на земле с верхней частью-блоком: ширина столба 8 текселей, высота 4..23.</summary>
        private static DestructibleMask Pillar()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 4, 19, 23, DestructibleMask.Full);
            return mask;
        }

        [Test]
        public void InvisibleResidueDoesNotHoldThePiece()
        {
            DestructibleMask mask = Pillar();
            // Щель: остатки покрытия ниже порога не видны и держать кусок не могут.
            for (int x = 12; x <= 19; x++)
            {
                mask.Solid[14 * mask.Width + x] = 40;
            }
            ConnectivitySolver old = Solver(mask);
            old.Solve();
            Assert.AreEqual(0, old.IslandCount, "the old rule kept it");
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
        }

        [Test]
        public void GapNarrowerThanACellSplitsThePiece()
        {
            DestructibleMask mask = Pillar();
            MaskFixture.SetRect(mask, 12, 14, 19, 14, 0);
            ConnectivitySolver old = Solver(mask);
            old.Solve();
            Assert.AreEqual(0, old.IslandCount, "the old rule kept it");
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
        }

        [Test]
        public void ThinBridgeDoesNotHoldThePiece()
        {
            DestructibleMask mask = Pillar();
            MaskFixture.SetRect(mask, 12, 12, 15, 15, 0);
            MaskFixture.SetRect(mask, 17, 12, 19, 15, 0);
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
        }

        [Test]
        public void ThickBridgeHoldsThePiece()
        {
            DestructibleMask mask = Pillar();
            MaskFixture.SetRect(mask, 16, 12, 19, 15, 0);
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            Assert.AreEqual(0, solver.IslandCount);
        }

        [Test]
        public void FringeNextToSupportStaysAndFloatingCrumbGoes()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 4, 12, 7, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 30, 20, 30, 20, DestructibleMask.Full);
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
            Assert.AreEqual(1, solver.IslandSize[0]);
        }

        [Test]
        public void FallingPieceTakesItsFringe()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 16, 19, 23, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 20, 18, 20, 21, DestructibleMask.Full);
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
            Assert.AreEqual(6, solver.IslandSize[0]);
        }

        [Test]
        public void VisibleSolveDoesNotAllocate()
        {
            DestructibleMask mask = Pillar();
            ConnectivitySolver solver = VisibleSolver(mask);
            solver.Solve();
            long before = GC.GetAllocatedBytesForCurrentThread();
            solver.Solve();
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }

        [Test]
        public void IntactGroundHasNoIslands()
        {
            DestructibleMask mask = MaskFixture.Filled(10);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(0, solver.IslandCount);
        }

        [Test]
        public void FloatingBlockIsAnIsland()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 16, 19, 23, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
            Assert.AreEqual(4, solver.IslandSize[0]);
        }

        [Test]
        public void PillarOnGroundStays()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 4, 15, 28, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(0, solver.IslandCount);
        }

        [Test]
        public void CuttingPillarDetachesTop()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 4, 15, 28, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 12, 12, 15, 15, 0);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
        }

        [Test]
        public void SeparateBlocksAreSeparateIslands()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 4, 16, 7, 19, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 24, 16, 27, 19, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(2, solver.IslandCount);
        }

        [Test]
        public void DiagonalTouchDoesNotConnect()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 4, 15, 11, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 16, 12, 19, 15, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
        }

        [Test]
        public void IslandListHasNoOverlaps()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 4, 16, 11, 23, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 24, 16, 31, 23, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            Assert.AreEqual(solver.IslandStart[0] + solver.IslandSize[0], solver.IslandStart[1]);
            CollectionAssert.AllItemsAreUnique(new ArraySegment<int>(solver.Cells, 0, solver.IslandSize[0] + solver.IslandSize[1]));
        }

        [Test]
        public void SolveClearsDirtyFlag()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            mask.ConnectivityDirty = true;
            Solver(mask).Solve();
            Assert.IsFalse(mask.ConnectivityDirty);
        }

        [Test]
        public void TooManyIslandsKeepDirtyFlag()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 4, 16, 7, 19, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 24, 16, 27, 19, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask, 1);
            solver.Solve();
            Assert.AreEqual(1, solver.IslandCount);
            Assert.IsTrue(mask.ConnectivityDirty);
        }

        [Test]
        public void SolveDoesNotAllocate()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 4, 16, 7, 19, DestructibleMask.Full);
            ConnectivitySolver solver = Solver(mask);
            solver.Solve();
            long before = GC.GetAllocatedBytesForCurrentThread();
            solver.Solve();
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class ChunkExtractorTests
    {
        private const int PixelSize = 32;

        private static ChunkExtractor Extract(DestructibleMask mask, ConnectivitySolver solver, int pixelSize = PixelSize)
        {
            solver.Init(mask, 4);
            solver.Solve();
            ChunkExtractor extractor = new ChunkExtractor();
            extractor.Init(mask, solver, pixelSize);
            return extractor;
        }

        [Test]
        public void CutRemovesIslandFromMask()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 16, 19, 23, DestructibleMask.Full);
            int before = mask.SolidCount;
            ChunkExtractor extractor = Extract(mask, new ConnectivitySolver());
            extractor.Cut(0, true);
            Assert.AreEqual(64, extractor.CutTexels);
            Assert.AreEqual(before - 64, mask.SolidCount);
            Assert.AreEqual(0, mask.Solid[16 * mask.Width + 12]);
            Assert.Greater(mask.DirtyCount, 0);
        }

        [Test]
        public void CutDoesNotTouchSupportedGround()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 16, 19, 23, DestructibleMask.Full);
            ChunkExtractor extractor = Extract(mask, new ConnectivitySolver());
            extractor.Cut(0, false);
            int remaining = 0;
            for (int i = 0; i < mask.Solid.Length; i++)
            {
                remaining += mask.Solid[i] != 0 ? 1 : 0;
            }
            Assert.AreEqual(4 * MaskFixture.Width, remaining);
        }

        [Test]
        public void PixelsCarryChannelsAndKeepEmptyBorder()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 16, 19, 23, DestructibleMask.Full);
            mask.Grass[16 * mask.Width + 12] = 200;
            mask.Buried[16 * mask.Width + 12] = 100;
            ChunkExtractor extractor = Extract(mask, new ConnectivitySolver());
            extractor.Cut(0, true);
            Color32 first = extractor.Pixels[PixelSize + 1];
            Assert.AreEqual(DestructibleMask.Full, first.r);
            Assert.AreEqual(200, first.b);
            Assert.AreEqual(100, first.a);
            Assert.AreEqual(0, extractor.Pixels[0].r);
            Assert.AreEqual(0, extractor.Pixels[1].r);
        }

        [Test]
        public void QuadMapsPixelsBackToWorldPositions()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 12, 16, 19, 23, DestructibleMask.Full);
            ChunkExtractor extractor = Extract(mask, new ConnectivitySolver());
            extractor.Cut(0, true);
            // Центр пикселя 1 показывает тексель 12: мировая X на этой UV должна совпасть с текселем.
            float u = 1.5f / PixelSize;
            float world = extractor.WorldMinX + (extractor.WorldMaxX - extractor.WorldMinX) * u / extractor.UvMaxX;
            Assert.AreEqual(mask.WorldX(12f), world, 1e-4f);
        }

        [Test]
        public void LargeIslandIsScaledToFitBuffer()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 0, 12, 39, 19, DestructibleMask.Full);
            ChunkExtractor extractor = Extract(mask, new ConnectivitySolver(), 16);
            extractor.Cut(0, true);
            Assert.AreEqual(3, extractor.Scale);
            Assert.LessOrEqual(extractor.UvMaxX, 1f);
            Assert.Greater(extractor.CutTexels, 300);
        }

        [Test]
        public void SecondCutDoesNotKeepOldPixels()
        {
            DestructibleMask mask = MaskFixture.Filled(4);
            MaskFixture.SetRect(mask, 4, 16, 19, 23, DestructibleMask.Full);
            MaskFixture.SetRect(mask, 28, 16, 31, 19, DestructibleMask.Full);
            ChunkExtractor extractor = Extract(mask, new ConnectivitySolver());
            extractor.Cut(0, true);
            extractor.Cut(1, true);
            Assert.AreEqual(0, extractor.Pixels[PixelSize + 10].r);
        }
    }

    public sealed class FlatParticlesTests
    {
        private FlatParticles particles;

        [SetUp]
        public void SetUp()
        {
            particles = new FlatParticles();
            particles.Init(4);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(particles.Mesh);
        }

        private void Spawn(float life)
        {
            particles.Spawn(1f, 2f, 0f, 1f, 1f, life, 0f, new Color32(1, 2, 3, 255));
        }

        [Test]
        public void SpawnBeyondCapacityIsDropped()
        {
            for (int i = 0; i < 10; i++)
            {
                Spawn(1f);
            }
            Assert.AreEqual(4, particles.ActiveCount);
        }

        [Test]
        public void ParticleDiesAtEndOfLife()
        {
            Spawn(0.5f);
            particles.Tick(0.4f);
            Assert.AreEqual(1, particles.ActiveCount);
            particles.Tick(0.2f);
            Assert.AreEqual(0, particles.ActiveCount);
        }

        [Test]
        public void ParticleShrinksWithAge()
        {
            Spawn(1f);
            particles.Tick(0.1f);
            float early = particles.Mesh.bounds.size.x + particles.Mesh.vertices[1].x - particles.Mesh.vertices[0].x;
            particles.Tick(0.7f);
            float late = particles.Mesh.bounds.size.x + particles.Mesh.vertices[1].x - particles.Mesh.vertices[0].x;
            Assert.Less(late, early);
        }

        [Test]
        public void GravityPullsParticleDown()
        {
            particles.Spawn(0f, 0f, 0f, 0f, 1f, 2f, 10f, new Color32(1, 2, 3, 255));
            particles.Tick(0.5f);
            Assert.Less(particles.Mesh.vertices[0].y, -1f);
        }

        [Test]
        public void ClearRemovesEverything()
        {
            Spawn(1f);
            Spawn(1f);
            particles.Clear();
            Assert.AreEqual(0, particles.ActiveCount);
            Assert.AreEqual(Vector3.zero, particles.Mesh.vertices[0]);
        }

        [Test]
        public void TickDoesNotAllocateManagedMemory()
        {
            Spawn(5f);
            particles.Tick(0.01f);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 10; i++)
            {
                particles.Tick(0.01f);
            }
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class WaterStripTests
    {
        [Test]
        public void WaveStaysWithinAmplitudeAndMoves()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            WaterStrip water = new WaterStrip();
            water.Init(config);
            float before = water.WaveAt(3f);
            water.Tick(0.3f);
            Assert.AreNotEqual(before, water.WaveAt(3f));
            Assert.LessOrEqual(Mathf.Abs(water.WaveAt(3f)), config.waterWaveAmplitude + 1e-5f);

            Vector3[] vertices = water.Mesh.vertices;
            for (int i = 1; i < vertices.Length; i += 2)
            {
                Assert.LessOrEqual(Mathf.Abs(vertices[i].y - config.waterLevel), config.waterWaveAmplitude + 1e-4f);
                Assert.Greater(vertices[i].y, vertices[i - 1].y, "top is above the bottom of the band");
            }
            Object.DestroyImmediate(water.Mesh);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void StripCoversLevelWidth()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            WaterStrip water = new WaterStrip();
            water.Init(config);
            Assert.LessOrEqual(water.Mesh.bounds.min.x, -config.levelWidth);
            Assert.GreaterOrEqual(water.Mesh.bounds.max.x, config.levelWidth);
            Object.DestroyImmediate(water.Mesh);
            Object.DestroyImmediate(config);
        }
    }

    public sealed class Level1MaskTests
    {
        private GameConfig config;
        private DestructibleMask mask;

        [SetUp]
        public void SetUp()
        {
            config = MaskFixture.LevelConfig();
            mask = DestructibleMask.CreateTerrain(config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SizeFitsTileGrid()
        {
            int stride = config.maskTileSize - 1;
            Assert.AreEqual(0, (config.terrainMask.width - 1) % stride);
            Assert.AreEqual(0, (config.terrainMask.height - 1) % stride);
            Assert.AreEqual(13, mask.TilesX);
            Assert.AreEqual(5, mask.TilesY);
        }

        [Test]
        public void MountainHeightMatchesSpec()
        {
            float top = 0f;
            for (float x = -6f; x <= 6f; x += 0.05f)
            {
                top = Mathf.Max(top, mask.SurfaceHeight(x));
            }
            Assert.GreaterOrEqual(top, 31f);
            Assert.LessOrEqual(top, 33f);
        }

        [Test]
        public void CrevasseGoesBelowWaterAndIsWiderThanBooster()
        {
            Assert.Less(mask.SurfaceHeight(0f), config.waterLevel);
            Assert.Greater(mask.SurfaceHeight(-1.6f), 5f);
            Assert.Greater(mask.SurfaceHeight(1.6f), 5f);
            Assert.Less(mask.SurfaceHeight(-1.2f), config.waterLevel);
            Assert.Less(mask.SurfaceHeight(1.2f), config.waterLevel);
            Assert.Less(config.boosterWidth * 0.5f, 1.2f);
        }

        [Test]
        public void CastleHillsAreRaisedAndFlat()
        {
            Assert.AreEqual(4.8f, mask.SurfaceHeight(config.playerCastleX), 0.1f);
            Assert.AreEqual(4.8f, mask.SurfaceHeight(config.enemyCastleX), 0.1f);
            Assert.AreEqual(mask.SurfaceHeight(config.playerCastleX - 4.3f), mask.SurfaceHeight(config.playerCastleX + 4.3f), 0.1f);
        }

        [Test]
        public void MaskCoversBothCastles()
        {
            Assert.Less(mask.OriginX, config.playerCastleX - GreyboxCastles.BlueHalfWidth);
            Assert.Greater(mask.WorldX(mask.Width - 1), config.enemyCastleX + GreyboxCastles.RedHalfWidth);
        }

        [Test]
        public void TerrainIsOneIntactBodyAtStart()
        {
            ConnectivitySolver solver = new ConnectivitySolver();
            solver.Init(mask, 8);
            solver.Solve();
            Assert.AreEqual(0, solver.IslandCount);
        }

        [Test]
        public void GroundRimFollowsTheTopSurface()
        {
            int x = (int)mask.TexelX(config.playerCastleX);
            int top = (int)mask.TexelY(mask.SurfaceHeight(config.playerCastleX));
            Assert.Greater(mask.Grass[(top - 1) * mask.Width + x], 200);
            Assert.AreEqual(0, mask.Grass[(top - 30) * mask.Width + x]);
        }

        [Test]
        public void RimAndBuriedShapesAreInsideMaterial()
        {
            for (int i = 0; i < mask.Solid.Length; i++)
            {
                if (mask.Solid[i] == 0)
                {
                    Assert.AreEqual(0, mask.Grass[i]);
                    Assert.AreEqual(0, mask.Buried[i]);
                }
            }
        }
    }

    public sealed class TerrainSystemTests
    {
        private const float Step = 0.05f;

        private GameConfig config;
        private GameObject root;
        private Material material;
        private FlatParticles particles;
        private DestructibleMask mask;
        private TerrainSystem terrain;
        private ChunkSystem chunks;
        private SurfaceDecor decor;

        [SetUp]
        public void SetUp()
        {
            config = MaskFixture.LevelConfig();
            root = new GameObject("TerrainTest");
            material = new Material(Shader.Find("Game/Terrain"));
            Rng rng = new Rng(1u);
            particles = new FlatParticles();
            particles.Init(config.fxParticleCapacity);
            LevelFx fx = new LevelFx();
            fx.Init(config, particles, rng);
            fx.ApplyPalette(ThemePalette.CreateNight());

            mask = DestructibleMask.CreateTerrain(config);
            MaskTileView view = new MaskTileView();
            view.Init(mask, material, config.maskEdgeSoftness, root.transform, 0);
            chunks = new ChunkSystem();
            chunks.Init(config, fx, rng, material, root.transform, 1);
            terrain = new TerrainSystem();
            terrain.Init(config, mask, view, chunks, fx);
            decor = new SurfaceDecor();
            decor.Init(config, mask, fx, rng);
            decor.Build(ThemePalette.CreateNight());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(particles.Mesh);
            Object.DestroyImmediate(decor.Mesh);
            Object.DestroyImmediate(config);
        }

        private void Run(float seconds)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                terrain.Tick(Step);
                chunks.Tick(Step);
                decor.Tick(Step);
                particles.Tick(Step);
            }
        }

        /// <summary>Сквозной пропил левой половины горы слева направо на высоте y.</summary>
        private void SliceLeftPeak(float y)
        {
            for (float x = -16f; x <= 0f; x += 0.5f)
            {
                terrain.Explode(x, y, 1f);
            }
        }

        [Test]
        public void ExplosionMakesCraterAndDust()
        {
            int before = mask.SolidCount;
            Assert.IsTrue(terrain.Explode(-4f, 3f, 1.2f));
            Assert.Less(mask.SolidCount, before);
            Assert.IsFalse(mask.IsSolidWorld(-4f, 3f));
            Assert.AreEqual(config.craterDustPuffs, particles.ActiveCount);
        }

        [Test]
        public void ExplosionInSkyDoesNothing()
        {
            Assert.IsFalse(terrain.Explode(-20f, 11f, 1.2f));
            Assert.AreEqual(0, particles.ActiveCount);
        }

        [Test]
        public void SlicedPeakBecomesFallingChunk()
        {
            SliceLeftPeak(24f);
            Run(config.connectivityInterval * 2f);
            Assert.AreEqual(1, chunks.ActiveCount);
            Assert.IsFalse(mask.IsSolidWorld(-3f, 28f), "peak left the mask");
            Assert.IsTrue(mask.IsSolidWorld(3f, 28f), "other half is untouched");
        }

        [Test]
        public void ChunkFallsIntoWaterAndIsRecycled()
        {
            SliceLeftPeak(24f);
            Run(5f);
            Assert.AreEqual(0, chunks.ActiveCount);
        }

        [Test]
        public void ChunkMakesSplash()
        {
            SliceLeftPeak(24f);
            Run(2.2f);
            Assert.Greater(particles.ActiveCount, 0);
        }

        [Test]
        public void TinyIslandVanishesWithoutChunk()
        {
            int row = mask.Height - 3;
            int column = 30;
            MaskFixture.SetRect(mask, column, row, column + 3, row + 1, DestructibleMask.Full);
            mask.ConnectivityDirty = true;
            Run(config.connectivityInterval * 2f);
            Assert.AreEqual(0, chunks.ActiveCount);
            Assert.AreEqual(0, mask.Solid[row * mask.Width + column]);
            Assert.Greater(particles.ActiveCount, 0);
        }

        [Test]
        public void IslandsPastSlotLimitBecomeDust()
        {
            config.chunkSlots = 1;
            ChunkSystem one = new ChunkSystem();
            one.Init(config, new LevelFx(), new Rng(2u), material, root.transform, 1);
            Assert.IsTrue(one.HasFreeSlot);
            ChunkExtractor extractor = new ChunkExtractor();
            ConnectivitySolver solver = new ConnectivitySolver();
            DestructibleMask small = MaskFixture.Filled(4);
            MaskFixture.SetRect(small, 4, 16, 11, 23, DestructibleMask.Full);
            solver.Init(small, 4);
            solver.Solve();
            extractor.Init(small, solver, config.chunkTextureSize);
            extractor.Cut(0, true);
            Assert.IsTrue(one.Spawn(extractor));
            Assert.IsFalse(one.HasFreeSlot);
            Assert.IsFalse(one.Spawn(extractor));
        }

        [Test]
        public void UploadsStayWithinFrameBudget()
        {
            for (float x = -30f; x <= 30f; x += 3f)
            {
                terrain.Explode(x, mask.SurfaceHeight(x), 2f);
            }
            int queued = mask.DirtyCount;
            Assert.Greater(queued, config.maxTileUploadsPerFrame);
            terrain.Tick(0f);
            Assert.LessOrEqual(terrain.LastUploads, config.maxTileUploadsPerFrame);
            Assert.Less(mask.DirtyCount, queued);
        }

        [Test]
        public void ConnectivityWaitsForInterval()
        {
            terrain.Explode(-4f, 3f, 1.2f);
            terrain.Tick(config.connectivityInterval * 0.5f);
            Assert.IsTrue(mask.ConnectivityDirty);
            terrain.Tick(config.connectivityInterval);
            Assert.IsFalse(mask.ConnectivityDirty);
        }

        [Test]
        public void ResetRestoresLevelAndRemovesChunks()
        {
            int solid = mask.SolidCount;
            SliceLeftPeak(6f);
            Run(0.3f);
            terrain.Reset();
            Assert.AreEqual(solid, mask.SolidCount);
            Assert.AreEqual(0, chunks.ActiveCount);
            Assert.AreEqual(0, mask.DirtyCount);
        }

        [Test]
        public void TreeFallsWhenGroundUnderItIsGone()
        {
            terrain.Explode(-11.6f, mask.SurfaceHeight(-11.6f), 2.5f);
            Run(Step);
            Assert.Greater(decor.FallingCount, 0);
        }

        [Test]
        public void TreesStandWhileGroundIsIntact()
        {
            terrain.Explode(-8f, 1f, 1f);
            Run(0.5f);
            Assert.AreEqual(0, decor.FallingCount);
        }

        [Test]
        public void FallenTreeDisappearsAndResetBringsItBack()
        {
            Vector3[] standing = decor.Mesh.vertices;
            terrain.Explode(-11.6f, mask.SurfaceHeight(-11.6f), 2.5f);
            Run(3f);
            Assert.AreEqual(0, decor.FallingCount);
            CollectionAssert.AreNotEqual(standing, decor.Mesh.vertices);

            terrain.Reset();
            decor.Reset();
            CollectionAssert.AreEqual(standing, decor.Mesh.vertices);
        }

        [Test]
        public void ThemeRebuildKeepsFallenTrees()
        {
            terrain.Explode(-11.6f, mask.SurfaceHeight(-11.6f), 2.5f);
            Run(3f);
            Vector3[] fallen = decor.Mesh.vertices;
            decor.Build(ThemePalette.CreateSunset());
            CollectionAssert.AreEqual(fallen, decor.Mesh.vertices);
        }
    }
}
