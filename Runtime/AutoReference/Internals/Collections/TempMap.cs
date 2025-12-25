using System;
using System.Collections.Generic;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// A temporary dictionary that can be disposed of and reused.
    /// </summary>
    internal sealed class TempMap<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable {
        private static readonly Stack<TempMap<TKey, TValue>> Pool = new Stack<TempMap<TKey, TValue>>(4);

        private TempMap() { }

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
        public static TempMap<TKey, TValue> Get() {
            var map = Pool.TryPop(out var result) ? result : new TempMap<TKey, TValue>();
            map.Clear();

            map.IsPooled = false;
            return map;
        }
    }
}
