// Copyright © 2023-2025 Charis Marangos (Zoodinger). Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Teo.AutoReference.Internals.Collections {
    internal static class Extensions {

        /// <summary>
        /// Converts an <c>IEnumerable&lt;T&gt;</c> to <c>T[]</c>.
        /// If the source is already an array, it returns the same instance instead of creating a new array.
        /// </summary>
        internal static T[] ToArraySmart<T>(this IEnumerable<T> source) {
            return source as T[] ?? source.ToArray();
        }

        /// <summary>
        /// Converts an <c>IEnumerable&lt;T&gt;</c> to <c>IReadOnlyList&lt;T&gt;</c>.
        /// If the source is already assignable to a readonly list, it returns the same instance instead of creating
        /// a new one. Note that this includes instances of non-readonly lists or arrays because they can still be
        /// assigned to a generic readonly list.
        /// </summary>
        internal static IReadOnlyList<T> ToReadOnlyListSmart<T>(this IEnumerable<T> source) {
            return source as IReadOnlyList<T> ?? source.ToArray();
        }

        /// <summary>
        /// Converts an <c>IEnumerable&lt;T&gt;</c> to <c>List&lt;T&gt;</c>.
        /// If the source is already a list, it returns the same instance instead of creating a new one.
        /// </summary>
        internal static List<T> ToListSmart<T>(this IEnumerable<T> source) {
            return source as List<T> ?? source.ToList();
        }

        /// <summary>
        /// Convert a <see cref="List{T}"/> to an array without creating a new array instance if the size is 0.
        /// Instead, if the list is empty it will return <see cref="Array.Empty{T}"/> which is a singleton.
        /// </summary>
        internal static T[] ToArrayOrEmpty<T>(this List<T> source) {
            return source.Count == 0 ? Array.Empty<T>() : source.ToArray();
        }

        internal static TempList<T> ToTempList<T>(this IEnumerable<T> enumerable) {
            return TempList<T>.Get(enumerable);
        }

        internal static TempSet<T> ToTempSet<T>(this IEnumerable<T> enumerable) {
            return TempSet<T>.Get(enumerable);
        }

        /// <summary>
        /// Returns a new array with a value prepended to it.
        /// </summary>
        internal static T[] PrependArray<T>(this T[] array, T value) {
            var newArray = new T[array.Length + 1];
            newArray[0] = value;
            array.CopyTo(newArray, 1);
            return newArray;
        }
    }
}
