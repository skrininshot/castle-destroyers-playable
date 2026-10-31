using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Вид пушек: три ствола и казённик спрайтами атласа в одном динамическом меше. Положение, поворот и отдачу берёт из
    /// CannonSystem. Выстрел, отрыв и подскок дают сквош (ТЗ §4.1). Падающая пушка кувыркается, ушедшая в воду схлопывается в точку.
    /// </summary>
    public sealed class CannonView : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int QuadsPerCannon = 1 + CannonSystem.BarrelCount;
        private const int BodyQuad = CannonSystem.BarrelCount;
        private const float BoundsPadding = 4f;

        private GameConfig config;
        private MatchState match;
        private CannonSystem cannons;
        private int playerCount;
        private int enemyCount;
        private Vector3[] vertices;
        private bool dirty;

        public Mesh Mesh { get; private set; }

        public void Init(MatchState matchState, CannonSystem cannonSystem, GameConfig gameConfig)
        {
            config = gameConfig;
            match = matchState;
            cannons = cannonSystem;
            playerCount = cannons.CountOf(Side.Player);
            enemyCount = cannons.CountOf(Side.Enemy);

            int total = (playerCount + enemyCount) * QuadsPerCannon;
            vertices = new Vector3[total * VerticesPerQuad];
            Color32[] colors = new Color32[vertices.Length];
            Vector2[] uv = new Vector2[vertices.Length];
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
            for (int v = 0; v < colors.Length; v++)
            {
                colors[v] = Color.white;
            }
            FillUv(uv, 0, playerCount, ArtSprites.CannonBarrelPlayer, ArtSprites.CannonBodyPlayer);
            FillUv(uv, playerCount, enemyCount, ArtSprites.CannonBarrelEnemy, ArtSprites.CannonBodyEnemy);

            Mesh = new Mesh();
            Mesh.name = "Cannons";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.colors32 = colors;
            Mesh.uv = uv;
            Mesh.triangles = triangles;
            float half = config.levelWidth + BoundsPadding;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(half * 2f, half * 2f, 1f));

            match.Changed += MarkDirty;
            dirty = true;
        }

        public void Tick(float dt)
        {
            if (!dirty && !cannons.Moved)
            {
                return;
            }
            dirty = false;
            WriteSide(Side.Player, 0, playerCount, 1f);
            WriteSide(Side.Enemy, playerCount, enemyCount, -1f);
            Mesh.vertices = vertices;
        }

        private void MarkDirty()
        {
            dirty = true;
        }

        /// <summary>Сначала стволы, поверх них казённик.</summary>
        private static void FillUv(Vector2[] uv, int firstCannon, int count, int barrel, int body)
        {
            for (int cannon = firstCannon; cannon < firstCannon + count; cannon++)
            {
                int quad = cannon * QuadsPerCannon;
                for (int q = 0; q < BodyQuad; q++)
                {
                    SetQuadUv(uv, (quad + q) * VerticesPerQuad, barrel);
                }
                SetQuadUv(uv, (quad + BodyQuad) * VerticesPerQuad, body);
            }
        }

        private static void SetQuadUv(Vector2[] uv, int v, int sprite)
        {
            int r = sprite * VerticesPerQuad;
            float[] source = ArtSprites.Uv;
            uv[v].x = source[r];
            uv[v].y = source[r + 1];
            uv[v + 1].x = source[r + 2];
            uv[v + 1].y = source[r + 1];
            uv[v + 2].x = source[r + 2];
            uv[v + 2].y = source[r + 3];
            uv[v + 3].x = source[r];
            uv[v + 3].y = source[r + 3];
        }

        /// <summary>Живые и падающие пушки рисуются, ушедшие в воду схлопываются.</summary>
        private void WriteSide(Side side, int firstCannon, int count, float facing)
        {
            float radians = cannons.AngleDeg(side) * Mathf.Deg2Rad;
            int barrelSprite = side == Side.Player ? ArtSprites.CannonBarrelPlayer : ArtSprites.CannonBarrelEnemy;
            int bodySprite = side == Side.Player ? ArtSprites.CannonBodyPlayer : ArtSprites.CannonBodyEnemy;
            float[] rects = ArtSprites.Rect;
            int barrelRect = barrelSprite * VerticesPerQuad;
            int bodyRect = bodySprite * VerticesPerQuad;
            for (int i = 0; i < count; i++)
            {
                int cannon = firstCannon + i;
                int quad = cannon * QuadsPerCannon;
                float x = cannons.WorldX(cannon);
                float y = cannons.WorldY(cannon) + cannons.JumpY(cannon);
                bool falling = cannons.IsFalling(cannon);
                float angle = falling ? radians + cannons.FallAngle(cannon) : radians;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                float alongX = facing * cos;
                float alongY = sin;
                float acrossX = -facing * sin;
                float acrossY = cos;
                if (!falling && !cannons.IsAlive(cannon))
                {
                    for (int q = 0; q < QuadsPerCannon; q++)
                    {
                        WriteQuad(quad + q, x, y, 0f, 0f, 0f, 0f, alongX, alongY, acrossX, acrossY);
                    }
                    continue;
                }

                float squash = Squash(cannon);
                float thick = 1f + squash;
                float longer = 1f - squash * config.cannonSquashLengthShare;
                for (int barrel = 0; barrel < CannonSystem.BarrelCount; barrel++)
                {
                    float offset = CannonSystem.BarrelOffset(barrel) * thick;
                    float back = cannons.Recoil(cannon, barrel);
                    WriteQuad(quad + barrel, x, y, rects[barrelRect] * longer - back, offset + rects[barrelRect + 1] * thick,
                        rects[barrelRect + 2] * longer - back, offset + rects[barrelRect + 3] * thick, alongX, alongY, acrossX, acrossY);
                }
                WriteQuad(quad + BodyQuad, x, y, rects[bodyRect] * longer, rects[bodyRect + 1] * thick, rects[bodyRect + 2] * longer, rects[bodyRect + 3] * thick,
                    alongX, alongY, acrossX, acrossY);
            }
        }

        /// <summary>Сквош пушки: от отдачи, толчка отрыва и подскока победителя. 0 — форма в покое.</summary>
        private float Squash(int cannon)
        {
            float kick = 0f;
            for (int barrel = 0; barrel < CannonSystem.BarrelCount; barrel++)
            {
                kick = Mathf.Max(kick, cannons.Recoil(cannon, barrel));
            }
            float recoil = kick / config.recoilDistance * config.cannonFireSquash;
            float jump = cannons.JumpY(cannon) / config.victoryJumpHeight;
            return recoil + cannons.DetachKick(cannon) * config.cannonDetachSquash + jump * config.cannonJumpSquash;
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
