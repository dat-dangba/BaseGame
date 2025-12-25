// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Discard components whose transform is a child of the transform of any other component in the input.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class IgnoreNestedAttribute : AutoReferenceFilterAttribute {
        protected override int PriorityOrder => FilterOrder.PostFilter;

        protected override Type TypeConstraint => Types.Component;

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            using var components = values.Cast<Component>().ToTempList();

            using var uniqueTransforms = components.Select(c => c.transform).ToTempSet();

            uniqueTransforms.RemoveWhere(transform => {
                for (var parent = transform.parent; parent != null; parent = parent.parent) {
                    if (uniqueTransforms.Contains(parent)) {
                        return true;
                    }
                }

                return false;
            });

            foreach (var component in components) {
                if (uniqueTransforms.Contains(component.transform)) {
                    yield return component;
                }
            }
        }
    }
}
