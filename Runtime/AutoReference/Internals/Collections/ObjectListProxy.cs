// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// An <c>IReadOnlyList&lt;Object&gt;</c> wrapper over a value that may be of type <c>T</c>, <c>T[]</c>
    /// or <c>List&lt;T&gt;</c> where <c>T</c> is assignable to <see cref="Object"/>.
    /// </summary>
    internal readonly struct ObjectListProxy : IReadOnlyList<Object> {
        private readonly IList _list;
        private readonly Object _value;

        public ObjectListProxy(object value) {
            _value = null;
            _list = null;
            switch (value) {
                default:
                case Object obj when obj == null: // explicit == null required because Unity overloads null equality.
                    break;
                case Object obj:
                    _value = obj;
                    break;
                case IList list:
                    _list = list;
                    break;
            }
        }

        IEnumerator<Object> IEnumerable<Object>.GetEnumerator() {
            return !(_list is IEnumerable<Object> enumerable) ? new Enumerator(this) : enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return !(_list is IEnumerable<Object> enumerable) ? new Enumerator(this) : enumerable.GetEnumerator();
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        public int Count {
            get {
                if (_list != null) {
                    return _list.Count;
                }

                return _value != null ? 1 : 0;
            }
        }

        public Object this[int index] {
            get {
                if (_list != null) {
                    return (Object)_list[index];
                }

                if (_value == null || index != 0) {
                    throw new IndexOutOfRangeException(index.ToString());
                }

                return _value;
            }
        }

        public struct Enumerator : IEnumerator<Object> {
            private int _index;
            private readonly ObjectListProxy _proxy;
            private readonly int _count;

            public Enumerator(in ObjectListProxy proxy) {
                _proxy = proxy;
                _count = _proxy.Count;
                _index = -1;
            }

            public Object Current => _proxy[_index];

            object IEnumerator.Current => Current;

            public bool MoveNext() {
                _index++;
                return _index < _count;
            }

            public void Reset() => _index = -1;

            public void Dispose() { }
        }
    }
}
