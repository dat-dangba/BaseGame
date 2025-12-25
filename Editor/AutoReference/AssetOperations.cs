// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEngine;


#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;

#else
using UnityEditor.SceneManagement;
#endif

namespace Teo.AutoReference.Editor {

    public static class AssetOperations {
        public static readonly string[] AssetsFolder = { "Assets" };

        internal static bool SyncOpenedPrefab(GameObject prefab) {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);
            using var behaviours = TempList<MonoBehaviour>.Get();

            prefab.GetComponentsInChildren(true, behaviours);

            var status = SyncStatus.None;
            var wereChangesMade = false;

            foreach (var behaviour in behaviours) {
                status |= AutoReference.Sync(behaviour);
                wereChangesMade = wereChangesMade || EditorUtility.IsDirty(behaviour);
            }

            LogContext.AppendStatusSummary(status);

            return wereChangesMade;
        }

        public static void SyncPrefab(GameObject prefab) {
            var path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) {
                return;
            }
            var stage = PrefabStageUtility.GetCurrentPrefabStage();

            if (stage != null && string.Equals(stage.assetPath, path, StringComparison.OrdinalIgnoreCase)) {
                SyncOpenedPrefab(stage.prefabContentsRoot);
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            var changed = SyncOpenedPrefab(root);
            if (changed) {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }


        public static void SyncAllPrefabs() {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            var status = SyncStatus.None;

            var guids = AssetDatabase.FindAssets("t:Prefab", AssetsFolder);
            using var behaviours = TempList<MonoBehaviour>.Get();
            using var progress = ProgressBar.Begin("Syncing Auto-References in Prefabs", guids.Length);

            for (var index = 0; index < guids.Length; index++) {
                var guid = guids[index];

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                progress.Update(index, path);

                behaviours.Clear();
                prefab.GetComponentsInChildren(true, behaviours);

                var wereChangesMade = false;

                foreach (var behaviour in behaviours) {
                    status |= AutoReference.Sync(behaviour);
                    wereChangesMade = wereChangesMade || EditorUtility.IsDirty(behaviour);
                }

                if (wereChangesMade) {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }

            LogContext.AppendStatusSummary(status);
        }
    }
}
