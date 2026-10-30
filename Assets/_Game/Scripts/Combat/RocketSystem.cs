using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Ракеты (ТЗ §3.4, D-034): SoA, один динамический меш. Позиция считается формулой параболы от точки вылета,
    /// поэтому не зависит от шага кадра и ракеты одного залпа идут по одной дуге. Попадание ищется по отрезку
    /// между положениями носа в соседних кадрах, туннелирования нет.
    /// </summary>
    public sealed class RocketSystem : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int SideCount = 2;
        private const byte NoSide = 255;
        private const float Epsilon = 0.0001f;
        private const float BoundsPadding = 4f;

        private GameConfig config;
        private DestructibleMask mask;
        private GreyboxLevel level;
        private ExplosionSystem explosions;
        private SmokeTrails smoke;
        private LevelFx fx;
        private IndexPool pool;

        private float[] originX;
        private float[] originY;
        private float[] velX;
        private float[] velY;
        private float[] age;
        private float[] noseX;
        private float[] noseY;
        private float[] facing;
        private float[] smokeTimer;
        private byte[] side;
        private int[] countBySide;
        private float[] headBySide;
        private Vector3[] vertices;
        private Color32[] colors;
        private Color32 playerColor;
        private Color32 enemyColor;
        private bool dirty;
        private bool colorsDirty;

        public Mesh Mesh { get; private set; }
        public int ActiveCount { get { return pool.ActiveCount; } }
        public int Capacity { get { return pool.Capacity; } }
        /// <summary>Лимит одновременно летящих ракет. Стресс-режим поднимает его до размера буфера.</summary>
        public int Budget { get; set; }

        public void Init(GameConfig gameConfig, DestructibleMask terrainMask, GreyboxLevel greyboxLevel, ExplosionSystem explosionSystem, SmokeTrails smokeTrails, LevelFx levelFx)
        {
            config = gameConfig;
            mask = terrainMask;
            level = greyboxLevel;
            explosions = explosionSystem;
            smoke = smokeTrails;
            fx = levelFx;

            int capacity = Mathf.Max(config.maxActiveRockets, config.stressRocketCount);
            Budget = config.maxActiveRockets;
            pool = new IndexPool(capacity);
            originX = new float[capacity];
            originY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            age = new float[capacity];
            noseX = new float[capacity];
            noseY = new float[capacity];
            facing = new float[capacity];
            smokeTimer = new float[capacity];
            side = new byte[capacity];
            for (int i = 0; i < capacity; i++)
            {
                side[i] = NoSide;
            }
            countBySide = new int[SideCount];
            headBySide = new float[SideCount];
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
            Mesh.name = "Rockets";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            float half = config.levelWidth + BoundsPadding;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(half * 2f, half * 2f, 1f));
        }

        public void ApplyPalette(PaletteSet palette)
        {
            playerColor = palette.rocketPlayer;
            enemyColor = palette.rocketEnemy;
            for (int i = 0; i < side.Length; i++)
            {
                if (side[i] != NoSide)
                {
                    Paint(i);
                }
            }
            Mesh.colors32 = colors;
        }

        /// <summary>Ракет стороны в полёте.</summary>
        public int Count(Side owner)
        {
            return countBySide[(int)owner];
        }

        /// <summary>X самой дальней ракеты стороны: голова потока для камеры. Имеет смысл, пока Count больше 0.</summary>
        public float HeadX(Side owner)
        {
            return headBySide[(int)owner];
        }

        /// <summary>Выпускает ракету из точки (x, y). startAge — время, на которое она уже опоздала внутри кадра. False, если бюджет исчерпан.</summary>
        public bool Spawn(float x, float y, float speedX, float speedY, Side owner, float startAge)
        {
            if (pool.ActiveCount >= Budget)
            {
                return false;
            }
            int i = pool.Alloc();
            if (i < 0)
            {
                return false;
            }
            originX[i] = x;
            originY[i] = y;
            velX[i] = speedX;
            velY[i] = speedY;
            age[i] = startAge;
            noseX[i] = x;
            noseY[i] = y;
            facing[i] = speedX >= 0f ? 1f : -1f;
            smokeTimer[i] = 0f;
            byte ownerIndex = (byte)owner;
            if (side[i] != ownerIndex)
            {
                side[i] = ownerIndex;
                Paint(i);
                colorsDirty = true;
            }
            countBySide[ownerIndex]++;
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (pool.IsUsed(i))
                {
                    Remove(i);
                }
            }
            Mesh.vertices = vertices;
            dirty = false;
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0 && !dirty)
            {
                return;
            }
            float gravity = config.gravity;
            float halfLength = config.rocketLength * 0.5f;
            float halfWidth = config.rocketWidth * 0.5f;
            float waterLevel = config.waterLevel;
            float levelEdge = config.levelWidth;
            float step = config.rocketCollisionStep;
            float smokeInterval = config.smokeInterval;
            int lodStep = 1 + pool.ActiveCount / config.smokeLodRocketThreshold;
            float smokeScale = 1f + (lodStep - 1) * config.smokeLodGrowth;
            float playerHead = float.NegativeInfinity;
            float enemyHead = float.PositiveInfinity;
            Vector3[] verts = vertices;

            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                float t = age[i] + dt;
                age[i] = t;
                float x = originX[i] + velX[i] * t;
                float y = originY[i] + velY[i] * t - 0.5f * gravity * t * t;
                float dirX = velX[i];
                float dirY = velY[i] - gravity * t;
                float dirLength = Mathf.Sqrt(dirX * dirX + dirY * dirY) + Epsilon;
                dirX /= dirLength;
                dirY /= dirLength;

                float fromX = noseX[i];
                float fromY = noseY[i];
                float toX = x + dirX * halfLength;
                float toY = y + dirY * halfLength;
                if (ResolveHit(i, fromX, fromY, toX, toY, step, waterLevel) || Mathf.Abs(x) > levelEdge)
                {
                    Remove(i);
                    continue;
                }
                noseX[i] = toX;
                noseY[i] = toY;

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

                if (side[i] == (byte)Side.Player)
                {
                    playerHead = x > playerHead ? x : playerHead;
                }
                else
                {
                    enemyHead = x < enemyHead ? x : enemyHead;
                }

                float timer = smokeTimer[i] - dt;
                if (timer <= 0f)
                {
                    timer += smokeInterval;
                    if (i % lodStep == 0)
                    {
                        smoke.Emit(x - forwardX, y - forwardY, smokeScale);
                    }
                }
                smokeTimer[i] = timer;
            }

            headBySide[(int)Side.Player] = playerHead;
            headBySide[(int)Side.Enemy] = enemyHead;
            Mesh.vertices = vertices;
            dirty = pool.ActiveCount > 0;
            if (colorsDirty)
            {
                Mesh.colors32 = colors;
                colorsDirty = false;
            }
        }

        /// <summary>Ближайшее препятствие на отрезке носа: земля, вода или чужой замок. True, если ракета закончила полёт.</summary>
        private bool ResolveHit(int i, float fromX, float fromY, float toX, float toY, float step, float waterLevel)
        {
            float solid = mask.FirstSolid(fromX, fromY, toX, toY, step);
            float water = toY < waterLevel ? (fromY - waterLevel) / (fromY - toY) : -1f;
            if (solid >= 0f && (water < 0f || solid <= water))
            {
                explosions.Explode(fromX + (toX - fromX) * solid, fromY + (toY - fromY) * solid, true);
                return true;
            }
            if (water >= 0f)
            {
                fx.Splash(fromX + (toX - fromX) * water, waterLevel, config.rocketSplashWidth);
                return true;
            }
            Side target = side[i] == (byte)Side.Player ? Side.Enemy : Side.Player;
            if (level.CastleContains(target, toX, toY))
            {
                explosions.Explode(toX, toY, false);
                return true;
            }
            return false;
        }

        private void Remove(int i)
        {
            int v = i * VerticesPerQuad;
            for (int k = 0; k < VerticesPerQuad; k++)
            {
                vertices[v + k].x = 0f;
                vertices[v + k].y = 0f;
            }
            countBySide[side[i]]--;
            pool.Free(i);
            dirty = true;
        }

        private void Paint(int i)
        {
            Color32 color = side[i] == (byte)Side.Player ? playerColor : enemyColor;
            int v = i * VerticesPerQuad;
            colors[v] = color;
            colors[v + 1] = color;
            colors[v + 2] = color;
            colors[v + 3] = color;
        }
    }
}
