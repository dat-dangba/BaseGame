// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference.System {
    using SyncResult = IEnumerable<Object>;

    /// <summary>
    /// Base class for all Auto-Reference Attributes.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public abstract class AutoReferenceAttribute : AutoReferenceBaseAttribute {
        private bool _hasFilterErrors;
        private ValidationResult _initResult = ValidationResult.Error("Auto-Reference attribute was not initialized");
        private bool _isInitialized;

        /// Whether this attribute should add support for GameObject fields when the field type is incompatible.
        /// i.e. this is only applicable when the field type is GameObject and the type constraint is Component.
        private bool _shouldAdaptGameObject;

        internal protected virtual SyncMode SyncMode { get; set; } = SyncMode.Default;

        internal protected virtual ContextMode Context { get; set; } = ContextMode.Default;

        /// <summary>
        /// Constraint this attribute to be used only on fields of this type.
        /// </summary>
        protected virtual Type TypeConstraint => Types.UnityObject;

        /// <summary>
        /// Whether this attribute can be used to retrieve components.
        /// </summary>
        internal bool IsComponentCompatible => Types.Component.IsSameOrSubclassOf(TypeConstraint)
                                               || TypeConstraint == Types.GameObject;

        ///<summary>
        /// The underlying type for the field this attribute is attached to. If the field is an array or a generic list,
        /// the underlying type is the element type. If the field is plain, the underlying type is the same as the type
        /// of the field.
        /// </summary>
        protected Type Type => FieldContext.Type;

        /// <summary>
        /// Provides an <see cref="ObjectField"/> value which contains information about the field this
        /// <see cref="AutoReferenceAttribute"/> is attached to.
        /// </summary>
        private ObjectField Field { get; set; }

        protected FieldContext FieldContext { get; private set; }

        /// <summary>
        /// The <see cref="MonoBehaviour"/> that contains the field this <see cref="AutoReferenceAttribute"/> is
        /// attached to.
        /// </summary>
        protected MonoBehaviour Behaviour => FieldContext.Behaviour;

        private AutoReferenceFilterAttribute[] Filters { get; set; }

        public bool IsValid => _initResult;

        public ref ValidationResult InitializationResult => ref _initResult;

        /// Gets all Objects that meet the requirements of this <see cref="AutoReferenceAttribute"/>
        protected abstract SyncResult GetObjects();

        /// <summary>
        /// Validates existing values that are already applied to the target field to ensure they fit with the
        /// requirements of this <see cref="AutoReferenceAttribute"/>. This method is not applicable when using
        /// <see cref="Teo.AutoReference.SyncMode.IfEmptyPermissive"/>.
        /// </summary>
        protected abstract SyncResult ValidateObjects(SyncResult objects);

        protected virtual ValidationResult OnInitialize() {
            return ValidationResult.Ok;
        }

        /// <summary>
        /// Returns whether the reference is valid. A reference is valid if it's neither null nor a mismatched
        /// reference.
        /// </summary>
        private bool IsValidReference(Object value) {
            if (value == null || value.IsMismatchedReference()) {
                return false;
            }

            var valueType = value.GetType();
            return valueType.IsSameOrSubclassOf(Type) || valueType.IsSameOrSubclassOf(FieldContext.UnderlyingType);
        }

        private static Object TransformToGameObject(Object obj) => obj is Transform t ? t.gameObject : obj;

        private static Object GameObjectToTransform(Object obj) => obj is GameObject o ? o.transform : obj;

        /// <summary>
        /// Apply filters and switch between GameObject and Component when required by using the Transform of the
        /// GameObject as a proxy. This allows using Component filters on GameObject fields.
        /// <seealso cref="ApplyFiltersSimple"/>
        /// </summary>
        private SyncResult ApplyFiltersAdapted(FieldContext fieldContext, SyncResult values) {
            // Shows whether the value stream is currently using GameObjects or Transforms
            var isGameObject = !_shouldAdaptGameObject;
            var adaptedContext = fieldContext.WithTypeOverride(Types.Transform);

            foreach (var filter in Filters) {
                // Convert Transform references to GameObjects and vice versa whenever required by the filter.
                switch (filter.IsComponentBased) {
                    case true when isGameObject:
                        isGameObject = false;
                        values = values.Select(GameObjectToTransform);
                        break;
                    case false when !isGameObject:
                        isGameObject = true;
                        values = values.Select(TransformToGameObject);
                        break;
                }

                var context = isGameObject ? fieldContext : adaptedContext;

                values = filter.Filter(context, values);
            }

            if (!isGameObject) {
                values = values.Select(TransformToGameObject);
            }

            return values;
        }

        /// <summary>
        /// Apply filters without GameObject ↔ Component conversions.
        /// <seealso cref="ApplyFiltersAdapted"/>
        /// </summary>
        private SyncResult ApplyFiltersSimple(FieldContext fieldContext, SyncResult values) {
            return Filters.Aggregate(values, (current, filter) => filter.Filter(fieldContext, current));
        }

        private SyncResult ApplyAllFilters(SyncResult input, bool validateExisting = false) {
            var newValues = ApplyBaseFilter(input);

            if (_shouldAdaptGameObject) {
                newValues = newValues.Select(GameObjectToTransform);
            }

            if (validateExisting) {
                newValues = ValidateObjects(newValues);
            }

            if (Filters.Length == 0) {
                if (_shouldAdaptGameObject) {
                    newValues = newValues.Select(TransformToGameObject);
                }

                return newValues;
            }

            var fieldContext = new FieldContext(Behaviour, Field);

            return _shouldAdaptGameObject || IsComponentCompatible && fieldContext.Type == Types.GameObject
                ? ApplyFiltersAdapted(fieldContext, newValues)
                : ApplyFiltersSimple(fieldContext, newValues);
        }

        private SyncResult ApplyBaseFilter(SyncResult input) {
            return input.Where(IsValidReference);
        }

        internal ValidationResult Initialize(Type type,
            SyncOptionsAttribute options,
            in ObjectField field,
            AutoReferenceFilterAttribute[] filters,
            bool hasFilterErrors
        ) {
            options ??= SyncOptionsAttribute.Default;
            SyncMode = options.SyncMode;
            Context = options.Context;

            Field = field;
            Filters = filters;
            _hasFilterErrors = hasFilterErrors;

            _shouldAdaptGameObject = field.Type == Types.GameObject && TypeConstraint == Types.Component;
            FieldContext = new FieldContext(type, field, _shouldAdaptGameObject ? Types.Transform : null);

            var constraint = TypeConstraint;

            if (!_shouldAdaptGameObject && !Field.Type.IsSameOrSubclassOf(constraint)) {
                var subclass = $"'{constraint.FullName}'";
                if (constraint == Types.Component) {
                    subclass += $" or '{Types.GameObject.FullName}'";
                }

                var error = $"Underlying field type must derive from {subclass}";
                return _initResult = ValidationResult.Error(error);
            }


            if (!Enum.IsDefined(Types.SyncMode, options.SyncMode)) {
                var error = $"Invalid {nameof(Teo.AutoReference.SyncMode)}: {options.SyncMode}";
                return _initResult = ValidationResult.Error(error);
            }

            if (!Enum.IsDefined(Types.ContextMode, options.Context)) {
                return _initResult =
                    ValidationResult.Error($"Invalid {nameof(ContextMode)}: {options.Context}");
            }

            return _initResult = OnInitialize();
        }

        private bool ValidateUsage() {
            var contextIsValid = Context switch {
                ContextMode.Scene => Behaviour.GetPrefabMode() == ObjectUtils.EditingMode.InScene,
                ContextMode.Prefab => Behaviour.GetPrefabMode() == ObjectUtils.EditingMode.InPrefab,
                ContextMode.Default => true,
                _ => true,
            };

            return contextIsValid;
        }

        private SyncResult DoValidateOnly() {
            return ApplyAllFilters(Field.GetValues(Behaviour), validateExisting: true);
        }

        private SyncResult DoExpanded() {
            var existingValues = ApplyAllFilters(Field.GetValues(Behaviour));
            var requiredValues = ApplyAllFilters(GetObjects());
            return requiredValues.Concat(existingValues).Distinct();
        }

        private SyncResult DoExpandedPermissive() {
            var existingValues = ApplyBaseFilter(Field.GetValues(Behaviour));
            var requiredValues = ApplyAllFilters(GetObjects());
            return requiredValues.Concat(existingValues).Distinct();
        }

        private SyncResult DoIfEmptyPermissive() {
            var existingValues = ApplyBaseFilter(Field.GetValues(Behaviour)).ToReadOnlyListSmart();

            return existingValues.Count == 0
                ? ApplyAllFilters(GetObjects())
                : existingValues;
        }

        private SyncResult DoIfEmpty() {
            var existing = ApplyAllFilters(Field.GetValues(Behaviour), validateExisting: true);

            IReadOnlyList<Object> existingValues;
            try {
                // Note: We only check for exceptions when consuming the results after applying all filters.
                existingValues = existing.ToReadOnlyListSmart();
            } catch (Exception e) {
                LogException(e);
                return null;
            }

            return existingValues.Count == 0
                ? ApplyAllFilters(GetObjects())
                : existingValues;
        }

        private SyncResult DoAlways() {
            return ApplyAllFilters(GetObjects());
        }

        private SyncResult GetSyncedValues() {
            switch (SyncMode) {
                case SyncMode.Default when Field.IsPlain:
                    goto case SyncMode.IfEmpty;
                case SyncMode.Default when Field.IsArrayOrList:
                    goto case SyncMode.Always;
                case SyncMode.ValidateOnly:
                    return DoValidateOnly();
                case SyncMode.IfEmptyPermissive:
                    return DoIfEmptyPermissive();
                case SyncMode.IfEmpty:
                    return DoIfEmpty();
                case SyncMode.Always:
                    return DoAlways();
                case SyncMode.ExpandedPermissive:
                    return DoExpandedPermissive();
                case SyncMode.Expanded:
                    return DoExpanded();
                default:
                    return null;
            }
        }

        internal SyncStatus SyncReferences(MonoBehaviour behaviour) {
            if (_initResult.IsError || _hasFilterErrors && SyncPreferences.SkipSyncOnFilterError) {
                return SyncStatus.UsageError;
            }

            var status = _initResult.IsWarning ? SyncStatus.UsageWarning : SyncStatus.None;

            FieldContext = FieldContext.WithNewTarget(behaviour);

            if (!ValidateUsage()) {
                // Not an error - this just skips the syncing when it's intentional to do so.
                return status | SyncStatus.Skip;
            }

            var result = GetSyncedValues();

            if (result == null) {
                return status | SyncStatus.RuntimeError;
            }

            try {
                // Note: We only check for exceptions when consuming the results after applying all filters.
                Field.SetValue(Behaviour, result);
            } catch (Exception e) {
                LogException(e);
                return status | SyncStatus.RuntimeError;
            }

            return status | SyncStatus.Complete;
        }

        [Conditional("UNITY_EDITOR")]
        private void LogException(Exception exception) {
            var message = Formatter.FormatFieldException(exception, Field.FieldInfo, "Syncing failed for ");
            Debug.LogError(message);
        }
    }
}
