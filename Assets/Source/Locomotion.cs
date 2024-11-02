using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class Locomotion : MonoBehaviour
	{
		protected Rigidbody2D Rigidbody { get; private set; }

		protected virtual void Awake()
		{
			Rigidbody = GetComponent<Rigidbody2D>();
		}

		public abstract Vector2 GetVelocity();

		private void FixedUpdate()
		{
			Rigidbody.linearVelocity = GetVelocity();
		}
	}
}
