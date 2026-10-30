using System.IO;
using CastleDestroyers.Destruction;
using UnityEditor;
using UnityEngine;

namespace CastleDestroyers.EditorTools
{
    /// <summary>
    /// Запекает Level1_TerrainMask.png в TerrainMaskData. Срабатывает сам при импорте PNG,
    /// поэтому после <c>python Tools/rasterize_svg.py</c> других шагов нет. Картинка в сборку не попадает.
    /// </summary>
    public sealed class TerrainMaskBaker : AssetPostprocessor
    {
        public const string SourcePath = "Assets/_Game/Art/Level/Level1_TerrainMask.png";
        public const string DataPath = "Assets/_Game/Configs/Level1_Terrain.asset";

        [MenuItem("Tools/Castle Destroyers/Bake Terrain Mask")]
        public static TerrainMaskData Bake()
        {
            // Файл читается напрямую: настройки импорта текстуры на данные не влияют.
            Texture2D image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            image.LoadImage(File.ReadAllBytes(SourcePath));
            Color32[] pixels = image.GetPixels32();
            int texels = pixels.Length;
            byte[] solid = new byte[texels];
            byte[] grass = new byte[texels];
            byte[] buried = new byte[texels];
            for (int i = 0; i < texels; i++)
            {
                solid[i] = pixels[i].r;
                grass[i] = pixels[i].g;
                buried[i] = pixels[i].b;
            }

            TerrainMaskData data = AssetDatabase.LoadAssetAtPath<TerrainMaskData>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<TerrainMaskData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }
            data.width = image.width;
            data.height = image.height;
            data.solidRuns = MaskRle.Encode(solid);
            data.grassRuns = MaskRle.Encode(grass);
            data.buriedRuns = MaskRle.Encode(buried);
            Object.DestroyImmediate(image);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            return data;
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            for (int i = 0; i < imported.Length; i++)
            {
                if (imported[i] == SourcePath)
                {
                    Bake();
                    return;
                }
            }
        }
    }
}
