using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Шлейфы ракет (ТЗ §3.4): плоские пуфы в одном динамическом меше. Пуф рождается цветом пламени,
    /// затем становится дымом, растёт и тает по альфе. Сверх smokeCapacity спавн отбрасывается.
    /// </summary>
    public sealed class SmokeTrails : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float BoundsSize = 500f;

        private GameConfig config;
        private IndexPool pool;
        private float[] posX;
        private float[] posY;
        private float[] age;
        private float[] scale;
        private byte[] flame;
        private Vector3[] vertices;
        private Color32[] colors;
        private byte flameR;
        private byte flameG;
        private byte flameB;
        private byte smokeR;
        private byte smokeG;
        private byte smokeB;
        private bool dirty;

        public Mesh Mesh { get; private set; }
        public int ActiveCount { get { return pool.ActiveCount; } }

        public void Init(GameConfig gameConfig)
        {
            config = gameConfig;
            int capacity = config.smokeCapacity;
            pool = new IndexPool(capacity);
            posX = new float[capacity];
            posY = new float[capacity];
            age = new float[capacity];
            scale = new float[capacity];
            flame = new byte[capacity];
            vertices = new Vector3[capacity * VerticesPerQuad];
            colors = new Color32[capacity * VerticesPerQuad];

            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[capacity * IndicesPerQuad];
            for (int i = 0; i < capacity; i++)
            {
                int v = i * VerticesPerQuad;
                uv[v + 1].x = 1f;
                uv[v + 2].y = 1f;
                uv[v + 3].x = 1f;
                uv[v + 3].y = 1f;
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            Mesh = new Mesh();
            Mesh.name = "SmokeTrails";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
        }

        public void ApplyPalette(PaletteSet palette)
        {
            flameR = palette.rocketTail.r;
            flameG = palette.rocketTail.g;
            flameB = palette.rocketTail.b;
            smokeR = palette.smoke.r;
            smokeG = palette.smoke.g;
            smokeB = palette.smoke.b;
        }

        /// <summary>Новый пуф в точке; sizeScale утолщает шлейф при LOD.</summary>
        public void Emit(float x, float y, float sizeScale)
        {
            int i = pool.Alloc();
            if (i < 0)
            {
                return;
            }
            posX[i] = x;
            posY[i] = y;
            age[i] = 0f;
            scale[i] = sizeScale;
            flame[i] = 1;
            int v = i * VerticesPerQuad;
            for (int k = v; k < v + VerticesPerQuad; k++)
            {
                colors[k].r = flameR;
                colors[k].g = flameG;
                colors[k].b = flameB;
            }
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0 && !dirty)
            {
                return;
            }
            float life = config.smokeLife;
            float sizeStart = config.smokeSizeStart;
            float sizeGrow = config.smokeSizeEnd - sizeStart;
            float flameFraction = config.smokeFlameFraction;
            float maxAlpha = config.smokeAlpha;
            Vector3[] verts = vertices;
            Color32[] paint = colors;

            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                float t = age[i] + dt;
                age[i] = t;
                if (t >= life)
                {
                    Collapse(i);
                    pool.Free(i);
                    continue;
                }
                float fraction = t / life;
                float half = (sizeStart + sizeGrow * fraction) * scale[i] * 0.5f;
                float x = posX[i];
                float y = posY[i];
                byte alpha = (byte)(maxAlpha * (1f - fraction));
                bool cooled = flame[i] != 0 && fraction >= flameFraction;
                if (cooled)
                {
                    flame[i] = 0;
                }
                int v = i * VerticesPerQuad;
                verts[v].x = x - half;
                verts[v].y = y - half;
                verts[v + 1].x = x + half;
                verts[v + 1].y = y - half;
                verts[v + 2].x = x - half;
                verts[v + 2].y = y + half;
                verts[v + 3].x = x + half;
                verts[v + 3].y = y + half;
                for (int k = v; k < v + VerticesPerQuad; k++)
                {
                    paint[k].a = alpha;
                    if (cooled)
                    {
                        paint[k].r = smokeR;
                        paint[k].g = smokeG;
                        paint[k].b = smokeB;
                    }
                }
            }
            Mesh.vertices = vertices;
            Mesh.colors32 = colors;
            dirty = pool.ActiveCount > 0;
        }

        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                Collapse(i);
            }
            pool.Clear();
            Mesh.vertices = vertices;
            dirty = false;
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
