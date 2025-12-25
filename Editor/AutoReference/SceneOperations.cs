// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Teo.AutoReference.Editor {
    /// <summary>
    /// Provides various methods for synchronizing Auto-References in Unity scenes or prefabs.
    /// </summary>
    public static class SceneOperations {
        /// <summary>
        /// Syncs all Auto-References in every scene that is currently open.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        public static SyncStatus SyncAllOpenScenes() {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            using var loadedScenes = GetAllOpenScenes().Where(s => s.isLoaded).ToTempList();
            using var progress = ProgressBar.Begin("Syncing Auto-References in Open Scenes", loadedScenes.Count);

            var status = SyncStatus.None;

            for (var i = 0; i < loadedScenes.Count; ++i) {
                var scene = loadedScenes[i];
                progress.Update(i, scene.path);
                status |= SyncOpenScene(scene);
            }

            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Get all scene paths of scenes under the Assets folder
        /// </summary>
        private static IEnumerable<string> GetAllSavedScenePaths() {
            var guids = AssetDatabase.FindAssets("t:Scene", AssetOperations.AssetsFolder);
            return guids.Select(AssetDatabase.GUIDToAssetPath);
        }

        /// <summary>
        /// Get all scene paths of scenes included in the build settings.
        /// </summary>
        private static IEnumerable<string> GetAllBuildScenePaths(bool includeDisabled = false) {
            if (includeDisabled) {
                return EditorBuildSettings.scenes.Select(scene => scene.path);
            }

            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path);
        }

        /// <summary>
        /// Get all scenes that are currently open.
        /// </summary>
        public static IEnumerable<Scene> GetAllOpenScenes() {
            return Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt);
        }

        /// <summary>
        /// Get all scenes that are currently open and loaded.
        /// </summary>
        public static IEnumerable<Scene> GetAllLoadedScenes() {
            return GetAllOpenScenes().Where(s => s.isLoaded);
        }

        /// <summary>
        /// Get the paths of all open scenes that are stored in the project and are currently loaded.
        /// </summary>
        public static IEnumerable<string> GetAllLoadedScenePaths() {
            return GetAllLoadedScenes().Where(s => s.path != string.Empty).Select(s => s.path);
        }

        /// <summary>
        /// Get all scenes that are currently loaded but aren't stored in the project.
        /// Note that unstored does not imply scenes with unsaved changes.
        /// </summary>
        public static IEnumerable<Scene> GetAllUnstoredScenes() {
            return GetAllOpenScenes().Where(s => s is { isLoaded: true, path: "" });
        }

        internal static SyncStatus SyncOpenScene(Scene scene) {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            var status = ObjectUtils.EnumerateAllComponentsInScene<MonoBehaviour>(scene)
                .Aggregate(SyncStatus.None,
                    (current, behaviour) => current | AutoReference.Sync(behaviour)
                );

            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Sync all Auto-References in all scenes stored in the project under the Assets folder.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        public static SyncStatus SyncAllProjectScenes() {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);
            var status = SyncScenesByPath(GetAllSavedScenePaths(), true);
            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Sync all Auto-References in all scenes included in the build.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        public static SyncStatus SyncAllBuildScenes(bool includeDisabled = false) {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);
            var status = SyncScenesByPath(GetAllBuildScenePaths(includeDisabled));
            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Syncs all Auto-References that are present in a scene.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        /// <param name="scene">The target scene to sync.</param>
        public static SyncStatus SyncScene(Scene scene) {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            if (!scene.IsValid()) {
                return SyncStatus.UsageError;
            }

            if (scene.isLoaded) {
                return SyncOpenScene(scene);
            }

            var isOpenButNotLoaded = GetAllOpenScenes().Any(s => s.path == scene.path);
            var status = SyncClosedScene(scene.path, !isOpenButNotLoaded);
            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Syncs all Auto-References that are present in a scene given its path.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        /// <param name="scenePath">The path of the target scene to sync.</param>
        public static SyncStatus SyncScene(string scenePath) {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            var scene = SceneManager.GetSceneByPath(scenePath);
            var status = SyncScene(scene);
            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Sync a closed scene by first opening it, then syncing, and finally closing it again.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        private static SyncStatus SyncClosedScene(string scenePath, bool removeScene) {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!scene.IsValid()) {
                return SyncStatus.UsageError;
            }

            var status = SyncStatus.None;

            try {
                // Avoid syncing twice because the scene will be synced when saving anyway.
                if (!SyncPreferences.SyncOnSceneSave) {
                    status |= SyncOpenScene(scene);
                }

                if (!EditorSceneManager.SaveScene(scene)) {
                    Debug.LogError($"Failed to save '{scene.path}' after syncing; the scene will remain unsynced.");
                }
            } catch (Exception e) {
                status |= SyncStatus.RuntimeError;
                Debug.LogError($"Failed to sync '{scene.path}' - {e.GetType()}: {e.Message}");
            } finally {
                EditorSceneManager.CloseScene(scene, removeScene);
            }

            LogContext.AppendStatusSummary(status);
            return status;
        }

        /// <summary>
        /// Sync all Auto-References in the scenes provided.
        /// Returns true if no sync-related errors were encountered.
        /// </summary>
        /// <param name="scenesToSync">The saved paths to all the scenes to sync.</param>
        /// <param name="includeUnstored">
        ///   If set to true, also includes scenes that are created and open but not yet stored in the project on disk,
        ///   i.e. open scenes whose path is empty. Note that this does not imply scenes with unsaved changes.
        /// </param>
        public static SyncStatus SyncScenesByPath(IEnumerable<string> scenesToSync, bool includeUnstored = false) {
            using var _ = LogContext.MakeContextInternal(SyncPreferences.BatchLogLevel);

            // There are three types of scenes to sync, which we do so separately:
            // - Scenes that are loaded: They don't need to be reopened, loaded, or saved - just to sync.
            // - Scenes that are in the scene manager but unloaded: they need to be reloaded, saved, and unloaded.
            // - Scenes that are not in the scene manager: they need to be loaded, saved, closed AND removed.
            // We identify each group and act accordingly.

            using var closedScenesToSync = scenesToSync
                .Where(scenePath => !string.IsNullOrWhiteSpace(scenePath))
                .ToTempSet();

            using var loadedScenesToSync = TempList<Scene>.Get();
            using var notLoadedScenesToSync = TempList<string>.Get();

            foreach (var scene in GetAllOpenScenes()) {
                if (includeUnstored && scene.path == string.Empty) {
                    loadedScenesToSync.Add(scene);
                } else if (closedScenesToSync.Contains(scene.path)) {
                    if (scene.isLoaded) {
                        loadedScenesToSync.Add(scene);
                    } else {
                        notLoadedScenesToSync.Add(scene.path);
                    }

                    closedScenesToSync.Remove(scene.path);
                }
            }

            var totalSyncCount = loadedScenesToSync.Count + notLoadedScenesToSync.Count + closedScenesToSync.Count;
            using var progress = ProgressBar.Begin("Syncing Auto-References in Scenes", totalSyncCount);

            var step = 0;

            var status = SyncStatus.None;

            foreach (var scene in loadedScenesToSync) {
                progress.Update(step++, scene.path);
                status |= SyncOpenScene(scene);
            }

            // These scenes are open but not loaded, so we reload/sync/save/close them but without removing them.
            foreach (var scenePath in notLoadedScenesToSync) {
                progress.Update(step++, scenePath);
                status |= SyncClosedScene(scenePath, false);
            }

            // These scenes are stored in the project but aren't open, so we open/sync/save/close them.
            foreach (var scenePath in closedScenesToSync) {
                progress.Update(step++, scenePath);
                status |= SyncClosedScene(scenePath, true);
            }

            LogContext.AppendStatusSummary(status);
            return status;
        }
    }
}
