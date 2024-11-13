using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	public class ProximityShow : MonoBehaviour
	{
		[SerializeField]
		private float ShowDistance = 2f;
		[SerializeField, Required]
		private GameObject TargetChild;

		private void FixedUpdate()
		{
			if (Time.frameCount % 2 == 0)
			{
				var player = PlayerManager.Instance.Player.transform;
				TargetChild.SetActive(player.position.DistanceTo(transform.position) <= ShowDistance);
			}
		}

		private void OnDisable()
		{
			TargetChild.SetActive(false);
		}
	}
}
