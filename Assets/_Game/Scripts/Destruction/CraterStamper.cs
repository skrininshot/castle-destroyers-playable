using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Штампы кратеров в маске (ТЗ §3.6). Земля — ровный круг с плоской тёмной каймой, камень — «погрызанный» круг
    /// с трещинами. Sqrt считается только для текселей на границах круга и каймы.
    /// </summary>
    public static class CraterStamper
    {
        private const float EdgeHalfWidth = 0.5f;
        private const float MaxByte = 255f;
        private const float FullTurn = Mathf.PI * 2f;
        private const float CrackMinHalfWidth = 0.5f;

        /// <summary>
        /// Круг с каймой, всё в текселях. scallopDepth > 0 делает внешний край каймы фестоном из scallopTeeth зубцов.
        /// Возвращает true, если материал изменился.
        /// </summary>
        public static bool StampCircle(DestructibleMask mask, float centerX, float centerY, float radius, float rimWidth,
            float scallopDepth, int scallopTeeth)
        {
            float rimOuter = radius + rimWidth;
            float reach = rimOuter + scallopDepth + EdgeHalfWidth;
            int width = mask.Width;
            int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - reach));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(centerX + reach));
            int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - reach));
            int maxY = Mathf.Min(mask.Height - 1, Mathf.CeilToInt(centerY + reach));
            if (minX > maxX || minY > maxY)
            {
                return false;
            }

            float innerEdge = Mathf.Max(0f, radius - EdgeHalfWidth);
            float innerSq = innerEdge * innerEdge;
            float reachSq = reach * reach;
            byte[] solid = mask.Solid;
            bool removed = false;
            bool touched = false;

            for (int y = minY; y <= maxY; y++)
            {
                float dy = y - centerY;
                int row = y * width;
                for (int x = minX; x <= maxX; x++)
                {
                    int index = row + x;
                    int cover = solid[index];
                    if (cover == 0)
                    {
                        continue;
                    }
                    float dx = x - centerX;
                    float distanceSq = dx * dx + dy * dy;
                    if (distanceSq >= reachSq)
                    {
                        continue;
                    }
                    touched = true;
                    if (distanceSq <= innerSq)
                    {
                        mask.ClearTexel(index, x, y);
                        removed = true;
                        continue;
                    }

                    float limit = rimOuter;
                    if (scallopDepth > 0f)
                    {
                        limit += scallopDepth * Mathf.Abs(Mathf.Sin(Mathf.Atan2(dy, dx) * scallopTeeth));
                    }
                    removed |= WriteEdgeTexel(mask, index, x, y, cover, Mathf.Sqrt(distanceSq), radius, limit);
                }
            }

            FinishStamp(mask, touched, removed, minX, minY, maxX, maxY);
            return touched;
        }

        /// <summary>
        /// «Погрызанный» круг камня: радиус в текселях умножается на зубцы teeth[0..count), кайма идёт за краем.
        /// Возвращает true, если материал изменился.
        /// </summary>
        public static bool StampGnawed(DestructibleMask mask, float centerX, float centerY, float radius, float rimWidth,
            float[] teeth, int count)
        {
            float lowest = float.PositiveInfinity;
            float highest = 0f;
            for (int i = 0; i < count; i++)
            {
                lowest = teeth[i] < lowest ? teeth[i] : lowest;
                highest = teeth[i] > highest ? teeth[i] : highest;
            }
            float reach = radius * highest + rimWidth + EdgeHalfWidth;
            int width = mask.Width;
            int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - reach));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(centerX + reach));
            int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - reach));
            int maxY = Mathf.Min(mask.Height - 1, Mathf.CeilToInt(centerY + reach));
            if (minX > maxX || minY > maxY)
            {
                return false;
            }

            float innerEdge = Mathf.Max(0f, radius * lowest - EdgeHalfWidth);
            float innerSq = innerEdge * innerEdge;
            float reachSq = reach * reach;
            byte[] solid = mask.Solid;
            bool removed = false;
            bool touched = false;

            for (int y = minY; y <= maxY; y++)
            {
                float dy = y - centerY;
                int row = y * width;
                for (int x = minX; x <= maxX; x++)
                {
                    int index = row + x;
                    int cover = solid[index];
                    if (cover == 0)
                    {
                        continue;
                    }
                    float dx = x - centerX;
                    float distanceSq = dx * dx + dy * dy;
                    if (distanceSq >= reachSq)
                    {
                        continue;
                    }
                    touched = true;
                    if (distanceSq <= innerSq)
                    {
                        mask.ClearTexel(index, x, y);
                        removed = true;
                        continue;
                    }
                    float edge = radius * ToothAt(Mathf.Atan2(dy, dx), teeth, count);
                    removed |= WriteEdgeTexel(mask, index, x, y, cover, Mathf.Sqrt(distanceSq), edge, edge + rimWidth);
                }
            }

            FinishStamp(mask, touched, removed, minX, minY, maxX, maxY);
            return touched;
        }

        /// <summary>
        /// Трещина-клин от точки (x, y), всё в текселях: ломаная из segments отрезков, сужается к концу.
        /// Пишется в канал B и только по материалу.
        /// </summary>
        public static void PaintCrack(DestructibleMask mask, float x, float y, float direction, float length, float startWidth,
            int segments, float bendRadians, Rng rng)
        {
            int width = mask.Width;
            int height = mask.Height;
            byte[] solid = mask.Solid;
            byte[] cracks = mask.Grass;
            float segmentLength = length / segments;
            int steps = Mathf.CeilToInt(segmentLength);
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;
            for (int s = 0; s < segments; s++)
            {
                float angle = direction + rng.Range(-bendRadians, bendRadians);
                float stepX = Mathf.Cos(angle) * segmentLength / steps;
                float stepY = Mathf.Sin(angle) * segmentLength / steps;
                for (int n = 0; n < steps; n++)
                {
                    float left = 1f - (s + (float)n / steps) / segments;
                    float half = Mathf.Max(CrackMinHalfWidth, startWidth * 0.5f * left);
                    int reach = Mathf.CeilToInt(half);
                    int column = Mathf.RoundToInt(x);
                    int row = Mathf.RoundToInt(y);
                    int fromY = Mathf.Max(0, row - reach + 1);
                    int toY = Mathf.Min(height - 1, row + reach - 1);
                    int fromX = Mathf.Max(0, column - reach + 1);
                    int toX = Mathf.Min(width - 1, column + reach - 1);
                    for (int py = fromY; py <= toY; py++)
                    {
                        int line = py * width;
                        for (int px = fromX; px <= toX; px++)
                        {
                            if (solid[line + px] != 0)
                            {
                                cracks[line + px] = DestructibleMask.Full;
                                minX = px < minX ? px : minX;
                                maxX = px > maxX ? px : maxX;
                                minY = py < minY ? py : minY;
                                maxY = py > maxY ? py : maxY;
                            }
                        }
                    }
                    x += stepX;
                    y += stepY;
                }
            }
            if (maxX >= 0)
            {
                mask.MarkDirtyRect(minX, minY, maxX, maxY);
            }
        }

        /// <summary>Зубцы идут ломаной: между соседними значениями линейная интерполяция.</summary>
        private static float ToothAt(float angle, float[] teeth, int count)
        {
            float position = (angle + Mathf.PI) / FullTurn * count;
            int first = (int)position;
            float blend = position - first;
            first = first >= count ? 0 : first;
            int next = first + 1 >= count ? 0 : first + 1;
            return teeth[first] + (teeth[next] - teeth[first]) * blend;
        }

        /// <summary>Тексель у края кратера: дробное покрытие и кайма. True, если материала стало меньше.</summary>
        private static bool WriteEdgeTexel(DestructibleMask mask, int index, int x, int y, int cover, float distance, float radius, float rimLimit)
        {
            bool removed = false;
            if (distance < radius + EdgeHalfWidth)
            {
                int edgeCover = (int)((distance - radius + EdgeHalfWidth) * MaxByte);
                if (edgeCover <= 0)
                {
                    mask.ClearTexel(index, x, y);
                    return true;
                }
                if (edgeCover < cover)
                {
                    mask.Solid[index] = (byte)edgeCover;
                    removed = true;
                }
            }
            int rim = (int)((rimLimit - distance + EdgeHalfWidth) * MaxByte);
            if (rim > 0)
            {
                byte value = rim > MaxByte ? DestructibleMask.Full : (byte)rim;
                if (value > mask.Shade[index])
                {
                    mask.Shade[index] = value;
                }
            }
            return removed;
        }

        private static void FinishStamp(DestructibleMask mask, bool touched, bool removed, int minX, int minY, int maxX, int maxY)
        {
            if (touched)
            {
                mask.MarkDirtyRect(minX, minY, maxX, maxY);
            }
            if (removed)
            {
                mask.ConnectivityDirty = true;
            }
        }
    }
}
