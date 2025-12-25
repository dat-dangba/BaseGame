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
    /// Gets a reference attached to a <see cref="UnityEngine.GameObject"/> in the current scene. If the script
    /// is attached to a prefab asset that is currently being edited, it will search for references on the prefab
    /// itself.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class FindInSceneAttribute : AutoReferenceAttribute {
        protected override Type TypeConstraint => Types.Component;

        protected override IEnumerable<Object> GetObjects() {
            var scene = Behaviour.gameObject.scene;

            if (scene.IsValid()) {
                return ObjectUtils.EnumerateAllComponentsInScene(Behaviour.gameObject.scene, Type);
            }

            // We assume we're in a prefab where there's exactly one root transform
            var root = Behaviour.transform;
            while (root.parent != null) {
                root = root.parent;
            }

            return root.GetComponentsInChildren(Type, true);
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            var currentScene = Behaviour.gameObject.scene;
            return objects.Where(o => ObjectUtils.IsInScene(currentScene, o));
        }
    }
}
