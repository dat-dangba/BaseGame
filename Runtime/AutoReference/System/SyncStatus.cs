// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;

namespace Teo.AutoReference.System {
    [Flags]
    public enum SyncStatus {
        None = 0,
        Skip = 0,
        Complete = 1 << 0,
        RuntimeError = 1 << 1,
        UsageWarning = 1 << 2,
        UsageError = 1 << 3,
        Unsupported = 1 << 4,
    }

    public static class SyncStatusExtensions {
        public static bool HasAny(this SyncStatus status, SyncStatus other) {
            return (status & other) != 0;
        }
    }
}
