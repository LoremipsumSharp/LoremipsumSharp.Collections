using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoremipsumSharp.Common
{
    public static class CollectionExtensions
    {
        public static DiffEnumerable<T> Diff<T>(this IEnumerable<T> left,
                                       IEnumerable<T> right,
   IEqualityComparer<T> comparer = null)
        {
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            var leftOnly = new HashSet<T>(left, comparer);
            var rightOnly = new HashSet<T>(right, comparer);
            var intersect = new HashSet<T>(leftOnly, comparer);

            intersect.IntersectWith(rightOnly);

            if (intersect.Count > 0)
            {
                leftOnly.ExceptWith(intersect);
                rightOnly.ExceptWith(intersect);
            }

            return new DiffEnumerable<T>(leftOnly, intersect, rightOnly);
        }


        public static DiffEnumerable<TLeft, TRight> Diff<TLeft, TRight, TKey>(this IEnumerable<TLeft> left,
                                                                              Func<TLeft, TKey> leftKeySelector,
                                                                              IEnumerable<TRight> right,
                                                                              Func<TRight, TKey> rightKeySelector,
            IEqualityComparer<TKey> comparer = null) where TKey : IComparable
        {
            var leftDictionary = left.ToDictionary(leftKeySelector);
            var rightDictionary = right.ToDictionary(rightKeySelector);

            var keyDiff = leftDictionary.Keys.Diff(rightDictionary.Keys, comparer);

            var leftOnly = keyDiff.LeftOnly.Select(k => leftDictionary[k])
                                  .ToList();
            var rightOnly = keyDiff.RightOnly.Select(k => rightDictionary[k])
                                   .ToList();
            var intersection = keyDiff
                               .Intersect.Select(k =>
                                   new DiffEnumerable<TLeft, TRight>.DiffValue(leftDictionary[k], rightDictionary[k]))
                               .ToList();

            return new DiffEnumerable<TLeft, TRight>(leftOnly, intersection, rightOnly);
        }

        public static int IndexOfBy<TSource, TKey>(this IEnumerable<TSource> source,
                                          TKey value,
                                          Func<TSource, TKey> keySelector)
        {
            return source.Select((x, i) => new
            {
                Index = i,
                Value = x
            })
                         .FirstOrDefault(x => keySelector(x.Value)?.Equals(value) ?? false)
                         ?.Index ?? -1;
        }


        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

    }
}