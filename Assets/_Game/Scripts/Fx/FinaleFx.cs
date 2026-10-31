using CastleDestroyers.Core;
using CastleDestroyers.Level;
using CastleDestroyers.UI;
using UnityEngine;

namespace CastleDestroyers.Fx
{
    /// <summary>
    /// Экранный слой финала (D-075): затемнение, вращающиеся лучи, круг за замком, ударная волна, конфетти, вспышка. Один динамический
    /// меш на атласе поверх мира и под канвасом. Единицы меша — высота экрана, начало в центре кадра, поэтому от зума камеры ничего не зависит.
    /// Порядок квадов задаёт порядок слоёв. Вне финала рендерер выключен.
    /// </summary>
    public sealed class FinaleFx : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const int RingSegments = 40;
        private const int HaloQuads = RingSegments * 2;
        private const int StarEvery = 3;
        private const float StarScale = 1.9f;
        private const float RibbonRatio = 1.7f;
        private const float BoundsSize = 8f;
        private const float FullTurn = Mathf.PI * 2f;
        private const float ScreenEdge = 0.56f;
        private const float WedgeShare = 0.5f;
        private const float CannonSpreadMin = 0.15f;
        private const float CannonSpreadMax = 1.15f;
        private const float CannonLiftMin = 0.95f;
        private const float CannonLiftMax = 1.55f;
        private const float BurstSpeedMin = 0.25f;
        private const float BurstSpeedMax = 0.75f;
        private const float RainDriftMax = 0.12f;
        private const float RainFallMin = 0.1f;
        private const float SizeFloor = 0.7f;
        private const float SpinMax = 9f;
        private const float FlipSpeedMin = 5f;
        private const float FlipSpeedMax = 14f;

        private GameConfig config;
        private Camera cam;
        private Transform meshTransform;
        private MeshRenderer meshRenderer;
        private Rng rng;

        private int dimQuad;
        private int firstRay;
        private int haloQuad;
        private int firstRing;
        private int firstConfetti;
        private int flashQuad;

        private IndexPool pool;
        private float[] posX;
        private float[] posY;
        private float[] velX;
        private float[] velY;
        private float[] size;
        private float[] angle;
        private float[] spin;
        private float[] flip;
        private float[] flipSpeed;
        private float[] ringCos;
        private float[] ringSin;
        private Color32[] pieceColors;

        private Vector3[] vertices;
        private Color32[] colors;
        private bool colorsDirty;
        private Color32 dimColor;
        private Color32 flashColor;

        private bool backdrop;
        private float backdropTime;
        private float rayAngle;
        private float rayCenterX;
        private float rayCenterY;
        private float haloScale;
        private bool raining;
        private float rainTimer;
        private float flashLeft;
        private float ringAge;
        private float ringWorldX;
        private float ringWorldY;
        private float aspect;

        public Mesh Mesh { get; private set; }
        public int ConfettiCount { get { return pool.ActiveCount; } }
        public bool RingActive { get { return ringAge < config.finaleRingLife; } }
        public bool FlashActive { get { return flashLeft > 0f; } }
        public bool BackdropActive { get { return backdrop; } }
        /// <summary>Есть что рисовать: рендерер включён.</summary>
        public bool Active { get { return backdrop || FlashActive || RingActive || pool.ActiveCount > 0; } }

        public void Init(GameConfig gameConfig, Camera camera, MeshFilter filter, Rng random)
        {
            config = gameConfig;
            cam = camera;
            rng = random;
            meshTransform = filter.transform;
            meshRenderer = filter.GetComponent<MeshRenderer>();

            int capacity = config.finaleConfettiCapacity;
            pool = new IndexPool(capacity);
            posX = new float[capacity];
            posY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            size = new float[capacity];
            angle = new float[capacity];
            spin = new float[capacity];
            flip = new float[capacity];
            flipSpeed = new float[capacity];
            ringCos = new float[RingSegments + 1];
            ringSin = new float[RingSegments + 1];
            for (int i = 0; i <= RingSegments; i++)
            {
                ringCos[i] = Mathf.Cos(FullTurn * i / RingSegments);
                ringSin[i] = Mathf.Sin(FullTurn * i / RingSegments);
            }

            dimQuad = 0;
            firstRay = 1;
            haloQuad = firstRay + config.finaleRays;
            firstRing = haloQuad + HaloQuads;
            firstConfetti = firstRing + RingSegments;
            flashQuad = firstConfetti + capacity;
            int quads = flashQuad + 1;

            vertices = new Vector3[quads * VerticesPerQuad];
            colors = new Color32[vertices.Length];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[quads * IndicesPerQuad];
            for (int q = 0; q < quads; q++)
            {
                int v = q * VerticesPerQuad;
                bool star = q >= firstConfetti && q < flashQuad && (q - firstConfetti) % StarEvery == 0;
                AtlasUv.SetStrip(uv, v, star ? ArtSprites.Spark : ArtSprites.White);
                int t = q * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            Mesh = new Mesh();
            Mesh.name = "Finale";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
            filter.sharedMesh = Mesh;

            haloScale = 1f;
            ringAge = config.finaleRingLife;
            meshRenderer.enabled = false;
        }

        public void ApplyPalette(PaletteSet palette)
        {
            dimColor = palette.hudTrack;
            flashColor = palette.hudText;
            pieceColors = new[] { palette.hudGold, palette.hudText, palette.roofPlayer, palette.rocketEnemy, palette.hudHealth, palette.booster20 };
            FillQuads(firstRay, config.finaleRays, palette.hudPanel);
            FillQuads(haloQuad, RingSegments, palette.hudGold);
            FillQuads(haloQuad + RingSegments, RingSegments, palette.hudPanel);
            FillQuads(firstRing, RingSegments, palette.blastCore);
            colorsDirty = true;
        }

        /// <summary>Открывает задник EndCard. Центр лучей в долях экрана, scale — масштаб круга.</summary>
        public void ShowBackdrop(float viewportX, float viewportY, float scale)
        {
            backdrop = true;
            backdropTime = 0f;
            SetRayCenter(viewportX, viewportY, scale);
        }

        /// <summary>Переставляет лучи на лету: смена ориентации.</summary>
        public void SetRayCenter(float viewportX, float viewportY, float scale)
        {
            rayCenterX = viewportX - 0.5f;
            rayCenterY = viewportY - 0.5f;
            haloScale = scale;
        }

        public void SetRain(bool enabled)
        {
            raining = enabled;
        }

        public void Flash()
        {
            flashLeft = config.finaleFlashTime;
        }

        /// <summary>Ударная волна из точки мира.</summary>
        public void Shockwave(float worldX, float worldY)
        {
            ringWorldX = worldX;
            ringWorldY = worldY;
            ringAge = 0f;
        }

        /// <summary>Две хлопушки из нижних углов экрана.</summary>
        public void ConfettiCannons()
        {
            float halfWidth = cam.aspect * 0.5f;
            for (int i = 0; i < config.finaleCannonPieces; i++)
            {
                float speedX = rng.Range(CannonSpreadMin, CannonSpreadMax) * cam.aspect;
                Spawn(-halfWidth, -0.5f, speedX, rng.Range(CannonLiftMin, CannonLiftMax));
                Spawn(halfWidth, -0.5f, -speedX, rng.Range(CannonLiftMin, CannonLiftMax));
            }
        }

        /// <summary>Круговой хлопок из точки экрана в долях.</summary>
        public void ConfettiBurst(float viewportX, float viewportY)
        {
            float x = (viewportX - 0.5f) * cam.aspect;
            float y = viewportY - 0.5f;
            for (int i = 0; i < config.finaleStarPieces; i++)
            {
                float direction = rng.Range(0f, FullTurn);
                float speed = rng.Range(BurstSpeedMin, BurstSpeedMax);
                Spawn(x, y, Mathf.Cos(direction) * speed, Mathf.Sin(direction) * speed);
            }
        }

        public void Clear()
        {
            backdrop = false;
            raining = false;
            flashLeft = 0f;
            ringAge = config.finaleRingLife;
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (pool.IsUsed(i))
                {
                    pool.Free(i);
                }
            }
            for (int v = 0; v < vertices.Length; v++)
            {
                vertices[v].x = 0f;
                vertices[v].y = 0f;
            }
            Mesh.vertices = vertices;
            meshRenderer.enabled = false;
        }

        public void Tick(float dt)
        {
            if (raining)
            {
                TickRain(dt);
            }
            bool active = Active;
            if (meshRenderer.enabled != active)
            {
                meshRenderer.enabled = active;
            }
            if (!active)
            {
                return;
            }
            aspect = cam.aspect;
            PlaceOnScreen();
            WriteBackdrop(dt);
            WriteRing(dt);
            TickConfetti(dt);
            WriteFlash(dt);
            Mesh.vertices = vertices;
            if (colorsDirty)
            {
                Mesh.colors32 = colors;
                colorsDirty = false;
            }
        }

        private void PlaceOnScreen()
        {
            float height = cam.orthographicSize * 2f;
            Vector3 center = cam.transform.position;
            meshTransform.position = new Vector3(center.x, center.y, 0f);
            meshTransform.localScale = new Vector3(height, height, 1f);
        }

        private void WriteBackdrop(float dt)
        {
            if (!backdrop)
            {
                CollapseQuads(dimQuad, firstRing - dimQuad);
                return;
            }
            backdropTime += dt;
            rayAngle += config.finaleRaySpeed * dt;
            float halfWidth = aspect * ScreenEdge;
            WriteRect(dimQuad, -halfWidth, -ScreenEdge, halfWidth, ScreenEdge);
            Color32 dim = dimColor;
            dim.a = (byte)(Mathf.Clamp01(backdropTime / config.endCardFadeTime) * config.endCardDimAlpha * 255f);
            FillQuads(dimQuad, 1, dim);

            int rays = config.finaleRays;
            float grow = Mathf.Clamp01(backdropTime / config.finaleRayGrowTime);
            float length = config.finaleRayLength * grow * (2f - grow);
            float half = Mathf.PI / rays * WedgeShare;
            for (int i = 0; i < rays; i++)
            {
                float middle = rayAngle + FullTurn * i / rays;
                WriteWedge(firstRay + i, middle - half, middle + half, length);
            }

            float pop = UiEase.OutBack(grow);
            float radius = config.finaleHaloRadius * haloScale * pop * (1f + Mathf.Sin(backdropTime * config.finaleHaloPulseSpeed) * config.finaleHaloPulse);
            WriteDisc(haloQuad, radius + config.finaleHaloRim * haloScale);
            WriteDisc(haloQuad + RingSegments, radius);
        }

        private void WriteRing(float dt)
        {
            if (!RingActive)
            {
                CollapseQuads(firstRing, RingSegments);
                return;
            }
            ringAge += dt;
            float t = Mathf.Clamp01(ringAge / config.finaleRingLife);
            float height = cam.orthographicSize * 2f;
            Vector3 center = cam.transform.position;
            float x = (ringWorldX - center.x) / height;
            float y = (ringWorldY - center.y) / height;
            float outer = config.finaleRingRadius * t * (2f - t);
            float inner = outer - config.finaleRingWidth * (1f - t);
            Vector3[] verts = vertices;
            for (int i = 0; i < RingSegments; i++)
            {
                int v = (firstRing + i) * VerticesPerQuad;
                verts[v].x = x + ringCos[i] * inner;
                verts[v].y = y + ringSin[i] * inner;
                verts[v + 1].x = x + ringCos[i + 1] * inner;
                verts[v + 1].y = y + ringSin[i + 1] * inner;
                verts[v + 2].x = x + ringCos[i] * outer;
                verts[v + 2].y = y + ringSin[i] * outer;
                verts[v + 3].x = x + ringCos[i + 1] * outer;
                verts[v + 3].y = y + ringSin[i + 1] * outer;
            }
        }

        private void WriteFlash(float dt)
        {
            if (flashLeft <= 0f)
            {
                CollapseQuads(flashQuad, 1);
                return;
            }
            flashLeft -= dt;
            float halfWidth = aspect * ScreenEdge;
            WriteRect(flashQuad, -halfWidth, -ScreenEdge, halfWidth, ScreenEdge);
            Color32 color = flashColor;
            color.a = (byte)(Mathf.Clamp01(flashLeft / config.finaleFlashTime) * 255f);
            FillQuads(flashQuad, 1, color);
        }

        private void TickRain(float dt)
        {
            rainTimer -= dt;
            if (rainTimer > 0f)
            {
                return;
            }
            rainTimer = config.finaleRainInterval;
            Spawn(rng.Range(-0.5f, 0.5f) * cam.aspect, ScreenEdge, rng.Range(-RainDriftMax, RainDriftMax), -rng.Range(RainFallMin, config.finaleConfettiFall));
        }

        private void Spawn(float x, float y, float speedX, float speedY)
        {
            int i = pool.Alloc();
            if (i < 0)
            {
                return;
            }
            posX[i] = x;
            posY[i] = y;
            velX[i] = speedX;
            velY[i] = speedY;
            size[i] = config.finaleConfettiSize * rng.Range(SizeFloor, 1f);
            angle[i] = rng.Range(0f, FullTurn);
            spin[i] = rng.Range(-SpinMax, SpinMax);
            flip[i] = rng.Range(0f, FullTurn);
            flipSpeed[i] = rng.Range(FlipSpeedMin, FlipSpeedMax);
            FillQuads(firstConfetti + i, 1, pieceColors[rng.Range(0, pieceColors.Length)]);
        }

        private void TickConfetti(float dt)
        {
            float gravity = config.finaleConfettiGravity * dt;
            float drag = Mathf.Clamp01(config.finaleConfettiDrag * dt);
            float fall = -config.finaleConfettiFall;
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                float speedX = velX[i] * (1f - drag);
                float speedY = velY[i] - gravity;
                if (speedY < fall)
                {
                    speedY += (fall - speedY) * drag;
                }
                velX[i] = speedX;
                velY[i] = speedY;
                float x = posX[i] + speedX * dt;
                float y = posY[i] + speedY * dt;
                posX[i] = x;
                posY[i] = y;
                if (y < -ScreenEdge)
                {
                    CollapseQuads(firstConfetti + i, 1);
                    pool.Free(i);
                    continue;
                }
                float turn = angle[i] + spin[i] * dt;
                angle[i] = turn;
                float side = flip[i] + flipSpeed[i] * dt;
                flip[i] = side;
                bool star = i % StarEvery == 0;
                float halfX = size[i] * (star ? StarScale * 0.5f : Mathf.Abs(Mathf.Cos(side)) * 0.5f);
                float halfY = size[i] * (star ? StarScale * 0.5f : RibbonRatio * 0.5f);
                WriteTurned(firstConfetti + i, x, y, halfX, halfY, Mathf.Cos(turn), Mathf.Sin(turn));
            }
        }

        private void WriteTurned(int quad, float x, float y, float halfX, float halfY, float cos, float sin)
        {
            int v = quad * VerticesPerQuad;
            Vector3[] verts = vertices;
            float ax = halfX * cos;
            float ay = halfX * sin;
            float bx = -halfY * sin;
            float by = halfY * cos;
            verts[v].x = x - ax - bx;
            verts[v].y = y - ay - by;
            verts[v + 1].x = x + ax - bx;
            verts[v + 1].y = y + ay - by;
            verts[v + 2].x = x - ax + bx;
            verts[v + 2].y = y - ay + by;
            verts[v + 3].x = x + ax + bx;
            verts[v + 3].y = y + ay + by;
        }

        /// <summary>Круг вокруг центра лучей веером клиньев: геометрия, в отличие от спрайта атласа, резкая на любом экране.</summary>
        private void WriteDisc(int firstQuad, float radius)
        {
            float x = rayCenterX * aspect;
            float y = rayCenterY;
            Vector3[] verts = vertices;
            for (int i = 0; i < RingSegments; i++)
            {
                int v = (firstQuad + i) * VerticesPerQuad;
                verts[v].x = x;
                verts[v].y = y;
                verts[v + 1].x = x;
                verts[v + 1].y = y;
                verts[v + 2].x = x + ringCos[i] * radius;
                verts[v + 2].y = y + ringSin[i] * radius;
                verts[v + 3].x = x + ringCos[i + 1] * radius;
                verts[v + 3].y = y + ringSin[i + 1] * radius;
            }
        }

        /// <summary>Клин от центра лучей: две вершины в центре, две на конце.</summary>
        private void WriteWedge(int quad, float from, float to, float length)
        {
            int v = quad * VerticesPerQuad;
            float x = rayCenterX * aspect;
            float y = rayCenterY;
            Vector3[] verts = vertices;
            verts[v].x = x;
            verts[v].y = y;
            verts[v + 1].x = x;
            verts[v + 1].y = y;
            verts[v + 2].x = x + Mathf.Cos(from) * length;
            verts[v + 2].y = y + Mathf.Sin(from) * length;
            verts[v + 3].x = x + Mathf.Cos(to) * length;
            verts[v + 3].y = y + Mathf.Sin(to) * length;
        }

        private void WriteRect(int quad, float minX, float minY, float maxX, float maxY)
        {
            int v = quad * VerticesPerQuad;
            Vector3[] verts = vertices;
            verts[v].x = minX;
            verts[v].y = minY;
            verts[v + 1].x = maxX;
            verts[v + 1].y = minY;
            verts[v + 2].x = minX;
            verts[v + 2].y = maxY;
            verts[v + 3].x = maxX;
            verts[v + 3].y = maxY;
        }

        private void CollapseQuads(int first, int count)
        {
            int last = (first + count) * VerticesPerQuad;
            Vector3[] verts = vertices;
            for (int v = first * VerticesPerQuad; v < last; v++)
            {
                verts[v].x = 0f;
                verts[v].y = 0f;
            }
        }

        private void FillQuads(int first, int count, Color32 color)
        {
            int last = (first + count) * VerticesPerQuad;
            for (int v = first * VerticesPerQuad; v < last; v++)
            {
                colors[v] = color;
            }
            colorsDirty = true;
        }
    }
}
