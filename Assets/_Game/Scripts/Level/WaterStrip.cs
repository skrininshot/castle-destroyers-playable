using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>Светлая волнистая кромка воды (ТЗ §3.8): лента с общими вершинами, каждый кадр двигается только верхний край.</summary>
    public sealed class WaterStrip : ITickable
    {
        private const int VerticesPerColumn = 2;
        private const float BoundsPadding = 4f;

        private GameConfig config;
        private Vector3[] vertices;
        private Color32[] colors;
        private float time;
        private int columns;

        public Mesh Mesh { get; private set; }

        public void Init(GameConfig gameConfig)
        {
            config = gameConfig;
            float extent = config.levelWidth;
            float step = config.waterWaveStep;
            columns = Mathf.CeilToInt(extent * 2f / step) + 1;
            vertices = new Vector3[columns * VerticesPerColumn];
            colors = new Color32[vertices.Length];
            int[] triangles = new int[(columns - 1) * 6];

            float bottom = config.waterLevel - config.waterRimThickness;
            for (int i = 0; i < columns; i++)
            {
                float x = -extent + i * step;
                int v = i * VerticesPerColumn;
                vertices[v].x = x;
                vertices[v].y = bottom;
                vertices[v + 1].x = x;
            }
            for (int i = 0; i < columns - 1; i++)
            {
                int v = i * VerticesPerColumn;
                int t = i * 6;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 1;
                triangles[t + 5] = v + 3;
            }

            Mesh = new Mesh();
            Mesh.name = "WaterStrip";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            float size = (extent + BoundsPadding) * 2f;
            Mesh.bounds = new Bounds(new Vector3(0f, config.waterLevel, 0f), new Vector3(size, BoundsPadding, 1f));
            UpdateWave();
        }

        public void ApplyPalette(PaletteSet palette)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = palette.waterRim;
            }
            Mesh.colors32 = colors;
        }

        public void Tick(float dt)
        {
            time += dt;
            UpdateWave();
        }

        /// <summary>Высота волны в точке x в текущий момент, относительно уровня воды.</summary>
        public float WaveAt(float x)
        {
            return config.waterWaveAmplitude * Mathf.Sin(x * config.waterWaveFrequency + time * config.waterWaveSpeed);
        }

        private void UpdateWave()
        {
            float level = config.waterLevel;
            Vector3[] verts = vertices;
            for (int i = 0; i < columns; i++)
            {
                int top = i * VerticesPerColumn + 1;
                verts[top].y = level + WaveAt(verts[top].x);
            }
            Mesh.vertices = vertices;
        }
    }
}
