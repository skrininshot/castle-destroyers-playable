using CastleDestroyers.Core;
using CastleDestroyers.Fx;
using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Падающие обломки (ТЗ §3.6): копия участка маски на кваде, гравитация и лёгкое вращение, без коллизий.
    /// Слоты с текстурой и мешем созданы заранее, новый обломок использует свободный.
    /// Вершины считаются здесь, поэтому Transform не трогается и кадр не аллоцирует.
    /// </summary>
    public sealed class ChunkSystem : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const float BoundsSize = 500f;
        private const float WaterTouchHalfHeightFraction = 0.5f;

        private GameConfig config;
        private LevelFx fx;
        private Rng rng;
        private IndexPool pool;

        private Texture2D[] textures;
        private Material[] materials;
        private Mesh[] meshes;
        private MeshRenderer[] renderers;
        private Vector3[][] vertices;
        private Vector2[][] uvs;

        private float[] posX;
        private float[] posY;
        private float[] velX;
        private float[] velY;
        private float[] angle;
        private float[] spin;
        private float[] halfWidth;
        private float[] halfHeight;
        private float[] radius;
        private byte[] splashed;

        public int ActiveCount { get { return pool.ActiveCount; } }
        public bool HasFreeSlot { get { return pool.ActiveCount < pool.Capacity; } }

        public void Init(GameConfig gameConfig, LevelFx levelFx, Rng random, Material template, Transform parent, int sortingOrder)
        {
            config = gameConfig;
            fx = levelFx;
            rng = random;

            int capacity = config.chunkSlots;
            pool = new IndexPool(capacity);
            textures = new Texture2D[capacity];
            materials = new Material[capacity];
            meshes = new Mesh[capacity];
            renderers = new MeshRenderer[capacity];
            vertices = new Vector3[capacity][];
            uvs = new Vector2[capacity][];
            posX = new float[capacity];
            posY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            angle = new float[capacity];
            spin = new float[capacity];
            halfWidth = new float[capacity];
            halfHeight = new float[capacity];
            radius = new float[capacity];
            splashed = new byte[capacity];

            int[] triangles = { 0, 2, 1, 2, 3, 1 };
            for (int i = 0; i < capacity; i++)
            {
                textures[i] = TerrainMaterial.CreateMaskTexture(config.chunkTextureSize);
                Material material = new Material(template);
                material.SetTexture(TerrainMaterial.MaskTex, textures[i]);
                TerrainMaterial.SetEdgeSoftness(material, config.maskEdgeSoftness);
                materials[i] = material;

                vertices[i] = new Vector3[VerticesPerQuad];
                uvs[i] = new Vector2[VerticesPerQuad];
                Mesh mesh = new Mesh();
                mesh.name = "Chunk";
                mesh.MarkDynamic();
                mesh.vertices = vertices[i];
                mesh.uv = uvs[i];
                mesh.triangles = triangles;
                mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
                meshes[i] = mesh;

                GameObject chunkObject = new GameObject("Chunk");
                chunkObject.transform.SetParent(parent, false);
                chunkObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer chunkRenderer = chunkObject.AddComponent<MeshRenderer>();
                chunkRenderer.sharedMaterial = material;
                chunkRenderer.sortingOrder = sortingOrder;
                chunkRenderer.enabled = false;
                renderers[i] = chunkRenderer;
            }
        }

        public void ApplyPalette(PaletteSet palette)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                TerrainMaterial.ApplyPalette(materials[i], palette);
            }
        }

        /// <summary>Создаёт обломок из результата вырезания. False, если свободных слотов нет.</summary>
        public bool Spawn(ChunkExtractor extractor)
        {
            int i = pool.Alloc();
            if (i < 0)
            {
                return false;
            }
            float minX = extractor.WorldMinX;
            float minY = extractor.WorldMinY;
            float maxX = extractor.WorldMaxX;
            float maxY = extractor.WorldMaxY;
            posX[i] = (minX + maxX) * 0.5f;
            posY[i] = (minY + maxY) * 0.5f;
            halfWidth[i] = (maxX - minX) * 0.5f;
            halfHeight[i] = (maxY - minY) * 0.5f;
            radius[i] = Mathf.Sqrt(halfWidth[i] * halfWidth[i] + halfHeight[i] * halfHeight[i]);
            velX[i] = rng.Range(-config.chunkMaxDrift, config.chunkMaxDrift);
            velY[i] = 0f;
            angle[i] = 0f;
            spin[i] = rng.Range(-config.chunkMaxSpinDeg, config.chunkMaxSpinDeg) * Mathf.Deg2Rad;
            splashed[i] = 0;

            Vector2[] uv = uvs[i];
            float uvMaxX = extractor.UvMaxX;
            float uvMaxY = extractor.UvMaxY;
            uv[0].x = 0f;
            uv[0].y = 0f;
            uv[1].x = uvMaxX;
            uv[1].y = 0f;
            uv[2].x = 0f;
            uv[2].y = uvMaxY;
            uv[3].x = uvMaxX;
            uv[3].y = uvMaxY;
            meshes[i].uv = uv;

            Texture2D texture = textures[i];
            texture.SetPixels32(extractor.Pixels);
            texture.Apply(false, false);
            WriteVertices(i);
            renderers[i].enabled = true;
            return true;
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0)
            {
                return;
            }
            float gravity = config.chunkGravity;
            float water = config.waterLevel;
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                velY[i] -= gravity * dt;
                posX[i] += velX[i] * dt;
                posY[i] += velY[i] * dt;
                angle[i] += spin[i] * dt;

                if (splashed[i] == 0 && posY[i] - halfHeight[i] * WaterTouchHalfHeightFraction < water)
                {
                    splashed[i] = 1;
                    fx.Splash(posX[i], water, halfWidth[i] * 2f);
                }
                if (posY[i] + radius[i] < water)
                {
                    renderers[i].enabled = false;
                    pool.Free(i);
                    continue;
                }
                WriteVertices(i);
            }
        }

        /// <summary>Убирает все обломки: рестарт сессии.</summary>
        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                renderers[i].enabled = false;
            }
            pool.Clear();
        }

        private void WriteVertices(int i)
        {
            float cos = Mathf.Cos(angle[i]);
            float sin = Mathf.Sin(angle[i]);
            float halfX = halfWidth[i];
            float halfY = halfHeight[i];
            float axisXx = cos * halfX;
            float axisXy = sin * halfX;
            float axisYx = -sin * halfY;
            float axisYy = cos * halfY;
            float x = posX[i];
            float y = posY[i];
            Vector3[] verts = vertices[i];
            verts[0].x = x - axisXx - axisYx;
            verts[0].y = y - axisXy - axisYy;
            verts[1].x = x + axisXx - axisYx;
            verts[1].y = y + axisXy - axisYy;
            verts[2].x = x - axisXx + axisYx;
            verts[2].y = y - axisXy + axisYy;
            verts[3].x = x + axisXx + axisYx;
            verts[3].y = y + axisXy + axisYy;
            meshes[i].vertices = verts;
        }
    }
}
