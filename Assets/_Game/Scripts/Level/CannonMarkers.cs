using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Greybox-пушки: трёхствольный блок на каждое крепление, один динамический меш.
    /// Погибшая пушка схлопывается в точку. Настоящие трёхстволки и отрыв появятся на этапе 5.
    /// </summary>
    public sealed class CannonMarkers : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int QuadsPerCannon = 4;
        private const float BodyLength = 0.5f;
        private const float BodyHeight = 0.66f;
        private const float BarrelLength = 1.25f;
        private const float BarrelThickness = 0.16f;
        private const float BarrelPitch = 0.21f;
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

            int total = (playerCount + enemyCount) * QuadsPerCannon;
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

        /// <summary>Перекрашивает пушки под новую палитру.</summary>
        public void ApplyPalette(PaletteSet palette)
        {
            FillColors(0, playerCount, palette.cannonPlayer, palette.cannonPlayerShade);
            FillColors(playerCount, enemyCount, palette.cannonEnemy, palette.cannonEnemyShade);
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

        /// <summary>Первый квад пушки — казённик теневым тоном, остальные — стволы основным.</summary>
        private void FillColors(int firstCannon, int count, Color32 barrel, Color32 body)
        {
            for (int cannon = firstCannon; cannon < firstCannon + count; cannon++)
            {
                int start = cannon * QuadsPerCannon * VerticesPerQuad;
                for (int v = 0; v < QuadsPerCannon * VerticesPerQuad; v++)
                {
                    colors[start + v] = v < VerticesPerQuad ? body : barrel;
                }
            }
        }

        /// <summary>Первые alive креплений рисуются пушкой, остальные схлопываются.</summary>
        private void WriteSide(int firstCannon, int count, float[] mountX, float[] mountY, int alive, float facing)
        {
            for (int i = 0; i < count; i++)
            {
                int quad = (firstCannon + i) * QuadsPerCannon;
                float x = mountX[i];
                float y = mountY[i];
                if (i >= alive)
                {
                    for (int q = 0; q < QuadsPerCannon; q++)
                    {
                        WriteQuad(quad + q, x, y, x, y);
                    }
                    continue;
                }
                float halfBody = BodyHeight * 0.5f;
                float halfBarrel = BarrelThickness * 0.5f;
                float muzzle = x + facing * BarrelLength;
                WriteQuad(quad, x, y - halfBody, x + facing * BodyLength, y + halfBody);
                WriteQuad(quad + 1, x, y - BarrelPitch - halfBarrel, muzzle, y - BarrelPitch + halfBarrel);
                WriteQuad(quad + 2, x, y - halfBarrel, muzzle, y + halfBarrel);
                WriteQuad(quad + 3, x, y + BarrelPitch - halfBarrel, muzzle, y + BarrelPitch + halfBarrel);
            }
        }

        private void WriteQuad(int quad, float x0, float y0, float x1, float y1)
        {
            int v = quad * VerticesPerQuad;
            vertices[v].x = x0;
            vertices[v].y = y0;
            vertices[v + 1].x = x1;
            vertices[v + 1].y = y0;
            vertices[v + 2].x = x1;
            vertices[v + 2].y = y1;
            vertices[v + 3].x = x0;
            vertices[v + 3].y = y1;
        }
    }
}
