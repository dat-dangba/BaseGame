// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Ignore components that are attached in the same game object as the current behaviour.
    /// Useful for <see cref="GetInChildrenAttribute"/>, <see cref="GetInParentAttribute"/>,
    /// <see cref="FindInSceneAttribute"/>, and <see cref="FindInParentAttribute"/>.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreSelfAttribute : AutoReferenceValidatorAttribute {
        protected override int PriorityOrder => FilterOrder.PreFilter;

        protected override Type TypeConstraint => Types.Component;

        /// <summary>
        /// Whether the validation should ignore only the current component instead of all components on the same game object.
        /// The default value is false.
        /// </summary>
        public bool ComponentOnly { get; set; } = false;

        protected override bool Validate(in FieldContext context, Object value) {
            return ComponentOnly
                ? !ReferenceEquals(context.Behaviour, value)
                : context.Behaviour.transform != ((Component)value).transform;
        }
    }
}
