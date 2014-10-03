using System;
using System.Collections.Generic;

namespace bstrkr.core.collections
{
	public static class CollectionExtensions
	{
		public static MergeStats Merge<T, TKey>(
			this ICollection<T> collection,
			IEnumerable<T> updates,
			Func<T, TKey> keySelector,
			Action<T, T> updateFactory) where T : class
		{
			return Merge(collection, updates, keySelector, item => item, updateFactory);
		}

		public static MergeStats Merge<T, TKey>(
			this ICollection<T> collection, 
			IEnumerable<T> updates,
			Func<T, TKey> keySelector,
			Func<T, T> addFactory,
			Action<T, T> updateFactory) where T : class
		{
			return Update(collection, updates, keySelector, keySelector, addFactory, updateFactory);
		}

		public static MergeStats Update<T, V, TKey>(
			this ICollection<T> collection,
			IEnumerable<V> updates,
			Func<T, TKey> itemKeySelector,
			Func<V, TKey> updateKeySelector,
			Func<V, T> addFactory,
			Action<T, V> updateFactory) where T : class
		{
			var newItems = 0;
			var updatedItems = 0;
			var deletedItems = 0;

			if (updates == null)
			{
				deletedItems = collection.Count;
				collection.Clear();
				return new MergeStats(0, 0, deletedItems);
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

			var removedKeys = new List<TKey>();
			foreach (TKey key in sourceDict.Keys)
			{
				if (!updatesDict.ContainsKey(key))
				{
					removedKeys.Add(key);
					deletedItems++;
				}
			}

			foreach (var key in removedKeys)
			{
				collection.Remove(sourceDict[key] as T);
				sourceDict.Remove(key);
			}

			foreach (var item in updates)
			{
				var key = updateKeySelector(item);
				if (sourceDict.ContainsKey(key))
				{
					updateFactory(sourceDict[key] as T, item);
					updatedItems++;
				}
				else
				{
					collection.Add(addFactory(item));
					newItems++;
				}
			}

			return new MergeStats(newItems, updatedItems, deletedItems);
		}

		public class MergeStats
		{
			public MergeStats(int newItems, int updatedItems, int deletedItems)
			{
				this.NewItems = newItems;
				this.UpdatedItems = updatedItems;
				this.DeletedItems = deletedItems;
			}

			public int NewItems { get; private set; }

			public int UpdatedItems { get; private set; }

			public int DeletedItems { get; private set; }
		}
	}
}