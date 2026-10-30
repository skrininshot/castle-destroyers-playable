using CastleDestroyers.Spike;
using NUnit.Framework;

namespace CastleDestroyers.Tests
{
    public class SpikeMaskTests
    {
        private const int Size = 512;
        private const int TileSize = 128;

        private static SpikeMask CreateCleanMask()
        {
            SpikeMask mask = new SpikeMask();
            mask.Init(Size, TileSize);
            while (mask.PopDirtyTile() >= 0)
            {
            }
            return mask;
        }

        [Test]
        public void Init_MarksAllTilesDirty()
        {
            SpikeMask mask = new SpikeMask();
            mask.Init(Size, TileSize);
            Assert.AreEqual(16, mask.DirtyCount);
            Assert.AreEqual(Size * Size, mask.SolidCount);
        }

        [Test]
        public void Stamp_RemovesCenterAndKeepsFarTexels()
        {
            SpikeMask mask = CreateCleanMask();
            mask.StampCrater(100, 100, 10, 4);
            Assert.AreEqual(0, mask.Solid[100 * Size + 100]);
            Assert.AreEqual(255, mask.Solid[100 * Size + 120]);
            Assert.Less(mask.SolidCount, Size * Size);
        }

        [Test]
        public void Stamp_WritesRimOutsideRadiusOnly()
        {
            SpikeMask mask = CreateCleanMask();
            mask.StampCrater(100, 100, 10, 4);
            Assert.Greater(mask.Shade[100 * Size + 111], 0);
            Assert.AreEqual(255, mask.Solid[100 * Size + 111]);
            Assert.AreEqual(0, mask.Shade[100 * Size + 120]);
        }

        [Test]
        public void Stamp_OnTileBorderDirtiesBothTilesOnce()
        {
            SpikeMask mask = CreateCleanMask();
            mask.StampCrater(TileSize, 50, 10, 4);
            mask.StampCrater(TileSize, 50, 10, 4);
            Assert.AreEqual(2, mask.DirtyCount);
            Assert.AreEqual(0, mask.PopDirtyTile());
            Assert.AreEqual(1, mask.PopDirtyTile());
            Assert.AreEqual(-1, mask.PopDirtyTile());
        }

        [Test]
        public void Stamp_OutsideMaskChangesNothing()
        {
            SpikeMask mask = CreateCleanMask();
            mask.StampCrater(-100, -100, 10, 4);
            mask.StampCrater(Size + 100, 20, 10, 4);
            Assert.AreEqual(Size * Size, mask.SolidCount);
            Assert.AreEqual(0, mask.DirtyCount);
        }

        [Test]
        public void Reset_RestoresMaterialAfterStamps()
        {
            SpikeMask mask = CreateCleanMask();
            mask.StampCrater(300, 300, 40, 4);
            mask.Reset();
            Assert.AreEqual(Size * Size, mask.SolidCount);
            Assert.AreEqual(16, mask.DirtyCount);
            Assert.AreEqual(255, mask.Solid[300 * Size + 300]);
        }
    }
}
