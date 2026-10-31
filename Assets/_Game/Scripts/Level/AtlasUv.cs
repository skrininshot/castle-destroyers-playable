using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>Запись UV спрайта атласа в готовый массив вершин.</summary>
    public static class AtlasUv
    {
        private const int FieldsPerSprite = 4;

        /// <summary>Порядок вершин полосой: левый низ, правый низ, левый верх, правый верх.</summary>
        public static void SetStrip(Vector2[] uv, int v, int sprite)
        {
            int r = sprite * FieldsPerSprite;
            float[] source = ArtSprites.Uv;
            uv[v].x = source[r];
            uv[v].y = source[r + 1];
            uv[v + 1].x = source[r + 2];
            uv[v + 1].y = source[r + 1];
            uv[v + 2].x = source[r];
            uv[v + 2].y = source[r + 3];
            uv[v + 3].x = source[r + 2];
            uv[v + 3].y = source[r + 3];
        }
    }
}
