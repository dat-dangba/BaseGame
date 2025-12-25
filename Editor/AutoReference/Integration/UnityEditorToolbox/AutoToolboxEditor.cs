// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using Teo.AutoReference.System;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {
#if !AUTOREF_DISABLE_ONINSPECT
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
#endif
    public class AutoToolboxEditor : ToolboxEditor {
        private bool _syncEnabled = true;

        protected virtual void OnEnable() {
#if !AUTOREF_DISABLE_ONINSPECT
            using (LogContext.MakeContextInternal(SyncPreferences.EditorSelectLogLevel)) {
                AutoReference.Sync(target as MonoBehaviour);
            }
#endif
        }

        protected void OnDisable() {
#if !AUTOREF_DISABLE_ONINSPECT
            AutoReference.Sync(target as MonoBehaviour);
#endif
        }

        protected virtual void DrawCustomAutoInspector() {
            base.DrawCustomInspector();
        }

        public sealed override void DrawCustomInspector() {
            if (target is not MonoBehaviour mono) {
                base.DrawCustomInspector();
                return;
            }

#if AUTOREF_DISABLE_ONINSPECT
            base.DrawCustomAutoInspector();
#else

            EditorGUI.BeginChangeCheck();

            DrawCustomAutoInspector();
            _syncEnabled = SyncToggle.DrawOverHeader(_syncEnabled);

            if (EditorGUI.EndChangeCheck() && _syncEnabled) {
                AutoReference.Sync(mono);
            }
#endif
        }
    }
}
