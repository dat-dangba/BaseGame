// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Wraps a compare method into an <c>IComparer&lt;Object&gt;</c>. The method must return <see cref="int"/> but may
    /// accept any type that inherits from <see cref="UnityEngine.Object"/>.
    /// </summary>
    internal class DynamicComparer : IComparer<Object> {
        private readonly MethodInfo _compareMethod;
        private object _comparerInstance;

        public DynamicComparer(MethodInfo compareMethod, object instance = null) {
            _compareMethod = compareMethod;
            _comparerInstance = instance;
        }

        public object Target {
            get => _comparerInstance;
            set => _comparerInstance = value;
        }

        public int Compare(Object lhs, Object rhs) {
            return (int)_compareMethod.Invoke(_comparerInstance, Types.TempParams(lhs, rhs));
        }
    }
}
