using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Destruction
{
    /// <summary>Параметры шейдера Game/Terrain: общие для тайлов и обломков, чтобы они выглядели одинаково.</summary>
    public static class TerrainMaterial
    {
        public static readonly int MaskTex = Shader.PropertyToID("_MaskTex");
        private static readonly int GroundColor = Shader.PropertyToID("_GroundColor");
        private static readonly int BandColor = Shader.PropertyToID("_BandColor");
        private static readonly int RimColor = Shader.PropertyToID("_RimColor");
        private static readonly int RimLightColor = Shader.PropertyToID("_RimLightColor");
        private static readonly int BlobColor = Shader.PropertyToID("_BlobColor");
        private static readonly int BuriedColor = Shader.PropertyToID("_BuriedColor");
        private static readonly int CraterColor = Shader.PropertyToID("_CraterColor");
        private static readonly int EdgeSoftness = Shader.PropertyToID("_EdgeSoftness");

        public static Texture2D CreateMaskTexture(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        public static void SetEdgeSoftness(Material material, float softness)
        {
            material.SetFloat(EdgeSoftness, softness);
        }

        public static void ApplyPalette(Material material, PaletteSet palette)
        {
            material.SetColor(GroundColor, palette.ground);
            material.SetColor(BandColor, palette.groundBand);
            material.SetColor(RimColor, palette.groundRim);
            material.SetColor(RimLightColor, palette.groundRimLight);
            material.SetColor(BlobColor, palette.groundShade);
            material.SetColor(BuriedColor, palette.groundBuried);
            material.SetColor(CraterColor, palette.groundCrater);
        }
    }
}
