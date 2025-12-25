// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Teo.AutoReference.Editor {
    [InitializeOnLoad]
    internal static class AutoReferenceEventHook {

        public static void Refresh() {
            EditorSceneManager.sceneSaving -= SyncScene;
            PrefabStage.prefabSaving -= OnPrefabSaved;
            if (SyncPreferences.SyncOnSceneSave) {
                EditorSceneManager.sceneSaving += SyncScene;
                PrefabStage.prefabSaving += OnPrefabSaved;
            }

            AssemblyReloadEvents.afterAssemblyReload -= SyncOpenScenes;
            if (SyncPreferences.SyncOnAssemblyReload) {
                AssemblyReloadEvents.afterAssemblyReload += SyncOpenScenes;
            }
        }

        static AutoReferenceEventHook() {
            Refresh();
        }

        private static void SyncOpenScenes() {
            SceneOperations.SyncAllOpenScenes();
        }

        private static void SyncScene(Scene scene, string path) {
            SceneOperations.SyncOpenScene(scene);
        }

        private static void OnPrefabSaved(GameObject prefabRoot) {
            var stage = PrefabStageUtility.GetPrefabStage(prefabRoot);
            if (stage == null) {
                return;
            }
            if (stage.mode != PrefabStage.Mode.InIsolation) {
                return;
            }

            AssetOperations.SyncOpenedPrefab(prefabRoot);
        }
    }
}
