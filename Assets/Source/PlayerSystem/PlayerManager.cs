using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class PlayerManager : MonoBehaviour
	{
		[SerializeField, Required, AssetsOnly]
		private GameObject PlayerGroupPrefab;

		public static PlayerManager Instance { get; private set; }

		public Player Player { get; private set; }
		public event System.Action OnPlayerSet;

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private void Start()
		{
			PlayerGroupPrefab.Clone();
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void SetPlayer(Player player)
		{
			if (Player == null)
			{
				Player = player;

				if (player != null)
				{
					OnPlayerSet?.Invoke();
				}
			}
		}
	}
}
