using System;
using UnityEngine;

namespace CastleDestroyers.Core
{
    /// <summary>Подсказка к ходу игрока (ТЗ §3.1.1). Данные в конфиге; визуал рук и подсветки появится на этапе 6.</summary>
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

        public TurnHint()
        {
        }

        public TurnHint(string label, int highlightBoosterMultiplier, bool showHand, float handAngleDeg, float handPower01)
        {
            this.label = label;
            this.highlightBoosterMultiplier = highlightBoosterMultiplier;
            this.showHand = showHand;
            this.handAngleDeg = handAngleDeg;
            this.handPower01 = handPower01;
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
