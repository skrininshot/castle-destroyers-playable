using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Штампы кратеров в маске (ТЗ §3.6). Земля — ровный круг с плоской тёмной каймой.
    /// Sqrt считается только для текселей на границах круга и каймы.
    /// </summary>
    public static class CraterStamper
    {
        private const float EdgeHalfWidth = 0.5f;
        private const float MaxByte = 255f;

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
            float outerEdge = radius + EdgeHalfWidth;
            float innerSq = innerEdge * innerEdge;
            float outerSq = outerEdge * outerEdge;
            float reachSq = reach * reach;
            byte[] solid = mask.Solid;
            byte[] shade = mask.Shade;
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

                    float distance = Mathf.Sqrt(distanceSq);
                    if (distanceSq < outerSq)
                    {
                        int edgeCover = (int)((distance - radius + EdgeHalfWidth) * MaxByte);
                        if (edgeCover <= 0)
                        {
                            mask.ClearTexel(index, x, y);
                            removed = true;
                            continue;
                        }
                        if (edgeCover < cover)
                        {
                            solid[index] = (byte)edgeCover;
                            removed = true;
                        }
                    }

                    float limit = rimOuter;
                    if (scallopDepth > 0f)
                    {
                        limit += scallopDepth * Mathf.Abs(Mathf.Sin(Mathf.Atan2(dy, dx) * scallopTeeth));
                    }
                    int rim = (int)((limit - distance + EdgeHalfWidth) * MaxByte);
                    if (rim > 0)
                    {
                        byte value = rim > MaxByte ? DestructibleMask.Full : (byte)rim;
                        if (value > shade[index])
                        {
                            shade[index] = value;
                        }
                    }
                }
            }

            if (touched)
            {
                mask.MarkDirtyRect(minX, minY, maxX, maxY);
            }
            if (removed)
            {
                mask.ConnectivityDirty = true;
            }
            return touched;
        }
    }
}
