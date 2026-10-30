#if GAME_DEBUG
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.DebugTools
{
    /// <summary>
    /// Чит «спавн 1000 ракет»: SoA-квады в одном динамическом меше, чистая баллистика, слоты через IndexPool.
    /// Ракеты вылетают из пушек по очереди и идут по одной дуге: случайности и волны нет (D-031, D-034).
    /// Только для нагрузочной проверки каркаса. Настоящий RocketSystem появится на этапе 3.
    /// </summary>
    public sealed class DebugRocketStress : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float Epsilon = 0.0001f;
        private const float BoundsPadding = 4f;

        private GameConfig config;
        private GreyboxLevel level;
        private IndexPool pool;

        private float[] originX;
        private float[] originY;
        private float[] velX;
        private float[] velY;
        private float[] facing;
        private float[] age;
        private Vector3[] vertices;
        private Color32[] colors;
        private bool dirty;

        public Mesh Mesh { get; private set; }
        public int ActiveCount { get { return pool.ActiveCount; } }

        public void Init(GameConfig gameConfig, GreyboxLevel greyboxLevel)
        {
            config = gameConfig;
            level = greyboxLevel;

            int capacity = config.stressRocketCount;
            pool = new IndexPool(capacity);
            originX = new float[capacity];
            originY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            facing = new float[capacity];
            age = new float[capacity];
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
            Mesh.name = "DebugRocketStress";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            float half = config.levelWidth + BoundsPadding;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(half * 2f, half * 2f, 1f));
        }

        /// <summary>Выпускает до count ракет из живых пушек стороны по очереди; сверх лимита буфера не создаёт.</summary>
        public int Spawn(int count, Side side, PaletteSet palette, int aliveCannons)
        {
            float direction = side == Side.Player ? 1f : -1f;
            float[] mountX = side == Side.Player ? level.PlayerMountX : level.EnemyMountX;
            float[] mountY = side == Side.Player ? level.PlayerMountY : level.EnemyMountY;
            int mounts = aliveCannons < 1 ? 1 : aliveCannons;
            Color32 color = side == Side.Player ? palette.rocketPlayer : palette.rocketEnemy;
            float angle = config.stressRocketAngleDeg * Mathf.Deg2Rad;
            float speedX = direction * config.stressRocketSpeed * Mathf.Cos(angle);
            float speedY = config.stressRocketSpeed * Mathf.Sin(angle);
            // Как настоящий залп: каждая пушка стреляет с интервалом rocketInterval, пушки стреляют вперемешку.
            float interval = config.rocketInterval / mounts;

            int spawned = 0;
            for (int n = 0; n < count; n++)
            {
                int i = pool.Alloc();
                if (i < 0)
                {
                    break;
                }
                int mount = n % mounts;
                originX[i] = mountX[mount];
                originY[i] = mountY[mount];
                velX[i] = speedX;
                velY[i] = speedY;
                facing[i] = direction;
                age[i] = -n * interval;

                int v = i * VerticesPerQuad;
                colors[v] = color;
                colors[v + 1] = color;
                colors[v + 2] = color;
                colors[v + 3] = color;
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
            Mesh.vertices = vertices;
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0 && !dirty)
            {
                return;
            }
            float gravity = config.gravity;
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
                if (t < 0f)
                {
                    continue;
                }

                float x = originX[i] + velX[i] * t;
                float y = originY[i] + velY[i] * t - 0.5f * gravity * t * t;
                if (t >= lifetime || y < waterLevel)
                {
                    Collapse(i);
                    pool.Free(i);
                    continue;
                }

                float dirX = velX[i];
                float dirY = velY[i] - gravity * t;
                float dirLength = Mathf.Sqrt(dirX * dirX + dirY * dirY) + Epsilon;
                dirX /= dirLength;
                dirY /= dirLength;

                float forwardX = dirX * halfLength;
                float forwardY = dirY * halfLength;
                // Верх картинки всегда смотрит вверх, поэтому у летящих влево ось инвертирована.
                float sideX = -dirY * halfWidth * facing[i];
                float sideY = dirX * halfWidth * facing[i];

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
