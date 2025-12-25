// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

// This class is not used in builds, but it's referenced in code that should be stripped out.
// So we define two versions: one for the editor, and a dummy version for the compilation to succeed in builds.

using System.Diagnostics;

#if UNITY_EDITOR
using System.Collections.Specialized;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
#endif

namespace Teo.AutoReference.Configuration {
    internal static class Preprocessor {
#if UNITY_EDITOR
        private static readonly OrderedDictionary Symbols = new OrderedDictionary();
        private static bool _isDirty;

#if UNITY_2021_2_OR_NEWER
        private static readonly NamedBuildTarget CurrentTarget;
#else
        private static readonly BuildTargetGroup CurrentTarget;
#endif

        static Preprocessor() {
#if UNITY_2021_2_OR_NEWER
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            CurrentTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            PlayerSettings.GetScriptingDefineSymbols(CurrentTarget, out var existingSymbols);
#else
            CurrentTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(CurrentTarget, out var existingSymbols);
#endif

            foreach (var symbol in existingSymbols) {
                Symbols.Add(symbol, null);
            }
        }

        public static bool IsDirty => _isDirty;

        public static void ApplyIfChanged() {
            if (!_isDirty) {
                return;
            }

            var symbols = Symbols.Keys.Cast<string>().ToArray();
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(CurrentTarget, symbols);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(CurrentTarget, symbols);
#endif

            _isDirty = false;

            EditorUtility.RequestScriptReload();
        }

        [Conditional("UNITY_EDITOR")]
        public static void Set(string symbol, bool value) {
            if (value == Symbols.Contains(symbol)) {
                return;
            }

            if (value) {
                Symbols.Add(symbol, null);
            } else {
                Symbols.Remove(symbol);
            }

            _isDirty = true;
        }
#else // UNITY_EDITOR
        public static bool IsDirty => false;

        [Conditional("UNITY_EDITOR")]
        public static void ApplyIfChanged() { }

        [Conditional("UNITY_EDITOR")]
        public static void Set(string symbol, bool value) { }
#endif // UNITY_EDITOR
    }
}
