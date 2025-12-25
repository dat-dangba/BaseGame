// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Internals.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Provides an abstraction over a field that is either of type <see cref="UnityEngine.Object"/> or a
    /// generic array or list with an underlying type of <see cref="UnityEngine.Object"/>.
    /// </summary>
    internal readonly struct ObjectField {
        /// The direct type of the field if it's a plain field, or the underlying type of the array or list.
        public Type Type { get; }

        /// <summary>
        /// The underlying <see cref="FieldInfo"/> that this object is interpreting.
        /// </summary>
        public FieldInfo FieldInfo { get; }

        /// <summary>
        /// A field is valid if it's of the type <see cref="UnityEngine.Object"/> or a generic array or list
        /// with <see cref="UnityEngine.Object"/> as its underlying type.</summary>
        public bool IsValid => ConstructType != ConstructType.Invalid;

        public string Name => FieldInfo.Name;

        /// A valid field that corresponts to an array or list.
        public bool IsArrayOrList => ConstructType == ConstructType.Array || ConstructType == ConstructType.List;

        /// A valid field that is neither an array nor a list.
        public bool IsPlain => ConstructType is ConstructType.Plain;

        public bool IsList => ConstructType is ConstructType.List;

        public bool IsArray => ConstructType is ConstructType.Array;

        public ConstructType ConstructType { get; }

        public Type DeclaringType => FieldInfo.DeclaringType;

        public static Type GetUnderlyingType(Type type) {
            if (type.IsArray && type.HasElementType) {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == Types.GenericList) {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static (ConstructType construct, Type type) GetConstructAndUnderlyingType(Type type) {
            if (type.IsArray && type.HasElementType) {
                return (ConstructType.Array, type.GetElementType());
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == Types.GenericList) {
                return (ConstructType.List, type.GetGenericArguments()[0]);
            }
            return (ConstructType.Plain, type);
        }

        public ObjectField(FieldInfo fieldInfo) {
            FieldInfo = fieldInfo;

            var type = fieldInfo.FieldType;
            if (type.IsArray && type.HasElementType) {
                ConstructType = ConstructType.Array;
                type = type.GetElementType();
            } else if (type.IsGenericType && type.GetGenericTypeDefinition() == Types.GenericList) {
                ConstructType = ConstructType.List;
                type = type.GetGenericArguments()[0];
            } else {
                ConstructType = ConstructType.Plain;
            }

            if (type == null || !type.IsUnityObject()) {
                ConstructType = ConstructType.Invalid;
            }

            Type = type;
        }

        /// <summary>
        /// Clear all values based on the type of container the field uses:
        ///   <list type="bullet">
        ///     <item><description>Arrays get an empty array.</description></item>
        ///     <item><description>Lists get an empty list.</description></item>
        ///     <item><description>Plain fields get a null value.</description></item>
        ///   </list>
        ///  Note: No operation is performed on invalid fields.
        /// </summary>
        public void ClearValues(MonoBehaviour target) {
            switch (ConstructType) {
                case ConstructType.Plain:
                    FieldInfo.SetValue(target, null);
                    break;
                case ConstructType.Array:
                    FieldInfo.SetValue(target, Array.CreateInstance(Type, 0));
                    break;
                case ConstructType.List:
                    var genericListType = Types.GenericList.MakeGenericType(Type);
                    var targetList = (IList)Activator.CreateInstance(genericListType);
                    FieldInfo.SetValue(target, targetList);
                    break;
                case ConstructType.Invalid:
                default:
                    return;
            }
        }

        /// <summary>
        ///   Sets the value of the field based on the type of container the field uses:
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         If the field is an array or list, it will be assigned a list or array containing all elements.
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         If the field is a regular field, it will receive the first item or null if the enumerable is empty.
        ///       </description>
        ///     </item>
        ///   </list>
        ///  Note: No operation is performed on invalid fields.
        /// </summary>
        public void SetValue(MonoBehaviour target, IEnumerable<Object> objects) {
            switch (ConstructType) {
                case ConstructType.Array: {
                    // We need to create an Object[] array to get its size before iterating it.
                    // Then we need to create another new array because Object[] is not assignable to a T[] array where
                    // T derives from Object.
                    var objectsArray = objects.ToArraySmart();
                    var targetArray = Array.CreateInstance(Type, objectsArray.Length);
                    Array.Copy(objectsArray, targetArray, objectsArray.Length);
                    FieldInfo.SetValue(target, targetArray);
                    break;
                }
                case ConstructType.List: {
                    var genericListType = Types.GenericList.MakeGenericType(Type);
                    var targetList = (IList)Activator.CreateInstance(genericListType);
                    foreach (var o in objects) {
                        targetList.Add(o);
                    }

                    FieldInfo.SetValue(target, targetList);
                    break;
                }
                case ConstructType.Plain:
                    FieldInfo.SetValue(target, objects.FirstOrDefault());
                    break;
                case ConstructType.Invalid:
                default:
                    return;
            }
        }

        /// <summary>
        ///   Retrieves the field's current value from a target MonoBehaviour and returns it as a read-only
        ///   <c>IList&lt;UnityEngine.Object&gt;</c>. The contents of the list are as follows:
        ///   <list type="bullet">
        ///     <item><description>
        ///         If the value is null (regardless of the field type) or the field is invalid (i.e. is not
        ///         <c>T</c>, <see cref="List{T}"/> or <see cref="T:T[]"/> where <c>T</c> derives from
        ///         <see cref="Object"/>), it returns an empty list.
        ///     </description></item>
        ///     <item><description>
        ///         If it's a <see cref="List{T}"/> or <see cref="T:Object[]"/>, it returns a list of all the elements.
        ///     </description></item>
        ///     <item><description>
        ///         If it's a plain field, it returns a list with its value as the only element.
        ///     </description></item>
        ///   </list>
        /// </summary>
        public ObjectListProxy GetValues(MonoBehaviour target) {
            return new ObjectListProxy(FieldInfo.GetValue(target));
        }
    }
}
