using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, Unity.Properties.GeneratePropertyBag]
	[Condition(name: "Target HP is Above", story: "[Target] is Above [Health] Percent HP", category: "Conditions", id: "b27b489c112db159a165818ee46131b7")]
	public partial class TargetHPIsAboveCondition : Condition
	{
		[SerializeReference]
		public BlackboardVariable<Health> Target;
		[SerializeReference]
		public BlackboardVariable<float> Health = new(0.5f);

		public override bool IsTrue()
		{
			return Target.Value != null && Target.Value.Percent > Health.Value;
		}
	}
}
