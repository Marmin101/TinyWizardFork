using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class Player : MonoBehaviour
	{
		private void Start()
		{
			PlayerManager.Instance.SetPlayer(this);
		}

		private void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.SetPlayer(null);
			}
		}
	}
}
