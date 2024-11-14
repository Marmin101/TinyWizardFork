using Quinn.DungeonGeneration;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[RequireComponent(typeof(AIMovement))]
	public class BTAgent : MonoBehaviour, IAgent
	{
		public Health Health { get; private set; }
		public AIMovement Movement { get; private set; }

		private bool _hasRoomStarted;
		private Room _room;

		public void Awake()
		{
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();
		}

		public void StartRoom(Room room)
		{
			_room = room;
			_hasRoomStarted = true;
		}
	}
}
