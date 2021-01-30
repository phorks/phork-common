using Phork;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static bool AddIfNotExists<T>(this ICollection<T> source, T item)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            if (source.Contains(item))
            {
                return false;
            }

            source.Add(item);

            return true;
        }

        public static void AddIfNotNull<T>(this IList<T> collection, T item)
            where T : class
        {
            Guard.ArgumentNotNull(collection, nameof(collection));

            if (item != null)
            {
                collection.Add(item);
            }
        }
    }
}
