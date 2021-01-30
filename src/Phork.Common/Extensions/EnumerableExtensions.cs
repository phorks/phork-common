using Phork;
using System.Collections.ObjectModel;

namespace System.Collections.Generic
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
            HashCode hashCode = new HashCode();

            foreach (var item in enumerable)
                hashCode.Add(item);

            return hashCode.ToHashCode();
        }
    }
}
