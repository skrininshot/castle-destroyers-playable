using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    /// <summary>
    /// Текстура маски замка: одна на замок, каналы R материал, G кайма, B трещины. Заливается целиком, но не чаще
    /// castleUploadInterval: замок меньше двух тайлов земли, частичная заливка не окупается.
    /// </summary>
    public sealed class CastleMaskView
    {
        private const float PixelCenter = 0.5f;

        private GameConfig config;
        private DestructibleMask mask;
        private Texture2D texture;
        private Color32[] pixels;
        private float timer;

        public Material Material { get; private set; }

        public void Init(GameConfig gameConfig, DestructibleMask destructibleMask, Material template, MeshRenderer target)
        {
            config = gameConfig;
            mask = destructibleMask;
            texture = new Texture2D(mask.Width, mask.Height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            pixels = new Color32[mask.Width * mask.Height];

            Material = CastleMaterial.Create(template, texture, config);
            float scaleX = mask.TexelsPerMeter / mask.Width;
            float scaleY = mask.TexelsPerMeter / mask.Height;
            float offsetX = PixelCenter / mask.Width - mask.OriginX * scaleX;
            float offsetY = PixelCenter / mask.Height - mask.OriginY * scaleY;
            CastleMaterial.SetMaskRect(Material, offsetX, offsetY, scaleX, scaleY);
            target.sharedMaterial = Material;
            UploadNow();
        }

        public void Tick(float dt)
        {
            timer -= dt;
            if (mask.DirtyCount > 0 && timer <= 0f)
            {
                UploadNow();
                timer = config.castleUploadInterval;
            }
        }

        public void UploadNow()
        {
            while (mask.PopDirtyTile() >= 0)
            {
            }
            byte[] solid = mask.Solid;
            byte[] shade = mask.Shade;
            byte[] cracks = mask.Grass;
            Color32[] target = pixels;
            for (int i = 0; i < target.Length; i++)
            {
                target[i].r = solid[i];
                target[i].g = shade[i];
                target[i].b = cracks[i];
                target[i].a = DestructibleMask.Full;
            }
            texture.SetPixels32(target);
            texture.Apply(false, false);
        }
    }
}
