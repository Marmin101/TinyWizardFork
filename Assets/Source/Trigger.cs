using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Collider2D))]
	public class Trigger : MonoBehaviour
	{
		public System.Action<Collider2D> OnTriggerEnter, OnTriggerExit;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			OnTriggerEnter?.Invoke(collision);
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			OnTriggerExit?.Invoke(collision);
		}
	}
}
