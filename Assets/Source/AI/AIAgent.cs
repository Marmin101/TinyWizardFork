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
			AIManager.Instance.AddAgent(this);
		}

		protected virtual void OnDestroy()
		{
			AIManager.Instance.RemoveAgent(this);
		}

		protected virtual void OnDeath()
		{
			AIManager.Instance.RemoveAgent(this);
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
				target.TakeDamage(damage, transform.position.DirectionTo(target.transform.position), Health.Team);
			}
		}
	}
}
