using Phork;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        public static bool AddIfNotExists<T>(this ICollection<T> collection, T item)
        {
            Guard.ArgumentNotNull(collection, nameof(collection));

            if (collection.Contains(item))
            {
                return false;
            }

            collection.Add(item);

            return true;
        }

        public static void AddIfNotNull<T>(this ICollection<T> collection, T item)
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
