using System.Collections.Generic;

namespace Phork.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            Guard.ArgumentNotNull(dictionary, nameof(dictionary));

            return dictionary.TryGetValue(key, out var value) ? value : null;
        }
    }
}
