namespace CastleDestroyers.EditorTools
{
    /// <summary>Конец этапа одним запуском: каждый -executeMethod стоит 5+ минут из-за таймаута Playworks.</summary>
    public static class StageBatch
    {
        public static void BuildAndCapture()
        {
            GameSceneBuilder.Build();
            GameCapture.CaptureAll();
        }
    }
}
