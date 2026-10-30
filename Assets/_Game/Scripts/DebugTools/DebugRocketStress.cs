#if GAME_DEBUG
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.DebugTools
{
    /// <summary>
    /// Чит «спавн 1000 ракет»: SoA-квады в одном динамическом меше, баллистика + синусоида, слоты через IndexPool.
    /// Только для нагрузочной проверки каркаса. Настоящий RocketSystem появится на этапе 3.
    /// </summary>
    public sealed class DebugRocketStress : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float MinAngleDeg = 35f;
        private const float MaxAngleDeg = 65f;
        private const float MuzzleOffsetX = 3f;
        private const float MuzzleOffsetY = 3f;
        private const float Epsilon = 0.0001f;
        private const float BoundsPadding = 4f;

        private GameConfig config;
        private Rng rng;
        private GreyboxLevel level;
        private IndexPool pool;

        private float[] originX;
        private float[] originY;
        private float[] velX;
        private float[] velY;
        private float[] phase;
        private float[] age;
        private Vector3[] vertices;
        private Color32[] colors;
        private bool dirty;

        public Mesh Mesh { get; private set; }
        public int ActiveCount { get { return pool.ActiveCount; } }

        public void Init(GameConfig gameConfig, GreyboxLevel greyboxLevel, Rng random)
        {
            config = gameConfig;
            level = greyboxLevel;
            rng = random;

            int capacity = config.stressRocketCount;
            pool = new IndexPool(capacity);
            originX = new float[capacity];
            originY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            phase = new float[capacity];
            age = new float[capacity];
            vertices = new Vector3[capacity * VerticesPerQuad];
            colors = new Color32[capacity * VerticesPerQuad];

            int[] triangles = new int[capacity * IndicesPerQuad];
            for (int i = 0; i < capacity; i++)
            {
                int v = i * VerticesPerQuad;
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            Mesh = new Mesh();
            Mesh.name = "DebugRocketStress";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            float half = config.levelWidth + BoundsPadding;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(half * 2f, half * 2f, 1f));
        }

        /// <summary>Выпускает до count ракет из замка стороны; сверх лимита буфера не создаёт.</summary>
        public int Spawn(int count, Side side, PaletteSet palette)
        {
            float facing = side == Side.Player ? 1f : -1f;
            float muzzleX = (side == Side.Player ? level.PlayerFocusX : level.EnemyFocusX) + facing * MuzzleOffsetX;
            float muzzleY = (side == Side.Player ? level.PlayerFocusY : level.EnemyFocusY) + MuzzleOffsetY;
            Color32 nose = side == Side.Player ? palette.rocketPlayer : palette.rocketEnemy;
            Color32 tail = palette.rocketTail;

            int spawned = 0;
            for (int n = 0; n < count; n++)
            {
                int i = pool.Alloc();
                if (i < 0)
                {
                    break;
                }
                float angle = rng.Range(MinAngleDeg, MaxAngleDeg) * Mathf.Deg2Rad;
                float speed = rng.Range(config.launchSpeedMin, config.launchSpeedMax);
                originX[i] = muzzleX;
                originY[i] = muzzleY;
                velX[i] = facing * speed * Mathf.Cos(angle);
                velY[i] = speed * Mathf.Sin(angle);
                phase[i] = i * config.wavePhaseStep;
                age[i] = rng.Range(0f, config.stressRocketLifetime * 0.1f);

                int v = i * VerticesPerQuad;
                colors[v] = tail;
                colors[v + 1] = nose;
                colors[v + 2] = tail;
                colors[v + 3] = nose;
                spawned++;
            }
            if (spawned > 0)
            {
                Mesh.colors32 = colors;
                dirty = true;
            }
            return spawned;
        }

        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (pool.IsUsed(i))
                {
                    Collapse(i);
                }
            }
            pool.Clear();
            dirty = true;
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0 && !dirty)
            {
                return;
            }
            float gravity = config.gravity;
            float amplitude = config.waveAmplitude;
            float frequency = config.waveFrequency;
            float lifetime = config.stressRocketLifetime;
            float halfLength = config.rocketLength * 0.5f;
            float halfWidth = config.rocketWidth * 0.5f;
            float waterLevel = config.waterLevel;
            Vector3[] verts = vertices;

            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                float t = age[i] + dt;
                age[i] = t;

                float tangentX = velX[i];
                float tangentY = velY[i] - gravity * t;
                float tangentLength = Mathf.Sqrt(tangentX * tangentX + tangentY * tangentY) + Epsilon;
                float normalX = -tangentY / tangentLength;
                float normalY = tangentX / tangentLength;

                float wave = frequency * t + phase[i];
                float offset = amplitude * Mathf.Sin(wave);
                float offsetRate = amplitude * frequency * Mathf.Cos(wave);

                float x = originX[i] + velX[i] * t + normalX * offset;
                float y = originY[i] + velY[i] * t - 0.5f * gravity * t * t + normalY * offset;
                if (t >= lifetime || y < waterLevel)
                {
                    Collapse(i);
                    pool.Free(i);
                    continue;
                }

                float dirX = tangentX + normalX * offsetRate;
                float dirY = tangentY + normalY * offsetRate;
                float dirLength = Mathf.Sqrt(dirX * dirX + dirY * dirY) + Epsilon;
                dirX /= dirLength;
                dirY /= dirLength;

                float forwardX = dirX * halfLength;
                float forwardY = dirY * halfLength;
                float sideX = -dirY * halfWidth;
                float sideY = dirX * halfWidth;

                int v = i * VerticesPerQuad;
                verts[v].x = x - forwardX - sideX;
                verts[v].y = y - forwardY - sideY;
                verts[v + 1].x = x + forwardX - sideX;
                verts[v + 1].y = y + forwardY - sideY;
                verts[v + 2].x = x - forwardX + sideX;
                verts[v + 2].y = y - forwardY + sideY;
                verts[v + 3].x = x + forwardX + sideX;
                verts[v + 3].y = y + forwardY + sideY;
            }
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
#endif
