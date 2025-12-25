// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Sort components by their world distance to the <see cref="GameObject"/> of the current script.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SortByDistanceAttribute : AutoReferenceFilterAttribute {
        protected override int PriorityOrder => FilterOrder.Sort;

        protected override Type TypeConstraint => Types.Component;

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            var behaviour = context.Behaviour;
            return values.OrderBy(o => {
                    var component = (Component)o;
                    return Vector3.Distance(behaviour.transform.position, component.transform.position);
                }
            );
        }
    }
}
