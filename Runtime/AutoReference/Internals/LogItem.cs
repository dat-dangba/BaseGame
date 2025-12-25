// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teo.AutoReference.Configuration;
using Teo.AutoReference.System;
using UnityEngine;

namespace Teo.AutoReference.Internals {
    internal struct LogItem {
        public const string DefaultFormat = "'%n' on '%s':\n%m.";

        public string Message { get; }

        public string AttributeName { get; }

        public string MemberName { get; }

        private SyncStatus Status => IsError ? SyncStatus.UsageError : SyncStatus.UsageWarning;

        public string Format { get; set; }

        private string Label => IsError ? ErrorLabel : WarningLabel;

        public bool IsError { get; }

        public string ErrorLabel { get; set; }

        public string WarningLabel { get; set; }

        public Type DeclaringType { get; }

        public LogItem(
            ValidationResult validation,
            MemberInfo member,
            string attributeName,
            string format = DefaultFormat
        ) : this(validation, member.DeclaringType, member.Name, attributeName, format) { }

        public LogItem(
            ValidationResult validation,
            FieldContext field,
            string attributeName,
            string format = DefaultFormat
        ) : this(validation, field.Field.FieldInfo, attributeName, format) { }

        public LogItem(
            ValidationResult validation,
            Type declaringType,
            string memberName,
            string attributeName,
            string format = DefaultFormat
        ) {
            IsError = validation.IsError;
            Message = validation.Message;
            DeclaringType = declaringType;

            AttributeName = attributeName;
            MemberName = memberName;
            Format = format;
            ErrorLabel = "Error";
            WarningLabel = "Warning";
        }

        private string Symbol =>
            string.IsNullOrWhiteSpace(MemberName)
                ? $"{DeclaringType.FormatCSharpName()}"
                : $"{DeclaringType.FormatCSharpName()}.{MemberName}";

        public void LogToConsole(string format = null) {
            if (string.IsNullOrWhiteSpace(format)) {
                format = Format;
            }

            var log = format
                .Replace("%e", Label)
                .Replace("%t", DeclaringType.FormatCSharpName())
                .Replace("%i", MemberName)
                .Replace("%s", Symbol)
                .Replace("%n", AttributeName)
                .Replace("%m", Message.TrimEnd('.'));

            if (IsError) {
                Debug.LogError(log);
            } else {
                Debug.LogWarning(log);
            }
        }

        /// <summary>
        /// Logs messages when appropriate and returns their accumulated result.
        /// </summary>
        public static SyncStatus ProcessMessages(IList<LogItem> messages) {
            if (messages.Count == 0) {
                return SyncStatus.Skip;
            }

            if (LogContext.CurrentLevel < LogLevel.Expanded) {
                return messages.Aggregate(SyncStatus.None, (current, message) => current | message.Status);
            }

            var status = SyncStatus.None;
            foreach (var message in messages) {
                message.LogToConsole();
                status |= message.Status;
            }

            return status;
        }
    }
}
