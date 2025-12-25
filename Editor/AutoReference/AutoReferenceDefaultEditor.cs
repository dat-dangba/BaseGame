// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System.Linq;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {
    [CanEditMultipleObjects]
#if !AUTOREF_DISABLE_ONINSPECT && !AUTOREF_ODIN_INSPECTOR && !AUTOREF_TRIINSPECTOR && !AUTOREF_EDITOR_TOOLBOX
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
#endif
    public sealed class AutoReferenceDefaultEditor : UnityEditor.Editor {
        private bool _syncEnabled = true;
        private bool _hasSyncInfo;

        private void OnEnable() {
            using (LogContext.MakeContextInternal(SyncPreferences.EditorSelectLogLevel)) {
                _hasSyncInfo = targets.Any(t => AutoReference.HasSyncInformation(t as MonoBehaviour));
                if (_hasSyncInfo) {
                    SyncAll();
                }
            }
        }

        private void SyncAll() {
            foreach (var t in targets) {
                AutoReference.Sync(t as MonoBehaviour);
            }
        }

        private void OnDisable() {
            if (_hasSyncInfo) {
                SyncAll();
            }
        }

        public override void OnInspectorGUI() {
            if (!_hasSyncInfo) {
                DrawDefaultInspector();
                return;
            }

            EditorGUI.BeginChangeCheck();
            var changed = DrawDefaultInspector();
            _syncEnabled = SyncToggle.DrawOverHeader(_syncEnabled);

            changed |= EditorGUI.EndChangeCheck();

            if (changed && _syncEnabled) {
                SyncAll();
            }
        }
    }
}
