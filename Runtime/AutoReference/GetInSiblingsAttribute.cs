using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using Teo.AutoReference.System;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Gets a reference attached to a sibling <see cref="GameObject"/>, including the <see cref="GameObject"/> of the
    /// script itself.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class GetInSiblingsAttribute : AutoReferenceAttribute {
        protected override Type TypeConstraint => Types.Component;

        protected override IEnumerable<Object> GetObjects() {
            var parent = Behaviour.transform.parent;
            IEnumerable<Transform> siblings;

            if (parent == null) {
                siblings = Behaviour.gameObject.scene.GetRootGameObjects()
                    .Select(go => go.transform);
            } else {
                siblings = Enumerable.Range(0, parent.childCount)
                    .Select(i => parent.GetChild(i));
            }

            return siblings.SelectMany(child => child.GetComponents(Type));
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            return objects.Where(Validate);
        }

        private bool Validate(Object value) {
            var component = (Component)value;
            var componentParent = component.transform.parent;

            if (Behaviour.transform.parent == null) {
                return component.gameObject.scene == Behaviour.gameObject.scene && component.transform.parent == null;
            }

            return componentParent != null && componentParent == Behaviour.transform.parent;
        }
    }
}
