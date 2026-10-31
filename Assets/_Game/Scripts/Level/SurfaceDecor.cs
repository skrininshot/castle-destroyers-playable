using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Fx;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Декор поверхности (ТЗ §3.6, §3.8): ёлки, кусты, камни и надгробия или кактусы стоят в точке контура, а когда материал
    /// под ними исчезает, падают в воду. Спрайты атласа в одном меше, рисуется до земли, поэтому основание скрыто в склоне.
    /// Вершины пересчитываются только у падающих. Расстановка — часть рисунка уровня, поэтому здесь, а не в GameConfig.
    /// </summary>
    public sealed class SurfaceDecor : ITickable
    {
        private const uint DecorSeed = 5309u;
        private const float TreeSink = 0.15f;
        private const float TreeTilt = 0f;
        private const float TreeMinScale = 0.85f;
        private const float TreeMaxScale = 1.35f;
        private const float OtherMinScale = 0.85f;
        private const float OtherMaxScale = 1.25f;
        private const float TreeMaxHeight = 3.2f;
        private const float SlopeProbe = 0.2f;

        private const byte Standing = 0;
        private const byte Falling = 1;
        private const byte Gone = 2;

        // Декор группами у подножий, X в метрах. Сперва ёлки, за ними кусты, камни и маркеры. Выше по склону пусто: силуэт горы чистый.
        private static readonly float[] TreeX =
        {
            -28.9f, -28f, -26.9f, -21.3f, -20.3f, -19.2f, -18.2f, -13.9f, -12.9f,
            12.7f, 13.8f, 18.3f, 19.4f, 20.4f, 21.5f, 26.8f, 28.1f, 29f,
            -27.5f, -19.8f, -13.3f, 13.3f, 19.9f, 27.5f,
            -22.6f, 15f, 23f,
            -25.3f, -16.2f, 16.4f, 25f
        };

        private const int SpruceCount = 18;
        private const int BushCount = 6;
        private const int RockCount = 3;

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly Rng decorRng = new Rng(DecorSeed);
        private GameConfig config;
        private DestructibleMask mask;
        private LevelFx fx;
        private Rng rng;

        private float[] anchorX;
        private float[] anchorY;
        private float[] offsetX;
        private float[] offsetY;
        private float[] velX;
        private float[] velY;
        private float[] angle;
        private float[] spin;
        private byte[] state;
        private byte[] splashed;
        private int[] firstVertex;
        private int[] vertexCount;

        private Vector3[] baseVertices;
        private Vector3[] vertices;
        private int seenRevision;
        private int fallingCount;

        public Mesh Mesh { get; private set; }
        public int TreeCount { get { return TreeX.Length; } }
        public int FallingCount { get { return fallingCount; } }

        public void Init(GameConfig gameConfig, DestructibleMask destructibleMask, LevelFx levelFx, Rng random)
        {
            config = gameConfig;
            mask = destructibleMask;
            fx = levelFx;
            rng = random;

            int count = TreeX.Length;
            anchorX = new float[count];
            anchorY = new float[count];
            offsetX = new float[count];
            offsetY = new float[count];
            velX = new float[count];
            velY = new float[count];
            angle = new float[count];
            spin = new float[count];
            state = new byte[count];
            splashed = new byte[count];
            firstVertex = new int[count];
            vertexCount = new int[count];

            for (int i = 0; i < count; i++)
            {
                anchorX[i] = TreeX[i];
                anchorY[i] = mask.SurfaceHeight(TreeX[i]);
            }
            Mesh = new Mesh();
            Mesh.name = "SurfaceDecor";
            Mesh.MarkDynamic();
            seenRevision = mask.Revision;
        }

        /// <summary>Собирает меш в цветах палитры (старт и смена темы). Упавшие деревья остаются на месте.</summary>
        public void Build(PaletteSet palette)
        {
            builder.Clear();
            decorRng.Reset(DecorSeed);
            for (int i = 0; i < TreeX.Length; i++)
            {
                firstVertex[i] = builder.VertexCount;
                AddItem(i);
                vertexCount[i] = builder.VertexCount - firstVertex[i];
            }
            builder.Apply(Mesh);
            baseVertices = Mesh.vertices;
            vertices = Mesh.vertices;
            for (int i = 0; i < TreeX.Length; i++)
            {
                if (state[i] != Standing)
                {
                    WriteTree(i);
                }
            }
            Mesh.vertices = vertices;
        }

        public void Tick(float dt)
        {
            if (mask.Revision != seenRevision)
            {
                seenRevision = mask.Revision;
                ReleaseUnsupported();
            }
            if (fallingCount == 0)
            {
                return;
            }
            float gravity = config.chunkGravity;
            float water = config.waterLevel;
            for (int i = 0; i < TreeX.Length; i++)
            {
                if (state[i] != Falling)
                {
                    continue;
                }
                velY[i] -= gravity * dt;
                offsetX[i] += velX[i] * dt;
                offsetY[i] += velY[i] * dt;
                angle[i] += spin[i] * dt;
                float y = anchorY[i] + offsetY[i];
                if (splashed[i] == 0 && y < water)
                {
                    splashed[i] = 1;
                    fx.Splash(anchorX[i] + offsetX[i], water, TreeMaxHeight * 0.5f);
                }
                if (y < water - TreeMaxHeight)
                {
                    state[i] = Gone;
                    fallingCount--;
                }
                WriteTree(i);
            }
            Mesh.vertices = vertices;
        }

        /// <summary>Возвращает все деревья на места: рестарт сессии.</summary>
        public void Reset()
        {
            for (int i = 0; i < TreeX.Length; i++)
            {
                state[i] = Standing;
                splashed[i] = 0;
                offsetX[i] = 0f;
                offsetY[i] = 0f;
                velX[i] = 0f;
                velY[i] = 0f;
                angle[i] = 0f;
                WriteTree(i);
            }
            fallingCount = 0;
            seenRevision = mask.Revision;
            Mesh.vertices = vertices;
        }

        private void ReleaseUnsupported()
        {
            float depth = config.decorAnchorDepth;
            for (int i = 0; i < TreeX.Length; i++)
            {
                if (state[i] == Standing && !mask.IsSolidWorld(anchorX[i], anchorY[i] - depth))
                {
                    state[i] = Falling;
                    fallingCount++;
                    velX[i] = rng.Range(-config.chunkMaxDrift, config.chunkMaxDrift);
                    spin[i] = rng.Range(-config.decorMaxSpinDeg, config.decorMaxSpinDeg) * Mathf.Deg2Rad;
                }
            }
        }

        /// <summary>Поворачивает дерево вокруг точки крепления и сдвигает. Для Gone схлопывает его в точку.</summary>
        private void WriteTree(int i)
        {
            int first = firstVertex[i];
            int last = first + vertexCount[i];
            Vector3[] source = baseVertices;
            Vector3[] target = vertices;
            if (state[i] == Gone)
            {
                for (int v = first; v < last; v++)
                {
                    target[v].x = 0f;
                    target[v].y = 0f;
                }
                return;
            }
            float cos = Mathf.Cos(angle[i]);
            float sin = Mathf.Sin(angle[i]);
            float pivotX = anchorX[i];
            float pivotY = anchorY[i];
            float moveX = pivotX + offsetX[i];
            float moveY = pivotY + offsetY[i];
            for (int v = first; v < last; v++)
            {
                float x = source[v].x - pivotX;
                float y = source[v].y - pivotY;
                target[v].x = moveX + x * cos - y * sin;
                target[v].y = moveY + x * sin + y * cos;
            }
        }

        private static int SpriteOf(int i)
        {
            if (i < SpruceCount)
            {
                return ArtSprites.Spruce;
            }
            if (i < SpruceCount + BushCount)
            {
                return ArtSprites.Bush;
            }
            return i < SpruceCount + BushCount + RockCount ? ArtSprites.Rock : ArtSprites.Marker;
        }

        private void AddItem(int i)
        {
            float x = anchorX[i];
            float slope = (mask.SurfaceHeight(x + SlopeProbe) - mask.SurfaceHeight(x - SlopeProbe)) / (SlopeProbe * 2f);
            float length = Mathf.Sqrt(1f + slope * slope);
            // Ось предмета — между вертикалью и нормалью склона.
            float upX = -slope / length * TreeTilt;
            float upY = 1f / length * TreeTilt + (1f - TreeTilt);
            float upLength = Mathf.Sqrt(upX * upX + upY * upY);
            upX /= upLength;
            upY /= upLength;
            bool spruce = i < SpruceCount;
            float scale = spruce ? decorRng.Range(TreeMinScale, TreeMaxScale) : decorRng.Range(OtherMinScale, OtherMaxScale);
            builder.AddSprite(SpriteOf(i), x - upX * TreeSink, anchorY[i] - upY * TreeSink, upY * scale, -upX * scale, upX * scale, upY * scale, Color.white);
        }
    }
}
