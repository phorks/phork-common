using Phork;
using System.Collections.ObjectModel;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static void Move<T>(this IList<T> collection, int oldIndex, int newIndex)
        {
            Guard.ArgumentNotNull(collection, nameof(collection));


            if (collection is ObservableCollection<T> observableCollection)
            {
                observableCollection.Move(oldIndex, newIndex);
            }
            else
            {
                var removedItem = collection[oldIndex];

                collection.RemoveAt(oldIndex);
                collection.Insert(newIndex, removedItem);
            }
        }
    }
}
