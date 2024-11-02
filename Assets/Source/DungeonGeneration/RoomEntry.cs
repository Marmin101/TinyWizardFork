using Sirenix.OdinInspector;

namespace Quinn.DungeonGeneration
{
	[System.Serializable]
	public record RoomEntry
	{
		[HorizontalGroup, HideLabel]
		public Room Prefab;
		[HorizontalGroup(0.1f), HideLabel]
		public float Weight;
	}
}
