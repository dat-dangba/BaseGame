// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Teo.AutoReference.Configuration;
using UnityEditor;
using UnityEngine;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.System {
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class LogContext {

        private static readonly List<Context> Contexts = new List<Context>();

        static LogContext() {
#if UNITY_EDITOR
            EditorApplication.update -= Cleanup;
            EditorApplication.update += Cleanup;
#endif
        }

        public static LogLevel CurrentLevel =>
            Contexts.Count > 0
                ? Contexts.Last().Level
                : SyncPreferences.DefaultLogLevel;

        public static ILogContext MakeContext(LogLevel level) {
#if UNITY_EDITOR
            return Context.Create(level, false);
#else
            return null;
#endif
        }

        internal static ILogContext MakeContextInternal(LogLevel level) {
#if UNITY_EDITOR
            return Context.Create(level, true);
#else
            return null;
#endif
        }

        internal static void AppendStatusSummary(SyncStatus status) {
#if UNITY_EDITOR
            if (CurrentLevel != LogLevel.Compact) {
                return;
            }

            _aggregatedStatus |= status;

            if (_isResultPending) {
                return;
            }

            _isResultPending = true;
#endif
        }

        private static void LogAggregatedStatus() {
#if UNITY_EDITOR
            const string suffix = "\nAdditional details can be found in the Auto-Reference window.";

            if (_aggregatedStatus.HasAny(SyncStatus.UsageError)) {
                Debug.LogError("Auto-Reference usage errors detected while syncing." + suffix);
            } else if (_aggregatedStatus.HasAny(SyncStatus.UsageWarning)) {
                Debug.LogWarning("Auto-Reference usage warnings detected while syncing." + suffix);
            }

            _aggregatedStatus = SyncStatus.None;
            _isResultPending = false;
#endif
        }

        private static void Cleanup() {
#if UNITY_EDITOR
            if (_isResultPending) {
                LogAggregatedStatus();
            }

            if (Contexts.Count <= 0) {
                return;
            }

            foreach (var context in Contexts) {
                context.DisposeNoRemove();
            }
            Contexts.Clear();
#endif
        }

        public interface ILogContext : IDisposable {
            public bool IsDisposed { get; }
        }

        private class Context : ILogContext {
            private static readonly Stack<Context> Pool = new Stack<Context>();
            private bool _shouldRecycle;

            public LogLevel Level { get; private set; }

            public void Dispose() {
                if (IsDisposed) {
                    return;
                }
                IsDisposed = true;

                var index = Contexts.LastIndexOf(this);
                if (index < 0) {
                    return;
                }

                for (var i = Contexts.Count - 1; i > index; --i) {
                    Contexts[i].DisposeNoRemove();
                }

                Contexts.RemoveRange(index, Contexts.Count - index);

                if (_shouldRecycle) {
                    Pool.Push(this);
                }
            }

            public bool IsDisposed { get; private set; } = false;

            public static Context Create(LogLevel level, bool recycle) {
                var context = Pool.TryPop(out var c) ? c : new Context();
                context.IsDisposed = false;
                context._shouldRecycle = recycle;
                context.Level = level.GetEffectiveLevel();
                Contexts.Add(context);
                return context;
            }

            public void DisposeNoRemove() {
                if (IsDisposed) {
                    return;
                }
                IsDisposed = true;

                if (_shouldRecycle) {
                    Pool.Push(this);
                }
            }
        }
#if UNITY_EDITOR
        private static SyncStatus _aggregatedStatus = SyncStatus.None;
        private static bool _isResultPending;
#endif
    }
}
