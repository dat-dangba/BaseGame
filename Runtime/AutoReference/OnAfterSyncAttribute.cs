// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace Teo.AutoReference {
    /// <summary>
    /// An attribute that marks methods as callbacks to run after all auto-references are synced.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class OnAfterSyncAttribute : Attribute {
        /// <summary>
        /// Constructs a new instance of the OnAfterSyncAttribute class.
        /// </summary>
        /// <param name="methodName">The method to invoke. Used only when this attribute is attached to a class, and it's
        /// ignored when it's attached to a method.</param>
        public OnAfterSyncAttribute(string methodName = null) {
            MethodName = methodName;
        }

        /// <summary>
        /// Gets the method name that is supplied to the attribute.
        /// This property is applicable only when the attribute is applied to a class.
        /// </summary>
        public string MethodName { get; }
    }
}
