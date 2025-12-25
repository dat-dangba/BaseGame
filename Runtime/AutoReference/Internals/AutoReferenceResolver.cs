// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Responsible for retrieving auto-reference information from types.
    /// </summary>
    internal static class
        AutoReferenceResolver {
        // Flags used to retrieve all fields.
        private const BindingFlags FieldFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        // Flags used to retrieve after-sync callback methods.
        private const BindingFlags SyncCallbackFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.DeclaredOnly;

        private static readonly string OnAfterSyncName = nameof(OnAfterSyncAttribute).TrimEnd("Attribute");

        private static readonly Dictionary<Type, AutoReferenceTypeInfo> CachedData =
            new Dictionary<Type, AutoReferenceTypeInfo>();

        public static int CacheSize => CachedData.Count;

        private static ValidationResult IsAutoField(FieldInfo field) {
            var (construct, type) = ObjectField.GetConstructAndUnderlyingType(field.FieldType);
            if (type.IsSameOrSubclassOf(Types.UnityObject)) {
                return field.MightBeSerializable() && !type.IsGenericType
                    ? ValidationResult.Ok
                    : ValidationResult.Error("Field must be serializable");
            }

            switch (construct) {
                case ConstructType.Array:
                    return ValidationResult.Warning("Array must contain a type deriving from 'UnityEngine.Object'");
                case ConstructType.List:
                    return ValidationResult.Warning("List must contain a type deriving from 'UnityEngine.Object'");
                case ConstructType.Invalid:
                case ConstructType.Plain:
                default:
                    return ValidationResult.Warning("Field must derive from 'UnityEngine.Object'");
            }
        }

        /// <summary>
        /// Get all fields of a MonoBehaviour type up to the MonoBehaviour class (excluded).
        /// </summary>
        /// <param name="type"></param>
        private static FieldInfo[] GetAllRelevantFields(Type type) {
            using var fields = TempList<FieldInfo>.Get();
            if (!type.IsSameOrSubclassOf(Types.Mono)) {
                return Array.Empty<FieldInfo>();
            }

            while (type != Types.Mono) {
                fields.AddRange(type!.GetFields(FieldFlags));
                type = type.BaseType;
            }

            return fields.ToArrayOrEmpty();
        }

        internal static AutoReferenceTypeInfo GetAutoReferenceInfo(Type type) {
            // Attempt to get reflection info from the cache. This info contains an auto-reference attribute,
            // all auto-reference filters, and references to after-sync callback methods.

            if (!SyncPreferences.CacheSyncInfo) {
                return GetAutoReferenceInfoRaw(type);
            }

            if (CachedData.TryGetValue(type, out var info)) {
                return info;
            }

            return CachedData[type] = GetAutoReferenceInfoRaw(type);
        }

        /// <summary>
        /// Gets all auto-reference related metadata of a type. This includes auto-reference fields,
        /// after-sync callback functions, and any other syncable fields.
        /// </summary>
        private static AutoReferenceTypeInfo GetAutoReferenceInfoRaw(Type type) {
            // This list will contain the fields that are serializable, valid, and contain an auto-reference attribute.
            // A valid field is one that's of type T, T[] or List<T>, and where T is a UnityEngine.Object
            using var autoFields = TempList<AutoReferenceField>.Get();

            // This list will contain all serializable fields that should be monitored for changes, which are mutually
            // exclusive to autoFields (because those are always monitored).
            using var syncedFields = TempList<FieldInfo>.Get();

            using var syncHandlerCalbacks = TempList<SyncObserverInfo>.Get();

            using var messages = TempList<LogItem>.Get();
            using var attributes = TempList<AutoReferenceAttribute>.Get();
            using var filters = TempList<AutoReferenceFilterAttribute>.Get();

            var syncCallbacks = GetAllSyncMethods(type, messages, out var declaredCount);
            var hasSyncCallbacks = syncCallbacks.Length > 0;

            ProcessSyncObserverErrors(type, messages);

            foreach (var field in GetAllRelevantFields(type)) {
                attributes.Clear();
                filters.Clear();

                var options = field.GetCustomAttribute<SyncOptionsAttribute>();
                attributes.AddRange(field.GetCustomAttributes<AutoReferenceAttribute>());
                filters.AddRange(field.GetCustomAttributes<AutoReferenceFilterAttribute>());

                var dataSyncMethods = SyncObserverResolver.GetSyncObserverCallbacks(field.FieldType);

                syncHandlerCalbacks.AddRange(
                    dataSyncMethods.Select(info => new SyncObserverInfo { targetField = field, methodInfo = info })
                );

                var isAutoField = false;
                if (attributes.Count + filters.Count > 0 || options != null) {
                    var shouldSyncResult = IsAutoField(field);
                    if (shouldSyncResult.IsOk) {
                        isAutoField = true;
                    } else {
                        var logName = GetLogName(attributes, filters, options);
                        // In this case the warning rejects this field too
                        messages.Add(new LogItem(shouldSyncResult, field, logName));
                    }
                }

                if (isAutoField) {
                    var autoField = GetAutoField(type, field, messages, options, attributes, filters);
                    if (autoField.IsValid) {
                        autoFields.Add(autoField);
                    }
                } else if ((hasSyncCallbacks || syncHandlerCalbacks.Count > 0) && field.MightBeSerializable()) {
                    syncedFields.Add(field);
                }
            }

            return new AutoReferenceTypeInfo {
                autoReferenceFields = autoFields.ToArrayOrEmpty(),
                syncedFields = syncedFields.ToArrayOrEmpty(),
                syncCallbacks = syncCallbacks,
                syncObserverCallbacks = syncHandlerCalbacks.ToArrayOrEmpty(),
                declaredCallbacksCount = declaredCount,
                messages = messages.ToArrayOrEmpty(),
            };
        }

        private static string GetLogName(
            List<AutoReferenceAttribute> attributes,
            List<AutoReferenceFilterAttribute> filters,
            SyncOptionsAttribute options
        ) => GetNameFromList(attributes) ?? GetNameFromList(filters) ?? options?.Name;

        private static string GetNameFromList<T>(IList<T> list) where T : AutoReferenceBaseAttribute {
            return list.Count switch {
                0 => null,
                1 => list[0].Name,
                _ => $"{list[0].Name} (+{list.Count - 1})",
            };
        }


        /// <summary>
        /// Get the <see cref="AutoReferenceField"/> from a specific field, which includes its effective
        /// <see cref="AutoReferenceAttribute"/> and any <see cref="AutoReferenceFilterAttribute"/>s that may be
        /// attached to it.
        /// </summary>
        private static AutoReferenceField GetAutoField(
            Type type,
            FieldInfo info,
            List<LogItem> errorList,
            SyncOptionsAttribute syncOptions,
            List<AutoReferenceAttribute> attributes,
            List<AutoReferenceFilterAttribute> filterAttributes
        ) {
            var field = new ObjectField(info);

            if (!field.IsValid) {
                return default;
            }

            // Use ApplyFiltersAttribute if none are provided
            var mainAttribute = attributes.Count > 0 ? attributes[0] : ApplyFiltersAttribute.Instance;

            if (attributes.Count > 1) {
                const string message = "Multiple primary attributes exist";
                const string format = "'%s': %m. Only '%n' will take effect.";
                errorList.Add(new LogItem(ValidationResult.Warning(message), info, mainAttribute.Name, format));
            }

            var fieldContext = new FieldContext(type, field);
            var hasErrors = false;
            var filters = filterAttributes
                .Where(filter => InitializeFilter(filter, mainAttribute, fieldContext, errorList, ref hasErrors))
                .OrderBy(f => f)
                .ToArray();

            if (mainAttribute is ApplyFiltersAttribute && filters.Length == 0) {
                // This happens only when no main attribute is given and all filters failed to initialize.
                return default;
            }
            if (ReferenceEquals(mainAttribute, ApplyFiltersAttribute.Instance)) {
                // Fixes an issue where the data of the ApplyFiltersAttribute is reset in some cases.
                // We still use the singleton instance up to now to avoid creating a new instance unless required.
                mainAttribute = new ApplyFiltersAttribute();
            }

            var result = mainAttribute.Initialize(type, syncOptions, field, filters, hasErrors);

            if (!result.IsOk) {
                var logItem = new LogItem(result, field.FieldInfo, mainAttribute.Name, "%e") {
                    ErrorLabel = "%n: Failed to sync field '%s':\n%m.",
                    WarningLabel = "%n: Field '%i' on '%t':\n%m.",
                };

                errorList.Add(logItem);
            }

            return new AutoReferenceField(field, mainAttribute);
        }

        private static bool InitializeFilter(
            AutoReferenceFilterAttribute filter,
            AutoReferenceAttribute attribute,
            FieldContext fieldContext,
            List<LogItem> errors,
            ref bool hasErrors
        ) {
            var result = filter.Initialize(attribute, fieldContext);

            if (result.IsOk) {
                return true;
            }

            hasErrors = hasErrors || result.IsError;

            const string warning = LogItem.DefaultFormat;
            const string error = "Skipping filter '%n' on '%s':\n%m.";

            errors.Add(new LogItem(result, fieldContext, filter.Name, result.IsError ? error : warning));

            return result;
        }

        private static IEnumerable<CallbackInfo> GetDeclaredCallbacks(Type type) {
            // The class-level OnAfterSync should be called first
            yield return new CallbackInfo(type);

            foreach (var method in type.GetMethods(SyncCallbackFlags)) {
                yield return new CallbackInfo(type, method);
            }
        }

        /// <summary>
        /// Retrieves all non-field methods with the 'OnAfterSync' attribute in a given type and its base types.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to retrieve the methods from.</param>
        /// <param name="errors"></param>
        /// <param name="declaredCount">Non-inherited callbacks that exist in this type.</param>
        private static MethodInfo[] GetAllSyncMethods(Type type, List<LogItem> errors, out int declaredCount) {
            declaredCount = 0;
            if (!type.IsSubclassOf(Types.Mono)) {
                return Array.Empty<MethodInfo>();
            }

            using var callbackList = TempList<MethodInfo>.Get();
            using var set = TempSet<(Type, string)>.Get();
            using var types = TempStack<Type>.Get();

            // We use a stack to put base callbacks first
            for (var currentType = type; currentType != Types.Mono; currentType = currentType!.BaseType) {
                types.Push(currentType);
            }

            while (types.TryPop(out var currentType)) {
                foreach (var callback in GetDeclaredCallbacks(currentType)) {
                    if (callback.TryGetLog(out var log)) {
                        errors.Add(log);
                    }

                    var method = callback.method;
                    if (method == null) {
                        continue;
                    }

                    // Identify unique methods via their real declaring type and name
                    // Simply putting them into a set isn't always accurate.
                    var declaringType = method.GetBaseDefinition().DeclaringType;

                    if (!set.Add((declaringType, method.Name))) {
                        continue;
                    }

                    if (callback.caller == type) {
                        ++declaredCount;
                    }

                    callbackList.Add(method);
                }
            }

            return callbackList.ToArrayOrEmpty();
        }

        public static void ProcessSyncObserverErrors(Type type, List<LogItem> messages) {
            if (!IsSyncObserverError(type)) {
                return;
            }

            var warning = ValidationResult.Warning(
                $"'{nameof(ISyncObserver)}' is not supported for types derived from 'UnityEngine.Object'"
            );
            messages.Add(new LogItem(warning, type, nameof(ISyncObserver), "", "%t"));
        }

        private static bool IsSyncObserverError(Type type) {
            // UnityEngine.Object types should not implement ISyncObserver. But we should only show a warning
            // when the type implements it directly to avoid repeating it in case of inheritance.

            if (!type.IsUnityObject() || !Types.SyncObserver.IsAssignableFrom(type)) {
                return false;
            }

            var baseType = type.BaseType;
            if (baseType == null || !Types.SyncObserver.IsAssignableFrom(baseType)) {
                // If the base type is not an ISyncObserver, then this type is a direct implementation
                return true;
            }

            // A type can implement an interface separate from its parent, so we take this into account as well
            var map = type.GetInterfaceMap(Types.SyncObserver);
            foreach (var m in map.TargetMethods) {
                if (m.DeclaringType == type && m.IsPrivate) {
                    return true;
                }
            }

            return false;
        }

        private readonly struct CallbackInfo {
            /// The type that calls the callback, which is not the same as the type that defines the method.
            public readonly Type caller;

            public readonly MethodInfo method;

            private readonly string _name;
            private readonly ValidationResult _result;

            public CallbackInfo(Type caller, MethodInfo methodInfo) {
                this.caller = caller;

                var call = methodInfo.GetCustomAttribute<OnAfterSyncAttribute>();
                if (call == null) {
                    method = null;
                    _name = null;
                    _result = ValidationResult.Ok;
                    return;
                }

                _name = methodInfo.Name;

                var callbackResult = CallbackMethodInfo.ValidateSignature(methodInfo, Types.Void, Type.EmptyTypes);
                if (callbackResult) {
                    if (!string.IsNullOrWhiteSpace(call.MethodName)) {
                        var warning = $"Method name '{call.MethodName}' will be ignored";
                        _result = ValidationResult.Warning(warning);
                    } else {
                        _result = callbackResult;
                    }

                    method = methodInfo;
                } else {
                    method = null;
                    _result = callbackResult;
                }
            }

            public CallbackInfo(Type caller) {
                this.caller = caller;
                var call = caller.GetCustomAttribute<OnAfterSyncAttribute>();

                if (call == null) {
                    method = null;
                    _name = null;
                    _result = ValidationResult.Ok;
                    return;
                }

                var info = CallbackMethodInfo.Create(caller, null, call.MethodName, Types.Void, Type.EmptyTypes);

                method = info.MethodInfo;

                _name = "";
                _result = info.Result;
            }

            public bool TryGetLog(out LogItem log) {
                if (_result.IsOk) {
                    log = default;
                    return false;
                }

                log = new LogItem(_result, caller, _name, OnAfterSyncName, "'%n' on '%t':\n%m");
                return true;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void ClearCache() {
            CachedData.Clear();
        }
    }
}
