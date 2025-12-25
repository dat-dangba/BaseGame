// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference.Editor.Window {
#if UNITY_6000_2_OR_NEWER
    // Unity 6.2 introduced generic versions and deprecated non-generics for these types.
    using TreeView = TreeView<int>;
    using TreeViewState = TreeViewState<int>;
    using TreeViewItem = TreeViewItem<int>;
#endif

    internal partial class AutoReferenceWindowContent : TreeView {
        private const int StatusColumn = 0;
        private const int NamespaceColumn = 1;
        private const int TypeColumn = 2;
        private const int TargetColumn = 3;
        private const int AttributeColumn = 4;
        private const int DescriptionColumn = 5;

        private const string StatusLabel = "";
        private const string NamespaceLabel = "Scope";
        private const string TypeLabel = "Type";
        private const string TargetLabel = "Target";
        private const string AttributeLabel = "Attribute";
        private const string DescriptionLabel = "Description";

        private static readonly Texture ErrorIcon = GetIcon("console.erroricon.sml");
        private static readonly Texture WarningIcon = GetIcon("console.warnicon.sml");
        private static readonly Texture ErrorIconInactive = GetIcon("console.erroricon.inactive.sml");
        private static readonly Texture WarningIconInactive = GetIcon("console.warnicon.inactive.sml");
        private static readonly Texture Icon = GetIcon("FilterByType");

        private static readonly string[] HeaderLabel = {
            StatusLabel,
            NamespaceLabel,
            TypeLabel,
            TargetLabel,
            AttributeLabel,
            DescriptionLabel,
        };

        private static GUIStyle _headerStyle;

        private ColumnComparer _columnComparer;
        private TreeViewItem[] _defaultRows;

        private ReportInfo[] _reports;
        private StatisticsInfo _stats;

        private StateInfo StateInfo { get; }

        private static readonly MultiColumnHeaderState.Column[] Columns = {
            new MultiColumnHeaderState.Column {
                headerContent = GUIContent.none, width = 24, minWidth = 24, maxWidth = 24,
                allowToggleVisibility = false, autoResize = false,
                contextMenuText = "Status",
            },
            new MultiColumnHeaderState.Column {
                headerContent = GUIContent.none, minWidth = 0,
                allowToggleVisibility = true, autoResize = true,
                width = 1,
                contextMenuText = NamespaceLabel,
            },
            new MultiColumnHeaderState.Column {
                headerContent = GUIContent.none, minWidth = 0,
                allowToggleVisibility = true, autoResize = true,
                width = 1,
                contextMenuText = TypeLabel,
            },
            new MultiColumnHeaderState.Column {
                headerContent = GUIContent.none, minWidth = 0,
                allowToggleVisibility = true, autoResize = true,
                width = 1,
                contextMenuText = TargetLabel,
            },
            new MultiColumnHeaderState.Column {
                headerContent = GUIContent.none, minWidth = 0,
                allowToggleVisibility = true, autoResize = true,
                width = 1,
                contextMenuText = AttributeLabel,
            },
            new MultiColumnHeaderState.Column {
                headerContent = GUIContent.none, minWidth = 0,
                allowToggleVisibility = false, autoResize = true,
                width = 1,
                contextMenuText = DescriptionLabel,
            }
        };

        private AutoReferenceWindowContent(StateInfo info) : base(info.TreeViewState, new Header(info)) {
            showAlternatingRowBackgrounds = true;
            cellMargin = 2;
            showBorder = true;

            Reload();

            StateInfo = info;

            multiColumnHeader.sortingChanged += h => SortRows(h, GetRows() as List<TreeViewItem>);
        }

        public static GUIContent CreateTitleContent() {
            return new GUIContent("Auto-Reference", Icon);
        }

        private void SortRows(MultiColumnHeader header, List<TreeViewItem> rows) {
            if (header.sortedColumnIndex == -1) {
                rows.Clear();
                rows.AddRange(_defaultRows);
                return;
            }

            _columnComparer ??= new ColumnComparer(header);

            // List<T>.Sort doesn't use stable sort, so we use OrderBy to sort rows instead.
            var tempList = rows.OrderBy(row => row, _columnComparer).ToTempList();
            rows.Clear();
            rows.AddRange(tempList);
        }

        protected override TreeViewItem BuildRoot() {
            return new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
            if (_reports == null || _reports.Length == 0) {
                return new List<TreeViewItem>();
            }

            var id = 0;
            _defaultRows = _reports
                .SelectMany(report => report.items.Select(logItem => new LogItemRow(id++, report.script, logItem)))
                .Cast<TreeViewItem>()
                .ToArray();

            var list = new List<TreeViewItem>(_defaultRows);
            if (multiColumnHeader.sortedColumnIndex != -1) {
                SortRows(multiColumnHeader, list);
            }

            return list;
        }

        public void RefreshReports() {
            (_reports, _stats) = GetReports();
            InitializeGUIOnReport();
            Reload();
        }

        protected override void DoubleClickedItem(int id) {
            var rows = GetRows();
            if (rows[id] is LogItemRow row) {
                AssetDatabase.OpenAsset(row.script);
            }
        }

        public static AutoReferenceWindowContent Create(ref StateInfo state) {
            if (!state.IsValid || state.HeaderState.columns?.Length != Columns.Length) {
                state = new StateInfo(new TreeViewState(), new MultiColumnHeaderState(Columns), new StateMetadata());
            } else {
                // We shouldn't use the serialized state for these
                for (var i = 0; i < Columns.Length; ++i) {
                    state.HeaderState.columns[i].headerContent = GUIContent.none;
                    state.HeaderState.columns[i].allowToggleVisibility = Columns[i].allowToggleVisibility;
                    state.HeaderState.columns[i].maxWidth = Columns[i].maxWidth;
                    state.HeaderState.columns[i].minWidth = Columns[i].minWidth;
                }
            }

            // Note: minWidth will be adjusted later during the first time we display the rows.

            return new AutoReferenceWindowContent(state);
        }

        private static MonoScript[] GetMonoTypes() {
            var allScriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            using var results = TempList<MonoScript>.Get();

            results.AddRange(
                allScriptPaths.Select(scriptPath => $"Assets{scriptPath.Substring(Application.dataPath.Length)}")
                    .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                    .Where(monoScript => monoScript != null)
                    .Where(script => script.GetClass()?.IsSubclassOf(Types.Mono) is true)
            );

            return results.ToArray();
        }

        private static Texture GetIcon(string icon) {
            return EditorGUIUtility.IconContent(icon).image;
        }

        protected override void ContextClickedItem(int id) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open File"), false, () => DoubleClickedItem(id));
            menu.ShowAsContext();
        }

        private class ColumnComparer : IComparer<TreeViewItem> {
            private readonly MultiColumnHeader _header;

            public ColumnComparer(MultiColumnHeader header) {
                _header = header;
            }

            private int Column => _header.sortedColumnIndex;

            public int Compare(TreeViewItem x, TreeViewItem y) {
                var ascending = _header.IsSortedAscending(Column);

                if (!(x is LogItemRow lhs) || !(y is LogItemRow rhs)) {
                    return 0;
                }

                if (!ascending) {
                    // Swap operands for descending order.
                    (lhs, rhs) = (rhs, lhs);
                }

                return string.Compare(lhs.GetContent(Column), rhs.GetContent(Column), StringComparison.Ordinal);
            }
        }

        private class LogItemRow : TreeViewItem {
            private readonly LogItem _item;
            public readonly MonoScript script;

            public LogItemRow(int id, MonoScript script, LogItem item) : base(id, -1) {
                this.script = script;
                _item = item;
            }

            public string Description => _item.Message;

            public string GetContent(int column) {
                return column switch {
                    StatusColumn => _item.IsError ? "Error" : "Warning",
                    NamespaceColumn => _item.DeclaringType.Namespace,
                    TypeColumn => _item.DeclaringType.FormatCSharpName(),
                    TargetColumn => _item.MemberName,
                    AttributeColumn => _item.AttributeName,
                    DescriptionColumn => _item.Message,
                    _ => "",
                };
            }

            public void DrawContent(AutoReferenceWindowContent content, Rect rect, int column) {
                if (column == 0) {
                    GUI.Label(rect, _item.IsError ? ErrorIcon : WarningIcon, content._centeredLabelStyle);
                } else {
                    var strContent = GetContent(column);
                    if (string.IsNullOrWhiteSpace(strContent)) {
                        using (new EditorGUI.DisabledGroupScope(true)) {
                            GUI.Label(rect, "-", content._labelStyle);
                        }
                    } else {
                        GUI.Label(rect, new GUIContent(strContent, strContent), content._labelStyle);
                    }
                }
            }
        }

        private static GUIStyle HeaderStyle => _headerStyle ??= new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(4, 4, 1, 0),
            margin = new RectOffset(4, 4, 0, 0),
            fontSize = EditorStyles.label.fontSize - 1
        };


        private class Header : MultiColumnHeader {
            private StateInfo _state;

            public Header(StateInfo info) : base(info.HeaderState) {
                _state = info;
                height = EditorGUIUtility.singleLineHeight + 8; // Leave some space for the sort arrow
                RefreshAutoResizeState();
            }

            private void RefreshAutoResizeState() {
                var columns = _state.HeaderState.columns;

                if (_state.Metadata.autoResize) {
                    for (var i = 0; i < columns.Length; ++i) {
                        // "All" columns doesn't apply to the status column
                        columns[i].autoResize = i != StatusColumn;
                    }
                } else {
                    foreach (var column in columns) {
                        column.autoResize = false;
                    }
                    // "Description" column should always be auto-resized
                    columns[DescriptionColumn].autoResize = true;
                }
            }

            protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int index) {
                var arrowRect = headerRect;
                arrowRect.y -= 2;
                base.ColumnHeaderGUI(column, arrowRect, index);
                GUI.Label(headerRect, HeaderLabel[index], HeaderStyle);
            }


            protected override void AddColumnHeaderContextMenuItems(GenericMenu menu) {
                using var visibility = TempList<bool>.Get(Enumerable.Range(0, state.columns.Length).Select(_ => false));
                foreach (var column in state.visibleColumns) {
                    visibility[column] = true;
                }
                for (var i = 0; i < state.columns.Length; i++) {
                    var index = i;
                    if (!state.columns[index].allowToggleVisibility) {
                        continue;
                    }
                    menu.AddItem(
                        new GUIContent($"{HeaderLabel[index]}"),
                        visibility[index],
                        () => ToggleVisibility(index)
                    );
                }
                menu.AddSeparator("");
                menu.AddItem(
                    new GUIContent("Auto-resize columns"),
                    _state.Metadata.autoResize,
                    () => {
                        _state.Metadata.autoResize = !_state.Metadata.autoResize;
                        RefreshAutoResizeState();
                    }
                );

                menu.AddItem(
                    new GUIContent("Allow overflow"),
                    _state.Metadata.allowOverflow,
                    () => { _state.Metadata.allowOverflow = !_state.Metadata.allowOverflow; }
                );

                menu.AddItem(
                    new GUIContent("Draw borders"),
                    _state.Metadata.drawBorders,
                    () => { _state.Metadata.drawBorders = !_state.Metadata.drawBorders; }
                );
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Clear sorting"), false, () => { sortedColumnIndex = -1; });
            }
        }
    }
}
