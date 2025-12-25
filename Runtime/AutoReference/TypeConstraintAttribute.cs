// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Only allow values that are of a specific type. This is useful for example when requiring all values to
    /// implement a certain interface.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeConstraintAttribute : AutoReferenceValidatorAttribute {
        private readonly bool _exactType;
        private readonly Type _type;

        public TypeConstraintAttribute(Type type, bool exactType = false) {
            _type = type;
            _exactType = exactType;
        }

        protected override int PriorityOrder => FilterOrder.First;

        protected override bool Validate(in FieldContext context, Object value) {
            var objType = value.GetType();
            return _type == objType || !_exactType && _type.IsAssignableFrom(objType);
        }

        protected override ValidationResult OnInitialize(in FieldContext context) {
            if (_type.IsInterface) {
                if (!_exactType) {
                    return ValidationResult.Ok;
                }

                var warning = $"Interface {_type.FormatCSharpName()} cannot be an exact type match";
                return ValidationResult.Warning(warning);
            }

            var type = context.BehaviourType;
            if (_type.IsSubclassOf(type)) {
                return ValidationResult.Ok;
            }

            if (_type == context.BehaviourType) {
                var warning = _exactType
                    ? $"Use filter '{nameof(ExactTypeAttribute).TrimEnd("Attribute")}' instead"
                    : "This filter will have no effect";

                return ValidationResult.Warning(warning);
            }

            var error = $"{_type.FormatCSharpName()} must be an interface or subclass of {type.FormatCSharpName()}";
            return ValidationResult.Error(error);
        }
    }
}
