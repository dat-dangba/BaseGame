// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Teo.AutoReference.Internals;
using UnityEngine;

namespace Teo.AutoReference.System {
    /// <summary>
    /// An abstraction over a field that belongs in a <see cref="MonoBehaviour"/> script.
    /// </summary>
    public readonly struct FieldContext {
        private readonly ObjectField _field;
        private readonly Type _typeOverride;

        /// <summary>
        /// The type of the <see cref="MonoBehaviour"/> in which the field is defined.
        /// </summary>
        public Type BehaviourType { get; }

        /// <summary>
        /// The <see cref="MonoBehaviour"/> instance in which the field is defined.
        /// </summary>
        public MonoBehaviour Behaviour { get; }

        /// <summary>
        /// Represents the type used in this context which is typically the same as the underlying type of
        /// the associated field, where type inherits from <see cref="UnityEngine.Object"/> and where the field could
        /// be plain, an array, or a list.
        /// This value may be overriden. See: <see cref="IsTypeOverriden"/> and <see cref="UnderlyingType"/>.
        /// </summary>
        public Type Type => _typeOverride ?? _field.Type;

        /// <summary>
        /// Represents the actual underlying type of the associated field, where type inherits from
        /// <see cref="UnityEngine.Object"/> and where the field could be plain, an array, or a list. This value is
        /// the same as <see cref="Type"/> unless the type is overriden.
        /// See: <see cref="Type"/> and <see cref="IsTypeOverriden"/>
        /// </summary>
        public Type UnderlyingType => _field.Type;

        /// <summary>
        /// Whether the type is overriden.
        /// </summary>
        public bool IsTypeOverriden => _typeOverride != null;

        /// <summary>
        /// Whether the field is plain (i.e. non-array and non-list)
        /// </summary>
        public bool IsPlainField => _field.ConstructType == ConstructType.Plain;

        /// <summary>
        /// Whether the field is an array or list
        /// </summary>
        public bool IsArrayOrList =>
            _field.ConstructType == ConstructType.Array || _field.ConstructType == ConstructType.List;

        /// <summary>
        /// Whether the field is valid. A valid field is either of type <c>T</c>, <c>T[]</c>, or
        /// <see cref="List{T}"/>, where <c>T</c> is <see cref="UnityEngine.Object"/>.
        /// </summary>
        public bool IsValid => _field.IsValid && BehaviourType != null;

        /// <summary>
        /// The declaring type of the field, which while be different from <see cref="BehaviourType"/> if it's
        /// defined in a parent class.
        /// </summary>
        public Type DeclaringType => _field.DeclaringType;

        internal ObjectField Field => _field;

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name => _field.Name;

        public string FullName => $"{BehaviourType.FullName}.{Name}";

        internal FieldContext(MonoBehaviour target, in ObjectField field, Type typeOverride = null) {
            Behaviour = target;
            BehaviourType = target != null ? target.GetType() : null;
            _field = field;

            _typeOverride = typeOverride == _field.Type ? null : typeOverride;
        }

        internal FieldContext(Type type, in ObjectField field, Type typeOverride = null) {
            BehaviourType = type;
            Behaviour = null;
            _field = field;

            _typeOverride = typeOverride == _field.Type ? null : typeOverride;
        }

        /// <summary>
        /// Returns a copy of this FieldContext with a type override.
        /// </summary>
        internal FieldContext WithTypeOverride(Type typeOverride) {
            return Behaviour == null
                ? new FieldContext(BehaviourType, in _field, typeOverride)
                : new FieldContext(Behaviour, in _field, typeOverride);
        }

        /// <summary>
        /// Returns a copy of this FieldContext with a new MonoBehaviour target.
        /// </summary>
        internal FieldContext WithNewTarget(MonoBehaviour target) {
            return target.GetType() == BehaviourType
                ? new FieldContext(target, in _field, _typeOverride)
                : new FieldContext((Type)null, in _field, _typeOverride);
        }
    }
}
