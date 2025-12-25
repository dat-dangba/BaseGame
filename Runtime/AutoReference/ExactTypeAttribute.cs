// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Applies a filter that only allows the exact type of the underlying field and ignores all derived classes.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ExactTypeAttribute : AutoReferenceValidatorAttribute {
        protected override int PriorityOrder => FilterOrder.First;

        protected override bool Validate(in FieldContext context, Object value) {
            return context.Type == value.GetType();
        }
    }
}
