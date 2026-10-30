using UnityEngine;

namespace CastleDestroyers.Spike
{
    /// <summary>Параметры технического спайка этапа 0.</summary>
    [CreateAssetMenu(fileName = "SpikeConfig", menuName = "Castle Destroyers/Spike Config")]
    public class SpikeConfig : ScriptableObject
    {
        [Header("Маска")]
        [Tooltip("Сторона маски в текселях")]
        [Range(128, 1024)] public int maskSize = 512;
        [Tooltip("Сторона тайла в текселях")]
        [Range(32, 256)] public int tileSize = 128;
        [Tooltip("Сторона маски в мировых единицах")]
        [Range(1f, 30f)] public float worldSize = 10f;
        [Tooltip("Радиус кратера в текселях")]
        [Range(2, 64)] public int craterRadius = 14;
        [Tooltip("Ширина тёмной каймы в текселях")]
        [Range(1, 32)] public int rimWidth = 6;
        [Tooltip("Лимит аплоадов тайлов за кадр")]
        [Range(1, 16)] public int maxTileUploadsPerFrame = 4;
        [Tooltip("Сколько кратеров в кадр ставит автотест")]
        [Range(1, 16)] public int autoCratersPerFrame = 3;
        [Tooltip("Доля материала, ниже которой автотест восстанавливает маску")]
        [Range(0.05f, 0.9f)] public float autoResetSolidRatio = 0.35f;
        [Tooltip("Интервал кратеров при удержании пальца, с")]
        [Range(0.01f, 0.5f)] public float pointerStampInterval = 0.05f;

        [Header("Ракеты")]
        [Tooltip("Ёмкость буферов ракет")]
        [Range(100, 2000)] public int rocketCapacity = 1000;
        [Tooltip("Рабочее число ракет в GO-тесте")]
        [Range(100, 2000)] public int rocketsGo = 600;
        [Tooltip("Длина ракеты, м")]
        [Range(0.05f, 1f)] public float rocketLength = 0.28f;
        [Tooltip("Ширина ракеты, м")]
        [Range(0.02f, 0.5f)] public float rocketWidth = 0.1f;
        [Tooltip("Минимальная скорость запуска, м/с")]
        [Range(1f, 30f)] public float launchSpeedMin = 7f;
        [Tooltip("Максимальная скорость запуска, м/с")]
        [Range(1f, 30f)] public float launchSpeedMax = 11f;
        [Tooltip("Гравитация, м/с²")]
        [Range(0f, 20f)] public float gravity = 4f;
        [Tooltip("Амплитуда синусоиды, м")]
        [Range(0f, 2f)] public float waveAmplitude = 0.35f;
        [Tooltip("Частота синусоиды, рад/с")]
        [Range(0f, 40f)] public float waveFrequency = 9f;
        [Tooltip("Сдвиг фазы между соседними ракетами, рад")]
        [Range(0f, 3.2f)] public float wavePhaseStep = 0.7f;
        [Tooltip("Время жизни ракеты, с")]
        [Range(0.5f, 10f)] public float rocketLifetime = 4f;

        [Header("Дым")]
        [Tooltip("Ёмкость буферов дыма")]
        [Range(100, 4000)] public int smokeCapacity = 2000;
        [Tooltip("Время жизни частицы, с")]
        [Range(0.2f, 5f)] public float smokeLifetime = 1.6f;
        [Tooltip("Размер частицы на старте, м")]
        [Range(0.05f, 2f)] public float smokeStartSize = 0.15f;
        [Tooltip("Размер частицы в конце, м")]
        [Range(0.05f, 3f)] public float smokeEndSize = 0.6f;
        [Tooltip("Скорость подъёма, м/с")]
        [Range(0f, 5f)] public float smokeRiseSpeed = 0.6f;
        [Tooltip("Разброс боковой скорости, м/с")]
        [Range(0f, 3f)] public float smokeDrift = 0.3f;

        [Header("Экран")]
        [Tooltip("Запас вокруг маски в кадре, множитель")]
        [Range(1f, 2f)] public float fitMargin = 1.05f;
        [Tooltip("Период обновления HUD, с")]
        [Range(0.1f, 2f)] public float hudRefreshInterval = 0.5f;
        [Tooltip("Максимальный шаг симуляции, с")]
        [Range(0.016f, 0.2f)] public float maxDeltaTime = 0.05f;
    }
}
