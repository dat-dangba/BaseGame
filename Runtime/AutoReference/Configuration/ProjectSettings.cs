// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.


using System;

namespace Teo.AutoReference.Configuration {
    [Serializable]
    internal class ProjectSettings {
        public bool syncOnInspect;
        public bool syncOnSceneSave;
        public bool syncOnAssemblyReload;
        public bool syncScenesOnBuild;
        public bool failBuildOnError;
        public bool failBuildOnWarnings;

        public bool skipSyncOnFilterError;

        public bool enableEditorToolboxIntegration;
        public bool enableTriInspectorIntegration;
        public bool enableOdinInspectorIntegration;

        public ProjectSettings() {
            ResetToDefaults();
        }

        public void ResetToDefaults() {
            syncOnInspect = Defaults.SyncOnInspect;
            syncOnSceneSave = Defaults.SyncOnSceneSave;
            syncOnAssemblyReload = Defaults.SyncOnAssemblyReload;
            syncScenesOnBuild = Defaults.SyncScenesOnBuild;
            failBuildOnError = Defaults.FailBuildOnError;
            failBuildOnWarnings = Defaults.FailBuildOnWarnings;

            skipSyncOnFilterError = Defaults.SkipSyncOnFilterError;

            enableEditorToolboxIntegration = Defaults.EnableUnityToolboxIntegration;
            enableTriInspectorIntegration = Defaults.EnableTriInspectorIntegration;
            enableOdinInspectorIntegration = Defaults.EnableOdinInspectorIntegration;
        }

        public static class Defaults {
            public const bool SyncOnInspect = true;
            public const bool SyncOnSceneSave = true;
            public const bool SyncOnAssemblyReload = true;
            public const bool SkipSyncOnFilterError = false;
            public const bool SyncScenesOnBuild = true;
            public const bool FailBuildOnError = false;
            public const bool FailBuildOnWarnings = false;

            public const bool EnableUnityToolboxIntegration = true;
            public const bool EnableTriInspectorIntegration = true;
            public const bool EnableOdinInspectorIntegration = true;
        }
    }
}
