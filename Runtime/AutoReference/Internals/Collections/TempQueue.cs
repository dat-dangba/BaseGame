// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary queue that can be disposed of and reused.
    /// </summary>
    internal sealed class TempQueue<T> : Queue<T>, IDisposable {
        private static readonly Stack<TempQueue<T>> Pool = new Stack<TempQueue<T>>(4);

        private TempQueue() { }

        /// <summary>
        /// Indicates if the queue is currently in the pool.
        /// </summary>
        public bool IsPooled { get; private set; }

        /// <summary>
        /// Releases the <c>TempQueue</c> object back to the pool. If it's already in the pool, it does nothing.
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
        /// Gets a <c>TempQueue</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempQueue<T> Get() {
            if (Pool.TryPop(out var queue)) {
                queue.Clear();
            } else {
                queue = new TempQueue<T>();
            }

            queue.IsPooled = false;
            return queue;
        }
    }
}
