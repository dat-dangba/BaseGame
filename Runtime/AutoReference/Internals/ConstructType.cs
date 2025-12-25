// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

namespace Teo.AutoReference.Internals {
    /// <summary>
    /// Enum representation of different data types involving <see cref="UnityEngine.Object"/> fields that may be
    /// auto-referenced.
    /// </summary>
    internal enum ConstructType {
        /// <summary>
        /// The Underlying type is neither an <see cref="UnityEngine.Object"/> an <see cref="UnityEngine.Object"/>[],
        /// or a <see cref="List"/>&lt;<see cref="UnityEngine.Object"/>&gt;
        /// </summary>
        Invalid,

        /// <summary>
        /// The underlying type is a reference to a <see cref="UnityEngine.Object"/>
        /// </summary>
        Plain,

        /// <summary>
        /// The underlying type is an array of <see cref="UnityEngine.Object"/> references.
        /// </summary>
        Array,

        /// <summary>
        /// The underlying type is a list of <see cref="UnityEngine.Object"/> references.
        /// </summary>
        List,
    }
}
