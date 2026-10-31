using System;
using UnityEngine;

namespace CastleDestroyers.Platform
{
    /// <summary>Обёртка над Luna API. В редакторе — заглушки с логом.</summary>
    public static class PlatformBridge
    {
        private static bool gameEnded;

        public static void Init(Action onPause, Action onResume)
        {
#if UNITY_LUNA
            Luna.Unity.LifeCycle.OnPause += onPause;
            Luna.Unity.LifeCycle.OnResume += onResume;
#endif
        }

        public static void InstallFullGame()
        {
#if UNITY_LUNA
            Luna.Unity.Playable.InstallFullGame();
#else
            Debug.Log("[Platform] InstallFullGame (stub)");
#endif
        }

        /// <summary>Конец геймплея для сети. После рестарта с EndCard повторно не отправляется.</summary>
        public static void GameEnded()
        {
            if (gameEnded)
            {
                return;
            }
            gameEnded = true;
#if UNITY_LUNA
            Luna.Unity.LifeCycle.GameEnded();
#else
            Debug.Log("[Platform] GameEnded (stub)");
#endif
        }
    }
}
