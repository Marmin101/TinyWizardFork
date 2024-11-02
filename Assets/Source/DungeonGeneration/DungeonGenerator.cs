using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class DungeonGenerator : MonoBehaviour
	{
		[SerializeField]
		private int MaxRoomSize = 48;
		[SerializeField, RequiredListLength(MinLength = 1)]
		private FloorSO[] Floors;

		public static DungeonGenerator Instance { get; private set; }

		public FloorSO ActiveFloor { get; private set; }
		private readonly HashSet<GameObject> _generatedRooms = new();
		private readonly HashSet<Vector2Int> _generatedRoomIndices = new();

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private async void Start()
		{
			var floor = Floors[Random.Range(0, Floors.Length)];
			await StartFloorAsync(floor);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public async void GenerateRoomAt(int x, int y)
		{
			if (_generatedRoomIndices.Contains(new(x, y)))
			{
				return;
			}

			// TODO: Only pick room that meets door requirements for all adjacent.
			Room prefab = ActiveFloor.Generatable.GetWeightedRandom(x => x.Weight).Prefab;
			await GenerateRoomAsync(prefab, x, y);
		}

		private async Awaitable StartFloorAsync(FloorSO floor)
		{
			foreach (var room in _generatedRooms)
			{
				Destroy(room);
			}

			ActiveFloor = floor;

			var prefab = floor.Generatable.GetWeightedRandom(x => x.Weight).Prefab;
			await GenerateRoomAsync(floor.StartingRoom, 0, 0);
		}

		private async Awaitable<Room> GenerateRoomAsync(Room prefab, int x, int y)
		{
			Vector2 pos = RoomGridToWorld(x, y) - Vector2.one;
			var instance = await prefab.gameObject.CloneAsync(pos);
			_generatedRooms.Add(instance);

			var room = instance.GetComponent<Room>();
			room.RoomGridIndex = new(x, y);

			_generatedRoomIndices.Add(new(x, y));
			return room;
		}

		private Vector2 RoomGridToWorld(int x, int y)
		{
			return new Vector2(x, y) * MaxRoomSize;
		}

		private Vector2Int WorldToRoomGrid(Vector2 worldPos)
		{
			worldPos /= MaxRoomSize;
			return new(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
		}
	}
}
