// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Gets a reference attached to the same or a parent <see cref="GameObject"/>.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class GetInParentAttribute : AutoReferenceAttribute {
        private int _maxDepth = -1;

        /// <summary>
        /// Limit the depth of the search, with a value of 1 being the direct parent of the object.
        /// A value of -1 is the default behaviour which searches in all parents.
        /// </summary>
        public int MaxDepth {
            get => _maxDepth;
            set => _maxDepth = Mathf.Max(-1, value);
        }

        protected override Type TypeConstraint => Types.Component;

        protected override IEnumerable<Object> GetObjects() {
            return MaxDepth < 0
                ? Behaviour.GetComponentsInParent(Type, true)
                : ObjectUtils.EnumerateInParentsWithDepth(Behaviour, Type, MaxDepth);
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            if (MaxDepth < 0) {
                return objects.Where(o => Behaviour.transform.IsChildOf(((Component)o).transform));
            }

            return objects.Where(v =>
                ObjectUtils.IsWithinDepthFromParent(Behaviour.transform, ((Component)v).transform, MaxDepth)
            );
        }
    }
}
