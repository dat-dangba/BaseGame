// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Editor.Window;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {
    internal static class SyncPreferencesEditor {
        private const string AssemblyReloadNote = "Changing setting will reload assembly";
        private const int BoxPadding = 10;
        private const int BottomPadding = 5;

        private const string SyncOnInspectorTooltip =
            "Automatically sync references when the inspector opens, closes, or the user makes modifications to the " +
            "script. Recommended in order to immediately verify that the references are correct.";

        private const string SyncOnSceneSaveTooltip =
            "Sync references in a scene before it is saved.";

        private const string SyncOnAssemblyReloadTooltip =
            "Sync references in open scenes when the assembly is reloaded.";

        private const string SyncScenesOnBuildTooltip =
            "Sync references in all scenes included in the build settings before building the project.";

        private const string SkipSyncOnFilterErrorTooltip =
            "Skip syncing a field entirely when a user error occurs during the initialization one of its filters " +
            "instead of ignoring only that filter.";

        private const string FailBuildOnErrorTooltip =
            "Cancel the build process if an error occurs during the syncing of included scenes.";

        private const string FailBuildOnWarningsTooltip =
            "Cancel the build process if a warning occurs during the syncing of included scenes.";

        private const string LogModeTooltip =
            "Specifies how errors and warnings are displayed in the Unity Console.";

        private const string EnableEditorToolboxNote =
            "Unity Editor Toolbox detected in the project with Toolbox Drawers enabled.\n" +
            "This setting enables inspector integration with ToolboxEditor.";

        private const string EnableOdinInspectorNote =
            "Odin inspector detected in the project.\n" +
            "This setting enables inspector integration with Odin.";

        private const string EnableTriInspectorNote =
            "Tri-Inspector detected in the project.\n" +
            "This setting enables inspector integration with Tri-Inspector.";

        private const string EditorSelectLogTooltip =
            "Log level for when built-in inspectors are activated if inspector integration is enabled.";

        private const string BatchLogTooltip =
            "Log level for batch operations";

        private const string DefaultLogTooltip =
            "Default log level for normal sync operations.";

        private const string CacheSyncInfoTooltip =
            "Cache sync information for faster syncing. Note: This increases memory usage.";

        private const string EnableFormatTooltip =
            "Enable exception formatting in the Unity Console. \n" +
            "This affects any exceptions logged during syncing, but it does not affect usage errors or warnings.";

        private const string BoldTooltip = "Bold";
        private const string ItalicTooltip = "Italic";

        private static GUIStyle _toggleNoteStyle;
        private static GUIStyle _bottomAreaStyle;
        private static GUIStyle _buttonStyle;

        // Styles used to create the help box
        private static GUIStyle _helpBoxStyle;
        private static GUIStyle _helpLabelStyle;
        private static GUIStyle _helpButtonStyle;
        private static GUIStyle _helpIconStyle;
        private static GUIContent _helpIconContent;

        // Styles for exception formatting
        private static GUIStyle _buttonStyleBold;
        private static GUIStyle _buttonStyleItalic;
        private static GUIContent _boldContent;
        private static GUIContent _italicContent;

        private static GUIStyle BottomAreaStyle => _bottomAreaStyle ??= new GUIStyle(GUIStyle.none) {
            margin = new RectOffset(BottomPadding, BottomPadding, 0, BottomPadding),
            padding = new RectOffset(0, 0, 0, 0)
        };

        private static float LineHeight => EditorGUIUtility.singleLineHeight;

        private static GUIStyle ToggleNoteStyle {
            get {
                if (_toggleNoteStyle != null) {
                    return _toggleNoteStyle;
                }

                var style = EditorStyles.label;

                _toggleNoteStyle = new GUIStyle(style) {
                    fontStyle = FontStyle.Italic,
                    padding = new RectOffset(BoxPadding, 0, 0, 0),
                    fontSize = style.fontSize - 2,
                };
                return _toggleNoteStyle;
            }
        }

        private static GUIStyle ButtonStyle =>
            _buttonStyle ??= new GUIStyle(GUI.skin.button) {
                padding = new RectOffset(BottomPadding, BottomPadding, BottomPadding, BottomPadding),
                margin = new RectOffset(BottomPadding, BottomPadding, 0, BottomPadding),
            };

        [SettingsProvider]
        public static SettingsProvider CreateProjectProvider() {
            return new SettingsProvider(SyncPreferences.ProjectSettingsPath, SettingsScope.Project) {
                label = "Auto-Reference",
                guiHandler = DrawProjectSettings,
            };
        }

        [SettingsProvider]
        public static SettingsProvider CreateEditorProvider() {
            return new SettingsProvider(SyncPreferences.UserSettingsPath, SettingsScope.User) {
                label = "Auto-Reference",
                guiHandler = DrawUserSettings
            };
        }

        private static bool Toggle(ref bool value, string label, string tooltip = "", string note = "") {
            using (Layout.Horizontal()) {
                var content = new GUIContent(label, tooltip: tooltip);
                EditorGUILayout.LabelField(content);
                var valueBefore = value;
                value = EditorGUILayout.ToggleLeft(note, value, ToggleNoteStyle);
                GUILayout.FlexibleSpace();
                return valueBefore != value;
            }
        }

        private static bool Toggle(
            SubScope scope,
            ref bool value,
            string label,
            string tooltip = "",
            string note = ""
        ) {
            var changed = Toggle(ref value, $"    {label}", tooltip, note);
            scope.DrawNotchAtLastRect();
            scope.DrawLine = true;
            return changed;
        }

        private static bool EnumPopup<T>(ref T value, string label, string tooltip = "") where T : Enum {
            var valueBefore = value;
            var content = new GUIContent(label, tooltip);
            value = (T)EditorGUILayout.EnumPopup(content, value);
            return !Equals(valueBefore, value);
        }

        private static bool EnumPopup<T>(
            SubScope scope,
            ref T value,
            string label,
            string tooltip = ""
        ) where T : Enum {
            var result = EnumPopup(ref value, $"    {label}", tooltip);
            scope.DrawNotchAtLastRect();
            scope.DrawLine = true;
            return result;
        }

        private static void DrawSyncingSection(ref ProjectSettings config) {
            Toggle(ref config.syncOnInspect, "In the inspector", SyncOnInspectorTooltip, AssemblyReloadNote);
            using (var scope = new SubScope(!config.syncOnInspect)) {
                if (SyncPreferences.IsEditorToolboxAvailable) {
                    Toggle(scope, ref config.enableEditorToolboxIntegration,
                        "Enable Editor Toolbox Integration",
                        EnableEditorToolboxNote, AssemblyReloadNote
                    );
                }

                if (SyncPreferences.IsOdinInspectorAvailable) {
                    Toggle(scope, ref config.enableOdinInspectorIntegration,
                        "Enable Odin Inspector Integration",
                        EnableOdinInspectorNote, AssemblyReloadNote
                    );
                }

                if (SyncPreferences.IsTriInspectorAvailable) {
                    Toggle(scope, ref config.enableTriInspectorIntegration,
                        "Enable Tri-Inspector Integration",
                        EnableTriInspectorNote, AssemblyReloadNote
                    );
                }
            }

            Toggle(ref config.syncOnSceneSave, "When saving the scene", SyncOnSceneSaveTooltip);
            Toggle(ref config.syncOnAssemblyReload, "On Assembly Reload", SyncOnAssemblyReloadTooltip);
            Toggle(ref config.syncScenesOnBuild, "On Build", SyncScenesOnBuildTooltip);
            using (var scope = new SubScope(!config.syncScenesOnBuild)) {
                Toggle(scope, ref config.failBuildOnError, "Fail Build On Errors", FailBuildOnErrorTooltip);
                Toggle(scope, ref config.failBuildOnWarnings, "Fail Build On Warnings", FailBuildOnWarningsTooltip);
            }
        }

        private static void DrawUserSettings(string searchContext) {
            using (new Box("Appearance")) {
                var side = SyncPreferences.SyncToggleSide;
                EnumPopup(ref side, "Toggle side");
                SyncPreferences.SyncToggleSide = side;

                var style = SyncPreferences.SyncToggleStyle;
                EnumPopup(ref style, "Toggle style");
                SyncPreferences.SyncToggleStyle = style;
            }

            using (new Box("Logging")) {
                var level = SyncPreferences.DefaultLogLevel;
                EnumPopup(ref level, "Default", DefaultLogTooltip);
                SyncPreferences.DefaultLogLevel = level;

                level = SyncPreferences.BatchLogLevel;
                EnumPopup(ref level, "Batch operations", BatchLogTooltip);
                SyncPreferences.BatchLogLevel = level;

                level = SyncPreferences.EditorSelectLogLevel;
                EnumPopup(ref level, "On editor select", EditorSelectLogTooltip);
                SyncPreferences.EditorSelectLogLevel = level;

                EditorGUILayout.Space();

                const string message =
                    "These options only control the logging of Auto-Reference usage errors or warnings. It does not " +
                    "affect any runtime logging that may happen by Unity while syncing.";
                EditorGUILayout.HelpBox(message, MessageType.Info, true);
            }

            using (new Box("Other")) {
                var cache = SyncPreferences.CacheSyncInfo;
                Toggle(ref cache, "Cache sync info", CacheSyncInfoTooltip);
                SyncPreferences.CacheSyncInfo = cache;

                var prettyPrint = SyncPreferences.EnableExceptionFormatting;
                Toggle(ref prettyPrint, "Enable exception formatting", EnableFormatTooltip);
                SyncPreferences.EnableExceptionFormatting = prettyPrint;

                if (prettyPrint) {
                    using var scope = new SubScope(false);

                    SyncPreferences.DefaultFormatInfo =
                        FormatInfo(scope, SyncPreferences.DefaultFormatInfo, "Text");

                    SyncPreferences.ExceptionFormatInfo =
                        FormatInfo(scope, SyncPreferences.ExceptionFormatInfo, "Exception Name");

                    SyncPreferences.MessageFormatInfo =
                        FormatInfo(scope, SyncPreferences.MessageFormatInfo, "Exception Message");

                    SyncPreferences.SymbolFormatInfo =
                        FormatInfo(scope, SyncPreferences.SymbolFormatInfo, "Symbol");

                }
            }

            GUILayout.FlexibleSpace();

            if (GoToOtherSettings("Project-specific settings can be found in ", "Project Settings")) {
                SettingsService.OpenProjectSettings(SyncPreferences.ProjectSettingsPath);
            }

            using (Layout.Horizontal(BottomAreaStyle)) {
                if (GUILayout.Button("Restore default settings", ButtonStyle)) {
                    if (ShowDialog("user")) {
                        SyncPreferences.ResetUserSettingsToDefault();
                    }
                }

                if (GUILayout.Button("View documentation", ButtonStyle)) {
                    AutoReferenceDocumentation.Open();
                }
            }
        }

        private static FormatInfo FormatInfo(SubScope scope, FormatInfo info, string label, string tooltip = null) {
            _buttonStyleBold ??= new GUIStyle(EditorStyles.miniButtonLeft) {
                fixedHeight = LineHeight,
                fixedWidth = LineHeight,
                fontStyle = FontStyle.Bold,
                stretchHeight = false,
                stretchWidth = false,
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
            };

            _buttonStyleItalic ??= new GUIStyle(EditorStyles.miniButtonRight) {
                fixedHeight = LineHeight,
                fixedWidth = LineHeight,
                fontStyle = FontStyle.Italic,
                stretchHeight = false,
                stretchWidth = false,
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
            };

            using (Layout.Horizontal()) {
                var content = new GUIContent($"    {label}", tooltip: tooltip);
                EditorGUILayout.LabelField(content);
                var indentBefore = EditorGUI.indentLevel;
                try {
                    EditorGUI.indentLevel = 0;

                    info.Bold = GUILayout.Toggle(info.Bold, new GUIContent("b", BoldTooltip), _buttonStyleBold);
                    info.Italic = GUILayout.Toggle(info.Italic, new GUIContent("i", ItalicTooltip), _buttonStyleItalic);

                    var enabled = info.ColorEnabled;
                    var rect = EditorGUILayout.GetControlRect(false, LineHeight);

                    var toggleRect = rect;
                    toggleRect.width = LineHeight;

                    rect.xMin += LineHeight;
                    enabled = EditorGUI.Toggle(toggleRect, GUIContent.none, enabled);

                    if (enabled) {
                        info.Color = EditorGUI.ColorField(rect, GUIContent.none, info.Color, true, false, false);
                    } else {
                        GUI.Box(rect, GUIContent.none);
                    }

                    info.ColorEnabled = enabled;

                } finally {
                    EditorGUI.indentLevel = indentBefore;
                }

                GUILayout.FlexibleSpace();
            }

            scope.DrawNotchAtLastRect();
            scope.DrawLine = true;
            return info;
        }

        private static void DrawProjectSettings(string searchContext) {
            var config = SyncPreferences.ProjectSettings;
            if (config == null) {
                return;
            }

            EditorGUI.BeginChangeCheck();
            using (new Box("Enable syncing")) {
                DrawSyncingSection(ref config);
            }

            using (new Box("Other")) {
                DrawOtherSection(ref config);
            }

            if (EditorGUI.EndChangeCheck()) {
                SyncPreferences.ApplyAndSaveProjectSettings();
                AutoReferenceEventHook.Refresh();
            }

            GUILayout.FlexibleSpace();

            if (GoToOtherSettings("More settings can be found in", "Preferences")) {
                SettingsService.OpenUserPreferences(SyncPreferences.UserSettingsPath);
            }

            using (Layout.Horizontal(BottomAreaStyle)) {
                if (GUILayout.Button("Restore default settings", ButtonStyle)) {
                    var result = ShowDialog("project");

                    if (result) {
                        SyncPreferences.ResetProjectSettingsToDefault();
                        AutoReferenceEventHook.Refresh();
                    }
                }

                if (GUILayout.Button("View documentation", ButtonStyle)) {
                    AutoReferenceDocumentation.Open();
                }

                if (GUILayout.Button("Open Window", ButtonStyle)) {
                    AutoReferenceWindow.Open();
                }
            }
        }

        private static bool GoToOtherSettings(string label, string button) {
            const int internalPadding = 4;

            _helpBoxStyle ??= new GUIStyle(EditorStyles.helpBox) {
                margin = new RectOffset(BoxPadding, BoxPadding, BoxPadding, BoxPadding),
                padding = new RectOffset(BoxPadding / 2, BoxPadding / 2, BoxPadding / 2, BoxPadding / 2),
            };

            _helpIconContent ??= new GUIContent(EditorGUIUtility.IconContent("console.infoicon").image);
            var size = new Vector2(_helpIconContent.image.width, _helpIconContent.image.height);
            size = EditorGUIUtility.PixelsToPoints(size);

            _helpIconStyle ??= new GUIStyle(EditorStyles.label) {
                stretchWidth = false,
                stretchHeight = false,
                fixedWidth = size.x,
                fixedHeight = size.y,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                imagePosition = ImagePosition.ImageOnly,
                contentOffset = new Vector2(0, 0),
                alignment = TextAnchor.MiddleCenter
            };

            _helpLabelStyle ??= new GUIStyle(EditorStyles.miniLabel) {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(internalPadding, internalPadding, 0, 0),
                fixedHeight = size.y,
                stretchWidth = false,
            };

            var buttonHeight = Mathf.Max(14, EditorStyles.miniButton.fixedHeight) + 4;
            var margin = Mathf.RoundToInt((size.y - buttonHeight) / 2f + _helpBoxStyle.padding.top);

            _helpButtonStyle ??= new GUIStyle(EditorStyles.miniButton) {
                fontSize = _helpLabelStyle.fontSize,
                margin = new RectOffset(0, 0, margin, 0),
                border = new RectOffset(0, 0, 0, 0),
                stretchWidth = false,
                fixedHeight = buttonHeight
            };

            using (Layout.Horizontal(_helpBoxStyle)) {
                GUILayout.Label(_helpIconContent, _helpIconStyle);
                GUILayout.Label(label, _helpLabelStyle);
                return GUILayout.Button(button, _helpButtonStyle);
            }
        }

        private static bool ShowDialog(string context) {
            return EditorUtility.DisplayDialog(
                "Restore default settings",
                $"Are you sure you want to restore all Auto-Reference {context} settings to their default values?",
                "Yes",
                "No"
            );
        }

        private static void DrawOtherSection(ref ProjectSettings config) {
            Toggle(ref config.skipSyncOnFilterError, "Skip sync on filter error", SkipSyncOnFilterErrorTooltip);
        }

        private class SubScope : IDisposable {
            private const int LineWidth = 1;

            private const int BottomOffset = 1;
            private readonly bool _enabledBefore;
            private bool _isDisposed;

            public SubScope(bool shouldDisable) {
                _enabledBefore = GUI.enabled;
                GUI.enabled = !shouldDisable;

                GUILayout.BeginVertical();
            }

            private static Color DisabledLineColor {
                get {
                    var color = EditorStyles.label.normal.textColor;
                    color.a = 0.16666f;
                    return color;
                }
            }

            private static Color EnabledLineColor {
                get {
                    var color = EditorStyles.label.normal.textColor;
                    color.a = 0.33333f;
                    return color;
                }
            }

            private static float LeftLinePadding => EditorStyles.label.CalcSize(new GUIContent(" ")).x;
            public bool DrawLine { get; set; }

            public void Dispose() {
                if (_isDisposed) {
                    return;
                }
                _isDisposed = true;

                GUILayout.EndVertical();

                if (DrawLine) {
                    var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());

                    rect.xMin += LeftLinePadding;
                    rect.width = LineWidth;
                    rect.height -= LineHeight / 2 - BottomOffset;

                    var color = GUI.enabled ? EnabledLineColor : DisabledLineColor;
                    EditorGUI.DrawRect(rect, color);
                }

                GUI.enabled = _enabledBefore;
            }

            public void DrawNotchAtLastRect() {
                var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
                rect.xMin += LeftLinePadding + LineWidth;
                rect.width = 6;
                rect.y = rect.yMin + (rect.yMax - rect.yMin) / 2 - LineWidth + BottomOffset;
                rect.height = LineWidth;
                var color = GUI.enabled ? EnabledLineColor : DisabledLineColor;
                EditorGUI.DrawRect(rect, color);
            }
        }

        private class Box : IDisposable {

            private static GUIStyle _boxStyle;
            private bool _isDisposed;

            public Box(string label) {
                _boxStyle ??= new GUIStyle(EditorStyles.helpBox) {
                    padding = new RectOffset(BoxPadding, BoxPadding, BoxPadding, BoxPadding),
                    margin = new RectOffset(BoxPadding, BoxPadding, BoxPadding, BoxPadding),
                };

                GUILayout.BeginVertical(_boxStyle);
                GUILayout.Label(label, EditorStyles.boldLabel);
                GUILayout.Space(4);
                ++EditorGUI.indentLevel;
            }

            public void Dispose() {
                if (_isDisposed) {
                    return;
                }
                _isDisposed = true;
                --EditorGUI.indentLevel;
                GUILayout.EndVertical();
            }
        }
    }
}
