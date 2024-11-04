using UnityEngine;

namespace Quinn.AI
{
	public class SlimeAI : AIAgent
	{
		protected async override void Start()
		{
			base.Start();

			await Awaitable.WaitForSecondsAsync(1f);
			Movement.JumpTo((Vector2)transform.position + new Vector2(-2f, -2f).normalized, 1f, 4f);
		}

		protected override void OnThink()
		{
			//Movement.MoveTo(Target.position);
		}
	}
}
