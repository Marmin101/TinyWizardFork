using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quinn
{
	public static class CollectionExtensions
	{
		public static T GetWeightedRandom<T>(this IEnumerable<T> collection, System.Func<T, float> getWeightCallback)
		{
			float sum = collection.Sum(x => getWeightCallback(x));
			float rand = Random.value;

			foreach (var item in collection)
			{
				if (getWeightCallback(item) / sum < rand)
				{
					return item;
				}
			}

			return collection.ElementAt(Random.Range(0, collection.Count()));
		}
	}
}
