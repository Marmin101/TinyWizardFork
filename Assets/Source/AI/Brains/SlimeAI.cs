using UnityEngine;

namespace Quinn.AI
{
	public class SlimeAI : AIAgent
	{
		[SerializeField]
		private float JumpInterval = 0.3f;
		[SerializeField]
		private float JumpDistance = 2f;

		protected async override void Start()
		{
			base.Start();

			while (true)
			{
				FaceTarget();

				Vector2 dest = transform.position;
				dest += Position.DirectionTo(TargetPos) * JumpDistance;
				await Jump(dest);

				if (destroyCancellationToken.IsCancellationRequested)
					return;

				FaceTarget();
				await Awaitable.WaitForSecondsAsync(JumpInterval, destroyCancellationToken);
				FaceTarget();
			}
		}

		protected override void OnThink() { }

		private async Awaitable Jump(Vector2 destination)
		{
			Animator.SetTrigger("PrimeJump");
			await Awaitable.WaitForSecondsAsync(0.5f, destroyCancellationToken);
			Animator.SetTrigger("Jump");

			float speed = JumpDistance;
			float dst = transform.position.DistanceTo(destination);

			float dur = dst / speed;
			float endTime = Time.time + dur;

			while (Time.time < endTime && !destroyCancellationToken.IsCancellationRequested)
			{
				Movement.MoveInDirection(transform.position.DirectionTo(destination), speed);
				await Awaitable.NextFrameAsync(destroyCancellationToken);
			}
		}
	}
}
