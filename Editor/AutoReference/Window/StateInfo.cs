// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Teo.AutoReference.Editor.Window {
#if UNITY_6000_2_OR_NEWER
    // Unity 6.2 introduced a generic version and deprecated the non-generic type of this type.
    using TreeViewState = TreeViewState<int>;
#endif

    [Serializable]
    internal struct StateInfo {
        public const int CurrentVersion = 1;

        [SerializeField] private TreeViewState _treeViewState;
        [SerializeField] private MultiColumnHeaderState _headerState;
        [SerializeField] private StateMetadata _metadata;
        [SerializeField] private int _version;

        public StateMetadata Metadata => _metadata;

        public StateInfo(TreeViewState treeViewState, MultiColumnHeaderState headerState, StateMetadata metadata) {
            _treeViewState = treeViewState;
            _headerState = headerState;
            _metadata = metadata;
            _version = CurrentVersion;
        }

        public TreeViewState TreeViewState => _treeViewState;
        public MultiColumnHeaderState HeaderState => _headerState;

        public bool IsValid =>
            TreeViewState != null
            && HeaderState != null && HeaderState.columns?.Length > 0
            && Metadata != null
            && _version == CurrentVersion;
    }

    [Serializable]
    internal class StateMetadata {
        public bool autoResize;
        public bool allowOverflow;
        public bool drawBorders = true;
    }
}
