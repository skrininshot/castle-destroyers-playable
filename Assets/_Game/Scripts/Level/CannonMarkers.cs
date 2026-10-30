using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Greybox-вид пушек: трёхствольный блок на каждое крепление, один динамический меш. Поворот и отдачу берёт из CannonSystem.
    /// Погибшая пушка схлопывается в точку. Настоящие трёхстволки и отрыв появятся на этапе 5.
    /// </summary>
    public sealed class CannonMarkers : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int QuadsPerCannon = 1 + CannonSystem.BarrelCount;
        private const float BodyLength = 0.5f;
        private const float BodyHeight = 0.66f;
        private const float BarrelThickness = 0.16f;
        private const float BoundsPadding = 4f;

        private GreyboxLevel level;
        private MatchState match;
        private CannonSystem cannons;
        private int playerCount;
        private int enemyCount;
        private Vector3[] vertices;
        private Color32[] colors;
        private bool dirty;

        public Mesh Mesh { get; private set; }

        public void Init(GreyboxLevel greyboxLevel, MatchState matchState, CannonSystem cannonSystem, GameConfig config)
        {
            level = greyboxLevel;
            match = matchState;
            cannons = cannonSystem;
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
            if (!dirty && !cannons.Moved)
            {
                return;
            }
            dirty = false;
            WriteSide(Side.Player, 0, playerCount, level.PlayerMountX, level.PlayerMountY, match.PlayerAlive, 1f);
            WriteSide(Side.Enemy, playerCount, enemyCount, level.EnemyMountX, level.EnemyMountY, match.EnemyAlive, -1f);
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
        private void WriteSide(Side side, int firstCannon, int count, float[] mountX, float[] mountY, int alive, float facing)
        {
            float radians = cannons.AngleDeg(side) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            float alongX = facing * cos;
            float alongY = sin;
            float acrossX = -facing * sin;
            float acrossY = cos;

            for (int i = 0; i < count; i++)
            {
                int cannon = firstCannon + i;
                int quad = cannon * QuadsPerCannon;
                float x = mountX[i];
                float y = mountY[i];
                if (i >= alive)
                {
                    for (int q = 0; q < QuadsPerCannon; q++)
                    {
                        WriteQuad(quad + q, x, y, 0f, 0f, 0f, 0f, alongX, alongY, acrossX, acrossY);
                    }
                    continue;
                }
                float halfBody = BodyHeight * 0.5f;
                float halfBarrel = BarrelThickness * 0.5f;
                WriteQuad(quad, x, y, 0f, -halfBody, BodyLength, halfBody, alongX, alongY, acrossX, acrossY);
                for (int barrel = 0; barrel < CannonSystem.BarrelCount; barrel++)
                {
                    float offset = CannonSystem.BarrelOffset(barrel);
                    float back = cannons.Recoil(cannon, barrel);
                    WriteQuad(quad + 1 + barrel, x, y, -back, offset - halfBarrel, CannonSystem.BarrelLength - back, offset + halfBarrel,
                        alongX, alongY, acrossX, acrossY);
                }
            }
        }

        /// <summary>Прямоугольник в осях пушки: along вдоль ствола, across поперёк.</summary>
        private void WriteQuad(int quad, float x, float y, float along0, float across0, float along1, float across1,
            float alongX, float alongY, float acrossX, float acrossY)
        {
            int v = quad * VerticesPerQuad;
            vertices[v].x = x + along0 * alongX + across0 * acrossX;
            vertices[v].y = y + along0 * alongY + across0 * acrossY;
            vertices[v + 1].x = x + along1 * alongX + across0 * acrossX;
            vertices[v + 1].y = y + along1 * alongY + across0 * acrossY;
            vertices[v + 2].x = x + along1 * alongX + across1 * acrossX;
            vertices[v + 2].y = y + along1 * alongY + across1 * acrossY;
            vertices[v + 3].x = x + along0 * alongX + across1 * acrossX;
            vertices[v + 3].y = y + along0 * alongY + across1 * acrossY;
        }
    }
}
