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
    /// Keep only the first values up to a specified amount. The amount can be given as a constant integer value
    /// or by a provided method within the script containing the field.
    /// This method can be either static or non-static and must have the signature <c>'int ()'</c>.
    ///<br/><br/>
    /// An alternative caller type that contains the method may optionally be specified, in which case the method
    /// must be static.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TakeAttribute : AutoReferenceFilterAttribute {
        private readonly Type _caller;
        private readonly int _count;
        private readonly string _methodName;

        private CallbackMethodInfo _callback;

        public TakeAttribute(int count) {
            _count = count;
            _caller = null;
            _methodName = null;
        }

        public TakeAttribute(string methodName) {
            _methodName = methodName;
            _caller = null;
        }

        protected TakeAttribute(Type caller, string methodName) {
            _caller = caller;
            _methodName = methodName;
        }

        protected override int PriorityOrder => FilterOrder.PostProcess;

        protected int GetCount(FieldContext context) {
            if (_callback.Result) {
                return _callback.Invoke<int>(context, Array.Empty<object>());
            }

            return _count;
        }

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.Take(GetCount(context));
        }

        protected override ValidationResult OnInitialize(in FieldContext context) {
            if (_methodName == null) {
                return ValidationResult.Ok;
            }

            _callback = CallbackMethodInfo.Create(context, _caller, _methodName, Types.Int, Type.EmptyTypes);
            return _callback.Result;
        }
    }
}
