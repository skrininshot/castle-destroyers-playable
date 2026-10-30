using CastleDestroyers.Core;
using CastleDestroyers.Fx;
using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Разрушаемый ландшафт целиком: взрывы, связность по таймеру, вырезание островов, заливка тайлов.
    /// Единственная точка входа для других систем: Explode, а маска для тестов попаданий — в Mask.
    /// </summary>
    public sealed class TerrainSystem : ITickable
    {
        private GameConfig config;
        private MaskTileView view;
        private ChunkSystem chunks;
        private LevelFx fx;
        private ConnectivitySolver solver;
        private ChunkExtractor extractor;
        private float solveTimer;

        public DestructibleMask Mask { get; private set; }
        public int ChunksActive { get { return chunks.ActiveCount; } }
        public int LastUploads { get { return view.Uploaded; } }
        public int LastIslands { get; private set; }

        public void Init(GameConfig gameConfig, DestructibleMask mask, MaskTileView tileView, ChunkSystem chunkSystem, LevelFx levelFx)
        {
            config = gameConfig;
            Mask = mask;
            view = tileView;
            chunks = chunkSystem;
            fx = levelFx;

            solver = new ConnectivitySolver();
            solver.Init(Mask, config.maxIslandsPerSolve);
            extractor = new ChunkExtractor();
            extractor.Init(Mask, solver, config.chunkTextureSize);
            view.UploadAll();
        }

        /// <summary>Кратер в точке мира, радиус в метрах. True, если задет материал.</summary>
        public bool Explode(float x, float y, float radius)
        {
            float texels = Mask.TexelsPerMeter;
            float scallop = config.rimScallop ? config.rimScallopDepth * texels : 0f;
            bool hit = CraterStamper.StampCircle(Mask, Mask.TexelX(x), Mask.TexelY(y), radius * texels, config.rimWidth * texels,
                scallop, config.rimScallopTeeth);
            if (hit)
            {
                fx.Dust(x, y, radius, config.craterDustPuffs);
            }
            return hit;
        }

        public void Tick(float dt)
        {
            solveTimer += dt;
            if (Mask.ConnectivityDirty && solveTimer >= config.connectivityInterval)
            {
                solveTimer = 0f;
                Solve();
            }
            view.UploadDirty(config.maxTileUploadsPerFrame);
        }

        /// <summary>Возвращает ландшафт к исходному: рестарт сессии.</summary>
        public void Reset()
        {
            chunks.Clear();
            Mask.Reset();
            view.UploadAll();
            solveTimer = 0f;
            LastIslands = 0;
        }

        private void Solve()
        {
            solver.Solve();
            LastIslands = solver.IslandCount;
            for (int island = 0; island < solver.IslandCount; island++)
            {
                int cells = solver.IslandSize[island];
                bool asChunk = cells >= config.minChunkCells && chunks.HasFreeSlot;
                extractor.Cut(island, asChunk);
                if (asChunk)
                {
                    chunks.Spawn(extractor);
                }
                else
                {
                    float spread = Mathf.Max(extractor.WorldMaxX - extractor.CenterX, extractor.WorldMaxY - extractor.CenterY);
                    fx.Dust(extractor.CenterX, extractor.CenterY, spread, Mathf.Min(cells, config.dustMaxPuffs));
                }
            }
        }
    }
}
