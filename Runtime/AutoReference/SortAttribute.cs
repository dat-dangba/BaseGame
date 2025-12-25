// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Sort the values. Only valid if the underlying type <c>T</c> of the field implements
    /// either <see cref="IComparable"/> or <see cref="IComparable{T}"/>
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class SortAttribute : AutoReferenceFilterAttribute {
        private string _compareMethod;
        private IComparer<Object> _comparer;
        private Type _comparerType;

        protected override int PriorityOrder => FilterOrder.Sort;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            // First we try to match with IComparable<...> (generic)
            _comparer = GetGenericComparer(context.Type);
            if (_comparer != null) {
                return ValidationResult.Ok;
            }

            // If that fails, we try to match with IComparable (non-generic)
            if (Types.Comparable.IsAssignableFrom(context.Type)) {
                return ValidationResult.Ok;
            }

            var error = $"{context.Type} must implement IComparable or IComparable<{context.Type}>";
            return ValidationResult.Error(error);
        }

        private static IComparer<Object> GetGenericComparer(Type type) {
            var isGenericComparable = type.GetInterfaces().Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == Types.GenericComparable
                && i.GenericTypeArguments[0] == type
            );

            if (!isGenericComparable) {
                return null;
            }

            // We can't pass a generic comparable in the OrderBy method, so we wrap the comparison in a comparer class.

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var args = Types.TempTypeParams(type);
            var method = type.GetMethod(nameof(IComparable.CompareTo), flags, Type.DefaultBinder, args, null);
            if (method == null || method.ReturnType != Types.Int) {
                return null;
            }

            return new DynamicCompareTo(method);
        }

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return _comparer == null ? values.OrderBy(v => v) : values.OrderBy(v => v, _comparer);
        }
    }
}
