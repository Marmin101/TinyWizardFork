using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class GameStart : MonoBehaviour
	{
		public void Awake()
		{
			PlayerManager.Instance.GameStart();
		}
	}
}
