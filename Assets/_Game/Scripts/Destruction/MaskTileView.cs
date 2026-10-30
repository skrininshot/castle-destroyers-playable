using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Отрисовка маски тайлами (ТЗ §5.1): своя Texture2D и квад на тайл, заливка только грязных и не больше лимита за кадр.
    /// Тайлы без материала не создаются, опустевшие гаснут и не тратят draw call.
    /// </summary>
    public sealed class MaskTileView
    {
        private const float PixelCenter = 0.5f;

        private DestructibleMask mask;
        private Texture2D[] textures;
        private Material[] materials;
        private MeshRenderer[] renderers;
        private Color32[] pixels;

        public int Uploaded { get; private set; }

        public void Init(DestructibleMask destructibleMask, Material template, float edgeSoftness, Transform parent, int sortingOrder)
        {
            mask = destructibleMask;
            int tileCount = mask.TilesX * mask.TilesY;
            textures = new Texture2D[tileCount];
            materials = new Material[tileCount];
            renderers = new MeshRenderer[tileCount];
            pixels = new Color32[mask.TileSize * mask.TileSize];

            Mesh quad = CreateTileQuad();
            float stride = mask.TileStride / mask.TexelsPerMeter;
            for (int tileY = 0; tileY < mask.TilesY; tileY++)
            {
                for (int tileX = 0; tileX < mask.TilesX; tileX++)
                {
                    if (!HasMaterial(tileX, tileY))
                    {
                        continue;
                    }
                    int tile = tileY * mask.TilesX + tileX;
                    textures[tile] = TerrainMaterial.CreateMaskTexture(mask.TileSize);
                    Material material = new Material(template);
                    material.SetTexture(TerrainMaterial.MaskTex, textures[tile]);
                    TerrainMaterial.SetEdgeSoftness(material, edgeSoftness);
                    materials[tile] = material;

                    GameObject tileObject = new GameObject("Tile");
                    tileObject.transform.SetParent(parent, false);
                    tileObject.transform.localPosition = new Vector3(mask.OriginX + tileX * stride, mask.OriginY + tileY * stride, 0f);
                    tileObject.transform.localScale = new Vector3(stride, stride, 1f);
                    tileObject.AddComponent<MeshFilter>().sharedMesh = quad;
                    MeshRenderer tileRenderer = tileObject.AddComponent<MeshRenderer>();
                    tileRenderer.sharedMaterial = material;
                    tileRenderer.sortingOrder = sortingOrder;
                    renderers[tile] = tileRenderer;
                }
            }
        }

        public void ApplyPalette(PaletteSet palette)
        {
            for (int tile = 0; tile < materials.Length; tile++)
            {
                if (materials[tile] != null)
                {
                    TerrainMaterial.ApplyPalette(materials[tile], palette);
                }
            }
        }

        /// <summary>Заливает не больше limit тайлов. Пустые тайлы из лимита не вычитаются.</summary>
        public void UploadDirty(int limit)
        {
            Uploaded = 0;
            while (Uploaded < limit)
            {
                int tile = mask.PopDirtyTile();
                if (tile < 0)
                {
                    return;
                }
                if (textures[tile] != null)
                {
                    UploadTile(tile);
                    Uploaded++;
                }
            }
        }

        /// <summary>Заливает всю очередь сразу: на старте и при рестарте.</summary>
        public void UploadAll()
        {
            UploadDirty(int.MaxValue);
        }

        private bool HasMaterial(int tileX, int tileY)
        {
            int firstX = tileX * mask.TileStride;
            int firstY = tileY * mask.TileStride;
            for (int y = firstY; y < firstY + mask.TileSize; y++)
            {
                int row = y * mask.Width;
                for (int x = firstX; x < firstX + mask.TileSize; x++)
                {
                    if (mask.Solid[row + x] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UploadTile(int tile)
        {
            int tileSize = mask.TileSize;
            int width = mask.Width;
            int originX = (tile % mask.TilesX) * mask.TileStride;
            int originY = (tile / mask.TilesX) * mask.TileStride;
            byte[] solid = mask.Solid;
            byte[] shade = mask.Shade;
            byte[] grass = mask.Grass;
            byte[] buried = mask.Buried;
            Color32[] target = pixels;
            int any = 0;
            int pixel = 0;
            for (int y = 0; y < tileSize; y++)
            {
                int source = (originY + y) * width + originX;
                for (int x = 0; x < tileSize; x++)
                {
                    byte cover = solid[source];
                    any |= cover;
                    target[pixel].r = cover;
                    target[pixel].g = shade[source];
                    target[pixel].b = grass[source];
                    target[pixel].a = buried[source];
                    pixel++;
                    source++;
                }
            }
            bool visible = any != 0;
            if (renderers[tile].enabled != visible)
            {
                renderers[tile].enabled = visible;
            }
            if (visible)
            {
                Texture2D texture = textures[tile];
                texture.SetPixels32(target);
                texture.Apply(false, false);
            }
        }

        /// <summary>Единичный квад: края UV на центрах крайних текселей, поэтому соседние тайлы стыкуются без шва.</summary>
        private Mesh CreateTileQuad()
        {
            float uvMin = PixelCenter / mask.TileSize;
            float uvMax = 1f - uvMin;
            Mesh mesh = new Mesh();
            mesh.name = "MaskTile";
            mesh.vertices = new[] { new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f) };
            mesh.uv = new[] { new Vector2(uvMin, uvMin), new Vector2(uvMax, uvMin), new Vector2(uvMin, uvMax), new Vector2(uvMax, uvMax) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            return mesh;
        }
    }
}
