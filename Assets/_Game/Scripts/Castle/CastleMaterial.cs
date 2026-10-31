using CastleDestroyers.Core;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    /// <summary>Параметры шейдера Game/Castle: общие для замка и его обломков.</summary>
    public static class CastleMaterial
    {
        private static readonly int ArtTex = Shader.PropertyToID("_MainTex");
        private static readonly int ArtRect = Shader.PropertyToID("_ArtRect");
        private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");
        private static readonly int MaskRect = Shader.PropertyToID("_MaskRect");
        private static readonly int CrackColor = Shader.PropertyToID("_CrackColor");
        private static readonly int RimDarken = Shader.PropertyToID("_RimDarken");
        private static readonly int EdgeSoftness = Shader.PropertyToID("_EdgeSoftness");

        public static Material Create(Material template, Texture2D mask, GameConfig config)
        {
            Material material = new Material(template);
            material.SetTexture(MaskTex, mask);
            material.SetFloat(RimDarken, config.castleRimDarken);
            material.SetFloat(EdgeSoftness, config.maskEdgeSoftness);
            return material;
        }

        public static void ApplyPalette(Material material, PaletteSet palette)
        {
            material.SetColor(CrackColor, palette.castleCrack);
        }

        /// <summary>Рисунок замка и отображение локальных координат в его UV: низ текстуры стоит на artMinY.</summary>
        public static void SetArt(Material material, Texture2D art, float artMinY)
        {
            float scaleX = 1f / ArtSprites.CastleArtWidthMeters;
            float scaleY = 1f / ArtSprites.CastleArtHeightMeters;
            material.SetTexture(ArtTex, art);
            material.SetVector(ArtRect, new Vector4(0.5f, -artMinY * scaleY, scaleX, scaleY));
        }

        /// <summary>Отображение локальных координат в UV маски: u = x * scaleX + offsetX.</summary>
        public static void SetMaskRect(Material material, float offsetX, float offsetY, float scaleX, float scaleY)
        {
            material.SetVector(MaskRect, new Vector4(offsetX, offsetY, scaleX, scaleY));
        }

        /// <summary>Ставит Transform в позу: поворот только вокруг Z, без Quaternion.Euler.</summary>
        public static void ApplyPose(Transform transform, float x, float y, float angle)
        {
            float half = angle * 0.5f;
            transform.SetPositionAndRotation(new Vector3(x, y, 0f), new Quaternion(0f, 0f, Mathf.Sin(half), Mathf.Cos(half)));
        }
    }
}
