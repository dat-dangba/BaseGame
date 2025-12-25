using System;

namespace Teo.AutoReference.Internals.Collections {
    /// <summary>
    /// Slice extensions for arrays that work in older versions that don't support range operations.
    /// </summary>
    public static class ArrayExtensions {
        public static T[] Slice<T>(this T[] data, int index, int length) {
            if (length == 0) {
                return Array.Empty<T>();
            }

            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] Slice<T>(this T[] data, int index) {
            var length = data.Length - index;
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
