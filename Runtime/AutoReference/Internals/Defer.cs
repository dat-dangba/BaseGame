// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals {
    public static class Defer {
        public static IDisposable Empty() {
            return DeferAction.Get(null);
        }

        public static IDisposable Register(Action action) {
            return DeferAction.Get(action);
        }

        public static IDisposable Register<T>(T data, Action<T> action) {
            return DeferAction<T>.Get(data, action);
        }

        private class DeferAction : IDisposable {
            private static readonly Stack<DeferAction> Pool = new Stack<DeferAction>(4);
            private Action _callback;
            private bool _isDisposed;

            public void Dispose() {
                if (_isDisposed) {
                    return;
                }

                _isDisposed = true;

                _callback?.Invoke();
                _callback = null;

                Pool.Push(this);
            }

            public static DeferAction Get(Action callback) {
                var deferAction = Pool.TryPop(out var result) ? result : new DeferAction();
                deferAction._isDisposed = false;
                deferAction._callback = callback;

                return deferAction;
            }
        }

        private class DeferAction<T> : IDisposable {
            private static readonly Stack<DeferAction<T>> Pool = new Stack<DeferAction<T>>(4);
            private Action<T> _callback;
            private T _data;
            private bool _isDisposed;

            public void Dispose() {
                if (_isDisposed) {
                    return;
                }

                _isDisposed = true;

                _callback?.Invoke(_data);
                _callback = null;
                _data = default;

                Pool.Push(this);
            }

            public static DeferAction<T> Get(T data, Action<T> callback) {
                var deferAction = Pool.TryPop(out var result) ? result : new DeferAction<T>();
                deferAction._isDisposed = false;
                deferAction._callback = callback;
                deferAction._data = data;

                return deferAction;
            }
        }
    }
}
