using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Fx;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    /// <summary>
    /// Падающие куски замков (ТЗ §3.6): тот же меш замка, вырезанный маской этого куска, с гравитацией и лёгким вращением,
    /// без коллизий. Слоты с текстурой созданы заранее, поэтому в бою нет аллокаций.
    /// </summary>
    public sealed class CastleChunks : ITickable
    {
        private GameConfig config;
        private LevelFx fx;
        private Rng rng;
        private IndexPool pool;

        private Texture2D[] textures;
        private Material[] materials;
        private MeshFilter[] filters;
        private MeshRenderer[] renderers;
        private Transform[] transforms;

        private float[] posX;
        private float[] posY;
        private float[] velX;
        private float[] velY;
        private float[] angle;
        private float[] spin;
        private float[] localX;
        private float[] localY;
        private float[] halfWidth;
        private float[] radius;
        private byte[] splashed;

        public int ActiveCount { get { return pool.ActiveCount; } }
        public bool HasFreeSlot { get { return pool.ActiveCount < pool.Capacity; } }
        /// <summary>Скорость вбок последнего созданного куска: пушки на нём падают вместе с ним.</summary>
        public float LastDrift { get; private set; }
        /// <summary>Вращение последнего созданного куска, рад/с.</summary>
        public float LastSpin { get; private set; }

        public void Init(GameConfig gameConfig, LevelFx levelFx, Rng random, Material template, Transform parent, int sortingOrder)
        {
            config = gameConfig;
            fx = levelFx;
            rng = random;

            int capacity = config.castleChunkSlots;
            pool = new IndexPool(capacity);
            textures = new Texture2D[capacity];
            materials = new Material[capacity];
            filters = new MeshFilter[capacity];
            renderers = new MeshRenderer[capacity];
            transforms = new Transform[capacity];
            posX = new float[capacity];
            posY = new float[capacity];
            velX = new float[capacity];
            velY = new float[capacity];
            angle = new float[capacity];
            spin = new float[capacity];
            localX = new float[capacity];
            localY = new float[capacity];
            halfWidth = new float[capacity];
            radius = new float[capacity];
            splashed = new byte[capacity];

            for (int i = 0; i < capacity; i++)
            {
                Texture2D texture = new Texture2D(config.castleChunkTextureSize, config.castleChunkTextureSize, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                textures[i] = texture;
                materials[i] = CastleMaterial.Create(template, texture, config);

                GameObject chunkObject = new GameObject("CastleChunk");
                chunkObject.transform.SetParent(parent, false);
                filters[i] = chunkObject.AddComponent<MeshFilter>();
                MeshRenderer chunkRenderer = chunkObject.AddComponent<MeshRenderer>();
                chunkRenderer.sharedMaterial = materials[i];
                chunkRenderer.sortingOrder = sortingOrder;
                chunkRenderer.enabled = false;
                renderers[i] = chunkRenderer;
                transforms[i] = chunkObject.transform;
            }
        }

        public void ApplyPalette(PaletteSet palette)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                CastleMaterial.ApplyPalette(materials[i], palette);
            }
        }

        /// <summary>Создаёт кусок из результата вырезания. False, если свободных слотов нет.</summary>
        public bool Spawn(ChunkExtractor extractor, CastleBody body, Mesh mesh)
        {
            int i = pool.Alloc();
            if (i < 0)
            {
                return false;
            }
            float minX = extractor.WorldMinX;
            float minY = extractor.WorldMinY;
            float sizeX = extractor.WorldMaxX - minX;
            float sizeY = extractor.WorldMaxY - minY;
            localX[i] = minX + sizeX * 0.5f;
            localY[i] = minY + sizeY * 0.5f;
            posX[i] = body.WorldX(localX[i], localY[i]);
            posY[i] = body.WorldY(localX[i], localY[i]);
            halfWidth[i] = sizeX * 0.5f;
            radius[i] = Mathf.Sqrt(sizeX * sizeX + sizeY * sizeY) * 0.5f;
            LastDrift = rng.Range(-config.chunkMaxDrift, config.chunkMaxDrift);
            LastSpin = rng.Range(-config.chunkMaxSpinDeg, config.chunkMaxSpinDeg) * Mathf.Deg2Rad;
            velX[i] = LastDrift;
            velY[i] = 0f;
            angle[i] = body.Angle;
            spin[i] = LastSpin;
            splashed[i] = 0;

            float scaleX = extractor.UvMaxX / sizeX;
            float scaleY = extractor.UvMaxY / sizeY;
            CastleMaterial.SetMaskRect(materials[i], -minX * scaleX, -minY * scaleY, scaleX, scaleY);
            Texture2D texture = textures[i];
            texture.SetPixels32(extractor.Pixels);
            texture.Apply(false, false);
            filters[i].sharedMesh = mesh;
            WritePose(i);
            renderers[i].enabled = true;
            return true;
        }

        public void Tick(float dt)
        {
            if (pool.ActiveCount == 0)
            {
                return;
            }
            float water = config.waterLevel;
            for (int i = 0; i < pool.Capacity; i++)
            {
                if (!pool.IsUsed(i))
                {
                    continue;
                }
                velY[i] -= config.chunkGravity * dt;
                posX[i] += velX[i] * dt;
                posY[i] += velY[i] * dt;
                angle[i] += spin[i] * dt;
                if (splashed[i] == 0 && posY[i] < water)
                {
                    splashed[i] = 1;
                    fx.Splash(posX[i], water, halfWidth[i] * 2f);
                }
                if (posY[i] + radius[i] < water)
                {
                    renderers[i].enabled = false;
                    pool.Free(i);
                    continue;
                }
                WritePose(i);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < pool.Capacity; i++)
            {
                renderers[i].enabled = false;
            }
            pool.Clear();
        }

        /// <summary>Центр куска лежит в (posX, posY), а меш поворачивается вокруг своего начала координат.</summary>
        private void WritePose(int i)
        {
            float cos = Mathf.Cos(angle[i]);
            float sin = Mathf.Sin(angle[i]);
            float x = posX[i] - (localX[i] * cos - localY[i] * sin);
            float y = posY[i] - (localX[i] * sin + localY[i] * cos);
            CastleMaterial.ApplyPose(transforms[i], x, y, angle[i]);
        }
    }
}
