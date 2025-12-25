// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary list that can be disposed of and reused.
    /// </summary>
    internal sealed class TempList<T> : List<T>, IDisposable {
        private static readonly Stack<TempList<T>> Pool = new Stack<TempList<T>>(4);

        private TempList() { }

        private TempList(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Indicates if the list is currently in the pool.
        /// </summary>
        public bool IsPooled { get; private set; }

        /// <summary>
        /// Releases the <c>TempList</c> object back to the pool. If it's already in the pool, it does nothing.
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
        /// Gets a <c>TempList</c> object from the pool or creates a new one if pool is empty.
        /// </summary>
        public static TempList<T> Get() {
            if (Pool.TryPop(out var list)) {
                list.Clear();
            } else {
                list = new TempList<T>();
            }

            list.IsPooled = false;
            return list;
        }

        /// <summary>
        /// Gets a <c>TempList</c> object from the pool or creates a new one if pool is empty, then fills the list
        /// with the elements in the specified collection.
        /// </summary>
        public static TempList<T> Get(IEnumerable<T> collection) {
            if (Pool.TryPop(out var list)) {
                list.Clear();
                list.AddRange(collection);
            } else {
                list = new TempList<T>(collection);
            }

            list.IsPooled = false;
            return list;
        }
    }
}
