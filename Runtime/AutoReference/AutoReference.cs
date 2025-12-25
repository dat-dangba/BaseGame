// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Provides methods for syncing and processing Auto-References in <see cref="MonoBehaviour"/> scripts.
    /// </summary>
    public static class AutoReference {
        private static readonly ObjectWatcher Watcher = new ObjectWatcher();

        static AutoReference() {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += () => {
                AutoReferenceResolver.ClearCache();
                SyncObserverResolver.ClearCache();
            };
#endif
        }

        /// <summary>
        /// Syncs all auto-references of all <see cref="MonoBehaviour"/>s on the given <see cref="GameObject"/>.
        /// </summary>
        public static SyncStatus Sync(GameObject gameObject) {
            if (Application.isPlaying) {
                return SyncStatus.None;
            }

            using var behaviours = TempList<MonoBehaviour>.Get();
            gameObject.GetComponents(behaviours);

            var status = behaviours.Aggregate(
                SyncStatus.None, (current, behaviour) => current | SyncNoAppend(behaviour)
            );

            LogContext.AppendStatusSummary(status);

            return status;
        }

        private static SyncStatus SyncNoAppend(MonoBehaviour behaviour) {
            if (Application.isPlaying) {
                return SyncStatus.Unsupported;
            }

            if (behaviour == null) {
                return SyncStatus.Skip;
            }

            var metadata = AutoReferenceResolver.GetAutoReferenceInfo(behaviour.GetType());

            if (DoSync(behaviour, metadata, out var syncStatus)) {
                SetDirty(behaviour);
            }
            return syncStatus | SyncStatus.Complete;
        }

        /// <summary>
        /// Syncs all auto-references on the given <see cref="MonoBehaviour"/>.
        /// </summary>
        public static SyncStatus Sync(MonoBehaviour behaviour) {
            var status = SyncNoAppend(behaviour);
            LogContext.AppendStatusSummary(status);
            return status;
        }

        public static bool IsInScene(Object obj) {
            return obj.GetPrefabMode() == ObjectUtils.EditingMode.InScene;
        }

        public static bool IsPrefab(Object obj) {
            return obj.GetPrefabMode() == ObjectUtils.EditingMode.InPrefab;
        }

        /// <summary>
        /// This method performs auto-reference syncing on a MonoBehaviour from the sync information taken from its
        /// type. It also runs after-sync validation calls and checks and returns if any synced fields have been
        /// modified.
        /// </summary>
        private static bool DoSync(
            MonoBehaviour behaviour,
            in AutoReferenceTypeInfo info,
            out SyncStatus status
        ) {
            status = LogItem.ProcessMessages(info.messages);

            if (!info.IsSyncable) {
                status |= SyncStatus.Skip;
                return false;
            }

            // If no synced fields exist (which include auto-reference fields and other serializable fields),
            // then no change detection is necessary, we just run the callbacks and assume nothing changed.
            if (!info.HasSyncableFields) {
                RunAfterSyncCallbacks(behaviour, info.syncCallbacks);
                return false;
            }

            // The watcher creates an internal serializable object that it uses to detect changes.
            using var watcher = Watcher.Init(behaviour, info);

            foreach (var autoField in info.autoReferenceFields) {
                status |= autoField.MainAttribute.SyncReferences(behaviour);
            }

            RunSyncObserverCallbacks(behaviour, info.syncObserverCallbacks);
            RunAfterSyncCallbacks(behaviour, info.syncCallbacks);

            return watcher.IsObjectModified();
        }


        private static void LogSyncException(Exception exception, MethodInfo callback) {
            var message = Formatter.FormaMethodException(exception, callback, "Sync method ");
            Debug.LogError(message);
        }

        private static void RunAfterSyncCallbacks(MonoBehaviour behaviour, MethodInfo[] callbacks) {
            foreach (var callback in callbacks) {
                try {
                    callback.Invoke(behaviour, null);
                } catch (Exception e) {
                    LogSyncException(e, callback);
                }
            }
        }

        private static void RunSyncObserverCallbacks(
            MonoBehaviour behaviour,
            SyncObserverInfo[] info
        ) {
            foreach (var syncable in info) {
                try {
                    var target = syncable.targetField.GetValue(behaviour);
                    if (target == null) {
                        continue;
                    }
                    syncable.methodInfo.Invoke(target, new object[] { behaviour });
                    syncable.targetField.SetValue(behaviour, target);
                } catch (Exception e) {
                    LogSyncException(e, syncable.methodInfo);
                }
            }
        }

        public static bool HasSyncInformation(MonoBehaviour behaviour) {
            return behaviour != null && AutoReferenceResolver.GetAutoReferenceInfo(behaviour.GetType()).IsSyncable;
        }

        public static bool HasSyncInformation(Type type) {
            return type.IsSubclassOf(Types.Mono) && AutoReferenceResolver.GetAutoReferenceInfo(type).IsSyncable;
        }

        /// <summary>
        /// Resolves and calls all chained implementations of <see cref="ISyncObserver"/> in the list.
        /// </summary>
        /// <param name="target">The <see cref="MonoBehaviour"/> which ultimately contains the data.</param>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        [Conditional("UNITY_EDITOR")]
        public static void SyncData<T>(MonoBehaviour target, IList<T> list) where T : ISyncObserver {
            if (Application.isPlaying || list == null) {
                return;
            }

            try {
                for (var index = 0; index < list.Count; index++) {
                    var element = list[index];
                    if (element == null) {
                        continue;
                    }
                    SyncDataByRefRaw(target, ref element);
                    list[index] = element;
                }
            } catch (Exception e) {
                var method = MethodBase.GetCurrentMethod();
                LogSyncException(e, method as MethodInfo);
            }
        }

        /// <summary>
        /// Manually resolves and calls all chained implementations of a <see cref="ISyncObserver"/> object.
        /// Useful for <see cref="ISyncObserver"/> objects with nested serializable <see cref="ISyncObserver"/> fields.
        /// </summary>
        /// <param name="target">The <see cref="MonoBehaviour"/> which ultimately contains the data.</param>
        /// <param name="list">A list that contains <see cref="ISyncObserver"/> types.</param>
        /// <param name="predicate">A predicate to filter which elements of the list to sync.</param>
        /// <typeparam name="T"></typeparam>
        [Conditional("UNITY_EDITOR")]
        public static void SyncData<T>(
            MonoBehaviour target,
            IList<T> list,
            Predicate<T> predicate
        ) where T : ISyncObserver {
            if (Application.isPlaying || list == null) {
                return;
            }

            try {
                for (var index = 0; index < list.Count; index++) {
                    var element = list[index];
                    if (element == null || !predicate.Invoke(element)) {
                        continue;
                    }
                    SyncDataByRefRaw(target, ref element);
                    list[index] = element;
                }
            } catch (Exception e) {
                var method = MethodBase.GetCurrentMethod();
                LogSyncException(e, method as MethodInfo);
            }
        }

        /// <summary>
        /// Manually resolves and calls all chained implementations of a <see cref="ISyncObserver"/> struct object.
        /// Useful for <see cref="ISyncObserver"/> objects with nested serializable <see cref="ISyncObserver"/> fields.
        /// </summary>
        /// <param name="target">The <see cref="MonoBehaviour"/> which ultimately contains the data.</param>
        /// <param name="observer"></param>
        [Conditional("UNITY_EDITOR")]
        public static void SyncData<T>(MonoBehaviour target, ref T observer) where T : struct, ISyncObserver {
            if (Application.isPlaying) {
                return;
            }

            SyncDataByRefRaw(target, ref observer);
        }

        /// <summary>
        /// Manually resolves and calls all chained implementations of a <see cref="ISyncObserver"/> class object.
        /// Useful for <see cref="ISyncObserver"/> objects with nested serializable <see cref="ISyncObserver"/> fields.
        /// </summary>
        /// <param name="target">The <see cref="MonoBehaviour"/> which ultimately contains the data.</param>
        /// <param name="observer"></param>
        [Conditional("UNITY_EDITOR")]
        public static void SyncData<T>(MonoBehaviour target, T observer) where T : class, ISyncObserver {
            if (Application.isPlaying || observer == null) {
                return;
            }

            var methods = SyncObserverResolver.GetSyncObserverCallbacks(target.GetType());
            foreach (var method in methods) {
                try {
                    method.Invoke(observer, Types.TempParams(target));
                } catch (Exception e) {
                    LogSyncException(e, method);
                }
            }
        }

        // Unlike the public non-raw methods, this works on both classes and structs without additional validation.
        private static void SyncDataByRefRaw<T>(MonoBehaviour target, ref T observer) where T : ISyncObserver {
            // Boxing before the method invocation is required for changes to be applied on structs.
            var boxed = (object)observer;
            var methods = SyncObserverResolver.GetSyncObserverCallbacks(boxed.GetType());
            foreach (var method in methods) {
                try {
                    method.Invoke(boxed, Types.TempParams(target));
                } catch (Exception e) {
                    LogSyncException(e, method);
                }
            }
            observer = (T)boxed;
        }


        /// <summary>
        /// Identical to <see cref="EditorUtility.SetDirty"/> but safe to call in builds (no-op)
        /// </summary>
        /// <param name="target"></param>
        [Conditional("UNITY_EDITOR")]
        public static void SetDirty(Object target) {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void ClearCache() {
            var typeCount = AutoReferenceResolver.CacheSize + SyncObserverResolver.CacheSize;
            if (typeCount == 0) {
                return;
            }

            AutoReferenceResolver.ClearCache();
            SyncObserverResolver.ClearCache();

            var types = Formatter.FormatCount(typeCount, "type");
            Debug.Log($"Cleared cached Auto-Reference information of {types}.");
        }
    }
}
