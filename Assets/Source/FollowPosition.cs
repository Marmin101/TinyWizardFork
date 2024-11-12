using UnityEngine;

namespace Quinn
{
	public class FollowPosition : MonoBehaviour
	{
		[field: SerializeField]
		public Transform Target { get; set; }

		private void LateUpdate()
		{
			if (Target != null)
			{
				transform.position = Target.position;
			}
		}
	}
}
