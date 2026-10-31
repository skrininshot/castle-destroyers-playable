namespace CastleDestroyers.EditorTools
{
    /// <summary>
    /// Конец этапа одним запуском. Выход явный: с -quit Unity ещё 300 с ждёт асинхронные операции Playworks.
    /// </summary>
    public static class StageBatch
    {
        public static void BuildAndCapture()
        {
            GameSceneBuilder.Build();
            GameCapture.CaptureAll();
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorApplication.Exit(0);
        }

        /// <summary>Только пересборка сцены, без долгого захвата скриншотов.</summary>
        public static void BuildScene()
        {
            GameSceneBuilder.Build();
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorApplication.Exit(0);
        }
    }
}
