using FMODUnity;
using System.Linq;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	[CreateAssetMenu(fileName = "Floor", menuName = "Floor")]
	public class FloorSO : ScriptableObject
	{
		public EventReference Ambience;
		public EventReference Music;

		[Space]
		public Room StartingRoom;
		public RoomEntry[] Generatable;

		[Space, Tooltip("Whole floor prefabs.")]
		public GameObject[] Variants;

		public void OnValidate()
		{
			if (Generatable != null)
			{
				float sum = Generatable.Sum(x => x.Weight);

				foreach (var entry in Generatable)
				{
					entry.SumWeight = sum;
				}
			}
		}
	}
}
