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
    /// Filters auto-reference values using a specified method within the script containing the field.
    /// This method can be either static or non-static and must have the signature
    /// <c>'bool (ValidType)'</c>, where <c>ValidType</c> is assignable to the field type.
    ///<br/><br/>
    /// An alternative caller type that contains the method may optionally be specified, in which case the method
    /// must be static.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FilterByAttribute : AutoReferenceFilterAttribute {
        private readonly Type _caller;
        private readonly string _methodName;
        private CallbackMethodInfo _callback;

        public FilterByAttribute(string methodName) {
            _caller = null;
            _methodName = methodName;
        }

        public FilterByAttribute(Type caller, string methodName) {
            _caller = caller;
            _methodName = methodName;
        }

        protected override int PriorityOrder => FilterOrder.Filter;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            _callback = CallbackMethodInfo.Create(
                context,
                _caller, _methodName, Types.Bool, Types.TempTypeParams(context.Type)
            );

            return _callback.Result;
        }

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.Where(v => _callback.Invoke<bool>(context, Types.TempParams(v)));
        }
    }
}
