// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;

namespace Teo.AutoReference {
    /// <summary>
    /// Enum that is used to determine when an auto-referenced field should be synced.
    /// </summary>
    [Flags]
    public enum ContextMode {
        /// Only sync when the script is placed in a game object in a scene.
        Scene = 1 << 0,

        /// Only sync when the script is placed in a game object in a prefab asset the user is currently editing.
        Prefab = 1 << 1,

        /// Sync in both Scene and Prefab mode.
        Default = Scene | Prefab,
    }
}
