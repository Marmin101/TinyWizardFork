using DG.Tweening;
using UnityEngine;

namespace Quinn.AI
{
	public class AIMovement : Locomotion
	{
		[field: SerializeField]
		public float MoveSpeed { get; set; } = 3f;

		public bool IsJumping { get; private set; }

		private Vector2 _cumulativeVel;

		private void OnDestroy()
		{
			transform.DOKill();
		}

		public override Vector2 GetVelocity()
		{
			var vel = _cumulativeVel;
			_cumulativeVel = Vector2.zero;

			return vel;
		}

		public bool MoveTo(Vector2 destination, bool usePathfinding = true, float stoppingDistance = 0.2f)
		{
			// TODO: Add pathfinding.

			if (transform.position.DistanceTo(destination) > stoppingDistance)
			{
				_cumulativeVel += MoveSpeed * (Vector2)transform.position.DirectionTo(destination);
				return false;
			}

			return true;
		}

		public void MoveInDirection(Vector2 direction)
		{
			_cumulativeVel += direction.normalized * MoveSpeed;
		}
		public void MoveInDirection(Vector2 direction, float speed)
		{
			_cumulativeVel += direction.normalized * speed;
		}

		public async Awaitable MoveInDirectionFor(Vector2 direction, float duration)
		{
			float endTime = Time.time + duration;

			while (Time.time < endTime && !destroyCancellationToken.IsCancellationRequested)
			{
				MoveInDirection(direction);
				await Awaitable.NextFrameAsync(destroyCancellationToken);
			}
		}

		public void JumpTo(Vector2 target, float height, float speed)
		{
			if (IsJumping)
				return;

			IsJumping = true;

			Vector2 peak = Vector2.Lerp(transform.position, target, 0.5f);
			float dst = transform.position.DistanceTo(peak) + peak.DistanceTo(target);

			float dur = dst / speed;
			transform.DOJump(target, height, 1, dur)
				.SetEase(Ease.Linear)
				.onComplete += () =>
				{
					IsJumping = false;
				};
		}
	}
}
