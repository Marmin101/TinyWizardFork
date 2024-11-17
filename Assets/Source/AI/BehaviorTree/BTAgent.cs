using Quinn.DungeonGeneration;
using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[RequireComponent(typeof(BehaviorGraphAgent))]
	[RequireComponent(typeof(AIMovement))]
	[RequireComponent(typeof(Health))]
	public class BTAgent : MonoBehaviour, IAgent
	{
		public Health Health { get; private set; }
		public AIMovement Movement { get; private set; }
		public Room Room { get; private set; }

		public void Awake()
		{
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();

			Health.OnDeath += OnDeath;
		}

		public void StartRoom(Room room)
		{
			Room = room;
		}

		private void OnDeath()
		{
			Destroy(gameObject); // TODO: Implement BTAgent death features.
		}
	}
}
