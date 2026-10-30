using UnityEngine;

namespace CastleDestroyers.Spike
{
    /// <summary>Тайлы маски: своя Texture2D и квад на тайл, заливка через SetPixels32.</summary>
    public sealed class SpikeMaskView
    {
        private const byte Opaque = 255;
        private const int DirtTextureSize = 64;
        private const float DirtTiling = 0.5f;
        private const float TileDepth = 1f;

        private SpikeMask mask;
        private Texture2D[] tileTextures;
        private Color32[] tilePixels;

        public void Init(SpikeMask spikeMask, SpikeConfig config, Material template, Transform parent)
        {
            mask = spikeMask;
            int tileCount = mask.TilesPerSide * mask.TilesPerSide;
            int tileTexels = mask.TileSize * mask.TileSize;
            tileTextures = new Texture2D[tileCount];
            tilePixels = new Color32[tileTexels];
            for (int i = 0; i < tileTexels; i++)
            {
                tilePixels[i].a = Opaque;
            }

            Mesh quad = CreateUnitQuad();
            Texture2D dirt = CreateDirtTexture();
            int maskTexId = Shader.PropertyToID("_MaskTex");
            int mainTexId = Shader.PropertyToID("_MainTex");
            int tilingId = Shader.PropertyToID("_WorldTiling");
            float tileWorld = config.worldSize / mask.TilesPerSide;
            float half = config.worldSize * 0.5f;

            for (int tileY = 0; tileY < mask.TilesPerSide; tileY++)
            {
                for (int tileX = 0; tileX < mask.TilesPerSide; tileX++)
                {
                    int tile = tileY * mask.TilesPerSide + tileX;
                    Texture2D texture = new Texture2D(mask.TileSize, mask.TileSize, TextureFormat.RGBA32, false);
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    tileTextures[tile] = texture;

                    Material material = new Material(template);
                    material.SetTexture(maskTexId, texture);
                    material.SetTexture(mainTexId, dirt);
                    material.SetFloat(tilingId, DirtTiling);

                    GameObject tileObject = new GameObject("Tile");
                    tileObject.transform.SetParent(parent, false);
                    tileObject.transform.localPosition = new Vector3(-half + tileX * tileWorld, -half + tileY * tileWorld, TileDepth);
                    tileObject.transform.localScale = new Vector3(tileWorld, tileWorld, 1f);
                    tileObject.AddComponent<MeshFilter>().sharedMesh = quad;
                    tileObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
        }

        /// <summary>Заливает не больше limit тайлов. Возвращает число залитых.</summary>
        public int UploadDirty(int limit)
        {
            int uploaded = 0;
            while (uploaded < limit)
            {
                int tile = mask.PopDirtyTile();
                if (tile < 0)
                {
                    break;
                }
                UploadTile(tile);
                uploaded++;
            }
            return uploaded;
        }

        private void UploadTile(int tile)
        {
            int tileSize = mask.TileSize;
            int size = mask.Size;
            int originX = (tile % mask.TilesPerSide) * tileSize;
            int originY = (tile / mask.TilesPerSide) * tileSize;
            byte[] solid = mask.Solid;
            byte[] shade = mask.Shade;
            Color32[] pixels = tilePixels;
            int target = 0;
            for (int y = 0; y < tileSize; y++)
            {
                int source = (originY + y) * size + originX;
                for (int x = 0; x < tileSize; x++)
                {
                    pixels[target].r = solid[source];
                    pixels[target].g = shade[source];
                    target++;
                    source++;
                }
            }
            Texture2D texture = tileTextures[tile];
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
        }

        private static Mesh CreateUnitQuad()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new[] { new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f) };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            return mesh;
        }

        private static Texture2D CreateDirtTexture()
        {
            SpikeRng rng = new SpikeRng(12345u);
            Color32[] pixels = new Color32[DirtTextureSize * DirtTextureSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                float k = rng.Range(0.8f, 1f);
                pixels[i] = new Color32((byte)(146 * k), (byte)(96 * k), (byte)(58 * k), Opaque);
            }
            Texture2D texture = new Texture2D(DirtTextureSize, DirtTextureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
