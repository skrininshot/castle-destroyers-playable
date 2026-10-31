using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Сцена уровня 1 (ТЗ §3.8, §3.10): статичные меши вокруг земли. BackLayers — слои фона для параллакса, рисуются до неё:
    /// звёзды, луна с облаками из атласа, затем три тайлящиеся полосы со своими текстурами. FrontMesh — вода, рисуется после земли,
    /// замков и обломков. Порядок внутри меша = порядок добавления. Замками занимается CastleSystem.
    /// </summary>
    public sealed class LevelScene
    {
        // Полосы фона идут после этих двух слоёв.
        public const int FirstBandLayer = 2;
        public const int BackLayerCount = 5;
        private const int BandCount = BackLayerCount - FirstBandLayer;
        private const int GroundSamples = 5;
        private const uint DecorSeed = 7321u;

        private const int StarCount = 90;
        private const int SparkStep = 6;
        private const float SparkSize = 0.34f;
        private const float SkyHorizonY = 2f;
        private const float SkyTopY = 44f;
        private const float SkyReach = 200f;
        private const byte MoonHaloAlpha = 15;
        private const float StarMinY = 10f;
        private const float StarMaxY = 60f;
        private const float StarMinSize = 0.06f;
        private const float StarMaxSize = 0.14f;
        private const float CloudMinY = 17f;
        private const float CloudMaxY = 35f;
        private const byte CloudAlpha = 150;
        private const float CloudMinScale = 0.7f;
        private const float CloudMaxScale = 1.2f;
        // Полосы шире уровня: слой уменьшается вместе с камерой и не должен кончаться в кадре.
        private const float BandExtentShare = 3f;
        // Строка полосы со сплошным цветом: ею закрашено всё ниже полосы.
        private const float BandSolidV = 0.01f;
        // Верх полосы не доходит до края текстуры: она тайлится и по V, билинейка затянула бы нижний ряд в небо.
        private const float BandTopV = 0.996f;
        private const float BeamMinX = -14f;
        private const float BeamMaxX = -6f;
        private const float BeamReach = 46f;
        private const float BeamDrift = 22f;
        private const byte BeamAlpha = 46;

        private const int WaterPillCount = 110;
        private const float WaterPillMinWidth = 0.5f;
        private const float WaterPillMaxWidth = 1.8f;
        private const float WaterPillHeight = 0.11f;
        private const float WaterPillDepth = 24f;
        private const float WaterPillTop = 0.6f;

        // Ореолы луны: плоские круги, радиус в метрах.
        private static readonly float[] MoonHalos = { 5.2f, 7f, 9.4f, 12.6f };
        private static readonly float[] BandWidth = { ArtSprites.BackFarWidthMeters, ArtSprites.BackMidWidthMeters, ArtSprites.BackNearWidthMeters };
        private static readonly float[] BandHeight = { ArtSprites.BackFarHeightMeters, ArtSprites.BackMidHeightMeters, ArtSprites.BackNearHeightMeters };

        private readonly FlatMeshBuilder builder = new FlatMeshBuilder();
        private readonly Rng decorRng = new Rng(DecorSeed);
        private GameConfig config;
        private DestructibleMask terrain;

        /// <summary>Слои фона от дальнего к ближнему: звёзды, луна, горы, лес, кусты.</summary>
        public Mesh[] BackLayers { get; private set; }
        public Mesh FrontMesh { get; private set; }
        public float PlayerFocusX { get; private set; }
        public float PlayerFocusY { get; private set; }
        public float EnemyFocusX { get; private set; }
        public float EnemyFocusY { get; private set; }
        public float[] PlayerMountX { get; private set; }
        public float[] PlayerMountY { get; private set; }
        public float[] EnemyMountX { get; private set; }
        public float[] EnemyMountY { get; private set; }

        private float playerGroundY;
        private float enemyGroundY;

        public void Init(GameConfig gameConfig, DestructibleMask terrainMask)
        {
            config = gameConfig;
            terrain = terrainMask;
            BackLayers = new Mesh[BackLayerCount];
            for (int i = 0; i < BackLayerCount; i++)
            {
                BackLayers[i] = new Mesh();
                BackLayers[i].name = "LevelBack";
            }
            FrontMesh = new Mesh();
            FrontMesh.name = "LevelFront";

            playerGroundY = CastleGroundY(config.playerCastleX, CastleShapes.BlueHalfWidth);
            enemyGroundY = CastleGroundY(config.enemyCastleX, CastleShapes.RedHalfWidth);

            float playerWallBase = playerGroundY + config.trackHeight + config.platformHeight;
            float enemyWallBase = enemyGroundY + config.trackHeight + config.platformHeight;
            PlayerFocusX = config.playerCastleX;
            PlayerFocusY = playerWallBase + (CastleShapes.BlueTowerTop + CastleShapes.BlueRoofHeight) * 0.5f;
            EnemyFocusX = config.enemyCastleX;
            EnemyFocusY = enemyWallBase + CastleShapes.RedTotalHeight() * 0.5f;

            PlayerMountX = new float[config.playerCannons];
            PlayerMountY = new float[config.playerCannons];
            EnemyMountX = new float[config.enemyCannons];
            EnemyMountY = new float[config.enemyCannons];
            ComputeMounts(PlayerMountX, PlayerMountY, CastleShapes.BlueMounts, config.playerCastleX, playerWallBase);
            ComputeMounts(EnemyMountX, EnemyMountY, CastleShapes.RedMounts, config.enemyCastleX, enemyWallBase);
        }

        /// <summary>Текущая высота ландшафта в точке x, метры.</summary>
        public float GroundHeight(float x)
        {
            return terrain.SurfaceHeight(x);
        }

        /// <summary>Высота ландшафта под гусеницами замка на старте, метры.</summary>
        public float CastleGround(Side side)
        {
            return side == Side.Player ? playerGroundY : enemyGroundY;
        }

        /// <summary>Пересобирает меши в цветах палитры (при старте и смене темы).</summary>
        public void Build(PaletteSet palette)
        {
            float extent = config.levelWidth;
            decorRng.Reset(DecorSeed);
            builder.Clear();
            AddStars(palette, extent);
            ApplyLayer(0);
            AddMoon(palette, extent);
            ApplyLayer(1);
            for (int band = 0; band < BandCount; band++)
            {
                AddBand(band);
                ApplyLayer(FirstBandLayer + band);
            }

            builder.Clear();
            AddWater(palette, extent);
            builder.Apply(FrontMesh);
        }

        /// <summary>Самая высокая точка ландшафта под замком: гусеницы не должны висеть над склоном.</summary>
        private float CastleGroundY(float centerX, float halfWidth)
        {
            float ground = float.NegativeInfinity;
            for (int i = 0; i < GroundSamples; i++)
            {
                float t = (float)i / (GroundSamples - 1);
                float h = GroundHeight(centerX - halfWidth + 2f * halfWidth * t);
                if (h > ground)
                {
                    ground = h;
                }
            }
            return ground;
        }

        private static void ComputeMounts(float[] mountX, float[] mountY, float[] table, float centerX, float wallBase)
        {
            for (int i = 0; i < mountX.Length; i++)
            {
                mountX[i] = centerX + table[i * 2];
                mountY[i] = wallBase + table[i * 2 + 1];
            }
        }

        private void ApplyLayer(int layer)
        {
            builder.Apply(BackLayers[layer]);
            builder.Clear();
        }

        private void AddStars(PaletteSet palette, float extent)
        {
            builder.AddRect(-SkyReach, -SkyReach, SkyReach, SkyHorizonY, palette.skyHorizon);
            builder.AddGradientRect(-SkyReach, SkyHorizonY, SkyReach, SkyTopY, palette.skyHorizon, palette.sky);
            builder.AddRect(-SkyReach, SkyTopY, SkyReach, SkyReach, palette.sky);
            for (int i = 0; i < StarCount; i++)
            {
                float x = decorRng.Range(-extent, extent);
                float y = decorRng.Range(StarMinY, StarMaxY);
                float size = decorRng.Range(StarMinSize, StarMaxSize);
                if (i % SparkStep == 0)
                {
                    builder.AddSprite(ArtSprites.Spark, x, y, SparkSize, palette.skyStar);
                }
                else
                {
                    builder.AddSprite(ArtSprites.Circle, x, y, size, palette.skyStar);
                }
            }
        }

        private void AddMoon(PaletteSet palette, float extent)
        {
            if (config.lightBeamEnabled)
            {
                // Луч прожектора: плоская полупрозрачная полоса по диагонали, как на кладбище Bowmasters.
                Color32 beam = palette.moon;
                beam.a = BeamAlpha;
                builder.AddQuad(BeamMinX, 0f, BeamMaxX, 0f, BeamMaxX + BeamDrift * 2f, BeamReach, BeamMinX + BeamDrift, BeamReach, beam);
            }
            Color32 halo = palette.moon;
            halo.a = MoonHaloAlpha;
            for (int i = MoonHalos.Length - 1; i >= 0; i--)
            {
                builder.AddSprite(ArtSprites.Circle, config.moonPosition.x, config.moonPosition.y, MoonHalos[i], halo);
            }
            builder.AddSprite(ArtSprites.Moon, config.moonPosition.x, config.moonPosition.y, 1f, Color.white);
            // Облака идут после луны: тонкие полосы перекрывают диск.
            float step = config.cloudStep;
            int clouds = (int)(extent * 2f / step);
            Color32 cloud = Color.white;
            cloud.a = CloudAlpha;
            for (int i = 0; i < clouds; i++)
            {
                float x = -extent + (i + decorRng.NextFloat()) * step;
                builder.AddSprite(ArtSprites.Cloud, x, decorRng.Range(CloudMinY, CloudMaxY), decorRng.Range(CloudMinScale, CloudMaxScale), cloud);
            }
        }

        /// <summary>Полоса-слой фона: тайлится по X, ниже неё до дна меша идёт её нижний сплошной цвет.</summary>
        private void AddBand(int band)
        {
            float extent = config.levelWidth * BandExtentShare;
            float width = BandWidth[band];
            float bottom = config.waterLevel + config.backBandBase[band];
            float top = bottom + BandHeight[band];
            builder.AddTexturedRect(-extent, config.levelBottom, extent, bottom, -extent / width, BandSolidV, extent / width, BandSolidV, Color.white);
            builder.AddTexturedRect(-extent, bottom, extent, top, -extent / width, BandSolidV, extent / width, BandTopV, Color.white);
        }

        /// <summary>Вода непрозрачная: основной тон и блики-пилюли. Волнистую кромку рисует WaterStrip (ТЗ §3.8).</summary>
        private void AddWater(PaletteSet palette, float extent)
        {
            float level = config.waterLevel;
            builder.AddRect(-extent, config.levelBottom, extent, level, palette.water);
            for (int i = 0; i < WaterPillCount; i++)
            {
                float x = decorRng.Range(-extent, extent);
                float y = level - decorRng.Range(WaterPillTop, WaterPillDepth);
                float width = decorRng.Range(WaterPillMinWidth, WaterPillMaxWidth);
                builder.AddRoundedRect(x, y, x + width, y + WaterPillHeight, WaterPillHeight * 0.5f, palette.waterHighlight);
            }
        }
    }
}
