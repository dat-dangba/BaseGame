// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teo.AutoReference.Internals;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.System {
    /// Base class for all filters used for Auto-References.
    [Conditional("UNITY_EDITOR")]
    public abstract class AutoReferenceFilterAttribute
        : AutoReferenceBaseAttribute, IComparable<AutoReferenceFilterAttribute> {
        private ValidationResult _initializedResult;

        private bool _isInitialized;

        private int _order;
        private bool _overrideOrder;
        protected virtual int PriorityOrder => 0;

        protected virtual Type TypeConstraint => Types.UnityObject;

        public int Order {
            get => _overrideOrder ? _order : PriorityOrder;
            set {
                _order = value;
                _overrideOrder = true;
            }
        }

        /// <summary>
        /// Whether this filter attribute is type-constrained to components.
        /// </summary>
        internal bool IsComponentBased => TypeConstraint == Types.Component;

        public int CompareTo(AutoReferenceFilterAttribute other) {
            return Order.CompareTo(other.Order);
        }

        internal ValidationResult Initialize(AutoReferenceAttribute attribute, FieldContext context) {
            if (_isInitialized) {
                return _initializedResult;
            }

            _isInitialized = true;

            if (TypeConstraint == Types.Component
                && context.Type == Types.GameObject
                && attribute.IsComponentCompatible) {
                // Allow GameObject type for component-based filters:
                // The transform of the game object will be passed to the filter later instead of the object itself.

                context = context.WithTypeOverride(Types.Transform);
            }

            _initializedResult = context.Type.IsSameOrSubclassOf(TypeConstraint)
                ? OnInitialize(context)
                : ValidationResult.Error($"A type of '{TypeConstraint.FullName}' is expected");

            return _initializedResult;
        }

        protected virtual ValidationResult OnInitialize(in FieldContext context) {
            return ValidationResult.Ok;
        }

        public abstract IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values);
    }
}
