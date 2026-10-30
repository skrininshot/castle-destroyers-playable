using UnityEditor;
using UnityEditor.Build;

namespace CastleDestroyers.EditorTools
{
    /// <summary>
    /// Следит, чтобы символ GAME_DEBUG был определён в dev-сборках (Editor, Standalone, WebGL).
    /// Релизная сборка этапа 9 снимает символ у WebGL; до тех пор debug-панель попадает и в Luna-превью.
    /// </summary>
    [InitializeOnLoad]
    public static class DebugDefines
    {
        public const string Symbol = "GAME_DEBUG";

        static DebugDefines()
        {
            Ensure(NamedBuildTarget.Standalone);
            Ensure(NamedBuildTarget.WebGL);
        }

        private static void Ensure(NamedBuildTarget target)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(target);
            if (HasSymbol(defines, Symbol))
            {
                return;
            }
            string updated = string.IsNullOrEmpty(defines) ? Symbol : defines + ";" + Symbol;
            PlayerSettings.SetScriptingDefineSymbols(target, updated);
        }

        public static bool HasSymbol(string defines, string symbol)
        {
            if (string.IsNullOrEmpty(defines))
            {
                return false;
            }
            string[] parts = defines.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == symbol)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
