using Quinn.DungeonGeneration;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	public class BTAgent : MonoBehaviour, IAgent
	{
		private bool _hasRoomStarted;
		private Room _room;

		public void StartRoom(Room room)
		{
			_room = room;
			_hasRoomStarted = true;
		}
	}
}
