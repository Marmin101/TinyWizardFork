using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
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

		private readonly Dictionary<Vector2Int, GameObject> _generatedRooms = new();

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
			if (_generatedRooms.ContainsKey(new Vector2Int(x, y)))
			{
				return;
			}

			Debug.Assert(ActiveFloor != null, "Failed to generate room. There is no active floor!");

			bool reqN = false;
			bool reqS = false;
			bool reqE = false;
			bool reqW = false;

			if (GetRoomAt(x, y + 1, out Room n))
				reqN = n.HasSouthDoor;
			if (GetRoomAt(x, y - 1, out Room s))
				reqS = s.HasNorthDoor;
			if (GetRoomAt(x, y + 1, out Room e))
				reqE = e.HasWestDoor;
			if (GetRoomAt(x, y + 1, out Room w))
				reqW = w.HasEastDoor;

			// Filter for rooms that support required doors.
			var validRooms = ActiveFloor.Generatable.Where(roomToGenerate => DoesRoomMatch(roomToGenerate.Prefab, reqN, reqS, reqE, reqW));
			// Get random (by weight) room from filtered collection.
			Room prefab = validRooms.GetWeightedRandom(x => x.Weight).Prefab;

			Debug.Assert(prefab != null, $"Failed to generate room. No valid option found! Required: N {reqN}, S {reqS}, E {reqE}, W {reqW}.");

			// Generate actual room.
			await GenerateRoomAsync(prefab, x, y);
		}

		private bool DoesRoomMatch(Room room, bool requireNorth, bool requireSouth, bool requireEast, bool requireWest)
		{
			if (requireNorth && !room.HasNorthDoor) return false;
			if (requireSouth && !room.HasSouthDoor) return false;
			if (requireWest && !room.HasWestDoor) return false;
			if (requireEast && !room.HasEastDoor) return false;

			return true;
		}

		private bool GetRoomAt(int x, int y, out Room room)
		{
			if (_generatedRooms.TryGetValue(new(x, y), out GameObject value))
			{
				room = value.GetComponent<Room>();
				return true;
			}

			room = null;
			return false;
		}

		private async Awaitable StartFloorAsync(FloorSO floor)
		{
			foreach (var room in _generatedRooms)
			{
				Destroy(room.Value);
			}

			ActiveFloor = floor;

			var prefab = floor.Generatable.GetWeightedRandom(x => x.Weight).Prefab;
			await GenerateRoomAsync(floor.StartingRoom, 0, 0);
		}

		private async Awaitable<Room> GenerateRoomAsync(Room prefab, int x, int y)
		{
			Vector2 pos = RoomGridToWorld(x, y) - Vector2.one;
			var instance = await prefab.gameObject.CloneAsync(pos, Quaternion.identity, transform);

			if (instance == null)
			{
				throw new System.NullReferenceException("Failed to generate room!");
			}

			_generatedRooms.Add(new(x, y), instance);

			var room = instance.GetComponent<Room>();
			room.RoomGridIndex = new(x, y);

			return room;
		}

		private Vector2 RoomGridToWorld(int x, int y)
		{
			return new Vector2(x, y) * MaxRoomSize;
		}
	}
}
