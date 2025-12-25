// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Only allow references with a specific name.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class NameAttribute : AutoReferenceValidatorAttribute {
        private readonly StringComparison _comparison;
        private readonly string[] _names;

        public NameAttribute(string name, params string[] names) :
            this(StringComparison.Ordinal, name, names) { }

        public NameAttribute(StringComparison comparison, string name, params string[] names) {
            _names = names.PrependArray(name);
            _comparison = comparison;
        }

        protected override int PriorityOrder => FilterOrder.Filter;

        protected override bool Validate(in FieldContext context, Object value) {
            return _names.Any(name => value.name.Equals(name, _comparison));
        }
    }
}
