using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.CameraRig
{
    /// <summary>
    /// Параллакс фона (ТЗ §3.10): коэффициент слоя — доля движения камеры, которую слой отрабатывает на экране.
    /// 0 — слой неподвижен на экране, 1 — движется вместе с уровнем. Зум учитывается так же: дальние слои
    /// масштабируются слабее. В центральном кадре все слои стоят на своих местах.
    /// </summary>
    public sealed class ParallaxBackground : ITickable
    {
        private GameConfig config;
        private Camera cam;
        private Transform camTransform;
        private Transform[] layers;
        private Vector3 position;
        private Vector3 scale;

        public void Init(GameConfig gameConfig, Camera camera, Transform[] layerTransforms)
        {
            config = gameConfig;
            cam = camera;
            camTransform = camera.transform;
            layers = layerTransforms;
            scale.z = 1f;
            Apply();
        }

        public void Tick(float dt)
        {
            Apply();
        }

        /// <summary>Ставит слои по текущей камере. Отдельно нужен после мгновенного Snap камеры.</summary>
        public void Apply()
        {
            float cameraX = camTransform.position.x;
            float cameraY = camTransform.position.y;
            float referenceX = config.centerFocus.x;
            float referenceY = config.centerFocus.y;
            float referenceSize = CameraFraming.OrthoSize(config.centerFrameWidth, config.minFrameHeight, cam.aspect);
            float zoom = cam.orthographicSize / referenceSize;
            float[] factors = config.parallaxFactors;
            for (int i = 0; i < layers.Length; i++)
            {
                float factor = factors[i];
                float layerScale = LayerScale(zoom, factor);
                position.x = LayerOffset(cameraX, referenceX, factor, layerScale);
                position.y = LayerOffset(cameraY, referenceY, factor, layerScale);
                scale.x = layerScale;
                scale.y = layerScale;
                layers[i].position = position;
                layers[i].localScale = scale;
            }
        }

        /// <summary>Масштаб слоя: при коэффициенте 1 — без изменений, при 0 — полностью компенсирует зум камеры.</summary>
        public static float LayerScale(float zoom, float factor)
        {
            return Mathf.Pow(zoom, 1f - factor);
        }

        /// <summary>Сдвиг слоя по оси. Точка reference слоя попадает на экран со смещением -factor * (camera - reference).</summary>
        public static float LayerOffset(float camera, float reference, float factor, float layerScale)
        {
            return camera - factor * (camera - reference) - layerScale * reference;
        }
    }
}
