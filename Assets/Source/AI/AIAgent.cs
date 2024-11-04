using Quinn.PlayerSystem;
using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(AIMovement))]
	public abstract class AIAgent : MonoBehaviour
	{
		public Health Health { get; private set; }
		public Team Team => Health.Team;

		protected AIMovement Movement { get; private set; }
		protected Transform Target { get; private set; }
		protected Health TargetHealth { get; private set; }

		protected virtual void Awake()
		{
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();

			Health.OnDamaged += OnDamaged;
			Health.OnDeath += OnDeath;

			AIManager.Instance.AddAgent(this);
		}

		protected virtual void Start()
		{
			SetTarget(GetTarget());
		}

		protected virtual void Update()
		{
			OnThink();
		}

		protected virtual void OnDestroy()
		{
			if (AIManager.Instance != null)
				AIManager.Instance.RemoveAgent(this);
		}

		public void SetTarget(Transform transform)
		{
			Target = transform;
			TargetHealth = transform.GetComponent<Health>();
		}

		protected abstract void OnThink();

		protected virtual Transform GetTarget()
		{
			Transform player = PlayerManager.Instance.Player.transform;

			if (Team is Team.Monster)
			{
				return player;
			}
			else if (Team is Team.CultistA)
			{
				var target = AIManager.Instance.CultistBAgents.GetClosestTo(transform.position).transform;

				if (target == null)
				{
					target = player;
				}

				return target;
			}
			else if (Team is Team.CultistB)
			{
				var target = AIManager.Instance.CultistAAgents.GetClosestTo(transform.position).transform;

				if (target == null)
				{
					target = player;
				}

				return target;
			}

			return player;
		}

		protected virtual void OnDamaged(float amount, Vector2 dir, GameObject source)
		{
			SetTarget(source.transform);
		}

		protected virtual void OnDeath()
		{
			AIManager.Instance.RemoveAgent(this);
			Destroy(gameObject);
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
				target.TakeDamage(damage, transform.position.DirectionTo(target.transform.position), Health.Team, gameObject);
			}
		}
	}
}
