using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class DungeonGenerator : MonoBehaviour
	{
		[System.Serializable]
		record RoomEntry
		{
			[HorizontalGroup, HideLabel]
			public Room Prefab;
			[HorizontalGroup(0.1f), HideLabel]
			public float Weight;
		}

		[SerializeField]
		private int MaxRoomSize = 48;
		[SerializeField]
		private RoomEntry[] RoomLibrary;

		private readonly HashSet<GameObject> _rooms = new();

		public static DungeonGenerator Instance { get; private set; }

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private async void Start()
		{
			var prefab = GetFromWeightedCollection(RoomLibrary);
			await GenerateRoomAsync(prefab, 0, 0);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		private async Awaitable<Room> GenerateRoomAsync(Room prefab, int x, int y)
		{
			Vector2 pos = RoomGridToWorld(x, y) - Vector2.one;
			var instance = await prefab.gameObject.CloneAsync(pos);

			_rooms.Add(instance);
			return instance.GetComponent<Room>();
		}

		private Room GetFromWeightedCollection(IEnumerable<RoomEntry> collection)
		{
			float sum = collection.Sum(x => x.Weight);
			float rand = Random.value;

			foreach (var entry in collection)
			{
				if (entry.Weight / sum < rand)
				{
					return entry.Prefab;
				}
			}

			return collection.ElementAt(Random.Range(0, collection.Count())).Prefab;
		}

		private Vector2 RoomGridToWorld(int x, int y)
		{
			return new Vector2(x, y) * MaxRoomSize;
		}
	}
}
