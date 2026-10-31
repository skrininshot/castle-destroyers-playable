using UnityEditor;

namespace CastleDestroyers.EditorTools
{
    /// <summary>Настройки импорта: каналы текстур-форм — это данные, а формы UI — белые спрайты; всё без сжатия и мипов.</summary>
    public sealed class GameTextureImport : AssetPostprocessor
    {
        public const string RocketPath = "Assets/_Game/Art/Level/Rocket.png";
        public const string UiFolder = "Assets/_Game/Art/Ui/";
        public const string PillPath = UiFolder + "UiPill.png";
        public const string CastlePath = UiFolder + "UiCastle.png";
        public const string RocketsPath = UiFolder + "UiRockets.png";
        public const string HandPath = UiFolder + "UiHand.png";

        private const float PillBorder = 31f;

        private void OnPreprocessTexture()
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            if (assetPath.StartsWith(UiFolder))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
                importer.npotScale = TextureImporterNPOTScale.None;
                if (assetPath == PillPath)
                {
                    importer.spriteBorder = new UnityEngine.Vector4(PillBorder, PillBorder, PillBorder, PillBorder);
                }
                return;
            }
            if (assetPath != RocketPath)
            {
                return;
            }
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
        }
    }
}
