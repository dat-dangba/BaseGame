using System.Collections.Generic;
using System.Linq;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using UnityEditor;
using UnityEngine;

namespace Teo.AutoReference.Editor.Window {
    internal partial class AutoReferenceWindowContent {
        private static GUIContent _errorIconContent = new GUIContent("");
        private static GUIContent _warningIconContent = new GUIContent("");

        private static GUIContent _descriptionContentCache = new GUIContent("");
        private static GUILayoutOption[] _toolbarOptions;
        private static GUILayoutOption[] _toolbarIconOptions;
        private static GUILayoutOption[] _statusBarOptions;
        private static GUILayoutOption[] _toolbarSeparatorOptions;
        private GUIStyle _centeredLabelStyle;

        private float _descriptionWidth;

        private GUIStyle _labelStyle;
        private GUIStyle _noErrorsBoxStyle;
        private GUIStyle _statusBarLabelStyle;
        private GUIStyle _toolbarButtonLeftStyle;
        private GUIStyle _toolbarButtonMidStyle;
        private GUIStyle _toolbarButtonRightStyle;
        private GUIStyle _toolbarButtonStyle;
        private GUIStyle _toolbarIconStyle;
        private GUIStyle _toolbarLabelStyle;
        private GUIStyle _toolbarSeparatorDisabledStyle;
        private GUIStyle _toolbarSeparatorStyle;
        private GUIStyle _iconButton;

        private GUIContent _settingsButtonContent;
        private GUIContent _helpButtonContent;

        private static float LineHeight => EditorGUIUtility.singleLineHeight;
        private static float ToolBarHeight => EditorGUIUtility.singleLineHeight * 1.6f;
        private static float StatusBarHeight => EditorGUIUtility.singleLineHeight * 1.25f;
        private static float HeaderHeight => EditorGUIUtility.singleLineHeight * 1.5f;

        private GUIContent SettingsButtonContent {
            get {
                return _settingsButtonContent ??= new GUIContent(EditorGUIUtility.IconContent("SettingsIcon")) {
                    tooltip = "Open Auto-Reference Project Settings"
                };
            }
        }

        private GUIContent HelpButtonContent {
            get {
#if UNITY_2021_2_OR_NEWER
                const string icon = "Help";
#else
                const string icon = "console.infoicon.sml";
#endif
                return _helpButtonContent ??= new GUIContent(EditorGUIUtility.IconContent(icon)) {
                    tooltip = "Open Auto-Reference Documentation"
                };
            }
        }

        private static GUIStyle MakeToolbarButtonStyle(GUIStyle original) {
            // Setting this as the top margin will center the button itself vertically
            var topMargin = (ToolBarHeight - original.fixedHeight) / 2;

            return new GUIStyle(original) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = EditorStyles.miniLabel.fontSize,
                margin = {
                    top = Mathf.RoundToInt(topMargin),
                },
                padding = new RectOffset(4, 4, 0, 0),
            };
        }

        private void InitializeStyles() {

#if UNITY_2021_2_OR_NEWER
            _iconButton = EditorStyles.iconButton;
#else
            _iconButton ??= new GUIStyle(EditorStyles.miniButton) {
                fixedWidth = 0,
                fixedHeight = 0,
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };
#endif

            _labelStyle ??= new GUIStyle(EditorStyles.label) {
                richText = true,
                fontSize = EditorStyles.label.fontSize - 1
            };

            _centeredLabelStyle ??= new GUIStyle(_labelStyle) {
                alignment = TextAnchor.MiddleCenter,
            };

            // We base this style on _labelStyle instead of GUI.skin.box directly, because
            // GUI.skin.box has a weird issue where the text appears black upon loading a project (??)
            _noErrorsBoxStyle ??= new GUIStyle(_labelStyle) {
                alignment = TextAnchor.MiddleCenter,
                // ...and for some weird internal Unity reason, this is what sets the background color
                name = GUI.skin.box.name,
            };

            _statusBarLabelStyle ??= new GUIStyle(_labelStyle) {
                margin = new RectOffset(),
                alignment = TextAnchor.MiddleLeft,
                richText = true,
            };

            _toolbarLabelStyle ??= new GUIStyle(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleCenter,
                margin = {
                    top = 0,
                    bottom = 0,
                },
                padding = {
                    left = 5,
                    top = 0,
                    bottom = 2,
                    right = 0,
                },
            };


            _toolbarOptions ??= new[] {
                GUILayout.Height(ToolBarHeight),
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            _statusBarOptions ??= new[] {
                GUILayout.Height(StatusBarHeight),
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            _toolbarIconOptions ??= new[] {
                GUILayout.Height(ToolBarHeight),
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(true),
            };

            _toolbarSeparatorStyle ??= new GUIStyle(GUI.skin.textArea) {
                margin = new RectOffset(),
            };

            _toolbarSeparatorOptions ??= new[] {
                GUILayout.Height(ToolBarHeight),
                GUILayout.Width(1),
            };

            _toolbarSeparatorDisabledStyle ??= new GUIStyle(GUI.skin.box) {
                margin = new RectOffset(),
                padding = new RectOffset(6, 5, 0, 0),
            };

            _toolbarButtonStyle ??= MakeToolbarButtonStyle(EditorStyles.miniButton);
            _toolbarButtonLeftStyle ??= MakeToolbarButtonStyle(EditorStyles.miniButtonLeft);
            _toolbarButtonMidStyle ??= MakeToolbarButtonStyle(EditorStyles.miniButtonMid);
            _toolbarButtonRightStyle ??= MakeToolbarButtonStyle(EditorStyles.miniButtonRight);

            _toolbarIconStyle ??= new GUIStyle(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleCenter,
                margin = { top = 0, bottom = 0 },
                padding = { right = 4 },
            };

            _descriptionContentCache ??= new GUIContent();

            // _descriptionWidth is reset to a negative value when the contents are refreshed.
            // If it's >= 0 it means it has already been calculated.
            if (_descriptionWidth >= 0) {
                return;
            }

            foreach (var row in GetRows().Cast<LogItemRow>()) {
                _descriptionContentCache.text = row.Description;
                var width = _labelStyle.CalcSize(_descriptionContentCache).x;
                _descriptionWidth = Mathf.Max(_descriptionWidth, width);
            }

            _descriptionWidth += cellMargin * 2;
        }

        public override void OnGUI(Rect rect) {
            InitializeStyles();

            var hasIssues = _stats.totalErrors + _stats.totalWarnings > 0;

            using (Layout.Vertical()) {
                using (Layout.Horizontal()) {
                    GUILayout.Label("Sync: ", _toolbarLabelStyle, _toolbarOptions);
                    if (GUILayout.Button("Open Scenes", _toolbarButtonLeftStyle)) {
                        SceneOperations.SyncAllOpenScenes();
                    }

                    if (GUILayout.Button("All Scenes", _toolbarButtonMidStyle)) {
                        SceneOperations.SyncAllProjectScenes();
                    }

                    if (GUILayout.Button("Build Scenes", _toolbarButtonRightStyle)) {
                        SceneOperations.SyncAllBuildScenes();
                    }

                    GUILayout.Space(4);

                    if (GUILayout.Button("Prefabs", _toolbarButtonStyle)) {
                        AssetOperations.SyncAllPrefabs();
                    }

                    GUILayout.FlexibleSpace();

                    var separatorStyle = hasIssues ? _toolbarSeparatorStyle : _toolbarSeparatorDisabledStyle;

                    GUILayout.Box("", separatorStyle, _toolbarSeparatorOptions);
                    GUILayout.Label(_warningIconContent, _toolbarIconStyle, _toolbarIconOptions);
                    GUILayout.Box("", separatorStyle, _toolbarSeparatorOptions);
                    GUILayout.Label(_errorIconContent, _toolbarIconStyle, _toolbarIconOptions);
                }

                // The tree view contents will be displayed in this space, but at the end of this method.
                GUILayout.FlexibleSpace();

                using (Layout.Horizontal(_statusBarOptions)) {
                    var totalTypes = Formatter.FormatCount(_stats.totalRelevantTypes, "type");
                    var totalFields = Formatter.FormatCount(_stats.totalFields, "field");
                    var totalCallbacks = Formatter.FormatCount(_stats.totalCallbacks, "callback");

                    GUILayout.Space(4);

                    var statusText =
                        "Auto-Reference Summary: " +
                        $"<b>{totalFields}</b> and <b>{totalCallbacks}</b> in <b>{totalTypes}</b>";
                    if (_stats.totalTypes > 0) {
                        statusText += $" (out of {_stats.totalTypes})";
                    }

                    GUILayout.Label(statusText, _statusBarLabelStyle, _statusBarOptions);

                    GUILayout.FlexibleSpace();
                    using (Layout.Vertical()) {
                        GUILayout.FlexibleSpace();
                        using (Layout.Horizontal()) {
                            if (GUILayout.Button(SettingsButtonContent, _iconButton)) {
                                SettingsService.OpenProjectSettings(SyncPreferences.ProjectSettingsPath);
                            }
                            if (GUILayout.Button(HelpButtonContent, _iconButton)) {
                                AutoReferenceDocumentation.Open();
                            }
                        }
                        GUILayout.FlexibleSpace();
                    }
                }
            }

            // The tree contents will be drawn now over the flexible space placed between the tool and status bars.

            rect.height -= ToolBarHeight;
            rect.y += ToolBarHeight;

            if (hasIssues) {
                useScrollView = true;
                // Let the height have a minimum value to improve drawing of scrollbars for tiny sizes
                var minHeight = HeaderHeight + LineHeight * 3;
                var drawRect = rect;
                drawRect.x -= 1;
                drawRect.width += 2;
                drawRect.height = Mathf.Max(minHeight, rect.height - StatusBarHeight);
                base.OnGUI(drawRect);
            } else {
                useScrollView = false;
                rect.height = Mathf.Max(rect.height - StatusBarHeight, 0);
                GUI.Label(rect, "No usage errors or warnings detected", _noErrorsBoxStyle);
            }
        }

        private void InitializeGUIOnReport() {
            var errorIcon = _stats.totalErrors == 0 ? ErrorIconInactive : ErrorIcon;
            var warningIcon = _stats.totalWarnings == 0 ? WarningIconInactive : WarningIcon;

            _errorIconContent = new GUIContent(_stats.totalErrors.ToString(), errorIcon);
            _warningIconContent = new GUIContent(_stats.totalWarnings.ToString(), warningIcon);

            _descriptionWidth = float.NegativeInfinity;
        }

        protected override void RowGUI(RowGUIArgs args) {
            if (!(args.item is LogItemRow item)) {
                return;
            }

            for (var i = 0; i < args.GetNumVisibleColumns(); i++) {
                var cellRect = args.GetCellRect(i);

                CenterRectUsingSingleLineHeight(ref cellRect);

                item.DrawContent(this, cellRect, args.GetColumn(i));
            }
        }

        private void DrawRowsOverflow(int[] visibleColumns) {
            base.BeforeRowsGUI();

            var width = visibleColumns
                .Where(index => index != DescriptionColumn)
                .Sum(index => multiColumnHeader.GetColumn(index).width);

            var description = multiColumnHeader.GetColumn(DescriptionColumn);
            var scrollWidth = GUI.skin.verticalScrollbar.fixedWidth;
            var resizedWidth = Mathf.Max(_descriptionWidth, treeViewRect.width - width - scrollWidth);

            description.width = resizedWidth;
        }

        private void DrawRowsWithoutOverflow(int[] visibleColumns) {
            var availableWidth = Mathf.Max(0f, treeViewRect.width - GUI.skin.verticalScrollbar.fixedWidth);
            if (availableWidth <= 1f) {
                // During layout/reload there can be bogus tiny widths - avoid fitting until we have a real viewport
                return;
            }

            using var widths = TempList<float>.Get();
            using var mins = TempList<float>.Get();
            using var maxs = TempList<float>.Get();

            foreach (var columnIndex in visibleColumns) {
                var column = multiColumnHeader.GetColumn(columnIndex);
                widths.Add(column.width);
                mins.Add(Mathf.Max(0f, column.minWidth));
                maxs.Add(column.maxWidth > 0 ? column.maxWidth : float.MaxValue);
            }

            var allColumnsTotalWidth = widths.Sum();

            if (allColumnsTotalWidth > availableWidth) {
                // If the viewport is smaller than the sum of minimum widths, do not force-collapse columns
                var minTotal = mins.Sum();
                if (availableWidth < minTotal) {
                    return;
                }

                // Shrink from the rightmost column backward until we fit, respecting each column's minimum
                var over = allColumnsTotalWidth - availableWidth;
                for (var i = widths.Count - 1; i >= 0 && over > 0f; --i) {
                    var shrinkAmount = Mathf.Min(over, widths[i] - mins[i]);
                    if (shrinkAmount > 0f) {
                        widths[i] -= shrinkAmount;
                        over -= shrinkAmount;
                    }
                }
            } else if (allColumnsTotalWidth < availableWidth) {
                // Grow the last column to fill the remaining space within its maximum
                var lastIndex = widths.Count - 1;
                var add = Mathf.Min(availableWidth - allColumnsTotalWidth, maxs[lastIndex] - widths[lastIndex]);
                if (add > 0f) {
                    widths[lastIndex] += add;
                }
            }

            for (var i = 0; i < widths.Count; ++i) {
                var column = multiColumnHeader.GetColumn(visibleColumns[i]);
                column.width = widths[i];
            }
        }

        private bool _isMinWidthInitialized;

        protected override void BeforeRowsGUI() {
            base.BeforeRowsGUI();

            var visibleColumns = multiColumnHeader.state.visibleColumns;
            if (visibleColumns == null || visibleColumns.Length == 0) {
                return;
            }

            if (!_isMinWidthInitialized) {
                // We can't do this when initializing the window because the label style is not initialized yet

                _isMinWidthInitialized = true;
                for (var i = 0; i < Columns.Length; ++i) {
                    var columns = StateInfo.HeaderState.columns;
                    if (i == StatusColumn) {
                        columns[i].minWidth = Columns[i].minWidth;
                        columns[i].width = Columns[i].width;
                        continue;
                    }

                    columns[i].minWidth = HeaderStyle.CalcSize(new GUIContent(HeaderLabel[i])).x;
                    columns[i].width = columns[i].width < columns[i].minWidth
                        ? Mathf.Max(80, columns[i].minWidth)
                        : Mathf.Max(columns[i].width, columns[i].minWidth);
                }
            }

            if (StateInfo.Metadata.allowOverflow) {
                DrawRowsOverflow(visibleColumns);
            } else {
                DrawRowsWithoutOverflow(visibleColumns);
            }
        }

        protected override void AfterRowsGUI() {
            if (!StateInfo.Metadata.drawBorders) {
                return;
            }
            var visibleColumns = multiColumnHeader.state.visibleColumns;
            var height = treeViewRect.height;

            // Note: Unlike similar methods, GetColumnRect does not use the real column index
            for (var i = 0; i < visibleColumns.Length; ++i) {
                var columnRect = multiColumnHeader.GetColumnRect(i);
                var x = Mathf.Round(columnRect.xMax) - 1f;
                EditorGUI.DrawRect(new Rect(x, 0, 1f, height), new Color(0f, 0f, 0f, 0.15f));
            }
        }
    }
}
