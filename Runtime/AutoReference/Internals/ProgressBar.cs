// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using UnityEditor;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Disposable wrapper for Unity's EditorUtility.DisplayProgressBar. Auto-clears and pools for reuse on disposal.
    /// </summary>
    internal class ProgressBar : IDisposable {
        private static readonly Stack<ProgressBar> Pool = new Stack<ProgressBar>(1);
        private bool _isDisposed;

        public int Count { get; set; }

        public string Title { get; set; }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }

            _isDisposed = true;
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
            Pool.Push(this);
        }

        public static ProgressBar Begin(string title, int count, bool showImmediately = false) {
            count = Math.Max(count, 1);
            if (!Pool.TryPop(out var progress)) {
                progress = new ProgressBar();
            }

            progress.Title = title;
            progress.Count = count;
            progress._isDisposed = false;

#if UNITY_EDITOR
            if (showImmediately) {
                EditorUtility.DisplayProgressBar(title, "", 0);
            }
#endif

            return progress;
        }

        public static ProgressBar Begin(string title, bool showImmediately = false) {
            return Begin(title, 0, showImmediately);
        }

        public void Update(int step, string item) {
            if (_isDisposed) {
                return;
            }
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar(
                Title,
                $"{step + 1}/{Count}: {item}", step / (float)Count);
#endif
        }
    }
}
