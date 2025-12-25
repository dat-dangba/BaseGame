// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Teo.AutoReference.Configuration {
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class SyncPreferences {
        public const string ProjectSettingsPath = "Project/Auto-Reference";
        public const string UserSettingsPath = "Preferences/Auto-Reference";

        private const string DisableOnInspectSymbol = "AUTOREF_DISABLE_ONINSPECT";
        private const string EnableTriInspectorSymbol = "AUTOREF_TRIINSPECTOR";
        private const string EnableUnityToolboxSymbol = "AUTOREF_EDITOR_TOOLBOX";
        private const string EnableOdinInspectorSymbol = "AUTOREF_ODIN_INSPECTOR";
        private const string TriInspectorClassCheck = "TriInspector.TriValueDrawer`1";
        private const string UnityEditorToolboxClassCheck = "Toolbox.Editor.ToolboxEditor";
        // ReSharper disable once UnusedMember.Local
        private const string OdinInspectorClassCheck = "Sirenix.OdinInspector.Editor.OdinEditor";

        private const string ConfigFilePath = "ProjectSettings/SyncConfiguration.json";

        private static ProjectSettings _cachedProjectSettings;

        public static bool SyncOnInspect => ProjectSettings.syncOnInspect;
        public static bool SyncOnSceneSave => ProjectSettings.syncOnSceneSave;
        public static bool SyncOnAssemblyReload => ProjectSettings.syncOnAssemblyReload;
        public static bool SyncScenesOnBuild => ProjectSettings.syncScenesOnBuild;
        public static bool FailBuildOnError => ProjectSettings.failBuildOnError;
        public static bool FailBuildOnWarnings => ProjectSettings.failBuildOnWarnings;
        public static bool SkipSyncOnFilterError => ProjectSettings.skipSyncOnFilterError;

        public static bool IsEditorToolboxIntegrationEnabled => ProjectSettings.enableEditorToolboxIntegration;
        public static bool IsTriInspectorIntegrationEnabled => ProjectSettings.enableTriInspectorIntegration;
        public static bool IsOdinInspectorIntegrationEnabled => ProjectSettings.enableOdinInspectorIntegration;

        public static bool IsEditorToolboxAvailable { get; private set; }
        public static bool IsTriInspectorAvailable { get; private set; }
        public static bool IsOdinInspectorAvailable { get; private set; }

        public static bool AnyIntegrationIsAvailable =>
            IsEditorToolboxAvailable || IsTriInspectorAvailable || IsOdinInspectorAvailable;

        internal static SyncToggleStyle SyncToggleStyle {
            get => UserSettings.SyncToggleStyle;
            set => UserSettings.SyncToggleStyle = value;
        }

        internal static SyncToggleSide SyncToggleSide {
            get => UserSettings.SyncToggleSide;
            set => UserSettings.SyncToggleSide = value;
        }

        public static LogLevel DefaultLogLevel {
            get => (LogLevel)UserSettings.DefaultLogLevel;
            internal set => UserSettings.DefaultLogLevel = (InternalLogLevel)value;
        }

        public static LogLevel EditorSelectLogLevel {
            get => (LogLevel)UserSettings.EditorSelectLogLevel;
            internal set => UserSettings.EditorSelectLogLevel = (InternalLogLevel)value;
        }

        public static LogLevel BatchLogLevel {
            get => (LogLevel)UserSettings.BatchLogLevel;
            set => UserSettings.BatchLogLevel = (InternalLogLevel)value;
        }

        public static bool CacheSyncInfo {
            get => UserSettings.CacheSyncInfo;
            internal set => UserSettings.CacheSyncInfo = value;
        }

        public static bool EnableExceptionFormatting {
            get => UserSettings.EnableExceptionFormatting;
            internal set => UserSettings.EnableExceptionFormatting = value;
        }

        internal static FormatInfo ExceptionFormatInfo {
            get => UserSettings.ExceptionNameFormat;
            set => UserSettings.ExceptionNameFormat = value;
        }

        internal static FormatInfo MessageFormatInfo {
            get => UserSettings.MessageFormat;
            set => UserSettings.MessageFormat = value;
        }

        internal static FormatInfo SymbolFormatInfo {
            get => UserSettings.SymbolFormat;
            set => UserSettings.SymbolFormat = value;
        }

        internal static FormatInfo DefaultFormatInfo {
            get => UserSettings.DefaultFormat;
            set => UserSettings.DefaultFormat = value;
        }

        internal static ProjectSettings ProjectSettings {
            get {
#if UNITY_EDITOR
                if (_cachedProjectSettings == null) {
                    ReloadConfiguration();
                }
                return _cachedProjectSettings;
#else
                return null;
#endif
            }
        }

#if UNITY_EDITOR

        static SyncPreferences() {
            ReloadConfiguration();
            ApplySettings();
        }

        private static void InitOrResetConfiguration(ref ProjectSettings config) {
            if (config == null) {
                config = new ProjectSettings();
            } else {
                config.ResetToDefaults();
            }
        }

        private static void ReloadConfiguration() {
            InitOrResetConfiguration(ref _cachedProjectSettings);

            if (!File.Exists(ConfigFilePath)) {
                SaveConfiguration();
                return;
            }

            try {
                var json = File.ReadAllText(ConfigFilePath);
                JsonUtility.FromJsonOverwrite(json, _cachedProjectSettings);
            } catch (Exception) {
                SaveConfiguration();
            }
        }

        private static void SaveConfiguration() {
            try {
                _cachedProjectSettings ??= new ProjectSettings();
                var json = JsonUtility.ToJson(_cachedProjectSettings, true);
                File.WriteAllText(ConfigFilePath, json);
            } catch (Exception e) {
                Debug.LogError($"AutoReference Toolkit: Failed to save configuration: {e.Message}");
            }
        }

        private static Assembly[] _assemblies;
        private static Assembly[] Assemblies => _assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

        private static void ApplySettings() {
            var config = ProjectSettings;

#if TOOLBOX_IGNORE_CUSTOM_EDITOR
            IsEditorToolboxAvailable = false;
#else
            IsEditorToolboxAvailable = TypeExists(UnityEditorToolboxClassCheck);
#endif

            IsTriInspectorAvailable = TypeExists(TriInspectorClassCheck);

#if ODIN_INSPECTOR
            IsOdinInspectorAvailable = TypeExists(OdinInspectorClassCheck);
#else
            IsOdinInspectorAvailable = false;
#endif

            var enableEditorToolbox =
                IsEditorToolboxAvailable && config.enableEditorToolboxIntegration && config.syncOnInspect;

            var enableTriInspector =
                IsTriInspectorAvailable && config.enableTriInspectorIntegration && config.syncOnInspect;

            var enableOdinInspector =
                IsOdinInspectorAvailable && config.enableOdinInspectorIntegration && config.syncOnInspect;

            Preprocessor.Set(DisableOnInspectSymbol, !config.syncOnInspect);

            Preprocessor.Set(EnableUnityToolboxSymbol, enableEditorToolbox);
            Preprocessor.Set(EnableTriInspectorSymbol, enableTriInspector);
            Preprocessor.Set(EnableOdinInspectorSymbol, enableOdinInspector);

            Preprocessor.ApplyIfChanged();
        }

        [Conditional("UNITY_EDITOR")]
        public static void ApplyAndSaveProjectSettings() {
            SaveConfiguration();
            ApplySettings();
        }

        private static bool TypeExists(string qualifiedName) {
            return Assemblies.Any(assembly => assembly.GetType(qualifiedName, false, false) != null);
        }

        public static void ResetProjectSettingsToDefault() {
            InitOrResetConfiguration(ref _cachedProjectSettings);
            SaveConfiguration();
            ApplySettings();
        }

        public static void ResetUserSettingsToDefault() {
            UserSettings.ResetToDefaults();
        }
#else
        [Conditional("UNITY_EDITOR")]
        public static void ApplyAndSaveProjectSettings() { }

        [Conditional("UNITY_EDITOR")]
        public static void ResetProjectSettingsToDefault() { }

        [Conditional("UNITY_EDITOR")]
        public static void ResetUserSettingsToDefault() { }
#endif

    }
}
