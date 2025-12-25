// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using UnityEditor;

namespace Teo.AutoReference.Editor.Window {
    internal partial class AutoReferenceWindowContent {
        private static (ReportInfo[], StatisticsInfo) GetReports() {
            using var results = TempList<ReportInfo>.Get();

            using var progress = ProgressBar.Begin("Initializing...");

            var monoTypes = GetMonoTypes();

            if (monoTypes.Length == 0) {
                return (Array.Empty<ReportInfo>(), default);

            }

            progress.Title = "Scanning...";
            progress.Count = monoTypes.Length;

            var stats = new StatisticsInfo {
                totalTypes = monoTypes.Length,
            };

            var step = 0;
            foreach (var monoScript in monoTypes) {
                var type = monoScript.GetClass();

                var syncInfo = AutoReferenceResolver.GetAutoReferenceInfo(monoScript.GetClass());
                var messages = syncInfo.messages.Where(m => m.DeclaringType == type).ToArray();

                var callbacks = syncInfo.declaredCallbacksCount;
                var fields = syncInfo.autoReferenceFields.Count(field => field.DeclaringType == type);

                if (callbacks + fields > 0) {
                    stats.totalCallbacks += callbacks;
                    stats.totalFields += fields;
                    stats.totalRelevantTypes += 1;
                }

                if (messages.Length > 0) {
                    results.Add(new ReportInfo(monoScript, messages));
                }

                progress.Update(++step, monoScript.GetType().FullName);
            }

            foreach (var item in results.SelectMany(report => report.items)) {
                if (item.IsError) {
                    ++stats.totalErrors;
                } else {
                    ++stats.totalWarnings;
                }
            }

            return (results.ToArray(), stats);
        }

        internal struct StatisticsInfo {
            public int totalFields;
            public int totalTypes;
            public int totalRelevantTypes;
            public int totalCallbacks;
            public int totalErrors;
            public int totalWarnings;
        }

        private readonly struct ReportInfo {
            public readonly LogItem[] items;
            public readonly MonoScript script;

            public ReportInfo(MonoScript script, LogItem[] items) {
                this.script = script;
                this.items = items;
            }
        }
    }
}
