using UnityEngine;

namespace CastleDestroyers.InputRig
{
    /// <summary>Первый палец или мышь. После касания мышь игнорируется: браузер дублирует тач событиями мыши.</summary>
    public sealed class PointerReader
    {
        private const float MouseAfterTouchGuard = 0.5f;

        private float lastTouchTime = -1f;
        private bool touchHeld;

        /// <summary>Палец или кнопка внизу. Тап короче кадра считается нажатием на один кадр.</summary>
        public bool Pressed { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        public void Read()
        {
            Pressed = false;
            float now = Time.unscaledTime;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                X = touch.position.x;
                Y = touch.position.y;
                bool held = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                Pressed = held || !touchHeld;
                touchHeld = held;
                lastTouchTime = now;
                return;
            }
            touchHeld = false;
            if (now - lastTouchTime > MouseAfterTouchGuard && (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)))
            {
                Vector3 mouse = Input.mousePosition;
                X = mouse.x;
                Y = mouse.y;
                Pressed = true;
            }
        }
    }
}
