// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary set that can be disposed of and reused.
    /// </summary>
    internal sealed class TempSet<T> : HashSet<T>, IDisposable {
        private static readonly Stack<TempSet<T>> Pool = new Stack<TempSet<T>>(4);

        private TempSet() { }

        private TempSet(IEnumerable<T> enumerable) : base(enumerable) { }

        /// <summary>
        /// Indicates if the list is currently in the pool.
        /// </summary>
        public bool IsPooled { get; private set; }

        /// <summary>
        /// Releases the <c>TempHashSet</c> object back to the pool. If it's already in the pool, it does nothing.
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
        /// Gets a <c>TempHashSet</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempSet<T> Get() {
            var set = Pool.TryPop(out var result) ? result : new TempSet<T>();
            set.Clear();

            set.IsPooled = false;
            return set;
        }

        /// <summary>
        /// Gets a <c>TempHashSet</c> object from the pool or creates a new one if pool is empty, then fills the list
        /// with the elements in the specified collection.
        /// </summary>
        public static TempSet<T> Get(IEnumerable<T> enumerable) {
            if (Pool.TryPop(out var set)) {
                set.Clear();
                set.UnionWith(enumerable);
            } else {
                set = new TempSet<T>(enumerable);
            }

            set.IsPooled = false;
            return set;
        }
    }
}
