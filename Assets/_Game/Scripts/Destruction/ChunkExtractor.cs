using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Вырезает остров из маски (ТЗ §3.6). Для обломка копирует его тексели в квадратный буфер Pixels в формате тайла;
    /// если остров не влезает, берёт каждый Scale-й тексель. Результат последнего вырезания лежит в свойствах.
    /// </summary>
    public sealed class ChunkExtractor
    {
        // Пустая рамка вокруг обломка, пикселей буфера: билинейка закрывает край нулём.
        private const int Padding = 1;
        private const float PixelCenter = 0.5f;

        private DestructibleMask mask;
        private ConnectivitySolver solver;

        public Color32[] Pixels { get; private set; }
        public int PixelSize { get; private set; }

        public int CutTexels { get; private set; }
        public int Scale { get; private set; }
        public float CenterX { get; private set; }
        public float CenterY { get; private set; }
        public float WorldMinX { get; private set; }
        public float WorldMinY { get; private set; }
        public float WorldMaxX { get; private set; }
        public float WorldMaxY { get; private set; }
        public float UvMaxX { get; private set; }
        public float UvMaxY { get; private set; }

        public void Init(DestructibleMask destructibleMask, ConnectivitySolver connectivitySolver, int pixelSize)
        {
            mask = destructibleMask;
            solver = connectivitySolver;
            PixelSize = pixelSize;
            Pixels = new Color32[pixelSize * pixelSize];
        }

        /// <summary>Убирает остров из маски. keepPixels — скопировать его в Pixels для обломка.</summary>
        public void Cut(int island, bool keepPixels)
        {
            int start = solver.IslandStart[island];
            int size = solver.IslandSize[island];
            int[] cells = solver.Cells;
            int cellsX = mask.CellsX;
            int cellSize = mask.CellSize;

            int minCellX = int.MaxValue;
            int minCellY = int.MaxValue;
            int maxCellX = 0;
            int maxCellY = 0;
            for (int i = 0; i < size; i++)
            {
                int cell = cells[start + i];
                int column = cell % cellsX;
                int row = cell / cellsX;
                minCellX = column < minCellX ? column : minCellX;
                maxCellX = column > maxCellX ? column : maxCellX;
                minCellY = row < minCellY ? row : minCellY;
                maxCellY = row > maxCellY ? row : maxCellY;
            }

            int minX = minCellX * cellSize;
            int minY = minCellY * cellSize;
            int maxX = Mathf.Min((maxCellX + 1) * cellSize, mask.Width) - 1;
            int maxY = Mathf.Min((maxCellY + 1) * cellSize, mask.Height) - 1;
            int spanX = maxX - minX + 1;
            int spanY = maxY - minY + 1;
            if (keepPixels)
            {
                ClearPixels();
            }
            int largest = spanX > spanY ? spanX : spanY;
            Scale = Mathf.Max(1, Mathf.CeilToInt((float)largest / (PixelSize - Padding * 2)));

            CutTexels = 0;
            for (int i = 0; i < size; i++)
            {
                int cell = cells[start + i];
                CutCell(cell % cellsX, cell / cellsX, minX, minY, keepPixels);
            }
            mask.MarkDirtyRect(minX, minY, maxX, maxY);

            CenterX = mask.WorldX((minX + maxX) * 0.5f);
            CenterY = mask.WorldY((minY + maxY) * 0.5f);

            // Пиксель j буфера показывает тексель min + (j - Padding) * Scale, его центр в UV (j + 0.5) / PixelSize.
            int pixelsX = (spanX + Scale - 1) / Scale + Padding * 2;
            int pixelsY = (spanY + Scale - 1) / Scale + Padding * 2;
            float texelLow = -(Padding + PixelCenter) * Scale;
            WorldMinX = mask.WorldX(minX + texelLow);
            WorldMinY = mask.WorldY(minY + texelLow);
            WorldMaxX = mask.WorldX(minX + texelLow + pixelsX * Scale);
            WorldMaxY = mask.WorldY(minY + texelLow + pixelsY * Scale);
            UvMaxX = (float)pixelsX / PixelSize;
            UvMaxY = (float)pixelsY / PixelSize;
        }

        private void CutCell(int column, int row, int minX, int minY, bool keepPixels)
        {
            int cellSize = mask.CellSize;
            int width = mask.Width;
            int x0 = column * cellSize;
            int y0 = row * cellSize;
            int x1 = Mathf.Min(x0 + cellSize, width);
            int y1 = Mathf.Min(y0 + cellSize, mask.Height);
            int scale = Scale;
            byte[] solid = mask.Solid;
            byte[] shade = mask.Shade;
            byte[] grass = mask.Grass;
            byte[] buried = mask.Buried;
            Color32[] pixels = Pixels;

            for (int y = y0; y < y1; y++)
            {
                int offsetY = y - minY;
                bool keepRow = keepPixels && offsetY % scale == 0;
                for (int x = x0; x < x1; x++)
                {
                    int index = y * width + x;
                    byte cover = solid[index];
                    if (cover == 0)
                    {
                        continue;
                    }
                    int offsetX = x - minX;
                    if (keepRow && offsetX % scale == 0)
                    {
                        int pixel = (offsetY / scale + Padding) * PixelSize + offsetX / scale + Padding;
                        pixels[pixel].r = cover;
                        pixels[pixel].g = shade[index];
                        pixels[pixel].b = grass[index];
                        pixels[pixel].a = buried[index];
                    }
                    mask.ClearTexel(index, x, y);
                    CutTexels++;
                }
            }
        }

        private void ClearPixels()
        {
            Color32[] pixels = Pixels;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = 0;
                pixels[i].g = 0;
                pixels[i].b = 0;
                pixels[i].a = 0;
            }
        }
    }
}
