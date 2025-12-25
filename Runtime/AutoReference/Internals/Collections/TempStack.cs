// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary stack that can be disposed of and reused.
    /// </summary>
    internal sealed class TempStack<T> : Stack<T>, IDisposable {
        private static readonly Stack<TempStack<T>> Pool = new Stack<TempStack<T>> (4);

        private TempStack() { }

        /// <summary>
        /// Indicates if the stack is currently in the pool.
        /// </summary>
        public bool IsPooled { get; private set; }

        /// <summary>
        /// Releases the <c>TempStack</c> object back to the pool. If it's already in the pool, it does nothing.
        /// </summary>
        public void Dispose() {
            if (IsPooled) {
                return;
            }

            IsPooled = true;

            Clear();
            Pool.Push(this);
        }

        /// <summary>
        /// Gets a <c>TempStack</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempStack<T> Get() {
            if (Pool.TryPop(out var stack)) {
                stack.Clear();
            } else {
                stack = new TempStack<T>();
            }

            stack.IsPooled = false;
            return stack;
        }
    }
}
