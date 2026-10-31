using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    /// <summary>
    /// Маска замка из его силуэта (ТЗ §3.6, D-042): треугольники собранного меша заливаются в сетку 16 тексел/м.
    /// Вызывается при старте. Маска на тексель шире силуэта: край меша задаёт геометрия, а не билинейка маски.
    /// Строка 0 совпадает с низом стены, это опора замка для связности.
    /// </summary>
    public static class CastleMaskBuilder
    {
        private const int Margin = 2;

        public static DestructibleMask Create(FlatMeshBuilder builder, GameConfig config, float wallBase)
        {
            float texelsPerMeter = config.maskTexelsPerMeter;
            Vector3[] vertices = builder.Vertices;
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            for (int v = 0; v < builder.VertexCount; v++)
            {
                minX = vertices[v].x < minX ? vertices[v].x : minX;
                maxX = vertices[v].x > maxX ? vertices[v].x : maxX;
                maxY = vertices[v].y > maxY ? vertices[v].y : maxY;
            }

            float originX = minX - Margin / texelsPerMeter;
            int width = Mathf.CeilToInt((maxX - minX) * texelsPerMeter) + Margin * 2 + 1;
            int height = Mathf.CeilToInt((maxY - wallBase) * texelsPerMeter) + Margin + 1;
            byte[] solid = new byte[width * height];

            int[] triangles = builder.Triangles;
            for (int t = 0; t + 2 < builder.TriangleIndexCount; t += 3)
            {
                Vector3 a = vertices[triangles[t]];
                Vector3 b = vertices[triangles[t + 1]];
                Vector3 c = vertices[triangles[t + 2]];
                FillTriangle(solid, width, height, (a.x - originX) * texelsPerMeter, (a.y - wallBase) * texelsPerMeter,
                    (b.x - originX) * texelsPerMeter, (b.y - wallBase) * texelsPerMeter,
                    (c.x - originX) * texelsPerMeter, (c.y - wallBase) * texelsPerMeter);
            }
            Dilate(solid, width, height);

            DestructibleMask mask = new DestructibleMask();
            mask.Init(width, height, Mathf.Max(width, height), config.castleConnectivityCellTexels, texelsPerMeter, originX, wallBase,
                solid, new byte[solid.Length], new byte[solid.Length]);
            return mask;
        }

        /// <summary>Закрашивает тексели, чьи узлы лежат в треугольнике; порядок обхода вершин не важен.</summary>
        private static void FillTriangle(byte[] solid, int width, int height, float ax, float ay, float bx, float by, float cx, float cy)
        {
            int fromX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ax, Mathf.Min(bx, cx))));
            int toX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(ax, Mathf.Max(bx, cx))));
            int fromY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ay, Mathf.Min(by, cy))));
            int toY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(ay, Mathf.Max(by, cy))));
            for (int y = fromY; y <= toY; y++)
            {
                for (int x = fromX; x <= toX; x++)
                {
                    float ab = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
                    float bc = (cx - bx) * (y - by) - (cy - by) * (x - bx);
                    float ca = (ax - cx) * (y - cy) - (ay - cy) * (x - cx);
                    bool sameSign = (ab >= 0f && bc >= 0f && ca >= 0f) || (ab <= 0f && bc <= 0f && ca <= 0f);
                    if (sameSign)
                    {
                        solid[y * width + x] = DestructibleMask.Full;
                    }
                }
            }
        }

        private static void Dilate(byte[] solid, int width, int height)
        {
            byte[] source = new byte[solid.Length];
            System.Array.Copy(solid, source, solid.Length);
            for (int y = 0; y < height; y++)
            {
                int fromY = y > 0 ? y - 1 : 0;
                int toY = y < height - 1 ? y + 1 : y;
                for (int x = 0; x < width; x++)
                {
                    int fromX = x > 0 ? x - 1 : 0;
                    int toX = x < width - 1 ? x + 1 : x;
                    for (int ny = fromY; ny <= toY; ny++)
                    {
                        for (int nx = fromX; nx <= toX; nx++)
                        {
                            if (source[ny * width + nx] != 0)
                            {
                                solid[y * width + x] = DestructibleMask.Full;
                            }
                        }
                    }
                }
            }
        }
    }
}
