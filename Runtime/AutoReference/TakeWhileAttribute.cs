// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Keep values in the order they're given until a condition becomes false. The condition is provided
    /// through a specified method within the script containing the field, which is invoked for every input
    /// value until it becomes false.
    /// This method can be either static or non-static and must have the signature
    /// <c>'bool (ValidType)'</c>, where <c>ValidType</c> is assignable to the field type.
    ///<br/><br/>
    /// An alternative caller type that contains the method may optionally be specified, in which case the method
    /// must be static.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TakeWhileAttribute : AutoReferenceFilterAttribute {
        private readonly Type _caller;
        private readonly string _methodName;

        private CallbackMethodInfo _callback;

        public TakeWhileAttribute(string methodName) {
            _methodName = methodName;
        }

        public TakeWhileAttribute(Type caller, string methodName) {
            _methodName = methodName;
            _caller = caller;
        }

        protected override int PriorityOrder => FilterOrder.PostProcess;

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            foreach (var v in values) {
                if (_callback.Invoke<bool>(context, Types.TempParams(v))) {
                    yield return v;
                } else {
                    yield break;
                }
            }
        }

        protected override ValidationResult OnInitialize(in FieldContext context) {
            _callback = CallbackMethodInfo.Create(
                context,
                _caller, _methodName, Types.Bool, Types.TempTypeParams(context.Type)
            );

            return _callback.Result;
        }
    }
}
