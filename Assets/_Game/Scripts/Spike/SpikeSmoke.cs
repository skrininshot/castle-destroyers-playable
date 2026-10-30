using UnityEngine;

namespace CastleDestroyers.Spike
{
    /// <summary>Частицы дыма на SoA, отдельный динамический меш с вершинной альфой.</summary>
    public sealed class SpikeSmoke
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const byte Grey = 170;
        private const float MaxAlpha = 110f;

        public int ActiveCount { get; private set; }
        public Mesh Mesh { get; private set; }

        private SpikeConfig config;
        private SpikeRng rng;

        private float[] posX;
        private float[] posY;
        private float[] velX;
        private float[] age;
        private Vector3[] vertices;
        private Color32[] colors;

        public void Init(SpikeConfig spikeConfig, SpikeRng spikeRng)
        {
            config = spikeConfig;
            rng = spikeRng;

            int capacity = config.smokeCapacity;
            posX = new float[capacity];
            posY = new float[capacity];
            velX = new float[capacity];
            age = new float[capacity];
            vertices = new Vector3[capacity * VerticesPerQuad];
            colors = new Color32[vertices.Length];

            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[capacity * IndicesPerQuad];
            for (int i = 0; i < capacity; i++)
            {
                int v = i * VerticesPerQuad;
                uv[v] = new Vector2(0f, 0f);
                uv[v + 1] = new Vector2(1f, 0f);
                uv[v + 2] = new Vector2(0f, 1f);
                uv[v + 3] = new Vector2(1f, 1f);
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }
            for (int v = 0; v < colors.Length; v++)
            {
                colors[v].r = Grey;
                colors[v].g = Grey;
                colors[v].b = Grey;
            }

            Mesh = new Mesh();
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
        }

        public void SetActiveCount(int count)
        {
            int clamped = Mathf.Clamp(count, 0, config.smokeCapacity);
            for (int i = ActiveCount; i < clamped; i++)
            {
                Respawn(i);
                age[i] = rng.Range(0f, config.smokeLifetime);
            }
            for (int v = clamped * VerticesPerQuad; v < ActiveCount * VerticesPerQuad; v++)
            {
                vertices[v].x = 0f;
                vertices[v].y = 0f;
                colors[v].a = 0;
            }
            ActiveCount = clamped;
        }

        public void Tick(float dt)
        {
            float lifetime = config.smokeLifetime;
            float riseStep = config.smokeRiseSpeed * dt;
            float sizeStart = config.smokeStartSize;
            float sizeRange = config.smokeEndSize - sizeStart;
            Vector3[] verts = vertices;
            Color32[] cols = colors;

            for (int i = 0; i < ActiveCount; i++)
            {
                float t = age[i] + dt;
                if (t >= lifetime)
                {
                    Respawn(i);
                    t = 0f;
                }
                age[i] = t;
                float x = posX[i] + velX[i] * dt;
                float y = posY[i] + riseStep;
                posX[i] = x;
                posY[i] = y;

                float life = t / lifetime;
                float half = (sizeStart + sizeRange * life) * 0.5f;
                byte alpha = (byte)((1f - life) * MaxAlpha);

                int v = i * VerticesPerQuad;
                verts[v].x = x - half;
                verts[v].y = y - half;
                verts[v + 1].x = x + half;
                verts[v + 1].y = y - half;
                verts[v + 2].x = x - half;
                verts[v + 2].y = y + half;
                verts[v + 3].x = x + half;
                verts[v + 3].y = y + half;
                cols[v].a = alpha;
                cols[v + 1].a = alpha;
                cols[v + 2].a = alpha;
                cols[v + 3].a = alpha;
            }
        }

        public void UploadMesh()
        {
            Mesh.vertices = vertices;
            Mesh.colors32 = colors;
        }

        private void Respawn(int i)
        {
            float half = config.worldSize * 0.5f;
            posX[i] = rng.Range(-half, half);
            posY[i] = rng.Range(-half, half);
            velX[i] = rng.Range(-config.smokeDrift, config.smokeDrift);
            age[i] = 0f;
        }
    }
}
