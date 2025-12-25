// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Remove all duplicate references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class UniqueAttribute : AutoReferenceFilterAttribute {
        protected override int PriorityOrder => FilterOrder.PreSort;

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.Distinct();
        }
    }
}
