// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Teo.AutoReference.Editor.Integration.Odin {
    // ReSharper disable once UnusedType.Global
    internal class OdinSyncProcessor : OdinAttributeProcessor<MonoBehaviour> {
        private static MonoBehaviour GetMonoBehaviour(InspectorProperty property) {
            foreach (var value in property.Tree.RootProperty.ValueEntry.WeakValues) {
                if (value is MonoBehaviour monoBehaviour) {
                    return monoBehaviour;
                }
            }

            return null;
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes) {
            var mono = GetMonoBehaviour(property);
            if (mono == null || !AutoReference.HasSyncInformation(mono)) {
                return;
            }
            attributes.Add(new OdinSyncAttribute(mono));
        }
    }
}
