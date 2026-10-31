using UnityEditor;

namespace CastleDestroyers.EditorTools
{
    /// <summary>Настройки импорта: каналы текстур-форм — это данные, UI — спрайты, арт тем — цветные текстуры; всё без сжатия и мипов.</summary>
    public sealed class GameTextureImport : AssetPostprocessor
    {
        public const string RocketPath = "Assets/_Game/Art/Level/Rocket.png";
        public const string UiFolder = "Assets/_Game/Art/Ui/";
        public const string PillPath = UiFolder + "UiPill.png";
        public const string SegmentPath = UiFolder + "UiSegment.png";
        public const string CirclePath = UiFolder + "UiCircle.png";
        public const string TowerPath = UiFolder + "UiTower.png";
        public const string DegreePath = UiFolder + "UiDegree.png";
        public const string RocketsPath = UiFolder + "UiRockets.png";
        public const string HandPath = UiFolder + "UiHand.png";
        public const string LogoPath = UiFolder + "UiLogo.png";
        public const string EndBluePath = UiFolder + "UiEndBlue.png";
        public const string EndRedPath = UiFolder + "UiEndRed.png";
        public const string FontPath = UiFolder + "Rubik-Game.ttf";
        public const string ThemeFolderPrefix = "Assets/_Game/Art/Theme";
        public const string AudioFolder = "Assets/_Game/Audio/";

        private const float PillBorder = 31f;
        private const float SegmentBorder = 10f;
        private const string BackPrefix = "/Back";
        private const int SoundSampleRate = 22050;

        private void OnPreprocessAudio()
        {
            AudioImporter importer = (AudioImporter)assetImporter;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = UnityEngine.AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = UnityEngine.AudioCompressionFormat.Vorbis;
            settings.quality = 0.4f;
            settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            settings.sampleRateOverride = SoundSampleRate;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
        }

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
                else if (assetPath == SegmentPath)
                {
                    importer.spriteBorder = new UnityEngine.Vector4(SegmentBorder, 0f, SegmentBorder, 0f);
                }
                return;
            }
            if (assetPath.StartsWith(ThemeFolderPrefix))
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = assetPath.Contains(BackPrefix) ? UnityEngine.TextureWrapMode.Repeat : UnityEngine.TextureWrapMode.Clamp;
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
