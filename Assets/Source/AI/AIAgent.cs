using Quinn.PlayerSystem;
using System.Threading;
using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(AIMovement))]
	public abstract class AIAgent : MonoBehaviour
	{
		public Health Health { get; private set; }
		public Team Team => Health.Team;

		public CancellationTokenSource DeathTokenSource { get; private set; } = new();

		protected Animator Animator { get; private set; }
		protected AIMovement Movement { get; private set; }
		protected Vector2 Position => transform.position;

		protected Transform Target { get; private set; }
		protected Health TargetHealth { get; private set; }

		protected Vector2 TargetPos
		{
			get
			{
				if (Target == null)
					return transform.position;

				return Target.position;
			}
		}
		protected float DstToTarget => transform.position.DistanceTo(TargetPos);
		protected Vector2 DirToTarget => transform.position.DirectionTo(TargetPos);

		protected virtual void Awake()
		{
			Animator = GetComponent<Animator>();
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
			DeathTokenSource.Cancel();

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
			DeathTokenSource.Cancel();

			AIManager.Instance.RemoveAgent(this);
			Destroy(gameObject);
		}

		protected void FacePosition(Vector2 position)
		{
			transform.localScale = new Vector3(Mathf.Sign(transform.position.DirectionTo(position).x), 1f, 1f);
		}

		protected void FaceTarget()
		{
			FacePosition(TargetPos);
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
