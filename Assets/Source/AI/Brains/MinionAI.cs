using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class MinionAI : AIAgent
	{
		[SerializeField, Required]
		private Missile Missile;
		[SerializeField, Required]
		private Transform MissileOrigin;
		[SerializeField]
		private float Interval = 0f;
		[SerializeField]
		private int Count = 1;
		[SerializeField]
		private MissileSpawnBehavior Behavior;
		[SerializeField]
		private float SpreadAngle = 30f;

		[SerializeField, Space]
		private Vector2 IdleDuration = new(1f, 5f);
		[SerializeField]
		private float IdleFireChance = 0.5f;
		[SerializeField]
		private float IdleFireInterval = 1f;
		[SerializeField]
		private float MaxTraverseDistance = 4f;
		[SerializeField]
		private float TraverseFireChance = 0.5f;
		[SerializeField]
		private float TraverseFireInterval = 1f;

		private float _idleEndTime;
		private bool _doesFireOnIdle;
		private float _nextIdleFireTime;

		private Vector2 _traversalOrigin;
		private Vector2 _traversalTarget;
		private bool _doesShootOnTraverse;
		private float _nextTraverseShootTime;

		protected override void OnThink()
		{
			// Idle; possibly shoot
			// Run to random position in room (give up after max dst); possibly shoot while running

			if (!IsRoomStarted)
				return;

			if (ActiveState is null)
			{
				if (Random.value < 0.5f)
				{
					TransitionTo(OnIdle);
				}
				else
				{
					TransitionTo(OnTraverse);
				}
			}

			FaceTarget();
			Animator.SetBool("IsMoving", ActiveState == OnTraverse);
		}

		protected override void OnDamaged(float amount, Vector2 dir, GameObject source)
		{
			base.OnDamaged(amount, dir, source);

			if (ActiveState is null || ActiveState == OnIdle)
			{
				_doesShootOnTraverse = true;
				TransitionTo(OnTraverse);
			}
		}

		private bool OnIdle(bool isStart)
		{
			if (isStart)
			{
				_idleEndTime = Time.time + IdleDuration.GetRandom();
				_doesFireOnIdle = Random.value < IdleFireChance;
			}

			if (_doesFireOnIdle && Time.time > _nextIdleFireTime)
			{
				_nextIdleFireTime = Time.time + TraverseFireInterval;
				FireMissile();
			}

			return Time.time > _idleEndTime;
		}

		private bool OnTraverse(bool isStart)
		{
			if (isStart)
			{
				_traversalOrigin = Position;
				_traversalTarget = GetRandomPositionInRadiusInRoom();

				_doesShootOnTraverse = Random.value < TraverseFireChance;
			}

			if (_doesShootOnTraverse && Time.time > _nextTraverseShootTime)
			{
				_nextTraverseShootTime = Time.time + IdleFireInterval;
				FireMissile();
			}

			bool reached = Movement.MoveTo(_traversalTarget);
			bool maxDst = Position.DistanceTo(_traversalOrigin) > MaxTraverseDistance;

			return reached || maxDst;
		}

		private void FireMissile()
		{
			MissileManager.Instance.SpawnMissile(gameObject, Missile, MissileOrigin.position, DirToTarget,
				Count, Interval, Behavior, SpreadAngle);
		}
	}
}
