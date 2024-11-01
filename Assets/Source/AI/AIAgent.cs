using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(Health))]
	public abstract class AIAgent : MonoBehaviour
	{
		public Health Health { get; private set; }

		protected virtual void Awake()
		{
			Health = GetComponent<Health>();
			Health.OnDeath += OnDeath;
		}

		protected virtual void Start()
		{
			AIGlobal.Instance.AddAgent(this);
		}

		protected virtual void OnDestroy()
		{
			AIGlobal.Instance.RemoveAgent(this);
		}

		protected virtual void OnDeath()
		{
			AIGlobal.Instance.RemoveAgent(this);
		}

		protected void DamageTarget(GameObject target, float damage)
		{
			if (target.TryGetComponent(out Health health))
			{
				DamageTarget(health, damage);
			}
		}
		protected void DamageTarget(Health target, float damage)
		{
			if (target.Team != Health.Team)
			{
				target.TakeDamage(damage);
			}
		}
	}
}
