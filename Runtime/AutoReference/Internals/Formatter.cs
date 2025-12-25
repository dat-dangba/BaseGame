// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEngine;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals {
    internal static class Formatter {
        private static readonly string ProjectPath =
            Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;

        /// <summary>
        /// Resolves and returns the human-readable C# name for the specified <see cref="Type"/>.
        /// </summary>
        public static string FormatCSharpName(this Type type, bool includeNamespace = false) {
            return TypeResolver.ResolveCSharpName(type, includeNamespace);
        }

        /// <summary>
        /// Removes all occurrences of a specified suffix from the end of a string.
        /// </summary>
        public static string TrimEnd(this string @string, string suffix) {
            while (@string.EndsWith(suffix)) {
                @string =  @string.Substring(0, @string.Length - suffix.Length);
            }

            return @string;
        }

        private static string MakeCodeLink(string filePath, int line) {
            // Unity 6 changed how console hyperlinks work. We need special handling, or the link will
            // only open the file without taking the user to the appropriate line.

            var relativePath = filePath.Replace(ProjectPath, "").Replace('\\', '/');
#if UNITY_6000_0_OR_NEWER
            // The outer <a href></a> tag is not necessary for the link itself,
            // but the link will not look clickable without this.
            return $"(at <a href><link=\"href='{relativePath}' line='{line}'\">{relativePath}:{line}</link></a>)";
#else
            return $"(at <a href=\"{filePath}\" line=\"{line}\">{relativePath}:{line}</a>)";
#endif
        }

        public static string FormatFieldException(Exception exception, FieldInfo field, string prefix) {
            using var fmt = FormatBuilder.Make();
            fmt.AppendText(prefix);
            fmt.AppendSymbol($"{field.DeclaringType}.{field.Name}");
            fmt.AppendText(": ");
            fmt.AppendLine();
            FormatStackTrace(fmt, exception);
            return fmt.Build();
        }

        public static string FormaMethodException(Exception exception, MethodInfo method, string prefix) {
            var methodName = CallbackMethodInfo.FormatMethod(method);

            using var fmt = FormatBuilder.Make();
            fmt.AppendText(prefix);
            fmt.AppendSymbol(methodName);
            fmt.AppendText(" in ");
            fmt.AppendSymbol(method.DeclaringType.FormatCSharpName(true));
            fmt.AppendText(" failed:\n");
            FormatStackTrace(fmt, exception);
            return fmt.Build();
        }

        private static void FormatStackTrace(FormatBuilder fmt, Exception exception) {
            using var frames = TempStack<(StackFrame, Exception)>.Get();
            while (exception != null) {
                frames.Push((new StackTrace(exception, true).GetFrame(0), exception));
                exception = exception.InnerException;
            }

            while (frames.TryPop(out var entry)) {
                var (frame, e) = entry;
                var lineNumber = frame?.GetFileLineNumber() ?? 0;
                var filePath = frame?.GetFileName();
                var link = filePath == null ? "" : $" {MakeCodeLink(filePath, lineNumber)}";

                fmt.AppendText("  ");
                fmt.AppendExceptionName(e.GetType().Name);
                fmt.AppendText(": ");
                fmt.AppendMessage($"'{e.Message}'");
                fmt.AppendText($" {link}");
                fmt.AppendLine();
            }
        }

        public static string FormatCount(int count, string word) {
            return count == 1 ? $"1 {word}" : $"{count} {word}s";
        }

        public static string FormatPlural(int count, string word) {
            return count == 1 ? word : $"{word}s";
        }
    }
}
