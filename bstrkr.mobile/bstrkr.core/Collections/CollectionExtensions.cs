using System;
using System.Collections.Generic;

namespace bstrkr.core.collections
{
    public static class CollectionExtensions
    {
        public static void Merge<T, TKey>(
            this ICollection<T> collection,
            IEnumerable<T> updates,
            Func<T, TKey> keySelector,
            Action<T, T> updateFactory) where T : class
        {
            Merge(collection, updates, keySelector, item => item, updateFactory);
        }

        public static void Merge<T, TKey>(
            this ICollection<T> collection,
            IEnumerable<T> updates,
            Func<T, TKey> keySelector,
            Action<T, T> updateFactory,
            MergeMode mergeMode) where T : class
        {
            Merge(collection, updates, keySelector, keySelector, item => item, updateFactory, mergeMode);
        }

        public static void Merge<T, TKey>(
            this ICollection<T> collection,
            IEnumerable<T> updates,
            Func<T, TKey> keySelector,
            Func<T, T> addFactory,
            Action<T, T> updateFactory) where T : class
        {
            Merge(collection, updates, keySelector, keySelector, addFactory, updateFactory, MergeMode.Full);
        }

        public static void Merge<T, V, TKey>(
            this ICollection<T> collection,
            IEnumerable<V> updates,
            Func<T, TKey> itemKeySelector,
            Func<V, TKey> updateKeySelector,
            Func<V, T> addFactory,
            Action<T, V> updateFactory,
            MergeMode mergeMode) where T : class
        {
            if (updates == null)
            {
                if (mergeMode == MergeMode.Full)
                {
                    collection.Clear();
                }

                return;
            }

            var sourceDict = new Dictionary<TKey, T>();
            foreach (var item in collection)
            {
                sourceDict[itemKeySelector(item)] = item;
            }

            var updatesDict = new Dictionary<TKey, V>();
            foreach (var item in updates)
            {
                updatesDict[updateKeySelector(item)] = item;
            }

            if (mergeMode == MergeMode.Full)
            {
                var removedKeys = new List<TKey>();
                foreach (TKey key in sourceDict.Keys)
                {
                    if (!updatesDict.ContainsKey(key))
                    {
                        removedKeys.Add(key);
                    }
                }

                foreach (var key in removedKeys)
                {
                    collection.Remove(sourceDict[key] as T);
                    sourceDict.Remove(key);
                }
            }

            foreach (var item in updates)
            {
                var key = updateKeySelector(item);
                if (sourceDict.ContainsKey(key))
                {
                    updateFactory(sourceDict[key] as T, item);
                }
                else
                {
                    collection.Add(addFactory(item));
                }
            }
        }
    }
}