// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Reverse the order of all references in the input.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReverseAttribute : AutoReferenceFilterAttribute {
        protected override int PriorityOrder => FilterOrder.PostSort;

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.Reverse();
        }
    }
}
