using Quinn.Pathfinding;
using UnityEngine;

namespace Quinn.AI
{
	public class AIMovement : Locomotion
	{
		[field: SerializeField]
		public float MoveSpeed { get; set; } = 3f;
		[SerializeField]
		private int ContinuousPathfindFrameDivision = 4;

		public bool IsJumping { get; private set; }

		private Vector2 _cumulativeVel;

		public override Vector2 GetVelocity()
		{
			var vel = _cumulativeVel;
			_cumulativeVel = Vector2.zero;

			return vel;
		}

		public bool MoveTo(Vector2 destination, float stoppingDistance = 0.2f)
		{
			if (transform.position.DistanceTo(destination) > stoppingDistance)
			{
				_cumulativeVel += MoveSpeed * (Vector2)transform.position.DirectionTo(destination);
				return false;
			}

			return true;
		}

		public async Awaitable PathTo(Vector2 target, float stoppingDistance = 0.2f)
		{
			Vector2[] path = await Pathfinder.Instance.FindPath(transform.position, target, destroyCancellationToken);
			int index = 0;

			if (path.Length == 0)
				return;

			while (index < path.Length - 1)
			{
				if (destroyCancellationToken.IsCancellationRequested)
					return;

				if (index < path.Length && MoveTo(path[index], stoppingDistance))
				{
					index++;
				}

				await Awaitable.NextFrameAsync();
			}
		}
		public async Awaitable PathTo(Transform target, float stoppingDistance = 0.2f)
		{
			int index = 0;
			Vector2[] path = await Pathfinder.Instance.FindPath(transform.position, target.position, destroyCancellationToken);

			while (index < path.Length - 1 && path.Length > 0)
			{
				if (destroyCancellationToken.IsCancellationRequested)
					return;

				if (Time.frameCount % ContinuousPathfindFrameDivision == 0)
					path = await Pathfinder.Instance.FindPath(transform.position, target.position, destroyCancellationToken);

				if (index < path.Length && MoveTo(path[index], stoppingDistance))
				{
					index++;
				}

				await Awaitable.NextFrameAsync();
			}
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
	}
}
