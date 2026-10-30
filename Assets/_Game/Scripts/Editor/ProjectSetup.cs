using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CastleDestroyers.EditorTools
{
    /// <summary>Воспроизводимая настройка проекта из batchmode: URP-ассеты, цветовое пространство, пустая сцена.</summary>
    public static class ProjectSetup
    {
        private const string ConfigsDir = "Assets/_Game/Configs";
        private const string RendererPath = ConfigsDir + "/URP_Renderer.asset";
        private const string PipelinePath = ConfigsDir + "/URP_Pipeline.asset";
        public const string EmptyScenePath = "Assets/_Game/Scenes/Empty.unity";

        [MenuItem("Tools/Castle Destroyers/Setup URP")]
        public static void SetupRenderPipeline()
        {
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                UniversalRendererData rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            pipeline.supportsHDR = false;
            pipeline.msaaSampleCount = 1;
            pipeline.shadowDistance = 0f;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);

            // WebGL 1 без sRGB-расширений: только Gamma.
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            AssetDatabase.SaveAssets();
        }

        public static void CreateEmptyScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.45f, 0.72f, 0.92f);
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            EditorSceneManager.SaveScene(scene, EmptyScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(EmptyScenePath, true) };
        }

        /// <summary>Точка входа для -executeMethod.</summary>
        public static void SetupEmptyBuild()
        {
            SetupRenderPipeline();
            CreateEmptyScene();
            AssetDatabase.SaveAssets();
        }
    }
}
