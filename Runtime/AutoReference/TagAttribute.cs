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
    /// Get or exclude components with specific tags.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TagAttribute : AutoReferenceValidatorAttribute {

        private readonly string[] _invalidTags;
        private readonly string[] _tags;

        public TagAttribute(string tag, params string[] tags) {
            using var valid = TempList<string>.Get();
            using var invalid = TempList<string>.Get();

            foreach (var t in tags.Prepend(tag)) {
#if UNITY_EDITOR
                if (UnityEditorInternal.InternalEditorUtility.tags.Contains(t)) {
                    valid.Add(t);
                } else {
                    invalid.Add(t);
                }
#endif
            }

            _tags = valid.ToArrayOrEmpty();
            _invalidTags = invalid.ToArrayOrEmpty();
        }

        protected override int PriorityOrder => FilterOrder.Filter;

        public bool Exclude { get; set; }

        protected override Type TypeConstraint => Types.Component;

        protected override ValidationResult OnInitialize(in FieldContext context) {
            if (_invalidTags.Length <= 0) {
                return ValidationResult.Ok;
            }

            var list = string.Join(", ", _invalidTags.Distinct().Select(tag => $"'{tag}'"));
            var message = $"Invalid {Formatter.FormatPlural(_invalidTags.Length, "tag")}: {list}";
            return ValidationResult.Warning(message);
        }

        protected override bool Validate(in FieldContext context, Object value) {
            var component = (Component)value;
            return Exclude != _tags.Any(t => component.gameObject.CompareTag(t));
        }
    }
}
