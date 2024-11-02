using UnityEngine;

namespace Quinn.DungeonGeneration
{
	[CreateAssetMenu(fileName = "Floor", menuName = "Floor")]
	public class FloorSO : ScriptableObject
	{
		public Room StartingRoom;
		public RoomEntry[] Generatable;
	}
}
