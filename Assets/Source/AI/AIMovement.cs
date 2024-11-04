using UnityEngine;

namespace Quinn.AI
{
	public class AIMovement : Locomotion
	{
		[field: SerializeField]
		public float MoveSpeed { get; set; } = 3f;

		public bool IsJumping { get; private set; }

		private Vector2 _cumulativeVel;

		private Vector2 _jumpOrigin;
		private Vector2 _jumpFinalTarget;
		private Vector2 _jumpPeakTarget;
		private Vector2 _jumpStraightDir;
		private float _jumpDur;
		private float _jumpSpeed;
		private float _jumpUpSpeed, _jumpDownSpeed;

		public override Vector2 GetVelocity()
		{
			UpdateJump();

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

		public void JumpTo(Vector2 target, float height, float speed)
		{
			_jumpSpeed = speed;

			_jumpOrigin = transform.position;
			_jumpFinalTarget = target;
			_jumpPeakTarget = Vector2.Lerp(transform.position, _jumpFinalTarget, 0.5f) + (Vector2.up * height);
			_jumpDur = transform.position.DistanceTo(_jumpFinalTarget) / speed;
			_jumpStraightDir = _jumpOrigin.DirectionTo(_jumpFinalTarget);

			_jumpUpSpeed = Mathf.Abs(transform.position.y - _jumpPeakTarget.y) / (_jumpDur / 2f);
			_jumpDownSpeed = Mathf.Abs(_jumpPeakTarget.y - _jumpFinalTarget.y) / (_jumpDur / 2f);

			IsJumping = true;
		}

		private void UpdateJump()
		{
			if (IsJumping)
			{
				Vector2 a = (Vector2)transform.position - _jumpOrigin;
				Vector2 b = _jumpFinalTarget - _jumpOrigin;

				Vector2 proj = Vector2.Dot(a, b) / Vector2.Dot(b, b) * b;

				float progress = proj.magnitude / b.magnitude;
				float yVel = progress < 0.5f ? _jumpUpSpeed : -_jumpDownSpeed;

				_cumulativeVel += (_jumpStraightDir * _jumpSpeed) + (Vector2.up * yVel);

				if (progress > 0.95f)
				{
					IsJumping = false;
				}
			}
		}
	}
}
