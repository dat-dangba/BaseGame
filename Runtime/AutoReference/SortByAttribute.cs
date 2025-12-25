// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Sorts auto-reference values using a specified method within the script containing the field.
    /// This method can be either static or non-static and must have the signature
    /// <c>'int (ValidType, ValidType)'</c>, where <c>ValidType</c> is assignable to the field type.
    ///<br/><br/>
    /// An alternative caller type that contains the method may optionally be specified, in which case the method
    /// must be static.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SortByAttribute : AutoReferenceFilterAttribute {
        private readonly Type _caller;
        private readonly string _methodName;

        private CallbackMethodInfo _callback;
        private DynamicComparer _comparer;

        public SortByAttribute(string methodName) {
            _methodName = methodName;
            _caller = null;
        }

        public SortByAttribute(Type caller, string methodName) {
            _methodName = methodName;
            _caller = caller;
        }

        protected override int PriorityOrder => FilterOrder.Sort;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            _callback = CallbackMethodInfo.Create(
                context, _caller, _methodName, Types.Int, Types.TempTypeParams(context.Type, context.Type));

            if (!_callback.Result) {
                return _callback.Result;
            }

            _comparer = new DynamicComparer(_callback.MethodInfo);

            return ValidationResult.Ok;
        }

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            if (_callback.IsOnBehaviour) {
                _comparer.Target = context.Behaviour;
            }

            return values.OrderBy(v => v, _comparer);
        }
    }
}
