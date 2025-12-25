// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Ignore components on <see cref="GameObject"/>s that are inactive.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreInactiveAttribute : AutoReferenceValidatorAttribute {
        protected override int PriorityOrder => FilterOrder.PreFilter;

        protected override Type TypeConstraint => Types.Component;

        protected override bool Validate(in FieldContext context, Object value) {
            return ((Component)value).gameObject.activeInHierarchy;
        }
    }
}
