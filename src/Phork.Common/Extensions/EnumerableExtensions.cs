using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Phork.Extensions
{
    public static class EnumerableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            Guard.ArgumentNotNull(enumerable, nameof(enumerable));

            return new ObservableCollection<T>(enumerable);
        }

        public static int GetSequenceHashCode<T>(this IEnumerable<T> enumerable)
        {
            Guard.ArgumentNotNull(enumerable, nameof(enumerable));

            HashCode hashCode = new HashCode();

            foreach (var item in enumerable)
                hashCode.Add(item);

            return hashCode.ToHashCode();
        }

        public static int GetSequenceHashCode<T>(
            this IEnumerable<T> enumerable,
            IEqualityComparer<T> equalityComparer)
        {
            Guard.ArgumentNotNull(enumerable, nameof(enumerable));
            Guard.ArgumentNotNull(equalityComparer, nameof(equalityComparer));

            HashCode hashCode = new HashCode();

            foreach (var item in enumerable)
                hashCode.Add(equalityComparer.GetHashCode(item));

            return hashCode.ToHashCode();
        }
    }
}
