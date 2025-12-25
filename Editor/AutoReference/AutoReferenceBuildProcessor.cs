// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using Teo.AutoReference.System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Teo.AutoReference.Editor {
    public class AutoReferenceBuildProcessor : IPreprocessBuildWithReport {
        public int callbackOrder => 0;

        private const SyncStatus FailOnErrorFlags =
            SyncStatus.Unsupported | SyncStatus.UsageError | SyncStatus.RuntimeError;

        private const SyncStatus FailOnWarningFlags = FailOnErrorFlags | SyncStatus.UsageWarning;

        public void OnPreprocessBuild(BuildReport report) {
            if (!SyncPreferences.SyncScenesOnBuild) {
                return;
            }

            var failFlags = SyncStatus.None;
            if (SyncPreferences.FailBuildOnWarnings) {
                failFlags = FailOnWarningFlags;
            } else if (SyncPreferences.FailBuildOnError) {
                failFlags = FailOnErrorFlags;
            }

            var result = SceneOperations.SyncAllBuildScenes();

            if (!result.HasAny(failFlags)) {
                return;
            }

            var why = result.HasAny(FailOnErrorFlags) ? "errors" : "warnings";

            throw new BuildFailedException($"Build will not proceed because Auto-Reference syncing contained {why}.");
        }
    }
}
