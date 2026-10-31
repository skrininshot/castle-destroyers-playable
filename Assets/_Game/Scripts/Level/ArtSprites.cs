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
        public const int HudPill = 21;
        public const int HudSegment = 22;
        public const int HudTower = 23;
        public const int HudRockets = 24;
        public const int Glyph0 = 25;
        public const int Glyph1 = 26;
        public const int Glyph2 = 27;
        public const int Glyph3 = 28;
        public const int Glyph4 = 29;
        public const int Glyph5 = 30;
        public const int Glyph6 = 31;
        public const int Glyph7 = 32;
        public const int Glyph8 = 33;
        public const int Glyph9 = 34;
        public const int Glyph10 = 35;
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
            0.393555f, 0.466797f, 0.397461f, 0.470703f,
            0.428223f, 0.548340f, 0.489746f, 0.609863f,
            0.698730f, 0.872559f, 0.822754f, 0.996582f,
            0.829590f, 0.872559f, 0.953613f, 0.996582f,
            0.671387f, 0.556152f, 0.725098f, 0.609863f,
            0.921387f, 0.482910f, 0.971191f, 0.528809f,
            0.003418f, 0.428223f, 0.053223f, 0.474121f,
            0.185059f, 0.458496f, 0.280762f, 0.474121f,
            0.287598f, 0.458496f, 0.383301f, 0.474121f,
            0.353027f, 0.651855f, 0.504395f, 0.740723f,
            0.003418f, 0.480957f, 0.428223f, 0.528809f,
            0.435059f, 0.480957f, 0.914551f, 0.528809f,
            0.599121f, 0.834473f, 0.691895f, 0.996582f,
            0.496582f, 0.548340f, 0.596191f, 0.609863f,
            0.060059f, 0.431152f, 0.141113f, 0.474121f,
            0.265137f, 0.641113f, 0.346191f, 0.740723f,
            0.003418f, 0.747559f, 0.252441f, 0.996582f,
            0.511230f, 0.659668f, 0.744629f, 0.740723f,
            0.259277f, 0.806152f, 0.365723f, 0.996582f,
            0.372559f, 0.806152f, 0.479004f, 0.996582f,
            0.485840f, 0.806152f, 0.592285f, 0.996582f,
            0.603027f, 0.548340f, 0.664551f, 0.609863f,
            0.147949f, 0.443848f, 0.178223f, 0.474121f,
            0.003418f, 0.616699f, 0.127441f, 0.740723f,
            0.134277f, 0.616699f, 0.258301f, 0.740723f,
            0.751465f, 0.666504f, 0.808105f, 0.740723f,
            0.814941f, 0.666504f, 0.858887f, 0.740723f,
            0.865723f, 0.666504f, 0.920410f, 0.740723f,
            0.927246f, 0.666504f, 0.982910f, 0.740723f,
            0.003418f, 0.535645f, 0.061035f, 0.609863f,
            0.067871f, 0.535645f, 0.122559f, 0.609863f,
            0.129395f, 0.535645f, 0.184082f, 0.609863f,
            0.190918f, 0.535645f, 0.240723f, 0.609863f,
            0.247559f, 0.535645f, 0.304199f, 0.609863f,
            0.311035f, 0.535645f, 0.364746f, 0.609863f,
            0.371582f, 0.535645f, 0.421387f, 0.609863f
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
            0.000000f, 0.000000f, 1.000000f, 1.000000f,
            0.000000f, 0.000000f, 1.000000f, 1.000000f,
            0.000000f, 0.000000f, 1.000000f, 1.000000f,
            0.000000f, 0.000000f, 1.000000f, 1.000000f,
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
