using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    /// <summary>
    /// Платформы и гусеницы обоих замков (ТЗ §3.7): спрайты атласа в одном меше, вершины считаются здесь. Форма строится
    /// при смене темы, в кадре пересчитываются только позиции и только когда поза менялась.
    /// Скрытая трещина платформы схлопнута в точку.
    /// </summary>
    public sealed class UndercarriageView : ITickable
    {
        private const int SideCount = 2;
        private const uint CrackSeed = 4177u;
        private const float CrackInsetFraction = 0.8f;

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly Rng crackRng = new Rng(CrackSeed);
        private GameConfig config;
        private CastleSystem castles;

        // Границы частей меша: [сторона], для трещин и гусениц ещё номер части.
        private int[] platformStart;
        private int[] platformEnd;
        private int[][] crackStart;
        private int[][] crackEnd;
        private int[][] trackStart;
        private int[][] trackEnd;
        private Vector3[] baseVertices;
        private Vector3[] vertices;

        public Mesh Mesh { get; private set; }

        public void Init(GameConfig gameConfig, CastleSystem castleSystem)
        {
            config = gameConfig;
            castles = castleSystem;
            platformStart = new int[SideCount];
            platformEnd = new int[SideCount];
            crackStart = new int[SideCount][];
            crackEnd = new int[SideCount][];
            trackStart = new int[SideCount][];
            trackEnd = new int[SideCount][];
            for (int s = 0; s < SideCount; s++)
            {
                crackStart[s] = new int[CastleSystem.MaxPlatformCracks];
                crackEnd[s] = new int[CastleSystem.MaxPlatformCracks];
                trackStart[s] = new int[CastleBody.TrackCount];
                trackEnd[s] = new int[CastleBody.TrackCount];
            }
            Mesh = new Mesh();
            Mesh.name = "Undercarriage";
            Mesh.MarkDynamic();
        }

        public void ApplyPalette(PaletteSet palette)
        {
            builder.Clear();
            crackRng.Reset(CrackSeed);
            for (int s = 0; s < SideCount; s++)
            {
                CastleBody body = castles.Body((Side)s);
                for (int t = 0; t < CastleBody.TrackCount; t++)
                {
                    trackStart[s][t] = builder.VertexCount;
                    builder.AddSprite(ArtSprites.Track, 0f, 0f, 1f, Color.white);
                    trackEnd[s][t] = builder.VertexCount;
                }
                platformStart[s] = builder.VertexCount;
                builder.AddSprite(s == (int)Side.Player ? ArtSprites.PlatformBlue : ArtSprites.PlatformRed, 0f, 0f, 1f, Color.white);
                for (int k = 0; k < CastleSystem.MaxPlatformCracks; k++)
                {
                    crackStart[s][k] = builder.VertexCount;
                    float place = (k + 0.5f) / CastleSystem.MaxPlatformCracks * 2f - 1f;
                    CastleShapes.AddPlatformCrack(builder, palette, crackRng, place * body.PlatformHalf * CrackInsetFraction, config.platformHeight);
                    crackEnd[s][k] = builder.VertexCount;
                }
                platformEnd[s] = builder.VertexCount;
            }
            builder.Apply(Mesh);
            if (baseVertices == null)
            {
                baseVertices = new Vector3[builder.VertexCount];
                vertices = new Vector3[builder.VertexCount];
            }
            builder.CopyVertices(baseVertices);
            builder.CopyVertices(vertices);
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(config.levelWidth * 2f, config.levelWidth * 2f, 1f));
            castles.UndercarriageDirty = true;
        }

        public void Tick(float dt)
        {
            if (!castles.UndercarriageDirty)
            {
                return;
            }
            castles.UndercarriageDirty = false;
            for (int s = 0; s < SideCount; s++)
            {
                WriteSide(s);
            }
            Mesh.vertices = vertices;
        }

        private void WriteSide(int s)
        {
            Side side = (Side)s;
            CastleBody body = castles.Body(side);
            if (body.State == CastleState.Gone)
            {
                Collapse(trackStart[s][0], platformEnd[s], body.PivotX, body.PivotY);
                return;
            }
            for (int t = 0; t < CastleBody.TrackCount; t++)
            {
                float angle = body.TrackAngle(t);
                Place(trackStart[s][t], trackEnd[s][t], body.TrackX(t), body.TrackY(t), Mathf.Cos(angle), Mathf.Sin(angle));
            }
            Place(platformStart[s], platformEnd[s], body.PivotX, body.PivotY, body.Cos, body.Sin);
            int visible = castles.PlatformCracks(side);
            for (int k = visible; k < CastleSystem.MaxPlatformCracks; k++)
            {
                Collapse(crackStart[s][k], crackEnd[s][k], body.PivotX, body.PivotY);
            }
        }

        private void Place(int first, int last, float x, float y, float cos, float sin)
        {
            Vector3[] source = baseVertices;
            Vector3[] target = vertices;
            for (int v = first; v < last; v++)
            {
                target[v].x = x + source[v].x * cos - source[v].y * sin;
                target[v].y = y + source[v].x * sin + source[v].y * cos;
            }
        }

        private void Collapse(int first, int last, float x, float y)
        {
            Vector3[] target = vertices;
            for (int v = first; v < last; v++)
            {
                target[v].x = x;
                target[v].y = y;
            }
        }
    }
}
