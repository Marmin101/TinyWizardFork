using Quinn.DungeonGeneration;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[RequireComponent(typeof(BehaviorGraphAgent))]
	[RequireComponent(typeof(AIMovement))]
	public class BTAgent : MonoBehaviour, IAgent
	{
		public Health Health { get; private set; }
		public AIMovement Movement { get; private set; }
		public Room Room { get; private set; }

		public void Awake()
		{
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();
		}

		public void StartRoom(Room room)
		{
			Room = room;
		}
	}
}
