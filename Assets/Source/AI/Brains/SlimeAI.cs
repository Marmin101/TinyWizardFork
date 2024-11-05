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

			while (gameObject != null && !DeathTokenSource.IsCancellationRequested)
			{
				FaceTarget();

				Vector2 dest = transform.position;
				dest += Position.DirectionTo(TargetPos) * JumpDistance;
				await Jump(dest);

				FaceTarget();
				await Wait.Seconds(JumpInterval);
				FaceTarget();
			}
		}

		protected override void OnThink() { }

		private async Awaitable Jump(Vector2 destination)
		{
			Animator.SetTrigger("PrimeJump");
			await Wait.Seconds(0.5f, DeathTokenSource.Token);
			Animator.SetTrigger("Jump");

			float speed = JumpDistance;
			float dst = transform.position.DistanceTo(destination);

			float dur = dst / speed;
			float endTime = Time.time + dur;

			while (Time.time < endTime && gameObject != null && !DeathTokenSource.IsCancellationRequested)
			{
				Movement.MoveInDirection(transform.position.DirectionTo(destination), speed);
				await Wait.NextFrame(DeathTokenSource.Token);
			}
		}
	}
}
