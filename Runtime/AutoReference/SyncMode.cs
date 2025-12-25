// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;

namespace Teo.AutoReference {
    public enum SyncMode {
        ///<summary>
        /// The default <see cref="SyncMode"/> which changes depending on the type of the field.
        /// It's equivalent to <see cref="IfEmpty"/> for single-value fields and
        /// <see cref="Always"/> for array or list fields.
        /// </summary>
        Default,

        /// <summary>
        /// Validate values to make sure they fit all constraints but do not automatically retrieve anything.
        /// </summary>
        ValidateOnly,

        /// <summary>
        /// Only retrieve references if the value is empty (e.g., null single-value fields or empty lists/arrays).
        /// Always validate all values.
        /// </summary>
        IfEmpty,

        [Obsolete("Use SyncMode.IfEmpty")]
        ValidateOrGetIfEmpty = IfEmpty,

        /// <summary>
        /// Only retrieve references if the value is empty (e.g., null single-value fields or empty lists/arrays),
        /// but only validate values while retrieving.
        /// </summary>
        IfEmptyPermissive,

        [Obsolete("Use SyncMode.IfEmptyPermissive")]
        GetIfEmpty = IfEmptyPermissive,

        /// <summary>
        /// Always retrieve and validate references. Any changes the user makes will be overriden the next time
        /// the auto-reference will sync.
        /// </summary>
        Always,
        [Obsolete("Use SyncMode.Always")]
        AlwaysGetAndValidate = Always,

        /// <summary>
        /// Always retrieve and validate references, but allow additional values that that are also validated.
        /// </summary>
        /// <remarks>
        /// This does not support adding duplicates.
        /// </remarks>
        Expanded,

        /// <summary>
        /// Always retrieve and validate references but allow any additional values without validation.
        /// </summary>
        /// <remarks>
        /// This does not support adding duplicate or null values.
        /// </remarks>
        ExpandedPermissive
    }
}
