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

#if UNITY_2021_1
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference {

    /// <summary>
    /// Find children components in another parent component based on its type. The search
    /// always happens on the first valid parent detected. The valid parent can be filtered.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class FindInParentAttribute : AutoReferenceAttribute {
        private readonly string _filterMethod;
        private readonly Type _parentType;
        private CallbackMethodInfo _callbackInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindInParentAttribute"/> class.
        /// </summary>
        /// <param name="parentType">
        ///     The type of the parent component to search for. Must be a subclass of <see cref="Component"/>.
        /// </param>
        /// <param name="filterMethod">
        ///     Optional. The name of a filter method to apply on the parent component. The method must return
        ///     <see cref="bool"/> and accept the parent type as a parameter.
        /// </param>
        public FindInParentAttribute(Type parentType, string filterMethod = null) {
            _parentType = parentType;
            _filterMethod = filterMethod;
        }

        /// <summary>
        /// Ignore the parent if it's located on the same <see cref="GameObject"/>
        /// </summary>
        public bool IgnoreSelfAsParent { get; set; } = false;

        public bool IgnoreParentInSearch { get; set; } = false;

        protected override Type TypeConstraint => typeof(Component);

        protected override ValidationResult OnInitialize() {
            if (_parentType == null) {
                return ValidationResult.Error("Parent type cannot be null");
            }

            if (!_parentType.IsSameOrSubclassOf(Types.Component)) {
                return ValidationResult.Error($"Parent type must be a subclass of '{nameof(Component)}'");
            }

            if (_filterMethod == null) {
                return ValidationResult.Ok;
            }

            _callbackInfo = CallbackMethodInfo.Create(
                FieldContext,
                _filterMethod,
                Types.Bool,
                Types.TempTypeParams(_parentType)
            );

            return _callbackInfo.Result;
        }

        private IEnumerable<Object> ApplySearchFilters(GameObject parent, IEnumerable<Object> objects) {
            if (IgnoreParentInSearch) {
                objects = objects.Where(o => ((Component)o).gameObject != parent);
            }

            if (IgnoreSelfAsParent) {
                objects = objects.Where(o => ((Component)o).gameObject != Behaviour.gameObject);
            }
            return objects;
        }

        private Component FindParent() {
            var root = IgnoreSelfAsParent ? Behaviour.transform.parent : Behaviour.transform;
            if (_callbackInfo.MethodInfo == null) {
                return root.GetComponentInParent(_parentType, true);
            }

            return root.GetComponentsInParent(_parentType, true)
                .Where(p => _callbackInfo.Invoke<bool>(Behaviour, Types.TempParams(p)))
                .FirstOrDefault();
        }

        protected override IEnumerable<Object> GetObjects() {
            var parent = FindParent();
            return parent == null
                ? Enumerable.Empty<Object>()
                : ApplySearchFilters(parent.gameObject, parent.GetComponentsInChildren(Type, true));
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            var parent = FindParent();
            return parent == null
                ? Enumerable.Empty<Object>()
                : objects.Where(o => ((Component)o).transform.IsChildOf(parent.transform));
        }
    }
}
