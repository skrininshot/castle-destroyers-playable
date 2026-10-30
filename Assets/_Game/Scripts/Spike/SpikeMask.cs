using UnityEngine;

namespace CastleDestroyers.Spike
{
    /// <summary>CPU-маска с очередью грязных тайлов. Без обращений к GPU.</summary>
    public sealed class SpikeMask
    {
        private const byte SolidValue = 255;

        public byte[] Solid { get; private set; }
        public byte[] Shade { get; private set; }
        public int Size { get; private set; }
        public int TileSize { get; private set; }
        public int TilesPerSide { get; private set; }
        public int SolidCount { get; private set; }
        public int DirtyCount { get; private set; }

        private int[] dirtyQueue;
        private byte[] tileQueued;
        private int dirtyHead;

        public void Init(int size, int tileSize)
        {
            Size = size;
            TileSize = tileSize;
            TilesPerSide = size / tileSize;
            int texels = size * size;
            int tiles = TilesPerSide * TilesPerSide;
            Solid = new byte[texels];
            Shade = new byte[texels];
            dirtyQueue = new int[tiles];
            tileQueued = new byte[tiles];
            Reset();
        }

        /// <summary>Восстанавливает материал и ставит все тайлы в очередь.</summary>
        public void Reset()
        {
            int texels = Size * Size;
            for (int i = 0; i < texels; i++)
            {
                Solid[i] = SolidValue;
                Shade[i] = 0;
            }
            SolidCount = texels;
            int tiles = TilesPerSide * TilesPerSide;
            for (int i = 0; i < tiles; i++)
            {
                MarkDirty(i);
            }
        }

        public void StampCrater(int centerX, int centerY, int radius, int rimWidth)
        {
            int outer = radius + rimWidth;
            int minX = Mathf.Max(0, centerX - outer);
            int maxX = Mathf.Min(Size - 1, centerX + outer);
            int minY = Mathf.Max(0, centerY - outer);
            int maxY = Mathf.Min(Size - 1, centerY + outer);
            if (minX > maxX || minY > maxY)
            {
                return;
            }

            int radiusSq = radius * radius;
            int outerSq = outer * outer;
            for (int y = minY; y <= maxY; y++)
            {
                int dy = y - centerY;
                int row = y * Size;
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - centerX;
                    int distSq = dx * dx + dy * dy;
                    if (distSq > outerSq)
                    {
                        continue;
                    }
                    int index = row + x;
                    if (Solid[index] == 0)
                    {
                        continue;
                    }
                    if (distSq <= radiusSq)
                    {
                        Solid[index] = 0;
                        Shade[index] = 0;
                        SolidCount--;
                    }
                    else
                    {
                        float t = 1f - (Mathf.Sqrt(distSq) - radius) / rimWidth;
                        int shade = (int)(t * SolidValue);
                        if (shade > Shade[index])
                        {
                            Shade[index] = (byte)shade;
                        }
                    }
                }
            }

            int firstTileX = minX / TileSize;
            int lastTileX = maxX / TileSize;
            int firstTileY = minY / TileSize;
            int lastTileY = maxY / TileSize;
            for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
            {
                for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
                {
                    MarkDirty(tileY * TilesPerSide + tileX);
                }
            }
        }

        /// <summary>Следующий грязный тайл или -1. Тайл стоит в очереди не более одного раза.</summary>
        public int PopDirtyTile()
        {
            if (DirtyCount == 0)
            {
                return -1;
            }
            int tile = dirtyQueue[dirtyHead];
            dirtyHead = (dirtyHead + 1) % dirtyQueue.Length;
            DirtyCount--;
            tileQueued[tile] = 0;
            return tile;
        }

        private void MarkDirty(int tile)
        {
            if (tileQueued[tile] != 0)
            {
                return;
            }
            tileQueued[tile] = 1;
            dirtyQueue[(dirtyHead + DirtyCount) % dirtyQueue.Length] = tile;
            DirtyCount++;
        }
    }
}
