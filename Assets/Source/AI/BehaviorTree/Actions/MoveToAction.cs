using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move To", story: "Move To [Position]", category: "Action", id: "f32a94d3ee9a64315b5d2513c4c95542")]
	public partial class MoveToAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;
		[SerializeReference]
		public BlackboardVariable<float> StoppingDistance = new(0.1f);

		private BTAgent _agent;

		protected override Status OnStart()
		{
			if (Position.Value == null)
				return Status.Failure;

			_agent = GameObject.GetComponent<BTAgent>();
			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			bool reached = _agent.Movement.MoveTo(Position.Value, StoppingDistance.Value);
			return reached ? Status.Success : Status.Running;
		}
	}
}
