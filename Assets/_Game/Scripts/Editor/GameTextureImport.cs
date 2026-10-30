using UnityEditor;

namespace CastleDestroyers.EditorTools
{
    /// <summary>Настройки импорта текстур-форм: каналы — это данные, поэтому без сжатия и мипов.</summary>
    public sealed class GameTextureImport : AssetPostprocessor
    {
        public const string RocketPath = "Assets/_Game/Art/Level/Rocket.png";

        private void OnPreprocessTexture()
        {
            if (assetPath != RocketPath)
            {
                return;
            }
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
        }
    }
}
