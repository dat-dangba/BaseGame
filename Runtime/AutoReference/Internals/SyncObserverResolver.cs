using System;
using System.Collections.Generic;
using System.Reflection;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals.Collections;

namespace Teo.AutoReference.Internals {
    internal static class SyncObserverResolver {
        private static readonly Dictionary<Type, MethodInfo[]> CachedData = new Dictionary<Type, MethodInfo[]>();
        public static int CacheSize => CachedData.Count;

        public static void ClearCache() {
            CachedData.Clear();
        }

        public static MethodInfo[] GetSyncObserverCallbacks(Type type) {
            if (type.IsUnityObject() || !Types.SyncObserver.IsAssignableFrom(type)) {
                return Array.Empty<MethodInfo>();
            }

            if (!SyncPreferences.CacheSyncInfo) {
                return GetSyncObserverCallbacksRaw(type);
            }
            if (CachedData.TryGetValue(type, out var info)) {
                return info;
            }

            return CachedData[type] = GetSyncObserverCallbacksRaw(type);
        }

        private static MethodInfo[] GetSyncObserverCallbacksRaw(Type type) {
            if (!Attribute.IsDefined(type, Types.Serializable)) {
                return Array.Empty<MethodInfo>();
            }

            // The order is significant. We use base types first.
            using var types = TempStack<Type>.Get();
            for (var ct = type; ct != Types.CsObject && Types.SyncObserver.IsAssignableFrom(ct); ct = ct!.BaseType) {
                types.Push(ct);
            }

            using var result = TempList<MethodInfo>.Get();
            foreach (var currentType in types) {
                var interfaceMap = currentType.GetInterfaceMap(Types.SyncObserver);

                // A type can have two versions of the same interface method.
                // - a virtual override from a parent class
                // - a direct or explicit implementation.
                // We handle both these cases separately:
                AddAndUpdateVirtualSyncObserver(currentType, interfaceMap, result);
                AddExplicitSyncObserver(currentType, interfaceMap, result);
            }

            return result.ToArray();
        }

        private static void AddAndUpdateVirtualSyncObserver(Type type, InterfaceMapping map, List<MethodInfo> result) {
            MethodInfo info = null;
            // Find the appropriate info
            for (var i = 0; i < map.InterfaceMethods.Length; ++i) {
                if (!ReferenceEquals(map.InterfaceMethods[i], Types.SyncObserverMethod)) {
                    continue;
                }
                var currentInfo = map.TargetMethods[i];
                if (currentInfo.DeclaringType == type) {
                    info = currentInfo;
                }
                break;
            }
            if (info == null || info.IsPrivate || !info.IsVirtual) {
                // If this is true then it will be found later in AddExplicitSyncObserver
                return;
            }

            var baseDef = info.GetBaseDefinition();
            var baseDefHandle = baseDef.MethodHandle;
            var baseDefModule = baseDef.Module;

            // We remove all previous overrides of the same method to maintain the expected outcome because:
            // - A virtual method that has many overrides should only appear once
            // - It should be the LAST override in the chain.

            for (var i = result.Count - 1; i >= 0; --i) {
                var existingBaseDef = result[i].GetBaseDefinition();
                if (existingBaseDef.MethodHandle == baseDefHandle && existingBaseDef.Module == baseDefModule) {
                    result.RemoveAt(i);
                    break;
                }
            }

            result.Add(info);
        }


        private static void AddExplicitSyncObserver(Type type, InterfaceMapping map, List<MethodInfo> result) {
            if (!Types.SyncObserver.IsAssignableFrom(type)) {
                return;
            }

            for (var i = 0; i < map.InterfaceMethods.Length; ++i) {
                if (!ReferenceEquals(map.InterfaceMethods[i], Types.SyncObserverMethod)) {
                    continue;
                }

                var implementation = map.TargetMethods[i];
                if (implementation.DeclaringType == type && implementation.IsPrivate) {
                    result.Add(implementation);
                }
                break;
            }

        }
    }
}
