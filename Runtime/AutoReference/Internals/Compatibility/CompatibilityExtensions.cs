// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
#if !UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
#endif

#if UNITY_2021_1
using UnityEngine;
#endif

namespace Teo.AutoReference.Internals.Compatibility {

    public static class CompatibilityExtensions {
#if UNITY_2021_1
        public static Component GetComponentInParent(this Transform transform, Type type, bool includeInactive) {
            return transform.gameObject.GetComponentInParent(type, includeInactive);
        }

        public static Component GetComponentInParent(this Component transform, Type type, bool includeInactive) {
            return transform.gameObject.GetComponentInParent(type, includeInactive);
        }
#endif

#if !UNITY_2021_2_OR_NEWER
        public static bool TryPop<T>(this Stack<T> stack, out T result) {
            if (stack.Count == 0) {
                result = default;
                return false;
            }
            result = stack.Pop();
            return true;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }
            if (count <= 0) {
                yield break;
            }

            var queue = new Queue<T>(count);
            foreach (var item in source) {
                if (queue.Count == count) {
                    queue.Dequeue();
                }
                queue.Enqueue(item);
            }

            foreach (var item in queue) {
                yield return item;
            }
        }

#endif
    }
}
