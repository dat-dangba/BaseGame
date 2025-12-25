// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using Teo.AutoReference.Editor.Window;
using Teo.AutoReference.Internals;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {
    public static class AutoReferenceMenu {
        [MenuItem("CONTEXT/MonoBehaviour/Sync Auto-References")]
        private static void SyncContextMenu(MenuCommand command) {
            if (command.context is MonoBehaviour mono) {
                AutoReference.Sync(mono);
            }
        }

        [MenuItem("CONTEXT/MonoBehaviour/Sync Auto-References", true)]
        private static bool SyncContextMenuValidation(MenuCommand command) {
            return command.context is MonoBehaviour mono && AutoReference.HasSyncInformation(mono);
        }

        [MenuItem("Tools/Auto-Reference/Sync current Scene(s)", false, 0)]
        private static void SyncAllAutoReferences() {
            SceneOperations.SyncAllOpenScenes();
        }

        [MenuItem("Tools/Auto-Reference/Sync all Prefabs", false, 1)]
        private static void SyncAllAutoReferencesInPrefabs() {
            AssetOperations.SyncAllPrefabs();
        }

        [MenuItem("Tools/Auto-Reference/Sync all Scenes in Project", false, 100)]
        private static void SyncAllAutoReferencesInEverySavedScene() {
            SceneOperations.SyncAllProjectScenes();
        }

        [MenuItem("Tools/Auto-Reference/Sync all Scenes included in Build", false, 101)]
        private static void SyncAllAutoReferencesInEverySceneIncludedInBuild() {
            SceneOperations.SyncAllBuildScenes();
        }

        [MenuItem("Tools/Auto-Reference/Clear Cache", true, 200)]
        private static bool CleanAutoReferenceCacheValidation() {
            return SyncPreferences.CacheSyncInfo
                   && AutoReferenceResolver.CacheSize + SyncObserverResolver.CacheSize > 0;
        }

        [MenuItem("Tools/Auto-Reference/Clear Cache", false, 200)]
        private static void CleanAutoReferenceCache() {
            AutoReference.ClearCache();
        }

        [MenuItem("Tools/Auto-Reference/Auto-Reference Window", false, 300)]
        private static void OpenAutoReferenceWindow() {
            AutoReferenceWindow.Open();
        }
    }
}
