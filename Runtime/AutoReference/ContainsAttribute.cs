// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Applies a filter which requires that the GameObject contains a specific component.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ContainsAttribute : AutoReferenceValidatorAttribute {
        private readonly Type _componentType;

        public ContainsAttribute(Type componentType) {
            _componentType = componentType;
        }

        protected override Type TypeConstraint => Types.Component;

        protected override int PriorityOrder => FilterOrder.First;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            return _componentType.IsSameOrSubclassOf(Types.Component)
                ? ValidationResult.Ok
                : ValidationResult.Error($"Type '{_componentType}' must derive from '{nameof(Component)}'");
        }

        protected override bool Validate(in FieldContext context, Object value) {
            return ((Component)value).TryGetComponent(_componentType, out _);
        }
    }
}
