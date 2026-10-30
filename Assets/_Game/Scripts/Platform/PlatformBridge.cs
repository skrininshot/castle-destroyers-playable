using System;
using UnityEngine;

namespace CastleDestroyers.Platform
{
    /// <summary>Обёртка над Luna API. В редакторе — заглушки с логом.</summary>
    public static class PlatformBridge
    {
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

        public static void GameEnded()
        {
#if UNITY_LUNA
            Luna.Unity.LifeCycle.GameEnded();
#else
            Debug.Log("[Platform] GameEnded (stub)");
#endif
        }
    }
}
