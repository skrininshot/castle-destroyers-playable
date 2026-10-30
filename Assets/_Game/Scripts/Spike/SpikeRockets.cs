using UnityEngine;

namespace CastleDestroyers.Spike
{
    /// <summary>Ракеты на SoA: баллистика + синусоида, один динамический меш на всю систему.</summary>
    public sealed class SpikeRockets
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float MinAngleDeg = 45f;
        private const float MaxAngleDeg = 75f;
        private const float Epsilon = 0.0001f;
        private const byte Opaque = 255;

        public int ActiveCount { get; private set; }
        public Mesh Mesh { get; private set; }

        private SpikeConfig config;
        private SpikeRng rng;
        private float startX;
        private float startY;

        private float[] velX;
        private float[] velY;
        private float[] phase;
        private float[] age;
        private Vector3[] vertices;

        public void Init(SpikeConfig spikeConfig, SpikeRng spikeRng)
        {
            config = spikeConfig;
            rng = spikeRng;
            startX = -config.worldSize * 0.5f;
            startY = -config.worldSize * 0.5f;

            int capacity = config.rocketCapacity;
            velX = new float[capacity];
            velY = new float[capacity];
            phase = new float[capacity];
            age = new float[capacity];
            vertices = new Vector3[capacity * VerticesPerQuad];

            Vector2[] uv = new Vector2[vertices.Length];
            Color32[] colors = new Color32[vertices.Length];
            int[] triangles = new int[capacity * IndicesPerQuad];
            Color32 tail = new Color32(255, 150, 40, Opaque);
            Color32 nose = new Color32(70, 130, 255, Opaque);
            for (int i = 0; i < capacity; i++)
            {
                int v = i * VerticesPerQuad;
                uv[v] = new Vector2(0f, 0f);
                uv[v + 1] = new Vector2(1f, 0f);
                uv[v + 2] = new Vector2(0f, 1f);
                uv[v + 3] = new Vector2(1f, 1f);
                colors[v] = tail;
                colors[v + 1] = nose;
                colors[v + 2] = tail;
                colors[v + 3] = nose;
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            Mesh = new Mesh();
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
        }

        /// <summary>Задаёт число живых ракет; лишние квады схлопываются в точку.</summary>
        public void SetActiveCount(int count)
        {
            int clamped = Mathf.Clamp(count, 0, config.rocketCapacity);
            for (int i = ActiveCount; i < clamped; i++)
            {
                Respawn(i);
                age[i] = rng.Range(0f, config.rocketLifetime);
            }
            for (int v = clamped * VerticesPerQuad; v < ActiveCount * VerticesPerQuad; v++)
            {
                vertices[v].x = 0f;
                vertices[v].y = 0f;
            }
            ActiveCount = clamped;
        }

        public void Tick(float dt)
        {
            float gravity = config.gravity;
            float amplitude = config.waveAmplitude;
            float frequency = config.waveFrequency;
            float lifetime = config.rocketLifetime;
            float halfLength = config.rocketLength * 0.5f;
            float halfWidth = config.rocketWidth * 0.5f;
            Vector3[] verts = vertices;

            for (int i = 0; i < ActiveCount; i++)
            {
                float t = age[i] + dt;
                if (t >= lifetime)
                {
                    Respawn(i);
                    t = 0f;
                }
                age[i] = t;

                float tangentX = velX[i];
                float tangentY = velY[i] - gravity * t;
                float tangentLength = Mathf.Sqrt(tangentX * tangentX + tangentY * tangentY) + Epsilon;
                float normalX = -tangentY / tangentLength;
                float normalY = tangentX / tangentLength;

                float wave = frequency * t + phase[i];
                float offset = amplitude * Mathf.Sin(wave);
                float offsetRate = amplitude * frequency * Mathf.Cos(wave);

                float x = startX + velX[i] * t + normalX * offset;
                float y = startY + velY[i] * t - 0.5f * gravity * t * t + normalY * offset;

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
        }

        public void UploadMesh()
        {
            Mesh.vertices = vertices;
        }

        private void Respawn(int i)
        {
            float angle = rng.Range(MinAngleDeg, MaxAngleDeg) * Mathf.Deg2Rad;
            float speed = rng.Range(config.launchSpeedMin, config.launchSpeedMax);
            velX[i] = speed * Mathf.Cos(angle);
            velY[i] = speed * Mathf.Sin(angle);
            phase[i] = i * config.wavePhaseStep;
            age[i] = 0f;
        }
    }
}
