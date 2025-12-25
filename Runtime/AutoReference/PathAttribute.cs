using System;
using System.Diagnostics;
using System.Linq;
using Teo.AutoReference.Internals.Collections;
using Teo.AutoReference.System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Teo.AutoReference
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class PathAttribute : AutoReferenceValidatorAttribute
    {
        private readonly StringComparison _comparison;
        private readonly string[] _paths;

        public PathAttribute(string path, params string[] paths) :
            this(StringComparison.Ordinal, path, paths)
        {
        }

        public PathAttribute(StringComparison comparison, string path, params string[] paths)
        {
            _paths = paths.PrependArray(path);
            _comparison = comparison;
        }

        protected override int PriorityOrder => FilterOrder.Filter;

        protected override bool Validate(in FieldContext context, Object value)
        {
// #if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(value);
            return _paths.Any(p => path.Equals(p, _comparison));
// #else
//             return true;
// #endif
        }
    }
}