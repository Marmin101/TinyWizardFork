using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class PlayerMovement : Locomotion
	{
		[SerializeField]
		private float MoveSpeed = 6f;

		public override Vector2 GetVelocity()
		{
			return MoveSpeed * InputManager.Instance.MoveDirection;
		}
	}
}
