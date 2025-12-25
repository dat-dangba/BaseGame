// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using UnityEngine;

namespace Teo.AutoReference.Editor.Integration.Odin {
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    internal class OdinSyncAttribute : Attribute {
        public OdinSyncAttribute(MonoBehaviour target) {
            Target = target;
        }

        public MonoBehaviour Target { get; }
    }
}
