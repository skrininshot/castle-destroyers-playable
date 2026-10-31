// Сгенерировано Tools/build_art.py, руками не править.
namespace CastleDestroyers.Level
{
    /// <summary>Раскладка атласа и размеры текстур: одна на обе темы.</summary>
    public static class ArtSprites
    {
        public const int White = 0;
        public const int Circle = 1;
        public const int StarEdge = 2;
        public const int StarMid = 3;
        public const int Spark = 4;
        public const int CannonBodyPlayer = 5;
        public const int CannonBodyEnemy = 6;
        public const int CannonBarrelPlayer = 7;
        public const int CannonBarrelEnemy = 8;
        public const int Track = 9;
        public const int PlatformBlue = 10;
        public const int PlatformRed = 11;
        public const int Spruce = 12;
        public const int Bush = 13;
        public const int Rock = 14;
        public const int Marker = 15;
        public const int Moon = 16;
        public const int Cloud = 17;
        public const int Booster10 = 18;
        public const int Booster20 = 19;
        public const int Booster50 = 20;
        public const int Glyph0 = 21;
        public const int Glyph1 = 22;
        public const int Glyph2 = 23;
        public const int Glyph3 = 24;
        public const int Glyph4 = 25;
        public const int Glyph5 = 26;
        public const int Glyph6 = 27;
        public const int Glyph7 = 28;
        public const int Glyph8 = 29;
        public const int Glyph9 = 30;
        public const int Glyph10 = 31;
        public const int GlyphPlus = Glyph10;

        public const float CastleArtWidthMeters = 8.82759f;
        public const float CastleArtHeightMeters = 17.65517f;
        /// <summary>Низ текстуры замка ниже низа стены на столько метров.</summary>
        public const float CastleArtBaseMeters = 0.13793f;
        public const float BackFarWidthMeters = 51.2000f;
        public const float BackFarHeightMeters = 25.6000f;
        public const float BackMidWidthMeters = 36.5714f;
        public const float BackMidHeightMeters = 18.2857f;
        public const float BackNearWidthMeters = 36.5714f;
        public const float BackNearHeightMeters = 18.2857f;

        /// <summary>UV спрайта: minU, minV, maxU, maxV.</summary>
        public static readonly float[] Uv =
        {
            0.899414f, 0.545898f, 0.903320f, 0.549805f,
            0.184082f, 0.572754f, 0.245605f, 0.634277f,
            0.698730f, 0.872559f, 0.822754f, 0.996582f,
            0.829590f, 0.872559f, 0.953613f, 0.996582f,
            0.358887f, 0.580566f, 0.412598f, 0.634277f,
            0.489746f, 0.507324f, 0.539551f, 0.553223f,
            0.546387f, 0.507324f, 0.596191f, 0.553223f,
            0.690918f, 0.537598f, 0.786621f, 0.553223f,
            0.793457f, 0.537598f, 0.889160f, 0.553223f,
            0.091309f, 0.651855f, 0.242676f, 0.740723f,
            0.419434f, 0.586426f, 0.844238f, 0.634277f,
            0.003418f, 0.505371f, 0.482910f, 0.553223f,
            0.599121f, 0.834473f, 0.691895f, 0.996582f,
            0.252441f, 0.572754f, 0.352051f, 0.634277f,
            0.603027f, 0.510254f, 0.684082f, 0.553223f,
            0.003418f, 0.641113f, 0.084473f, 0.740723f,
            0.003418f, 0.747559f, 0.252441f, 0.996582f,
            0.249512f, 0.659668f, 0.482910f, 0.740723f,
            0.259277f, 0.806152f, 0.365723f, 0.996582f,
            0.372559f, 0.806152f, 0.479004f, 0.996582f,
            0.485840f, 0.806152f, 0.592285f, 0.996582f,
            0.489746f, 0.666504f, 0.546387f, 0.740723f,
            0.553223f, 0.666504f, 0.597168f, 0.740723f,
            0.604004f, 0.666504f, 0.658691f, 0.740723f,
            0.665527f, 0.666504f, 0.721191f, 0.740723f,
            0.728027f, 0.666504f, 0.785645f, 0.740723f,
            0.792480f, 0.666504f, 0.847168f, 0.740723f,
            0.854004f, 0.666504f, 0.908691f, 0.740723f,
            0.915527f, 0.666504f, 0.965332f, 0.740723f,
            0.003418f, 0.560059f, 0.060059f, 0.634277f,
            0.066895f, 0.560059f, 0.120605f, 0.634277f,
            0.127441f, 0.560059f, 0.177246f, 0.634277f
        };

        /// <summary>Прямоугольник спрайта относительно точки привязки: minX, minY, maxX, maxY.</summary>
        public static readonly float[] Rect =
        {
            0.000000f, 0.000000f, 1.000000f, 1.000000f,
            -1.000000f, -1.000000f, 1.000000f, 1.000000f,
            -1.000000f, -1.000000f, 1.000000f, 1.000000f,
            -1.000000f, -1.000000f, 1.000000f, 1.000000f,
            -1.000000f, -1.000000f, 1.000000f, 1.000000f,
            -0.580000f, -0.370000f, 0.220000f, 0.370000f,
            -0.580000f, -0.370000f, 0.220000f, 0.370000f,
            -0.280000f, -0.130000f, 1.260000f, 0.130000f,
            -0.280000f, -0.130000f, 1.260000f, 0.130000f,
            -1.390000f, -0.220000f, 1.390000f, 1.420000f,
            -3.890000f, -0.040000f, 3.890000f, 0.840000f,
            -4.390000f, -0.040000f, 4.390000f, 0.840000f,
            -0.750000f, -0.100000f, 0.750000f, 2.500000f,
            -0.800000f, -0.140000f, 0.800000f, 0.860000f,
            -0.650000f, -0.080000f, 0.650000f, 0.620000f,
            -0.650000f, -0.100000f, 0.650000f, 1.500000f,
            -3.600000f, -3.600000f, 3.600000f, 3.600000f,
            -3.600000f, -1.260000f, 3.600000f, 1.260000f,
            -0.859000f, -1.531000f, 0.859000f, 1.531000f,
            -0.859000f, -1.531000f, 0.859000f, 1.531000f,
            -0.859000f, -1.531000f, 0.859000f, 1.531000f,
            0.000000f, 0.000000f, 0.766234f, 1.000000f,
            0.000000f, 0.000000f, 0.597403f, 1.000000f,
            0.000000f, 0.000000f, 0.740260f, 1.000000f,
            0.000000f, 0.000000f, 0.753247f, 1.000000f,
            0.000000f, 0.000000f, 0.779221f, 1.000000f,
            0.000000f, 0.000000f, 0.740260f, 1.000000f,
            0.000000f, 0.000000f, 0.740260f, 1.000000f,
            0.000000f, 0.000000f, 0.675325f, 1.000000f,
            0.000000f, 0.000000f, 0.766234f, 1.000000f,
            0.000000f, 0.000000f, 0.727273f, 1.000000f,
            0.000000f, 0.000000f, 0.675325f, 1.000000f
        };
    }
}
