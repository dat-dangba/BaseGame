// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Editor {
    public static class Layout {
        public static IDisposable Horizontal(params GUILayoutOption[] options) {
            return TempLayout.Make(isHorizontal: true, options: options);
        }

        public static IDisposable Vertical(params GUILayoutOption[] options) {
            return TempLayout.Make(isHorizontal: false, options: options);
        }

        public static IDisposable Horizontal(GUIStyle style, params GUILayoutOption[] options) {
            return TempLayout.Make(isHorizontal: true, style, options);
        }

        public static IDisposable Vertical(GUIStyle style, params GUILayoutOption[] options) {
            return TempLayout.Make(isHorizontal: false, style, options);
        }

        public static IDisposable Disabled(bool value) {
            return TempDisabled.Make(value);
        }

        private class TempDisabled : IDisposable {
            private bool _isDisposed;
            private bool _enabledBefore;

            private static readonly Stack<TempDisabled> Pool = new Stack<TempDisabled>();

            private TempDisabled() { }

            public void Dispose() {
                if (_isDisposed) {
                    return;
                }
                _isDisposed = true;
                GUI.enabled = _enabledBefore;
            }

            public static TempDisabled Make(bool value) {
                var disabled = Pool.TryPop(out var result) ? result : new TempDisabled();
                disabled._enabledBefore = GUI.enabled;
                disabled._isDisposed = false;
                GUI.enabled = !value;
                return disabled;
            }
        }

        private class TempLayout : IDisposable {
            private bool _isHorizontal;
            private bool _isDisposed;
            private static readonly Stack<TempLayout> Pool = new Stack<TempLayout>();

            private TempLayout() { }

            public static TempLayout Make(bool isHorizontal, GUIStyle style = null, params GUILayoutOption[] options) {
                var layout = Pool.TryPop(out var result) ? result : new TempLayout();
                layout._isHorizontal = isHorizontal;
                layout._isDisposed = false;

                if (isHorizontal) {
                    if (style == null) {
                        GUILayout.BeginHorizontal(options);
                    } else {
                        GUILayout.BeginHorizontal(style, options);
                    }
                } else {
                    if (style == null) {
                        GUILayout.BeginVertical(options);
                    } else {
                        GUILayout.BeginVertical(style, options);
                    }
                }

                return layout;
            }

            public void Dispose() {
                if (_isDisposed) {
                    return;
                }
                _isDisposed = true;
                Pool.Push(this);

                if (_isHorizontal) {
                    GUILayout.EndHorizontal();
                } else {
                    GUILayout.EndVertical();
                }
            }
        }
    }
}
