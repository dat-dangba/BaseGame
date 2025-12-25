// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System.Linq;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {
    [CanEditMultipleObjects]
    public class AutoReferenceEditor : UnityEditor.Editor {
        private bool _hasSyncInfo;
        private bool _syncEnabled = true;

        protected virtual void OnEnable() {
            using (LogContext.MakeContextInternal(SyncPreferences.EditorSelectLogLevel)) {
                _hasSyncInfo = targets.Any(t => AutoReference.HasSyncInformation(t as MonoBehaviour));
                if (_hasSyncInfo) {
                    SyncAll();
                }
            }
        }

        protected void OnDisable() {
            if (_hasSyncInfo) {
                SyncAll();
            }
        }

        /// <summary>
        /// Draws the Auto-Reference header, which closely resembles the built-in Unity script field
        /// displayed in the default inspector. This header includes a toggle button that allows checking
        /// whether synchronization should occur when changes are made. If the MonoBehaviour does not
        /// contain any syncable fields or after-sync callbacks, the button will be omitted.
        /// </summary>
        /// <param name="behaviour">The MonoBehaviour for which the header will be drawn.</param>
        /// <param name="syncEnabled">Indicates whether synchronization is currently enabled.</param>
        /// <returns>
        /// Returns the updated value indicating whether synchronization is enabled. If the script does not
        /// contain any auto-reference information, this method always returns false.
        /// </returns>
        public static bool DrawSyncHeader(MonoBehaviour behaviour, bool syncEnabled) {
            if (!AutoReference.HasSyncInformation(behaviour)) {
                DrawDefaultHeader(behaviour);
                return false;
            }

            DrawDefaultHeader(behaviour);
            var rect = GUILayoutUtility.GetLastRect();
            return SyncToggle.DrawAtY(rect.y, syncEnabled);
        }

        public static void DrawDefaultHeader(MonoBehaviour behaviour) {
            using (Layout.Disabled(true)) {
                EditorGUILayout.ObjectField(
                    "Script",
                    MonoScript.FromMonoBehaviour(behaviour),
                    typeof(MonoScript),
                    false
                );
            }
        }

        private void DrawSyncHeaderInstance(MonoBehaviour behaviour) {
            DrawDefaultHeader(behaviour);
            var rect = GUILayoutUtility.GetLastRect();
            _syncEnabled = SyncToggle.DrawAtY(rect.y, _syncEnabled);
        }

        protected virtual void DrawCustomInspector() {
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }

        private void SyncAll() {
            foreach (var t in targets) {
                AutoReference.Sync(t as MonoBehaviour);
            }
        }

        public sealed override void OnInspectorGUI() {
            var mono = target as MonoBehaviour;
            if (mono == null) {
                EditorGUILayout.HelpBox("This script is not a MonoBehaviour", MessageType.Error);
                return;
            }

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            DrawSyncHeaderInstance(mono);
            DrawCustomInspector();

            var changed = serializedObject.ApplyModifiedProperties() | EditorGUI.EndChangeCheck();

            if (changed && _syncEnabled) {
                SyncAll();
            }
        }
    }
}
