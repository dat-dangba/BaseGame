using System;
using System.Collections.Generic;
using System.Text;
using Teo.AutoReference.Configuration;
using UnityEngine;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

namespace Teo.AutoReference.Internals {
    internal class FormatBuilder : IDisposable {
        private static readonly Stack<FormatBuilder> Pool = new Stack<FormatBuilder>();
        private readonly StringBuilder _sb;
        private bool _bold, _italic;
        private Color32? _color;
        private FormatInfo _default;
        private bool _isDisposed;

        private bool _isEnabled;
        private FormatInfo _message;
        private FormatInfo _name;
        private FormatInfo _symbol;

        private static readonly string[] NewlineSplit = { "\n" };

        private FormatBuilder() {
            _sb = new StringBuilder();
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            _isDisposed = true;
            Clear();
            Pool.Push(this);
        }

        public void AppendText(string text) {
            Append(_default, text);
        }

        public void AppendMessage(string text) {
            Append(_message, text);
        }

        public void AppendExceptionName(string text) {
            Append(_name, text);
        }

        public void AppendSymbol(string text) {
            Append(_symbol, text);
        }

        /// <summary>
        /// Attempt to close the tag if the current state is on and the next state is off.
        /// Return whether the tag should open (previous state was off and next state is on.
        /// </summary>
        private bool TryCloseTag(ref bool currentState, bool newState, string closeTag) {
            if (currentState == newState) {
                return false;
            }

            if (currentState) {
                _sb.Append(closeTag);
            }

            currentState = newState;
            return newState;
        }

        private bool TryCloseColorTag(ref Color32? current, Color32? newValue) {
            if (current.HasValue == newValue.HasValue) {
                if (!current.HasValue) {
                    // Both colors are disabled
                    return false;
                }

                var ca = current.Value;
                var cb = newValue.Value;
                if (ca.r == cb.r && ca.g == cb.g && ca.b == cb.b) {
                    // Colors are the same
                    return false;
                }
            }

            if (current.HasValue) {
                _sb.Append("</color>");
            }

            current = newValue;

            return newValue.HasValue;
        }

        public void Append(in FormatInfo format, string text) {
            if (!_isEnabled) {
                _sb.Append(text);
                return;
            }

            if (string.IsNullOrEmpty(text)) {
                return;
            }

            if (text.Contains("\n")) {
                // Treat each line as an individual message.
                // i.e. We close the tags at the end of each line and reopen them when necessary.
                // This is necessary because older versions of Unity don't support tags spanning multiple lines.
                var lines = text.Split(NewlineSplit, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < lines.Length; ++i) {
                    Append(format, lines[i]);
                    if (i < lines.Length - 1) {
                        AppendLine();
                    }
                }

                if (text.EndsWith("\n")) {
                    AppendLine();
                }
                return;
            }

            var newBold = format.Bold;
            var newItalic = format.Italic;
            var newColor = format.ColorEnabled ? format.Color : (Color32?)null;

            // Close previous tags IF necessary
            var openColor = TryCloseColorTag(ref _color, newColor);
            var openItalic = TryCloseTag(ref _italic, newItalic, "</i>");
            var openBold = TryCloseTag(ref _bold, newBold, "</b>");

            // Open new tags IF necessary
            if (openBold) {
                _sb.Append("<b>");
            }
            if (openItalic) {
                _sb.Append("<i>");
            }
            if (openColor) {
                _sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(_color!.Value)}>");
            }

            // Note: The close/open ordering must be reversed

            _sb.Append(text);
        }

        public static FormatBuilder Make() {
            if (!Pool.TryPop(out var formatter)) {
                formatter = new FormatBuilder();
                formatter.Clear();
            }

            formatter._isDisposed = false;
            formatter.RefreshSettings();
            return formatter;
        }

        public void Clear() {
            _sb.Clear();
            _bold = false;
            _italic = false;
            _color = null;
        }

        private void CloseAllTags() {
            // Note: The order must be the same as in Append
            TryCloseColorTag(ref _color, null);
            TryCloseTag(ref _italic, false, "</i>");
            TryCloseTag(ref _bold, false, "</b>");
        }

        public void AppendLine() {
            CloseAllTags();
            _sb.Append('\n');
        }

        private void RefreshSettings() {
            _isEnabled = SyncPreferences.EnableExceptionFormatting;
            _default = SyncPreferences.DefaultFormatInfo;
            _name = SyncPreferences.ExceptionFormatInfo;
            _message = SyncPreferences.MessageFormatInfo;
            _symbol = SyncPreferences.SymbolFormatInfo;
        }

        public string Build() {
            CloseAllTags();
            return _sb.ToString();
        }
    }
}
