// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Teo.AutoReference.Internals.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.Internals {
    internal static class ObjectUtils {
        private static GameObject GetGameObject(Object value) {
            return value switch {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null,
            };
        }

        /// <summary>
        /// Determines whether the two objects are in the same scene.
        /// </summary>
        internal static bool AreInTheSameScene(Object lhsObject, Object rhsObject) {
            var lhs = GetGameObject(lhsObject);
            if (lhs == null) {
                return false;
            }

            var rhs = GetGameObject(rhsObject);
            if (rhs == null) {
                return false;
            }

            return lhs.scene.handle == rhs.scene.handle;
        }

        internal static bool IsInScene(Scene scene, Object obj) {
            var go = GetGameObject(obj);
            if (go == null) {
                return false;
            }

            return go.scene.handle == scene.handle;
        }

        public static EditingMode GetPrefabMode(this Component component) {
            return component.gameObject.GetPrefabMode();
        }

        internal static void GetAllComponentsInScene<T>(Scene scene, List<T> list) where T : Component {
            using var objects = TempList<GameObject>.Get();
            scene.GetRootGameObjects(objects);

            using var innerList = TempList<T>.Get();
            foreach (var go in objects) {
                go.GetComponentsInChildren(true, innerList);
                list.AddRange(innerList);
            }
        }

        internal static IEnumerable<T> EnumerateAllComponentsInScene<T>(Scene scene) where T : Component {
            using var rootObjects = TempList<GameObject>.Get();
            scene.GetRootGameObjects(rootObjects);

            using var list = TempList<T>.Get();
            foreach (var go in rootObjects) {
                go.GetComponentsInChildren(true, list);
                foreach (var component in list) {
                    yield return component;
                }
            }
        }

        internal static IEnumerable<Component> EnumerateAllComponentsInScene(Scene scene, Type type) {
            using var rootObjects = TempList<GameObject>.Get();
            scene.GetRootGameObjects(rootObjects);

            foreach (var component in rootObjects.SelectMany(go => go.GetComponentsInChildren(type, true))) {
                yield return component;
            }
        }

        internal static Component[] GetAllComponentsInScene(Scene scene, Type type) {
            using var objects = TempList<GameObject>.Get();
            scene.GetRootGameObjects(objects);

            using var result = TempList<Component>.Get();

            foreach (var go in objects) {
                result.AddRange(go.GetComponentsInChildren(type, true));
            }

            return result.ToArray();
        }

        public static EditingMode GetPrefabMode(this GameObject gameObject) {
            var handle = gameObject.scene.handle;

            for (var i = 0; i < SceneManager.sceneCount; ++i) {
                if (SceneManager.GetSceneAt(i).handle == handle) {
                    return EditingMode.InScene;
                }
            }

            return EditingMode.InPrefab;
        }

        public static EditingMode GetPrefabMode(this Object obj) {
            if (obj is Component component) {
                return component.gameObject.GetPrefabMode();
            }

            return EditingMode.Unsupported;
        }

        /// <summary>
        /// Whether this reference is a mismatched reference. A mismatched reference is when an object has an actual
        /// type that's different from the C# type. This can happen when a reference is serialized as one type but
        /// the type of the field was changed to a different type.
        /// </summary>
        public static bool IsMismatchedReference(this Object target) {
            return target.GetInstanceType() != target.GetType();
        }

        /// <summary>
        /// Determines whether a given Transform is within a specified depth from its parent Transform.
        /// </summary>
        public static bool IsWithinDepthFromParent(
            Transform transform,
            Transform parent,
            int maxDepth
        ) {
            for (var i = 0; i <= maxDepth; ++i) {
                if (transform == parent) {
                    return true;
                }

                transform = transform.parent;

                if (transform == null) {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Enumerates all components of the specified type in the parents of a given component, up to a maximum depth.
        /// maximum depth.
        /// </summary>
        public static IEnumerable<Component> EnumerateInParentsWithDepth(
            Component component,
            Type type,
            int maxDepth = 0
        ) {
            if (maxDepth < 0) {
                yield break;
            }
            if (maxDepth == 0) {
                using var list = TempList<Component>.Get();
                component.GetComponents(type, list);
                foreach (var item in list) {
                    yield return item;
                }
            } else {
                using var list = TempList<Component>.Get();

                var transform = component.transform;

                for (var depth = 0; transform != null && depth <= maxDepth; transform = transform.parent, ++depth) {
                    transform.GetComponents(type, list);
                    foreach (var item in list) {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all components of a specified type in the children of a specified component, up to a maximum
        /// depth.
        /// </summary>
        /// <remarks>
        /// This method performs a breadth-first search and therefore the ordering
        /// of components will be different from Unity's GetComponentsInChildren.
        /// </remarks>
        public static IEnumerable<Component> EnumerateInChildrenWithDepth(
            Component component,
            Type type,
            int maxDepth = 0
        ) {
            if (maxDepth < 0) {
                yield break;
            }

            if (maxDepth == 0) {
                using var list = TempList<Component>.Get();
                component.transform.GetComponents(type, list);
                foreach (var item in list) {
                    yield return item;
                }
            } else {
                // Perform a breadth-first search.

                using var list = TempList<Component>.Get();
                using var queue = TempQueue<Transform>.Get();
                queue.Enqueue(component.transform);

                for (var depth = 0; queue.Count > 0 && depth <= maxDepth; ++depth) {
                    // Note: Don't inline this counter because queue.Count will change in the for loop.
                    var queueCount = queue.Count;

                    for (var i = 0; i < queueCount; i++) {
                        var current = queue.Dequeue();
                        current.GetComponents(type, list);

                        foreach (var item in list) {
                            yield return item;
                        }

                        for (var childId = 0; childId < current.childCount; ++childId) {
                            var child = current.GetChild(childId);
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        internal enum EditingMode {
            Unsupported,
            InScene,
            InPrefab,
        }
    }
}
