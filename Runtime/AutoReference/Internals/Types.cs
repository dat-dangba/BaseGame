// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Teo.AutoReference.System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.Internals {
    internal static class Types {
        public const BindingFlags CallbackFlags = BindingFlags.Instance | BindingFlags.Static |
                                                  BindingFlags.Public | BindingFlags.NonPublic;

        public static readonly Type AutoReferenceAttribute = typeof(AutoReferenceAttribute);
        public static readonly Type AutoReferenceFilterAttribute = typeof(AutoReferenceFilterAttribute);
        public static readonly Type AutoReferenceBaseAttribute = typeof(AutoReferenceBaseAttribute);
        public static readonly Type CsObject = typeof(object);
        public static readonly Type SyncObserver = typeof(ISyncObserver);
        public static readonly Type GameObject = typeof(GameObject);
        public static readonly Type Transform = typeof(Transform);
        public static readonly Type Void = typeof(void);
        public static readonly Type UnityObject = typeof(Object);
        public static readonly Type Int = typeof(int);
        public static readonly Type Bool = typeof(bool);
        public static readonly Type GenericList = typeof(List<>);
        public static readonly Type GenericComparable = typeof(IComparable<>);
        public static readonly Type Comparable = typeof(IComparable);
        public static readonly Type NonSerialized = typeof(NonSerializedAttribute);
        public static readonly Type Serializable = typeof(SerializableAttribute);
        public static readonly Type SerializeField = typeof(SerializeField);
        public static readonly Type Mono = typeof(MonoBehaviour);
        public static readonly Type OnAfterSyncAttribute = typeof(OnAfterSyncAttribute);
        public static readonly Type Component = typeof(Component);
        public static readonly Type Behaviour = typeof(Behaviour);
        public static readonly Type SyncMode = typeof(SyncMode);
        public static readonly Type ContextMode = typeof(ContextMode);
        public static readonly Type Nullable = typeof(Nullable<>);
#if UNITY_2021_2_OR_NEWER
        public static readonly Type AnyTuple = typeof(ITuple);
#endif
        public static readonly Type SerializeReference = typeof(SerializeReference);
        public static readonly MethodInfo SyncObserverMethod = SyncObserver.GetMethod(nameof(ISyncObserver.OnSync));

        private static readonly Type[] TypeParameterBuffer1 = new Type[1];
        private static readonly Type[] TypeParameterBuffer2 = new Type[2];
        private static readonly object[] ObjectParameterBuffer1 = new object[1];
        private static readonly object[] ObjectParameterBuffer2 = new object[2];

        /// <summary>
        /// Returns an array with one type as its only element. The same array is always returned, so this should only
        /// be used when a temporary array is needed. Evidently not thread-safe.
        /// </summary>
        public static Type[] TempTypeParams(Type type) {
            TypeParameterBuffer1[0] = type;
            return TypeParameterBuffer1;
        }

        /// <summary>
        /// Returns a 2-lengthed array with the two types as its two elements. The same array is always returned, so
        /// this should only be used when a temporary array is needed. Evidently not thread-safe.
        /// </summary>
        public static Type[] TempTypeParams(Type type1, Type type2) {
            TypeParameterBuffer2[0] = type1;
            TypeParameterBuffer2[1] = type2;
            return TypeParameterBuffer2;
        }

        public static object[] TempParams(object obj) {
            ObjectParameterBuffer1[0] = obj;
            return ObjectParameterBuffer1;
        }

        public static object[] TempParams(object obj1, object obj2) {
            ObjectParameterBuffer2[0] = obj1;
            ObjectParameterBuffer2[1] = obj2;
            return ObjectParameterBuffer2;
        }

        /// <summary>
        /// Gets whether a type is an instantiable class, i.e. is non-abstract and has a default parameterless
        /// constructor.
        /// </summary>
        public static bool IsInstantiable(this Type type) {
            var hasDefaultConstructor = type.GetConstructor(Type.EmptyTypes) != null;
            return type.IsClass && !type.IsAbstract && hasDefaultConstructor;
        }

        /// <summary>
        /// Gets whether a type is either <see cref="Object"/> or inherits from <see cref="Object"/>
        /// </summary>
        public static bool IsUnityObject(this Type type) {
            return type == UnityObject || type.IsSubclassOf(UnityObject);
        }

        public static bool IsSameOrSubclassOf(this Type type, Type other) {
            return type == other || type.IsSubclassOf(other);
        }

        /// <summary>
        /// Checks if a given field is potentially serializable.
        /// </summary>
        /// <param name="info">A <see cref="FieldInfo"/> representing the field to check.</param>
        public static bool MightBeSerializable(this FieldInfo info) {
            return info is { IsStatic: false, IsInitOnly: false, IsLiteral: false } &&
                   (info.IsPublic || Attribute.IsDefined(info, Types.SerializeField))
                   && !Attribute.IsDefined(info, Types.NonSerialized);
        }

        /// <summary>
        /// Gets the actual type of a <see cref="Object"/>. Unlike <c>GetType()</c>, this method takes mismatched
        /// references into account. This only works in the Unity editor and defaults to normal `GetType()` in builds.
        /// </summary>
        public static Type GetInstanceType(this Object target) {
#if UNITY_EDITOR
            return EditorUtility.InstanceIDToObject(target.GetInstanceID()).GetType();
#else
            return target.GetType();
#endif
        }
    }
}
