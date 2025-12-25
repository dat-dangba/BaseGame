// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Wraps a single-argument comparable method into an <c>IComparer&lt;Object&gt;</c>. The method must return
    /// <see cref="int"/> but may accept any type that inherits from <see cref="UnityEngine.Object"/>.
    /// </summary>
    /// <remarks>
    /// Not thread-safe even among different instance.
    /// </remarks>
    internal class DynamicCompareTo : IComparer<Object> {
        private readonly MethodInfo _compareMethod;

        public DynamicCompareTo(MethodInfo compareMethod) {
            _compareMethod = compareMethod;
        }

        public int Compare(Object lhs, Object rhs) {
            return (int)_compareMethod.Invoke(lhs, Types.TempParams(rhs));
        }
    }
}
