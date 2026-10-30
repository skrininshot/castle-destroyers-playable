using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Greybox-пушки: по прямоугольнику-стволу на каждое крепление, один динамический меш.
    /// Погибшая пушка схлопывается в точку. Настоящие трёхстволки и отрыв появятся на этапе 5.
    /// </summary>
    public sealed class CannonMarkers : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float BarrelLength = 1f;
        private const float BarrelThickness = 0.35f;
        private const float BoundsPadding = 4f;

        private GreyboxLevel level;
        private MatchState match;
        private int playerCount;
        private int enemyCount;
        private Vector3[] vertices;
        private Color32[] colors;
        private bool dirty;

        public Mesh Mesh { get; private set; }

        public void Init(GreyboxLevel greyboxLevel, MatchState matchState, GameConfig config)
        {
            level = greyboxLevel;
            match = matchState;
            playerCount = level.PlayerMountX.Length;
            enemyCount = level.EnemyMountX.Length;

            int total = playerCount + enemyCount;
            vertices = new Vector3[total * VerticesPerQuad];
            colors = new Color32[total * VerticesPerQuad];
            int[] triangles = new int[total * IndicesPerQuad];
            for (int i = 0; i < total; i++)
            {
                int v = i * VerticesPerQuad;
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 2;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 2;
                triangles[t + 5] = v + 3;
            }

            Mesh = new Mesh();
            Mesh.name = "CannonMarkers";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            float half = config.levelWidth + BoundsPadding;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(half * 2f, half * 2f, 1f));

            match.Changed += MarkDirty;
            dirty = true;
        }

        /// <summary>Перекрашивает стволы под новую палитру.</summary>
        public void ApplyPalette(PaletteSet palette)
        {
            FillColors(0, playerCount, palette.cannonPlayer);
            FillColors(playerCount, enemyCount, palette.cannonEnemy);
            Mesh.colors32 = colors;
            dirty = true;
        }

        public void Tick(float dt)
        {
            if (!dirty)
            {
                return;
            }
            dirty = false;
            WriteSide(0, playerCount, level.PlayerMountX, level.PlayerMountY, match.PlayerAlive, 1f);
            WriteSide(playerCount, enemyCount, level.EnemyMountX, level.EnemyMountY, match.EnemyAlive, -1f);
            Mesh.vertices = vertices;
        }

        private void MarkDirty()
        {
            dirty = true;
        }

        private void FillColors(int firstQuad, int count, Color32 color)
        {
            int start = firstQuad * VerticesPerQuad;
            int end = (firstQuad + count) * VerticesPerQuad;
            for (int v = start; v < end; v++)
            {
                colors[v] = color;
            }
        }

        /// <summary>Первые alive креплений рисуются стволом, остальные схлопываются.</summary>
        private void WriteSide(int firstQuad, int count, float[] mountX, float[] mountY, int alive, float facing)
        {
            float halfThickness = BarrelThickness * 0.5f;
            for (int i = 0; i < count; i++)
            {
                int v = (firstQuad + i) * VerticesPerQuad;
                float x0 = mountX[i];
                float y = mountY[i];
                float x1 = x0 + facing * BarrelLength;
                float halfH = i < alive ? halfThickness : 0f;
                if (i >= alive)
                {
                    x1 = x0;
                }
                vertices[v].x = x0;
                vertices[v].y = y - halfH;
                vertices[v + 1].x = x1;
                vertices[v + 1].y = y - halfH;
                vertices[v + 2].x = x1;
                vertices[v + 2].y = y + halfH;
                vertices[v + 3].x = x0;
                vertices[v + 3].y = y + halfH;
            }
        }
    }
}
