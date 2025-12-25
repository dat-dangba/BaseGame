// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Teo.AutoReference.Configuration;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor {

    using Style = SyncToggleStyle;

    public static class SyncToggle {

        private const string ToolTipOn = "This script has auto-reference functionality and editing will trigger " +
                                         "syncing.\n\nUncheck this button to temporarily disable " +
                                         "syncing while editing.";

        private const string ToolTipOff = "This script has auto-reference functionality. Automatic syncing while "
                                          + "editing is currently disabled.";

        private static GUIStyle _syncButtonRoundStyle;
        private static GUIStyle _syncButtonSquareStyle;
        private static GUIStyle _syncButtonCheckStyle;
        private static GUIStyle _syncButtonBoxStyle;
        private static GUIStyle _syncButtonNoStyle;
        private static GUIStyle _syncButtonNormalStyle;

        private static GUIStyle _syncButtonTextStyle;

        private static readonly GUIContent OnContent = new GUIContent("", ToolTipOn);
        private static readonly GUIContent OffContent = new GUIContent("", ToolTipOff);

        private static readonly GUIContent OnContentText = new GUIContent("A", ToolTipOn);
        private static readonly GUIContent OffContentText = new GUIContent("A", ToolTipOff);

        private static Vector2 Size {
            get {
                var background = EditorStyles.radioButton.normal.background;
                return new Vector2(background.width, background.height);
            }
        }

        private static GUIStyle SyncButtonTextStyle => _syncButtonTextStyle ??= new GUIStyle(EditorStyles.label) {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 8,
            fontStyle = FontStyle.Bold,
            contentOffset = new Vector2(0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        private static GUIStyle SyncButtonRoundStyle {
            get {
                var size = Size;
                return _syncButtonRoundStyle ??= new GUIStyle(EditorStyles.radioButton) {
                    fixedWidth = size.x,
                    fixedHeight = size.y
                };
            }
        }

        private static GUIStyle SyncButtonSquareStyle {
            get {
                var size = Size;
                return _syncButtonSquareStyle ??= new GUIStyle(EditorStyles.textField) {
                    fixedWidth = size.x,
                    fixedHeight = size.y
                };
            }
        }

        private static GUIStyle SyncButtonCheckboxStyle {
            get {
                var size = Size;
                return _syncButtonCheckStyle ??= new GUIStyle(EditorStyles.toggle) {
                    fixedWidth = size.x,
                    fixedHeight = size.y
                };
            }
        }

        private static GUIStyle SyncButtonBoxStyle {
            get {
                var size = Size;
                return _syncButtonBoxStyle ??= new GUIStyle(GUI.skin.box) {
                    fixedWidth = size.x,
                    fixedHeight = size.y
                };
            }
        }

        private static GUIStyle SyncButtonNormalStyle {
            get {
                var size = Size;
                return _syncButtonNormalStyle ??= new GUIStyle(GUI.skin.button) {
                    fixedWidth = size.x,
                    fixedHeight = size.y,

                    padding = new RectOffset(0, 0, 0, 0),
                    alignment = SyncButtonTextStyle.alignment,
                    fontSize = SyncButtonTextStyle.fontSize,
                    fontStyle = SyncButtonTextStyle.fontStyle,
                    contentOffset = SyncButtonTextStyle.contentOffset,
                };
            }
        }

        public static bool DrawAtY(float y, bool value) {
            var x = SyncPreferences.SyncToggleSide switch {
                SyncToggleSide.Left => 1,
                _ => EditorGUIUtility.labelWidth + 1
            };

            var height1 = Size.y;
            var height2 = EditorGUIUtility.singleLineHeight;

            var min = Mathf.Min(height1, height2);
            var max = Mathf.Max(height1, height2);
            var offset = (max - min) / 2;

            return Draw(new Vector2(x, y + offset), value);
        }

        public static bool Draw(Vector2 position, bool value) {
            var rect = new Rect(position, Size);

            var style = SyncPreferences.SyncToggleStyle;

            StyleInfo info = style switch {
                Style.TextOnly => (toggle: false, text: true, EditorStyles.label),
                Style.Circle => (toggle: false, text: true, SyncButtonRoundStyle),
                Style.Square => (toggle: false, text: true, SyncButtonSquareStyle),
                Style.Button => (toggle: true, text: false, SyncButtonNormalStyle, OnContentText, OffContentText),
                Style.RadioButton => (toggle: true, text: false, SyncButtonRoundStyle),
                Style.Checkbox => (toggle: true, text: false, SyncButtonCheckboxStyle),
                Style.DarkBox => (toggle: false, text: true, SyncButtonBoxStyle),
                _ => (toggle: false, text: true, SyncButtonBoxStyle)
            };

            var content = value ? info.on : info.off;

            if (info.toggle) {
                value = GUI.Toggle(rect, value, content, info.guiStyle);
            } else {
                if (GUI.Button(rect, content, info.guiStyle)) {
                    value = !value;
                }
            }

            if (info.text && value) {
                GUI.Label(rect, "A", SyncButtonTextStyle);
            }

            return value;
        }

        public static bool DrawOverHeader(bool value, float yOffset = 0) {
            var editorPadding = EditorStyles.inspectorDefaultMargins.padding;
            var x = SyncPreferences.SyncToggleSide switch {
                SyncToggleSide.Left => 0,
                _ => EditorGUIUtility.labelWidth + 2
            };
            var y = editorPadding.top + EditorGUIUtility.standardVerticalSpacing / 2 + yOffset;
            return Draw(new Vector2(x, y), value);
        }

        private struct StyleInfo {
            public bool toggle;
            public bool text;
            public GUIStyle guiStyle;

            public GUIContent on;
            public GUIContent off;

            public static implicit operator StyleInfo((bool toggle, bool text, GUIStyle guiStyle) tuple) {
                return new StyleInfo {
                    toggle = tuple.toggle,
                    text = tuple.text,
                    guiStyle = tuple.guiStyle,
                    on = OnContent,
                    off = OffContent
                };
            }

            public static implicit operator StyleInfo(
                (bool toggle, bool text, GUIStyle guiStyle, GUIContent on, GUIContent off) tuple
            ) {
                return new StyleInfo {
                    toggle = tuple.toggle,
                    text = tuple.text,
                    guiStyle = tuple.guiStyle,
                    on = tuple.on,
                    off = tuple.off
                };
            }
        }
    }
}
