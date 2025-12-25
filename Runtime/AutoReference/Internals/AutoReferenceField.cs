// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using Teo.AutoReference.System;

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Contains all auto-reference information of a field.
    /// </summary>
    internal readonly struct AutoReferenceField {
        public bool IsValid { get; }
        public ObjectField ObjectField { get; }
        public AutoReferenceAttribute MainAttribute { get; }

        public Type DeclaringType => ObjectField.DeclaringType;

        public AutoReferenceField(ObjectField field, AutoReferenceAttribute attribute) {
            IsValid = true;
            ObjectField = field;
            MainAttribute = attribute;
        }
    }
}
