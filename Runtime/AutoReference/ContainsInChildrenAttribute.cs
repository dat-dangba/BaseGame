// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Applies a filter which requires that a child of a GameObject contains a specific component.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ContainsInChildrenAttribute : AutoReferenceValidatorAttribute {
        private readonly Type _componentType;

        private int _maxDepth = -1;

        public ContainsInChildrenAttribute(Type componentType) {
            _componentType = componentType;
        }

        public bool IncludeSelf { get; set; } = true;

        public bool IncludeInactive { get; set; } = true;

        /// <summary>
        /// Limit the depth of the search, with a value of 1 being the direct children of the object.
        /// A value of -1 is the default behaviour which searches in all children.
        /// </summary>
        public int MaxDepth {
            get => _maxDepth;
            set => _maxDepth = Mathf.Max(-1, value);
        }

        protected override Type TypeConstraint => Types.Component;

        protected override int PriorityOrder => FilterOrder.First;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            return _componentType.IsSameOrSubclassOf(Types.Component)
                ? ValidationResult.Ok
                : ValidationResult.Error($"Type '{_componentType}' must derive from '{nameof(Component)}'");
        }

        protected override bool Validate(in FieldContext context, Object value) {
            var component = (Component)value;

            if (MaxDepth >= 0) {
                var values = ObjectUtils.EnumerateInChildrenWithDepth(component, context.Type, MaxDepth);
                if (!IncludeSelf) {
                    var fieldContext = context;
                    values = values.Where(v => v != fieldContext.Behaviour);
                }

                if (!IncludeInactive) {
                    values = values.Where(v => v.gameObject.activeInHierarchy);
                }

                return values.Any();
            }

            // Default behaviour:

            if (IncludeSelf) {
                return component.GetComponentInChildren(_componentType, IncludeInactive) != null;
            }

            var transform = component.transform;

            var count = transform.childCount;
            for (var i = 0; i < count; ++i) {
                var child = transform.GetChild(i);
                if (child.GetComponentInChildren(_componentType, IncludeInactive) != null) {
                    return true;
                }
            }

            return false;
        }
    }
}
