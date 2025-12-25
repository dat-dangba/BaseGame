// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Teo.AutoReference.Configuration {
    internal static class UserSettings {
        private const string CacheSyncInfoPath = "Auto-Reference/CacheSyncInfo";
        private const string SyncToggleSidePath = "Auto-Reference/SyncToggleSide";
        private const string SyncToggleStylePath = "Auto-Reference/SyncToggleStyle";

        private const string BatchLogLevelPath = "Auto-Reference/BatchLogLevel";
        private const string EditorSelectLogLevelPath = "Auto-Reference/EditorSelectLogLevel";
        private const string DefaultLogLevelPath = "Auto-Reference/DisableLogLevel";

        private const string EnablePrettyPrintPath = "Auto-Reference/EnableExceptionFormatting";
        private const string ExceptionNameFormatPath = "Auto-Reference/ExceptionNameFormat";
        private const string MessageFormatPath = "Auto-Reference/MessageFormat";
        private const string SymbolFormatPath = "Auto-Reference/SymbolFormat";
        private const string DefaultFormatPath = "Auto-Reference/DefaultFormat";

        private const SyncToggleSide DefaultSyncToggleSide = SyncToggleSide.Middle;
        private const SyncToggleStyle DefaultSyncToggleStyle = SyncToggleStyle.Circle;

        private const InternalLogLevel DefaultBatchLogLevel = InternalLogLevel.Compact;
        private const InternalLogLevel DefaultEditorSelectLogLevel = InternalLogLevel.Expanded;
        private const InternalLogLevel DefaultDefaultLogLevel = InternalLogLevel.Off;

        private const bool DefaultCacheSyncInfo = true;
        private const bool DefaultEnablePrettyPrint = false;

        private static readonly Color32 DefaultExceptionNameColor = new Color32(255, 102, 102, 255);
        private static readonly Color32 DefaultMessageColor = new Color32(255, 255, 102, 255);
        private static readonly Color32 DefaultSymbolColor = new Color32(77, 255, 235, 255);
        private static readonly Color32 DefaultDefaultColor = new Color32(255, 255, 255, 255);

        private static FormatInfo _cachedExceptionFormatInfo;
        private static FormatInfo _cachedMessageFormatInfo;
        private static FormatInfo _cachedSymbolFormatInfo;
        private static FormatInfo _cachedDefaultFormatInfo;

        private static FormatInfo DefaultExceptionNameInfo { get; } = new FormatInfo(DefaultExceptionNameColor);
        private static FormatInfo DefaultMessageInfo { get; } = new FormatInfo(DefaultMessageColor);
        private static FormatInfo DefaultSymbolInfo { get; } = new FormatInfo(DefaultSymbolColor);
        private static FormatInfo DefaultDefaultInfo { get; } = new FormatInfo(DefaultDefaultColor);

        public static SyncToggleSide SyncToggleSide {
            get => Get(SyncToggleSidePath, DefaultSyncToggleSide);
            set => Set(SyncToggleSidePath, value);
        }

        public static SyncToggleStyle SyncToggleStyle {
            get => Get(SyncToggleStylePath, DefaultSyncToggleStyle);
            set => Set(SyncToggleStylePath, value);
        }

        public static InternalLogLevel BatchLogLevel {
            get => Get(BatchLogLevelPath, DefaultBatchLogLevel);
            set => Set(BatchLogLevelPath, value);
        }

        public static InternalLogLevel EditorSelectLogLevel {
            get => Get(EditorSelectLogLevelPath, DefaultEditorSelectLogLevel);
            set => Set(EditorSelectLogLevelPath, value);
        }

        public static InternalLogLevel DefaultLogLevel {
            get => Get(DefaultLogLevelPath, DefaultDefaultLogLevel);
            set => Set(DefaultLogLevelPath, value);
        }

        public static bool CacheSyncInfo {
            get => Get(CacheSyncInfoPath, DefaultCacheSyncInfo);
            set => Set(CacheSyncInfoPath, value);
        }

        public static bool EnableExceptionFormatting {
            get => Get(EnablePrettyPrintPath, DefaultEnablePrettyPrint);
            set => Set(EnablePrettyPrintPath, value);
        }

        public static FormatInfo ExceptionNameFormat {
            get => Get(ExceptionNameFormatPath, ref _cachedExceptionFormatInfo, DefaultExceptionNameInfo);
            set => Set(ExceptionNameFormatPath, value, out _cachedExceptionFormatInfo);
        }

        public static FormatInfo MessageFormat {
            get => Get(MessageFormatPath, ref _cachedMessageFormatInfo, DefaultMessageInfo);
            set => Set(MessageFormatPath, value, out _cachedMessageFormatInfo);
        }

        public static FormatInfo SymbolFormat {
            get => Get(SymbolFormatPath, ref _cachedSymbolFormatInfo, DefaultSymbolInfo);
            set => Set(SymbolFormatPath, value, out _cachedSymbolFormatInfo);
        }

        public static FormatInfo DefaultFormat {
            get => Get(DefaultFormatPath, ref _cachedDefaultFormatInfo, DefaultDefaultInfo);
            set => Set(DefaultFormatPath, value, out _cachedDefaultFormatInfo);
        }

        private static FormatInfo Get(string path, ref FormatInfo cachedValue, in FormatInfo defaultValue) {
#if UNITY_EDITOR
            var str = EditorPrefs.GetString(path, null);
            if (str == null) {
                cachedValue = defaultValue;
                return defaultValue;
            }

            if (str == cachedValue.SerializedValue) {
                return cachedValue;
            }

            if (FormatInfo.TryParse(str, out var info)) {
                cachedValue = info;
                return info;
            }

            cachedValue = defaultValue;
            EditorPrefs.DeleteKey(path);
            return defaultValue;
#else
            return defaultValue;
#endif
        }

        private static void Set(string path, FormatInfo value, out FormatInfo cachedValue) {
#if UNITY_EDITOR
            var str = value.SerializedValue;
            EditorPrefs.SetString(path, str);
            cachedValue = value;
#else
            cachedValue = default;
#endif
        }

        private static bool Get(string path, bool defaultValue) {
#if UNITY_EDITOR
            return EditorPrefs.GetBool(path, defaultValue);
#else
            return defaultValue;
#endif
        }

        private static void Set(string path, bool value) {
#if UNITY_EDITOR
            EditorPrefs.SetBool(path, value);
#endif
        }

        private static T Get<T>(string path, T defaultValue) where T : Enum {
#if UNITY_EDITOR
            var value = EditorPrefs.GetInt(path, Convert.ToInt32(defaultValue));
            var enumValue = (T)Enum.ToObject(typeof(T), value);
            if (!Enum.IsDefined(typeof(T), enumValue)) {
                enumValue = defaultValue;
            }
            return enumValue;
#else
            return defaultValue;
#endif
        }

        private static void Set<T>(string path, T value) where T : Enum {
#if UNITY_EDITOR
            if (!Enum.IsDefined(typeof(T), value)) {
                return;
            }

            EditorPrefs.SetInt(path, Convert.ToInt32(value));
#endif
        }

        public static void ResetToDefaults() {
            CacheSyncInfo = DefaultCacheSyncInfo;
            SyncToggleSide = DefaultSyncToggleSide;
            SyncToggleStyle = DefaultSyncToggleStyle;

            DefaultLogLevel = DefaultDefaultLogLevel;
            BatchLogLevel = DefaultBatchLogLevel;
            EditorSelectLogLevel = DefaultEditorSelectLogLevel;
            EnableExceptionFormatting = DefaultEnablePrettyPrint;

            ExceptionNameFormat = DefaultExceptionNameInfo;
            MessageFormat = DefaultMessageInfo;
            SymbolFormat = DefaultSymbolInfo;
            DefaultFormat = DefaultDefaultInfo;
        }
    }
}
