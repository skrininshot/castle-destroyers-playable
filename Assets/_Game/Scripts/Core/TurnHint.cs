using System;
using UnityEngine;

namespace CastleDestroyers.Core
{
    /// <summary>Подсказка к ходу игрока (ТЗ §3.1.1). Данные в конфиге, показывает TurnGuide.</summary>
    [Serializable]
    public sealed class TurnHint
    {
        [Tooltip("Название подсказки для debug-оверлея и плашки.")]
        public string label = "";
        [Tooltip("Множитель бустера, который подсвечивается (0 = без подсветки).")]
        public int highlightBoosterMultiplier;
        [Tooltip("Показывать руку-подсказку.")]
        public bool showHand = true;
        [Range(0f, 90f)]
        [Tooltip("Угол прицела руки и автовыстрела, градусы.")]
        public float handAngleDeg = 45f;
        [Range(0f, 1f)]
        [Tooltip("Сила прицела руки и автовыстрела, доля от максимума.")]
        public float handPower01 = 0.8f;
        [Tooltip("В начале хода камера показывает гору: подсвеченный бустер не виден с замка.")]
        public bool panToBooster;

        public TurnHint()
        {
        }

        public TurnHint(string label, int highlightBoosterMultiplier, bool showHand, float handAngleDeg, float handPower01, bool panToBooster = false)
        {
            this.label = label;
            this.highlightBoosterMultiplier = highlightBoosterMultiplier;
            this.showHand = showHand;
            this.handAngleDeg = handAngleDeg;
            this.handPower01 = handPower01;
            this.panToBooster = panToBooster;
        }
    }

    /// <summary>Хороший выстрел ИИ: угол и сила, к которым добавляется шум сложности.</summary>
    [Serializable]
    public sealed class AiShotPreset
    {
        [Tooltip("Название для debug-оверлея.")]
        public string label = "";
        [Range(0f, 90f)]
        [Tooltip("Угол выстрела, градусы.")]
        public float angleDeg = 45f;
        [Range(0f, 1f)]
        [Tooltip("Сила выстрела, доля от максимума.")]
        public float power01 = 0.7f;

        public AiShotPreset()
        {
        }

        public AiShotPreset(string label, float angleDeg, float power01)
        {
            this.label = label;
            this.angleDeg = angleDeg;
            this.power01 = power01;
        }
    }

    /// <summary>Бустер уровня: множитель и положение центра, метры.</summary>
    [Serializable]
    public sealed class BoosterSpec
    {
        [Tooltip("Во сколько раз увеличивается число ракет.")]
        public int multiplier = 10;
        [Tooltip("Центр бустера в мировых координатах, метры.")]
        public Vector2 position;

        public BoosterSpec()
        {
        }

        public BoosterSpec(int multiplier, float x, float y)
        {
            this.multiplier = multiplier;
            position = new Vector2(x, y);
        }
    }
}
