// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEngine;
using Object = UnityEngine.Object;
using Types = Teo.AutoReference.Internals.Types;

namespace Teo.AutoReference {
    /// <summary>
    /// Get or exclude components with specific layers.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LayerAttribute : AutoReferenceValidatorAttribute {
        private readonly int[] _invalidLayerIds;
        private readonly string[] _invalidLayerNames;
        private readonly int[] _layers;

        private bool _include;

        public LayerAttribute(string layer, params string[] layers) {
            using var invalid = TempList<string>.Get();
            using var valid = TempList<int>.Get();

            foreach (var label in layers.Prepend(layer)) {
                var id = LayerMask.NameToLayer(label);

                if (id == -1) {
                    invalid.Add(label);
                } else {
                    valid.Add(id);
                }
            }

            _layers = valid.ToArrayOrEmpty();
            _invalidLayerNames = invalid.ToArrayOrEmpty();
            _invalidLayerIds = Array.Empty<int>();
        }

        public LayerAttribute(int layer, params int[] layers) {
            using var invalid = TempList<int>.Get();
            using var valid = TempList<int>.Get();

            foreach (var id in layers.Prepend(layer)) {
                var name = LayerMask.LayerToName(id);

                if (string.IsNullOrEmpty(name)) {
                    invalid.Add(id);
                } else {
                    valid.Add(id);
                }
            }

            _layers = valid.ToArrayOrEmpty();
            _invalidLayerIds = invalid.ToArrayOrEmpty();
            _invalidLayerNames = Array.Empty<string>();
        }

        public bool Exclude { get; set; }

        protected override int PriorityOrder => FilterOrder.Filter;

        protected override Type TypeConstraint => Types.Component;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            if (_invalidLayerIds.Length > 0) {
                var list = string.Join(", ", _invalidLayerIds.Distinct().Select(i => i.ToString()));
                var message = $"Invalid layer {Formatter.FormatPlural(_invalidLayerIds.Length, "ID")}: {list}";
                return ValidationResult.Warning(message);
            }

            if (_invalidLayerNames.Length > 0) {
                var list = string.Join(", ", _invalidLayerNames.Distinct().Select(l => $"'{l}'"));
                var message = $"Invalid {Formatter.FormatPlural(_invalidLayerNames.Length, "layer")}: {list}";
                return ValidationResult.Warning(message);
            }

            return ValidationResult.Ok;
        }

        protected override bool Validate(in FieldContext context, Object value) {
            return Exclude != _layers.Contains(((Component)value).gameObject.layer);
        }
    }
}
