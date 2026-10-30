using UnityEngine;

namespace CastleDestroyers.InputRig
{
    /// <summary>Первый палец или мышь. После касания мышь игнорируется: браузер дублирует тач событиями мыши.</summary>
    public sealed class PointerReader
    {
        private const float MouseAfterTouchGuard = 0.5f;

        private float lastTouchTime = -1f;

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
                Pressed = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                lastTouchTime = now;
            }
            else if (now - lastTouchTime > MouseAfterTouchGuard && Input.GetMouseButton(0))
            {
                Vector3 mouse = Input.mousePosition;
                X = mouse.x;
                Y = mouse.y;
                Pressed = true;
            }
        }
    }
}
