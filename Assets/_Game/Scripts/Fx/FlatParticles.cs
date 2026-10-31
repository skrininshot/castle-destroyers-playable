using CastleDestroyers.Core;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.Fx
{
    /// <summary>
    /// Плоские частицы (ТЗ §4.1): круги и 4-лучевые звёзды из атласа, SoA, один динамический меш. Тают размером, а не альфой,
    /// поэтому цвета заливаются только при спавне. Сверх capacity спавн молча отбрасывается.
    /// </summary>
    public sealed class FlatParticles : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float BoundsSize = 500f;

        private IndexPool pool;
        private float[] posX;
        private float[] posY;
        private float[] velX;
        private float[] velY;
        private float[] gravity;
        private float[] size;
        private float[] age;
        private float[] life;
        private Vector3[] vertices;
        private Color32[] colors;
        private Vector2[] uv;
        private byte[] star;
        private bool uvDirty;
        private bool colorsDirty;
        private bool verticesDirty;

        public Mesh Mesh { get; private set; }
        public int ActiveCount { get { return pool.ActiveCount; } }
        public int Capacity { get { return pool.Capacity; } }

        public void Init(int capacity)
        {
            pool = new IndexPool(capacity);
            posX = new float[capacity];
            posY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            gravity = new float[capacity];
            size = new float[capacity];
            age = new float[capacity];
            life = new float[capacity];
            vertices = new Vector3[capacity * VerticesPerQuad];
            colors = new Color32[capacity * VerticesPerQuad];

            uv = new Vector2[vertices.Length];
            star = new byte[capacity];
            int[] triangles = new int[capacity * IndicesPerQuad];
            for (int i = 0; i < capacity; i++)
            {
                int v = i * VerticesPerQuad;
                AtlasUv.SetStrip(uv, v, ArtSprites.Circle);
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            Mesh = new Mesh();
            Mesh.name = "FlatParticles";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
        }

        /// <summary>Искра-звезда (фейерверк, ТЗ §3.11): как круглая частица, другая форма.</summary>
        public void SpawnStar(float x, float y, float speedX, float speedY, float startSize, float lifetime, float fall, Color32 color)
        {
            int i = Spawn(x, y, speedX, speedY, startSize, lifetime, fall, color);
            if (i >= 0)
            {
                star[i] = 1;
                AtlasUv.SetStrip(uv, i * VerticesPerQuad, ArtSprites.Spark);
                uvDirty = true;
            }
        }

        /// <summary>Возвращает слот частицы или -1, если пул полон.</summary>
        public int Spawn(float x, float y, float speedX, float speedY, float startSize, float lifetime, float fall, Color32 color)
        {
            int i = pool.Alloc();
            if (i < 0)
            {
                return -1;
            }
            if (star[i] != 0)
            {
                star[i] = 0;
                AtlasUv.SetStrip(uv, i * VerticesPerQuad, ArtSprites.Circle);
                uvDirty = true;
            }
            posX[i] = x;
            posY[i] = y;
            velX[i] = speedX;
            velY[i] = speedY;
            gravity[i] = fall;
            size[i] = startSize;
            age[i] = 0f;
            life[i] = lifetime;
            int v = i * VerticesPerQuad;
            colors[v] = color;
            colors[v + 1] = color;
            colors[v + 2] = color;
            colors[v + 3] = color;
            colorsDirty = true;
            return i;
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0 && !verticesDirty)
            {
                return;
            }
            Vector3[] verts = vertices;
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                float t = age[i] + dt;
                age[i] = t;
                if (t >= life[i])
                {
                    Collapse(i);
                    pool.Free(i);
                    continue;
                }
                velY[i] -= gravity[i] * dt;
                float x = posX[i] + velX[i] * dt;
                float y = posY[i] + velY[i] * dt;
                posX[i] = x;
                posY[i] = y;

                float half = size[i] * 0.5f * (1f - t / life[i]);
                int v = i * VerticesPerQuad;
                verts[v].x = x - half;
                verts[v].y = y - half;
                verts[v + 1].x = x + half;
                verts[v + 1].y = y - half;
                verts[v + 2].x = x - half;
                verts[v + 2].y = y + half;
                verts[v + 3].x = x + half;
                verts[v + 3].y = y + half;
            }
            Mesh.vertices = vertices;
            verticesDirty = pool.ActiveCount > 0;
            if (colorsDirty)
            {
                Mesh.colors32 = colors;
                colorsDirty = false;
            }
            if (uvDirty)
            {
                Mesh.uv = uv;
                uvDirty = false;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                Collapse(i);
            }
            pool.Clear();
            Mesh.vertices = vertices;
            verticesDirty = false;
        }

        private void Collapse(int i)
        {
            int v = i * VerticesPerQuad;
            for (int k = 0; k < VerticesPerQuad; k++)
            {
                vertices[v + k].x = 0f;
                vertices[v + k].y = 0f;
            }
        }
    }
}
