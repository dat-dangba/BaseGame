// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using UnityEngine;

namespace Teo.AutoReference.Configuration {
    internal struct FormatInfo : IEquatable<FormatInfo> {
        private Color32 _color;
        private bool _bold;
        private bool _italic;
        private bool _colorEnabled;

        public bool Bold {
            readonly get => _bold;
            set {
                _bold = value;
                UpdateSerializedValue();
            }
        }

        public bool Italic {
            readonly get => _italic;
            set {
                _italic = value;
                UpdateSerializedValue();
            }
        }

        public Color32 Color {
            readonly get => _color;
            set {
                _color = value;
                UpdateSerializedValue();
            }
        }

        public bool ColorEnabled {
            readonly get => _colorEnabled;
            set {
                _colorEnabled = value;
                UpdateSerializedValue();
            }
        }

        public string SerializedValue { get; private set; }

        private void UpdateSerializedValue() {
            var colorEnabled = _colorEnabled ? "1" : "0";
            var bold = _bold ? "1" : "0";
            var italic = _italic ? "1" : "0";
            var color = ColorUtility.ToHtmlStringRGB(_color);
            SerializedValue = $"{colorEnabled};{color};{bold}{italic}";
        }

        public FormatInfo(Color32 color, bool bold = false, bool italic = false, bool colorEnabled = true) {
            SerializedValue = null;
            _color = color;
            _bold = bold;
            _italic = italic;
            _colorEnabled = colorEnabled;
            UpdateSerializedValue();
        }

        public FormatInfo(bool bold = false, bool italic = false) {
            SerializedValue = null;
            _color = new Color32(255, 255, 255, 255);
            _bold = bold;
            _italic = italic;
            _colorEnabled = false;
            UpdateSerializedValue();
        }

        public static bool TryParse(string value, out FormatInfo formatInfo) {
            formatInfo = new FormatInfo();

            if (string.IsNullOrEmpty(value)) {
                return false;
            }

            var parts = value.Split(';');
            if (parts.Length != 3) {
                return false;
            }

            formatInfo._colorEnabled = parts[0] == "1";

            if (!ColorUtility.TryParseHtmlString($"#{parts[1]}", out var color)) {
                return false;
            }
            formatInfo._color = color;

            if (parts[2].Length != 2) {
                return false;
            }

            formatInfo._bold = parts[2][0] == '1';
            formatInfo._italic = parts[2][1] == '1';

            formatInfo.UpdateSerializedValue();

            return true;
        }


        public override bool Equals(object obj) {
            return obj is FormatInfo other && SerializedValue == other.SerializedValue;
        }

        public bool Equals(FormatInfo other) {
            return SerializedValue == other.SerializedValue;
        }

        public override int GetHashCode() {
            return SerializedValue != null ? SerializedValue.GetHashCode() : 0;
        }

        public static bool operator ==(FormatInfo left, FormatInfo right) {
            return left.Equals(right);
        }

        public static bool operator !=(FormatInfo left, FormatInfo right) {
            return !(left == right);
        }
    }
}
