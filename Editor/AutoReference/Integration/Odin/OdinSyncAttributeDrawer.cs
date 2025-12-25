// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using Sirenix.OdinInspector.Editor;
using UnityEngine;
using System;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.System;

namespace Teo.AutoReference.Editor.Integration.Odin {
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    // ReSharper disable once UnusedType.Global
    internal class OdinSyncAttributeDrawer<T> : OdinAttributeDrawer<OdinSyncAttribute, T>, IDisposable {
        private bool _syncEnabled = true;

        protected override void Initialize() {
            _syncEnabled = true;

            ValueEntry.OnChildValueChanged += _ => {
                if (_syncEnabled) {
                    AutoReference.Sync(Attribute.Target);
                }
            };

            using (LogContext.MakeContextInternal(SyncPreferences.EditorSelectLogLevel)) {
                AutoReference.Sync(Attribute.Target);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label) {
            var valueBefore = _syncEnabled;
            _syncEnabled = SyncToggle.DrawOverHeader(_syncEnabled);
            if (_syncEnabled && !valueBefore) {
                AutoReference.Sync(Attribute.Target);
            }
            CallNextDrawer(label);
        }

        public void Dispose() {
            AutoReference.Sync(Attribute.Target);
        }
    }
}
