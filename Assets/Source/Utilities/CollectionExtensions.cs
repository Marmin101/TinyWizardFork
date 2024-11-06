using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quinn
{
	public static class CollectionExtensions
	{
		public static T GetRandom<T>(this IEnumerable<T> collection)
		{
			if (!collection.Any())
				return default;

			return collection.ElementAt(Random.Range(0, collection.Count()));
		}

		public static T GetWeightedRandom<T>(this IEnumerable<T> collection, System.Func<T, float> getWeightCallback)
		{
			float sum = collection.Sum(x => getWeightCallback(x));
			float rand = Random.value;

			foreach (var item in collection)
			{
				if (getWeightCallback(item) / sum > rand)
				{
					return item;
				}
			}

			if (!collection.Any())
			{
				return default;
			}

			return collection.ElementAt(Random.Range(0, collection.Count()));
		}

		public static T GetClosestTo<T>(this IEnumerable<T> collection, Vector2 point) where T : Component
		{
			T t = default;
			float nearest = float.PositiveInfinity;

			foreach (var item in collection)
			{
				float dst = item.transform.position.DistanceTo(point);

				if (dst < nearest)
				{
					nearest = dst;
					t = item;
				}
			}

			return t;
		}
		public static T GetClosestTo<T>(this IEnumerable<T> collection, System.Func<T, float> dstCallback)
		{
			T t = default;
			float nearest = float.PositiveInfinity;

			foreach (var item in collection)
			{
				float dst = dstCallback(item);

				if (dst < nearest)
				{
					nearest = dst;
					t = item;
				}
			}

			return t;
		}
	}
}
