// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.System {
    /// <summary>
    /// A specialized derived version of <see cref="AutoReferenceFilterAttribute"/> that works by validating
    /// individual objects rather than an enumeration.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public abstract class AutoReferenceValidatorAttribute : AutoReferenceFilterAttribute {
        protected abstract bool Validate(in FieldContext context, Object value);

        public sealed override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.Where(o => Validate(context, o));
        }
    }
}
