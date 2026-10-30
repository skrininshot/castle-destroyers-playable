using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Fx;
using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Взрывы (ТЗ §3.4, §4.1, §5.4): три плоских круга в одном меше и осколки в общем пуле частиц.
    /// Кратер штампуется на каждый взрыв, а близкие взрывы одного кадра сливаются в один визуальный.
    /// </summary>
    public sealed class ExplosionSystem : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int QuadsPerExplosion = 3;
        private const float BoundsSize = 500f;
        private const float FullTurn = Mathf.PI * 2f;
        private const float ScatterFloor = 0.4f;
        private const float AccentChance = 0.5f;
        private const float GrowthShare = 0.3f;
        private const float MidRing = 0.72f;
        private const float CoreRing = 0.42f;

        private GameConfig config;
        private TerrainSystem terrain;
        private FlatParticles particles;
        private LevelFx fx;
        private ScreenShake shake;
        private Rng rng;

        private IndexPool pool;
        private float[] posX;
        private float[] posY;
        private float[] age;
        private float[] strength;
        private int[] frameSlots;
        private int frameCount;
        private Vector3[] vertices;
        private Color32[] colors;
        private Color32 shardColor;
        private Color32 shardAccent;
        private bool dirty;

        public Mesh Mesh { get; private set; }
        public int ActiveCount { get { return pool.ActiveCount; } }
        /// <summary>Сколько взрывов слилось с соседними с начала сессии.</summary>
        public int MergedTotal { get; private set; }

        public void Init(GameConfig gameConfig, TerrainSystem terrainSystem, FlatParticles flatParticles, LevelFx levelFx, ScreenShake screenShake, Rng random)
        {
            config = gameConfig;
            terrain = terrainSystem;
            particles = flatParticles;
            fx = levelFx;
            shake = screenShake;
            rng = random;

            int capacity = config.explosionPoolSize;
            pool = new IndexPool(capacity);
            posX = new float[capacity];
            posY = new float[capacity];
            age = new float[capacity];
            strength = new float[capacity];
            frameSlots = new int[capacity];

            int quads = capacity * QuadsPerExplosion;
            vertices = new Vector3[quads * VerticesPerQuad];
            colors = new Color32[quads * VerticesPerQuad];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[quads * IndicesPerQuad];
            for (int i = 0; i < quads; i++)
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
            Mesh.name = "Explosions";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
        }

        /// <summary>Порядок кругов: внешний, средний, ядро. Цвета привязаны к квадам и не меняются в бою.</summary>
        public void ApplyPalette(PaletteSet palette)
        {
            for (int v = 0; v < colors.Length; v++)
            {
                int ring = v / VerticesPerQuad % QuadsPerExplosion;
                colors[v] = ring == 0 ? palette.blastEdge : (ring == 1 ? palette.blastMid : palette.blastCore);
            }
            Mesh.colors32 = colors;
            shardColor = palette.blastEdge;
            shardAccent = palette.groundCrater;
        }

        /// <summary>Взрыв в точке мира. crater = false — без кратера: попадание в замок до масок этапа 5.</summary>
        public void Explode(float x, float y, bool crater)
        {
            bool hit = crater && terrain.Stamp(x, y, config.craterRadiusGround);

            float mergeSquared = config.explosionMergeRadius * config.explosionMergeRadius;
            for (int n = 0; n < frameCount; n++)
            {
                int other = frameSlots[n];
                float dx = posX[other] - x;
                float dy = posY[other] - y;
                if (dx * dx + dy * dy <= mergeSquared)
                {
                    strength[other] = Mathf.Min(strength[other] + config.explosionMergeGrowth, config.explosionMaxStrength);
                    MergedTotal++;
                    return;
                }
            }

            int i = pool.Alloc();
            if (i < 0)
            {
                return;
            }
            posX[i] = x;
            posY[i] = y;
            age[i] = 0f;
            strength[i] = 1f;
            frameSlots[frameCount] = i;
            frameCount++;

            shake.Add(config.shakePerExplosion);
            SpawnShards(x, y);
            if (hit)
            {
                fx.Dust(x, y, config.craterRadiusGround, config.craterDustPuffs);
            }
        }

        public void Tick(float dt)
        {
            frameCount = 0;
            if (pool.ActiveCount == 0 && !dirty)
            {
                return;
            }
            float life = config.explosionLife;
            float radius = config.explosionRadius;
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
                float size = radius * strength[i] * Pulse(t / life);
                int quad = i * QuadsPerExplosion;
                WriteCircle(quad, posX[i], posY[i], size);
                WriteCircle(quad + 1, posX[i], posY[i], size * MidRing);
                WriteCircle(quad + 2, posX[i], posY[i], size * CoreRing);
            }
            Mesh.vertices = vertices;
            dirty = pool.ActiveCount > 0;
        }

        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                Collapse(i);
            }
            pool.Clear();
            frameCount = 0;
            MergedTotal = 0;
            Mesh.vertices = vertices;
            dirty = false;
        }

        /// <summary>Быстрый рост и более долгое схлопывание, 0..1.</summary>
        public static float Pulse(float fraction)
        {
            float p = fraction < GrowthShare ? fraction / GrowthShare : (1f - fraction) / (1f - GrowthShare);
            return p * (2f - p);
        }

        private void SpawnShards(float x, float y)
        {
            for (int n = 0; n < config.explosionShards; n++)
            {
                float angle = rng.Range(0f, FullTurn);
                float speed = config.explosionShardSpeed * rng.Range(ScatterFloor, 1f);
                particles.Spawn(x, y, Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, config.explosionShardSize * rng.Range(ScatterFloor, 1f),
                    config.explosionShardLife * rng.Range(ScatterFloor, 1f), config.dustGravity, rng.Chance(AccentChance) ? shardColor : shardAccent);
            }
        }

        private void WriteCircle(int quad, float x, float y, float radius)
        {
            int v = quad * VerticesPerQuad;
            Vector3[] verts = vertices;
            verts[v].x = x - radius;
            verts[v].y = y - radius;
            verts[v + 1].x = x + radius;
            verts[v + 1].y = y - radius;
            verts[v + 2].x = x - radius;
            verts[v + 2].y = y + radius;
            verts[v + 3].x = x + radius;
            verts[v + 3].y = y + radius;
        }

        private void Collapse(int i)
        {
            int quad = i * QuadsPerExplosion;
            for (int q = 0; q < QuadsPerExplosion; q++)
            {
                WriteCircle(quad + q, 0f, 0f, 0f);
            }
        }
    }
}
