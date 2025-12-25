// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using Teo.AutoReference.Internals;

namespace Teo.AutoReference.System {
    public abstract class AutoReferenceBaseAttribute : Attribute {
        public virtual string Name => GetType().Name.TrimEnd("Attribute");
    }
}
