// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Gets a reference attached to the same <see cref="GameObject"/>.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class GetAttribute : AutoReferenceAttribute {
        protected override Type TypeConstraint => Types.Component;

        protected override IEnumerable<Object> GetObjects() {
            return Behaviour.GetComponents(Type);
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            return objects.Where(o => ((Component)o).transform == Behaviour.transform);
        }
    }
}
