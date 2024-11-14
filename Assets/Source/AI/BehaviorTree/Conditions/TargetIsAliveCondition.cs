using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, Unity.Properties.GeneratePropertyBag]
	[Condition(name: "TargetIsAlive", story: "[Target] is Alive", category: "Conditions", id: "eff4a53a5663fc9240e43f18e03c6a1a")]
	public partial class TargetIsAliveCondition : Condition
	{
		[SerializeReference]
		public BlackboardVariable<Health> Target;

		public override bool IsTrue()
		{
			return Target.Value != null && !Target.Value.IsDead;
		}
	}
}
