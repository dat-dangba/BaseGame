// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Sort references by their instance ID.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class SortByIDAttribute : AutoReferenceFilterAttribute {
        protected override int PriorityOrder => FilterOrder.Sort;

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.OrderBy(o => o.GetInstanceID());
        }
    }
}
