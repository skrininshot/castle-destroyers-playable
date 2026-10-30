namespace CastleDestroyers.CameraRig
{
    /// <summary>Кадр камеры: точка фокуса и ширина видимой области, метры.</summary>
    public sealed class CameraFrame
    {
        public float x;
        public float y;
        public float width;

        public void Set(float frameX, float frameY, float frameWidth)
        {
            x = frameX;
            y = frameY;
            width = frameWidth;
        }
    }

    /// <summary>Чистые расчёты кадра, без зависимости от сцены.</summary>
    public static class CameraFraming
    {
        /// <summary>
        /// Половина высоты ортографической камеры (orthographicSize), при которой по ширине
        /// помещается frameWidth, а по высоте — не меньше minHeight. В ландшафте ограничивает minHeight.
        /// </summary>
        public static float OrthoSize(float frameWidth, float minHeight, float aspect)
        {
            float safeAspect = aspect < 0.01f ? 0.01f : aspect;
            float byWidth = frameWidth * 0.5f / safeAspect;
            float byHeight = minHeight * 0.5f;
            return byWidth > byHeight ? byWidth : byHeight;
        }
    }
}
