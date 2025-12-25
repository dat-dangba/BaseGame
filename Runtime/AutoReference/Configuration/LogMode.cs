// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

namespace Teo.AutoReference.Configuration {
    public enum LogLevel {
        Off,
        Compact,
        Expanded,
        Default,
    }

    // Used only in the editor
    internal enum InternalLogLevel {
        Off = LogLevel.Off,
        Compact = LogLevel.Compact,
        Expanded = LogLevel.Expanded,
    }

    internal static class LogModeExtensions {
        public static LogLevel GetEffectiveLevel(this LogLevel logLevel) {
            return logLevel == LogLevel.Default ? SyncPreferences.DefaultLogLevel : logLevel;
        }
    }
}
