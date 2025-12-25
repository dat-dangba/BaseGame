// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

#if !UNITY_2021_2_OR_NEWER
using Teo.AutoReference.Internals.Compatibility;
#endif

using Object = UnityEngine.Object;

// This class is not used in builds, but it's referenced in code that should be stripped out anyway.
// So we define two versions: one for the editor and a dummy version for the compilation to succeed in builds.

#if !UNITY_EDITOR
namespace Teo.AutoReference.Internals {
    internal class ObjectWatcher : IDisposable {
        public ObjectWatcher Init(Object target, AutoReferenceTypeInfo info) => this;

        public void Dispose() { }

        public bool IsObjectModified() => false;
    }
}
#else

namespace Teo.AutoReference.Internals {
    internal class ObjectWatcher : IDisposable {
        private readonly List<string> _fields = new List<string>();
        private readonly Func<string, bool> _isPropertyModifiedFunc;
        private readonly Dictionary<string, SerializedProperty> _newProperties =
            new Dictionary<string, SerializedProperty>();
        private readonly Dictionary<string, SerializedProperty> _oldProperties =
            new Dictionary<string, SerializedProperty>();

        private bool _isInitialized;
        private SerializedObject _newObject;
        private SerializedObject _oldObject;

        private Object _target;

        public ObjectWatcher() {
            _isPropertyModifiedFunc = IsPropertyModified;
        }

        public void Dispose() {
            _target = null;

            _fields.Clear();

            _oldObject = null;
            _newObject = null;

            _oldProperties.Clear();
            _newProperties.Clear();

            _isInitialized = false;
        }

        public ObjectWatcher Init(Object target, in AutoReferenceTypeInfo info) {
            if (_isInitialized) {
                throw new InvalidOperationException($"{nameof(ObjectWatcher)} has already been initialized");
            }

            _isInitialized = true;

            _target = target;

            _fields.AddRange(info.autoReferenceFields.Select(f => f.ObjectField.FieldInfo.Name));
            _fields.AddRange(info.syncedFields.Select(f => f.Name));

            if (_fields.Count == 0) {
                return this;
            }

            _oldObject = new SerializedObject(target);

            BuildPropertyMap(_oldObject, _oldProperties);

            return this;
        }

        private static void BuildPropertyMap(SerializedObject obj, Dictionary<string, SerializedProperty> map) {
            map.Clear();

            // No need to check the first property
            var it = obj.GetIterator();
            it.Next(true);

            do {
                map[it.name] = it.Copy();
            } while (it.Next(false));
        }

        public bool IsObjectModified() {
            if (!_isInitialized) {
                throw new InvalidOperationException($"{nameof(ObjectWatcher)} has not been initialized");
            }

            if (_fields.Count == 0) {
                return false;
            }

            _newObject = new SerializedObject(_target);

            BuildPropertyMap(_newObject, _newProperties);

            return _fields.Any(_isPropertyModifiedFunc);
        }

        private bool IsPropertyModified(string fieldName) {
            var before = _oldProperties.GetValueOrDefault(fieldName);
            var after = _newProperties.GetValueOrDefault(fieldName);

            if (before == null || after == null) {
                return before != after;
            }

            return !SerializedProperty.DataEquals(before, after);
        }
    }
}

#endif
