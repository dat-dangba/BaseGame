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
    /// Gets a reference attached to the same or a child <see cref="GameObject"/>.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class GetInChildrenAttribute : AutoReferenceAttribute {
        private int _maxDepth = -1;

        /// <summary>
        /// Limit the depth of the search, with a value of 1 being the direct children of the object.
        /// A value of -1 is the default behaviour which searches in all children.
        /// </summary>
        public int MaxDepth {
            get => _maxDepth;
            set => _maxDepth = Mathf.Max(-1, value);
        }

        protected override Type TypeConstraint => Types.Component;

        protected override IEnumerable<Object> GetObjects() {
            return MaxDepth < 0
                ? Behaviour.GetComponentsInChildren(Type, true)
                : ObjectUtils.EnumerateInChildrenWithDepth(Behaviour, Type, MaxDepth);
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            if (MaxDepth < 0) {
                return objects.Where(o => ((Component)o).transform.IsChildOf(Behaviour.transform));
            }

            return objects.Where(v =>
                ObjectUtils.IsWithinDepthFromParent(((Component)v).transform, Behaviour.transform, MaxDepth)
            );
        }
    }
}
