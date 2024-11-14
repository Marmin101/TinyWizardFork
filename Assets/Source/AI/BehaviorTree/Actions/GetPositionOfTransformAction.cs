using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Get Position of Transform", story: "Get [Position] of [Transform]", category: "Action", id: "e693960eb04ac74be9dcaa29aa9adbee")]
	public partial class GetPositionOfTransformAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;
		[SerializeReference]
		public BlackboardVariable<Transform> Transform;

		protected override Status OnStart()
		{
			if (Transform.Value == null)
				return Status.Failure;

			Position.Value = Transform.Value.position;
			return Status.Success;
		}
	}
}
