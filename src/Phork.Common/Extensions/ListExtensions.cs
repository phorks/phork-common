using Phork;
using System.Collections.ObjectModel;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            Guard.ArgumentNotNull(list, nameof(list));


            if (list is ObservableCollection<T> observableCollection)
            {
                observableCollection.Move(oldIndex, newIndex);
            }
            else
            {
                var removedItem = list[oldIndex];

                list.RemoveAt(oldIndex);
                list.Insert(newIndex, removedItem);
            }
        }
    }
}
