// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teo.AutoReference.System;
using Teo.AutoReference.Internals;
using Object = UnityEngine.Object;

namespace Teo.AutoReference {
    /// <summary>
    /// Allow applying auto-reference filters to a field that does not have a primary Auto Reference attribute.
    /// This attribute is implicitly added when necessary.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    internal sealed class ApplyFiltersAttribute : AutoReferenceAttribute {
        internal static readonly ApplyFiltersAttribute Instance = new ApplyFiltersAttribute();

        private SyncMode _syncModeCheck;

        protected internal override SyncMode SyncMode {
            get => SyncMode.ValidateOnly;
            set => _syncModeCheck = value;
        }

        // Simple hack for user improvement:
        // - Any warning should only appear if SyncOptionsAttribute is added
        // - ApplyFilters is implicitly added and hidden, so we show SyncOptions instead
        public override string Name => nameof(SyncOptionsAttribute).TrimEnd("Attribute");

        protected override IEnumerable<Object> GetObjects() {
            return Array.Empty<Object>();
        }

        protected override IEnumerable<Object> ValidateObjects(IEnumerable<Object> objects) {
            return objects;
        }

        protected override ValidationResult OnInitialize() {
            if (_syncModeCheck != SyncMode.Default && _syncModeCheck != SyncMode.ValidateOnly) {
                return ValidationResult.Warning(
                    $"{_syncModeCheck} will be ignored for field without a main Auto-Reference attribute"
                );
            }

            return ValidationResult.Ok;
        }
    }
}
