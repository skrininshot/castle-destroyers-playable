using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    /// <summary>
    /// Два замка (ТЗ §3.3, §3.6, §3.7): попадания, «погрызанные» кратеры и трещины, связность с опорой на платформу,
    /// падающие куски, поза на ландшафте и отрыв пушек, потерявших опору. Единственная точка входа для ракет.
    /// </summary>
    public sealed class CastleSystem : ITickable
    {
        private const int SideCount = 2;
        private const int MaxTeeth = 16;
        private const uint MeshSeed = 8821u;
        private const float FullTurn = Mathf.PI * 2f;
        private const float CrackLengthFloor = 0.6f;
        private const int SupportSamples = 8;
        public const int MaxPlatformCracks = 4;

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly Rng meshRng = new Rng(MeshSeed);
        private readonly float[] teeth = new float[MaxTeeth];

        private GameConfig config;
        private DestructibleMask terrain;
        private CannonSystem cannons;
        private LevelFx fx;
        private Rng rng;
        private CastleChunks chunks;

        private CastleBody[] bodies;
        private CastleMaskView[] views;
        private Mesh[] meshes;
        private Transform[] transforms;
        private MeshRenderer[] renderers;
        private ConnectivitySolver[] solvers;
        private ChunkExtractor[] extractors;
        private float[] solveTimer;
        private int[] seenRevision;
        private int seenTerrainRevision;
        private int[] supportStart;

        public int ChunksActive { get { return chunks.ActiveCount; } }
        /// <summary>Поза или трещины менялись: виду шасси пора перестроить меш.</summary>
        public bool UndercarriageDirty { get; set; }
        /// <summary>Последний FirstHit пришёлся в платформу или гусеницы.</summary>
        public bool LastHitPlatform { get; private set; }

        public void Init(GameConfig gameConfig, GreyboxLevel level, DestructibleMask terrainMask, CannonSystem cannonSystem, LevelFx levelFx, Rng random,
            Material template, MeshFilter[] filters, Transform chunkParent, int chunkSortingOrder)
        {
            config = gameConfig;
            terrain = terrainMask;
            cannons = cannonSystem;
            fx = levelFx;
            rng = random;

            bodies = new CastleBody[SideCount];
            views = new CastleMaskView[SideCount];
            meshes = new Mesh[SideCount];
            transforms = new Transform[SideCount];
            renderers = new MeshRenderer[SideCount];
            solvers = new ConnectivitySolver[SideCount];
            extractors = new ChunkExtractor[SideCount];
            solveTimer = new float[SideCount];
            seenRevision = new int[SideCount];
            seenTerrainRevision = terrain.Revision;

            PaletteSet silhouette = new PaletteSet();
            for (int s = 0; s < SideCount; s++)
            {
                Side side = (Side)s;
                DrawBody(side, silhouette);
                DestructibleMask mask = CastleMaskBuilder.Create(builder, config, config.platformHeight);
                CastleBody body = new CastleBody();
                float centerX = side == Side.Player ? config.playerCastleX : config.enemyCastleX;
                float halfWidth = side == Side.Player ? GreyboxCastles.BlueHalfWidth : GreyboxCastles.RedHalfWidth;
                body.Init(config, terrain, side, centerX, level.CastleGround(side), halfWidth, mask);
                bodies[s] = body;

                ConnectivitySolver solver = new ConnectivitySolver();
                solver.Init(mask, config.maxIslandsPerSolve);
                solvers[s] = solver;
                ChunkExtractor extractor = new ChunkExtractor();
                extractor.Init(mask, solver, config.castleChunkTextureSize);
                extractors[s] = extractor;
                seenRevision[s] = mask.Revision;

                meshes[s] = new Mesh();
                meshes[s].name = "CastleBody";
                filters[s].sharedMesh = meshes[s];
                transforms[s] = filters[s].transform;
                renderers[s] = filters[s].GetComponent<MeshRenderer>();
                views[s] = new CastleMaskView();
                views[s].Init(config, mask, template, renderers[s]);
            }
            chunks = new CastleChunks();
            chunks.Init(config, fx, rng, template, chunkParent, chunkSortingOrder);
            supportStart = new int[cannons.TotalCannons];
            for (int s = 0; s < SideCount; s++)
            {
                Side side = (Side)s;
                int first = cannons.FirstCannon(side);
                for (int c = first; c < first + cannons.CountOf(side); c++)
                {
                    supportStart[c] = CountSupport(bodies[s].Mask, c);
                }
            }
            PushPoses();
        }

        public CastleBody Body(Side side)
        {
            return bodies[(int)side];
        }

        public void ApplyPalette(PaletteSet palette)
        {
            for (int s = 0; s < SideCount; s++)
            {
                DrawBody((Side)s, palette);
                builder.Apply(meshes[s]);
                Vector3[] vertices = meshes[s].vertices;
                Vector2[] local = new Vector2[vertices.Length];
                for (int v = 0; v < vertices.Length; v++)
                {
                    local[v].x = vertices[v].x;
                    local[v].y = vertices[v].y;
                }
                meshes[s].uv = local;
                CastleMaterial.ApplyPalette(views[s].Material, palette);
            }
            chunks.ApplyPalette(palette);
            UndercarriageDirty = true;
        }

        /// <summary>Попадание ракеты стороны-противника target на отрезке мира: доля пути 0..1 или -1.</summary>
        public float FirstHit(Side target, float fromX, float fromY, float toX, float toY, float step)
        {
            CastleBody body = bodies[(int)target];
            float hit = body.FirstHit(fromX, fromY, toX, toY, step);
            LastHitPlatform = body.HitPlatform;
            return hit;
        }

        /// <summary>Взрыв по камню: кратер, трещины, пыль, покачивание, урон пушкам.</summary>
        public void HitWall(Side target, float x, float y, float weight)
        {
            float radius = config.craterRadiusCastle * BoosterSystem.CraterScale(weight, config.boosterMaxCraterScale);
            CastleBody body = bodies[(int)target];
            Stamp(body, x, y, radius);
            fx.StoneDust(x, y, radius, config.castleDustPuffs);
            body.AddSway(x);
            cannons.ApplyBlast(target, x, y, radius);
        }

        /// <summary>Взрыв по платформе: она не вырезается, только копит трещины-декали (ТЗ §3.6).</summary>
        public void HitPlatform(Side target, float x, float y, float weight)
        {
            float radius = config.craterRadiusCastle * BoosterSystem.CraterScale(weight, config.boosterMaxCraterScale);
            CastleBody body = bodies[(int)target];
            body.PlatformHits++;
            body.AddSway(x);
            cannons.ApplyBlast(target, x, y, radius);
            UndercarriageDirty = true;
        }

        /// <summary>Взрыв в земле рядом с замком: качает его и задевает пушки.</summary>
        public void GroundBlast(Side target, float x, float y, float weight)
        {
            CastleBody body = bodies[(int)target];
            if (body.State != CastleState.Standing || Mathf.Abs(x - body.CenterX) > body.PlatformHalf + config.swayBlastReach)
            {
                return;
            }
            body.AddSway(x);
            cannons.ApplyBlast(target, x, y, config.craterRadiusCastle * BoosterSystem.CraterScale(weight, config.boosterMaxCraterScale));
        }

        /// <summary>Сколько платформенных трещин видно.</summary>
        public int PlatformCracks(Side side)
        {
            int count = bodies[(int)side].PlatformHits / config.platformHitsPerCrack;
            return count < MaxPlatformCracks ? count : MaxPlatformCracks;
        }

#if GAME_DEBUG
        /// <summary>Debug-взрыв по обоим замкам: кратер заданного радиуса в метрах.</summary>
        public void DebugBlast(float x, float y, float radius)
        {
            for (int s = 0; s < SideCount; s++)
            {
                Stamp(bodies[s], x, y, radius);
                cannons.ApplyBlast((Side)s, x, y, radius);
            }
        }
#endif

        public void Tick(float dt)
        {
            bool terrainChanged = terrain.Revision != seenTerrainRevision;
            seenTerrainRevision = terrain.Revision;
            for (int s = 0; s < SideCount; s++)
            {
                TickCastle(s, dt, terrainChanged);
            }
            chunks.Tick(dt);
        }

        /// <summary>Возвращает замки к исходному: рестарт сессии. Пушки должны быть уже сброшены.</summary>
        public void Reset()
        {
            chunks.Clear();
            for (int s = 0; s < SideCount; s++)
            {
                CastleBody body = bodies[s];
                body.Mask.Reset();
                System.Array.Clear(body.Mask.Grass, 0, body.Mask.Grass.Length);
                body.Reset();
                views[s].UploadNow();
                seenRevision[s] = body.Mask.Revision;
                solveTimer[s] = 0f;
                renderers[s].enabled = true;
            }
            seenTerrainRevision = terrain.Revision;
            PushPoses();
        }

        private void TickCastle(int s, float dt, bool terrainChanged)
        {
            CastleBody body = bodies[s];
            Side side = (Side)s;
            DestructibleMask mask = body.Mask;
            if (mask.Revision != seenRevision[s])
            {
                ReleaseUnsupported(s, false, 0f, 0f);
            }
            solveTimer[s] += dt;
            if (mask.ConnectivityDirty && solveTimer[s] >= config.connectivityInterval)
            {
                solveTimer[s] = 0f;
                Solve(s);
            }
            seenRevision[s] = mask.Revision;

            body.Tick(dt, terrainChanged);
            if (body.JustSank)
            {
                cannons.LoseAll(side);
            }
            if (body.JustHitWater)
            {
                fx.Splash(body.PivotX, config.waterLevel, body.PlatformHalf * 2f);
            }
            if (body.PoseChanged)
            {
                PushPose(s);
            }
            if (body.State == CastleState.Gone && renderers[s].enabled)
            {
                renderers[s].enabled = false;
            }
            views[s].Tick(dt);
        }

        private void Solve(int s)
        {
            CastleBody body = bodies[s];
            ConnectivitySolver solver = solvers[s];
            ChunkExtractor extractor = extractors[s];
            solver.Solve();
            for (int island = 0; island < solver.IslandCount; island++)
            {
                int cells = solver.IslandSize[island];
                bool asChunk = cells >= config.minChunkCells && chunks.HasFreeSlot;
                extractor.Cut(island, asChunk);
                if (asChunk)
                {
                    chunks.Spawn(extractor, body, meshes[s]);
                    ReleaseUnsupported(s, true, chunks.LastDrift, chunks.LastSpin);
                }
                else
                {
                    float spread = Mathf.Max(extractor.WorldMaxX - extractor.CenterX, extractor.WorldMaxY - extractor.CenterY);
                    fx.StoneDust(body.WorldX(extractor.CenterX, extractor.CenterY), body.WorldY(extractor.CenterX, extractor.CenterY), spread,
                        Mathf.Min(cells, config.dustMaxPuffs));
                    ReleaseUnsupported(s, false, 0f, 0f);
                }
            }
        }

        /// <summary>
        /// Пушки без материала под креплением отрываются (ТЗ §3.3, условия 2 и 3). riding — пушка уезжает вместе с куском,
        /// иначе просто срывается вниз. Последнюю пушку игрока Director не отдаёт: она съезжает на платформу.
        /// </summary>
        private void ReleaseUnsupported(int s, bool riding, float drift, float spin)
        {
            Side side = (Side)s;
            DestructibleMask mask = bodies[s].Mask;
            int first = cannons.FirstCannon(side);
            int end = first + cannons.CountOf(side);
            for (int c = first; c < end; c++)
            {
                if (!cannons.IsAlive(c) || cannons.IsOnPlatform(c) || IsSupported(mask, c))
                {
                    continue;
                }
                float speedX = riding ? drift : rng.Range(-config.cannonDetachSpeed, config.cannonDetachSpeed);
                float speedY = riding ? 0f : config.cannonDetachLift;
                float turn = riding ? spin : rng.Range(-config.cannonDetachSpinDeg, config.cannonDetachSpinDeg) * Mathf.Deg2Rad;
                if (!cannons.Detach(c, speedX, speedY, turn))
                {
                    cannons.DropToPlatform(c);
                }
            }
        }

        /// <summary>Пушка держится, пока материал есть под креплением и вокруг него: на тонкой перемычке она не висит.</summary>
        private bool IsSupported(DestructibleMask mask, int cannon)
        {
            return mask.IsSolidWorld(cannons.LocalX(cannon), cannons.LocalY(cannon))
                && CountSupport(mask, cannon) >= supportStart[cannon] * config.cannonSupportShare;
        }

        /// <summary>Сколько точек круга вокруг крепления стоит на материале.</summary>
        private int CountSupport(DestructibleMask mask, int cannon)
        {
            float x = cannons.LocalX(cannon);
            float y = cannons.LocalY(cannon);
            float radius = config.cannonSupportRadius;
            int count = 0;
            for (int i = 0; i < SupportSamples; i++)
            {
                float angle = FullTurn * i / SupportSamples;
                if (mask.IsSolidWorld(x + Mathf.Cos(angle) * radius, y + Mathf.Sin(angle) * radius))
                {
                    count++;
                }
            }
            return count;
        }

        private void Stamp(CastleBody body, float x, float y, float radius)
        {
            DestructibleMask mask = body.Mask;
            float texels = mask.TexelsPerMeter;
            float centerX = mask.TexelX(body.LocalX(x, y));
            float centerY = mask.TexelY(body.LocalY(x, y));
            float texelRadius = radius * texels;

            int count = rng.Range(config.castleCraterTeethMin, config.castleCraterTeethMax + 1);
            count = count > MaxTeeth ? MaxTeeth : count;
            for (int i = 0; i < count; i++)
            {
                // Чередование: зубцы и впадины должны читаться и при малом разбросе.
                float offset = rng.Range(0.2f, 1f) * config.castleCraterNoise;
                teeth[i] = i % 2 == 0 ? 1f + offset : 1f - offset;
            }
            bool hit = CraterStamper.StampGnawed(mask, centerX, centerY, texelRadius, config.castleRimWidth * texels, teeth, count);
            if (!hit || !rng.Chance(config.castleCrackChance))
            {
                return;
            }
            float bend = config.castleCrackBendDeg * Mathf.Deg2Rad;
            for (int i = 0; i < config.castleCrackCount; i++)
            {
                float angle = rng.Range(0f, FullTurn);
                CraterStamper.PaintCrack(mask, centerX + Mathf.Cos(angle) * texelRadius, centerY + Mathf.Sin(angle) * texelRadius, angle,
                    config.castleCrackLength * texels * rng.Range(CrackLengthFloor, 1f), config.castleCrackWidth * texels,
                    config.castleCrackSegments, bend, rng);
            }
        }

        private void DrawBody(Side side, PaletteSet palette)
        {
            builder.Clear();
            meshRng.Reset(MeshSeed + (uint)side);
            if (side == Side.Player)
            {
                GreyboxCastles.DrawBlue(builder, palette, meshRng, config.platformHeight);
            }
            else
            {
                GreyboxCastles.DrawRed(builder, palette, meshRng, config.platformHeight);
            }
        }

        private void PushPoses()
        {
            for (int s = 0; s < SideCount; s++)
            {
                PushPose(s);
            }
        }

        private void PushPose(int s)
        {
            CastleBody body = bodies[s];
            CastleMaterial.ApplyPose(transforms[s], body.PivotX, body.PivotY, body.Angle);
            cannons.SetBodyPose((Side)s, body.PivotX, body.PivotY, body.Cos, body.Sin);
            UndercarriageDirty = true;
        }
    }
}
