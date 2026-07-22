using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpers.ExtMethods {
    public static class CollectionHelper {

        #region PrivateFields
        private static readonly Random _random = new Random();
        #endregion

        #region IList<T>

        public static T GetRandom<T>(this IList<T> source) {
            if (source == null || source.Count == 0)
                throw new InvalidOperationException("Collection is empty.");

            return source[_random.Next(source.Count)];
        }

        public static void ForEach<T>(this IList<T> source, Action<T> action) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            for (int i = 0; i < source.Count; i++)
                action(source[i]);
        }

        public static void Shuffle<T>(this IList<T> source) {
            if (source == null) throw new ArgumentNullException(nameof(source));

            for (int i = source.Count - 1; i > 0; i--) {
                int j = _random.Next(i + 1);
                (source[i], source[j]) = (source[j], source[i]);
            }
        }

        #endregion

        #region Array

        public static T GetRandom<T>(this T[] source) {
            if (source == null || source.Length == 0)
                throw new InvalidOperationException("Array is empty.");

            return source[_random.Next(source.Length)];
        }

        public static void ForEach<T>(this T[] source, Action<T> action) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            for (int i = 0; i < source.Length; i++)
                action(source[i]);
        }

        #endregion

        #region ICollection<T>

        public static T GetRandom<T>(this ICollection<T> source) {
            if (source == null || source.Count == 0)
                throw new InvalidOperationException("Collection is empty.");

            int index = _random.Next(source.Count);
            return source.ElementAt(index); // uses LINQ to access by index
        }

        public static void ForEach<T>(this ICollection<T> source, Action<T> action) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
                action(item);
        }

        /// <summary>
        /// Shuffles an ICollection by creating a temporary list.
        /// Reorders only if the collection is a List<T>.
        /// </summary>
        public static void Shuffle<T>(this ICollection<T> source) {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source is IList<T> list) {
                list.Shuffle();
            }
            else {
                // fallback: copy to list, shuffle, clear + re-add
                var temp = source.ToList();
                temp.Shuffle();

                source.Clear();
                foreach (var item in temp)
                    source.Add(item);
            }
        }

        #endregion
    }
}