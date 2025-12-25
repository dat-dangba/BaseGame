using Teo.AutoReference.Configuration;
using Teo.AutoReference.Editor.Integration.TriInspector;
using Teo.AutoReference.System;
using TriInspector;
using UnityEngine;

[assembly: RegisterTriValueDrawer(typeof(TryInspectorSyncDrawer), TriDrawerOrder.Decorator)]

namespace Teo.AutoReference.Editor.Integration.TriInspector {
    public class TryInspectorSyncDrawer : TriValueDrawer<MonoBehaviour> {
        private MonoBehaviour _target;

        public override TriExtensionInitializationResult Initialize(TriPropertyDefinition definition) {
            if (definition.OwnerType != null || definition.GetValue(null, 0) is not MonoBehaviour mono) {
                return TriExtensionInitializationResult.Skip;
            }

            _target = mono;
            return AutoReference.HasSyncInformation(mono)
                ? TriExtensionInitializationResult.Ok
                : TriExtensionInitializationResult.Skip;
        }

        public override TriElement CreateElement(TriValue<MonoBehaviour> propertyValue, TriElement next) {
            var element = new SyncDrawerElement(propertyValue.Property, _target);
            element.AddChild(next);
            return element;
        }

        private class SyncDrawerElement : TriElement {
            private readonly TriProperty _rootProperty;
            private readonly MonoBehaviour _target;

            private bool _syncEnabled = true;

            public SyncDrawerElement(TriProperty property, MonoBehaviour target) {
                _rootProperty = property;
                _target = target;
            }

            protected override void OnAttachToPanel() {
                base.OnAttachToPanel();
                _rootProperty.ChildValueChanged += OnValueChanged;

                using (LogContext.MakeContextInternal(SyncPreferences.EditorSelectLogLevel)) {
                    AutoReference.Sync(_target);
                }
            }

            protected override void OnDetachFromPanel() {
                _rootProperty.ChildValueChanged -= OnValueChanged;
                base.OnDetachFromPanel();

                AutoReference.Sync(_target);
            }

            private void OnValueChanged(TriProperty changedProperty) {
                _rootProperty.PropertyTree.ApplyChanges();
                if (_syncEnabled) {
                    AutoReference.Sync(_target);
                }
                _rootProperty.PropertyTree.Update();
            }

            public override void OnGUI(Rect position) {
                base.OnGUI(position);

                var valueBefore = _syncEnabled;

                _syncEnabled = SyncToggle.DrawOverHeader(_syncEnabled, -2);

                if (_syncEnabled && !valueBefore) {
                    AutoReference.Sync(_target);
                }
            }
        }
    }
}
