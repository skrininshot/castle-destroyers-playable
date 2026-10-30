using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Запечённая маска ландшафта: каналы Level1_TerrainMask.png в RLE (см. <see cref="MaskRle"/>), строка 0 — низ уровня.
    /// Создаётся TerrainMaskBaker. В сборку идёт этот ассет, а не PNG: текстуры Luna может пережать и уменьшить (D-024).
    /// </summary>
    public sealed class TerrainMaskData : ScriptableObject
    {
        public int width;
        public int height;
        public int[] solidRuns;
        public int[] grassRuns;
        public int[] buriedRuns;
    }
}
