// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teo.AutoReference.System;

#if UNITY_2021_2_OR_NEWER
using System.Linq;
#else
using Teo.AutoReference.Internals.Compatibility;
#endif

using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Keep only the last values up to a specified amount. The amount can be given as a constant integer value
    /// or by a provided method within the script containing the field.
    /// The method can be either static or non-static and must have the signature
    /// <c>'int ()'</c>.
    ///<br/><br/>
    /// An alternative caller type that contains the method may optionally be specified, in which case the method
    /// must be static.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TakeLastAttribute : TakeAttribute {
        public TakeLastAttribute(int count) : base(count) { }

        public TakeLastAttribute(string methodName) : base(methodName) { }

        public TakeLastAttribute(Type caller, string methodName) : base(caller, methodName) { }

        public override IEnumerable<Object> Filter(FieldContext context, IEnumerable<Object> values) {
            return values.TakeLast(GetCount(context));
        }
    }
}
