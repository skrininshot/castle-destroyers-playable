using CastleDestroyers.Core;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// CPU-маска разрушаемого объекта (ТЗ §5.3) без обращений к GPU. Значения — покрытие текселя 0..255:
    /// на срезах оно промежуточное, поэтому край остаётся гладким при низком разрешении.
    /// Соседние тайлы делят строку текселей: шаг тайла на 1 меньше его размера, иначе билинейка даёт шов.
    /// Строка 0 — низ маски.
    /// </summary>
    public sealed class DestructibleMask
    {
        /// <summary>С этого значения тексель считается материалом для попаданий и формы.</summary>
        public const byte SolidThreshold = 128;
        public const byte Full = 255;

        public byte[] Solid { get; private set; }
        public byte[] Shade { get; private set; }
        public byte[] Grass { get; private set; }
        public byte[] Buried { get; private set; }
        /// <summary>Сколько непустых текселей в каждой ячейке сетки связности.</summary>
        public int[] CellCounts { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileSize { get; private set; }
        public int TileStride { get; private set; }
        public int TilesX { get; private set; }
        public int TilesY { get; private set; }
        public int CellSize { get; private set; }
        public int CellsX { get; private set; }
        public int CellsY { get; private set; }
        public float TexelsPerMeter { get; private set; }
        public float OriginX { get; private set; }
        public float OriginY { get; private set; }

        /// <summary>Непустых текселей.</summary>
        public int SolidCount { get; private set; }
        public int DirtyCount { get; private set; }
        /// <summary>Растёт при каждом изменении: зависимым системам не нужно проверять маску каждый кадр.</summary>
        public int Revision { get; private set; }
        /// <summary>Материал убывал со времени последней проверки связности.</summary>
        public bool ConnectivityDirty { get; set; }

        private byte[] pristineSolid;
        private int[] dirtyQueue;
        private byte[] tileQueued;
        private int dirtyHead;

        /// <summary>Собирает маску ландшафта из запечённых данных конфига.</summary>
        public static DestructibleMask CreateTerrain(GameConfig config)
        {
            TerrainMaskData data = config.terrainMask;
            int texels = data.width * data.height;
            byte[] solid = new byte[texels];
            byte[] grass = new byte[texels];
            byte[] buried = new byte[texels];
            MaskRle.Decode(data.solidRuns, solid);
            MaskRle.Decode(data.grassRuns, grass);
            MaskRle.Decode(data.buriedRuns, buried);

            DestructibleMask mask = new DestructibleMask();
            mask.Init(data.width, data.height, config.maskTileSize, config.connectivityCellTexels,
                config.maskTexelsPerMeter, config.terrainMaskOrigin.x, config.terrainMaskOrigin.y, solid, grass, buried);
            return mask;
        }

        /// <summary>Размеры должны быть кратны шагу тайла плюс 1. Массивы переходят во владение маски.</summary>
        public void Init(int width, int height, int tileSize, int cellSize, float texelsPerMeter, float originX, float originY,
            byte[] solid, byte[] grass, byte[] buried)
        {
            Width = width;
            Height = height;
            TileSize = tileSize;
            TileStride = tileSize - 1;
            TilesX = (width - 1) / TileStride;
            TilesY = (height - 1) / TileStride;
            CellSize = cellSize;
            CellsX = (width + cellSize - 1) / cellSize;
            CellsY = (height + cellSize - 1) / cellSize;
            TexelsPerMeter = texelsPerMeter;
            OriginX = originX;
            OriginY = originY;

            Solid = solid;
            Grass = grass;
            Buried = buried;
            Shade = new byte[solid.Length];
            pristineSolid = new byte[solid.Length];
            System.Array.Copy(solid, pristineSolid, solid.Length);
            CellCounts = new int[CellsX * CellsY];
            int tiles = TilesX * TilesY;
            dirtyQueue = new int[tiles];
            tileQueued = new byte[tiles];
            Reset();
        }

        /// <summary>Возвращает материал к исходному и ставит все тайлы в очередь.</summary>
        public void Reset()
        {
            System.Array.Copy(pristineSolid, Solid, Solid.Length);
            System.Array.Clear(Shade, 0, Shade.Length);
            System.Array.Clear(CellCounts, 0, CellCounts.Length);
            SolidCount = 0;
            for (int y = 0; y < Height; y++)
            {
                int row = y * Width;
                int cellRow = y / CellSize * CellsX;
                for (int x = 0; x < Width; x++)
                {
                    if (Solid[row + x] != 0)
                    {
                        SolidCount++;
                        CellCounts[cellRow + x / CellSize]++;
                    }
                }
            }
            for (int tile = 0; tile < dirtyQueue.Length; tile++)
            {
                MarkTileDirty(tile);
            }
            ConnectivityDirty = false;
            Revision++;
        }

        public float WorldX(float texelX)
        {
            return OriginX + texelX / TexelsPerMeter;
        }

        public float WorldY(float texelY)
        {
            return OriginY + texelY / TexelsPerMeter;
        }

        public float TexelX(float worldX)
        {
            return (worldX - OriginX) * TexelsPerMeter;
        }

        public float TexelY(float worldY)
        {
            return (worldY - OriginY) * TexelsPerMeter;
        }

        /// <summary>Есть ли материал в точке мира. За границами маски материала нет.</summary>
        public bool IsSolidWorld(float x, float y)
        {
            int texelX = (int)(TexelX(x) + 0.5f);
            int texelY = (int)(TexelY(y) + 0.5f);
            if (texelX < 0 || texelY < 0 || texelX >= Width || texelY >= Height)
            {
                return false;
            }
            return Solid[texelY * Width + texelX] >= SolidThreshold;
        }

        /// <summary>Верхняя точка материала в колонке x, метры. Если колонка пуста или вне маски — низ маски.</summary>
        public float SurfaceHeight(float x)
        {
            int texelX = (int)(TexelX(x) + 0.5f);
            if (texelX < 0 || texelX >= Width)
            {
                return OriginY;
            }
            for (int y = Height - 1; y >= 0; y--)
            {
                if (Solid[y * Width + texelX] >= SolidThreshold)
                {
                    return WorldY(y);
                }
            }
            return OriginY;
        }

        /// <summary>Убирает тексель целиком. Границы не проверяются: вызывается из циклов штамповки.</summary>
        public void ClearTexel(int index, int x, int y)
        {
            Solid[index] = 0;
            Shade[index] = 0;
            SolidCount--;
            CellCounts[y / CellSize * CellsX + x / CellSize]--;
        }

        /// <summary>Ставит в очередь тайлы, в которые входят тексели прямоугольника (границы включены).</summary>
        public void MarkDirtyRect(int minX, int minY, int maxX, int maxY)
        {
            int firstX = FirstTile(minX);
            int lastX = LastTile(maxX, TilesX);
            int firstY = FirstTile(minY);
            int lastY = LastTile(maxY, TilesY);
            for (int tileY = firstY; tileY <= lastY; tileY++)
            {
                for (int tileX = firstX; tileX <= lastX; tileX++)
                {
                    MarkTileDirty(tileY * TilesX + tileX);
                }
            }
            Revision++;
        }

        /// <summary>Тексель на стыке принадлежит и левому тайлу, поэтому начинаем на тексель раньше.</summary>
        private int FirstTile(int texel)
        {
            int tile = (texel - 1) / TileStride;
            return tile < 0 ? 0 : tile;
        }

        private int LastTile(int texel, int tiles)
        {
            int tile = texel / TileStride;
            return tile >= tiles ? tiles - 1 : tile;
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

        private void MarkTileDirty(int tile)
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
