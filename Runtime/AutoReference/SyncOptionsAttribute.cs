// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using Teo.AutoReference.Internals;

namespace Teo.AutoReference {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SyncOptionsAttribute : Attribute {

        internal static readonly SyncOptionsAttribute Default = new SyncOptionsAttribute();

        public SyncOptionsAttribute() { }

        public SyncOptionsAttribute(SyncMode syncMode, ContextMode context) {
            Context = context;
            SyncMode = syncMode;
        }

        public SyncOptionsAttribute(ContextMode context) {
            Context = context;
        }

        public SyncOptionsAttribute(SyncMode syncMode) {
            SyncMode = syncMode;
        }

        public ContextMode Context { get; set; } = ContextMode.Default;
        public SyncMode SyncMode { get; set; } = SyncMode.Default;

        internal string Name => GetType().Name.TrimEnd("Attribute");
    }
}
