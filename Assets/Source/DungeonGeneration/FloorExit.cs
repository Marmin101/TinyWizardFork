using Quinn.PlayerSystem;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class FloorExit : MonoBehaviour
	{
		[SerializeField]
		private SpriteMask Mask;

		private bool _isTriggered;

		public void Awake()
		{
			Mask.enabled = false;
		}

		public async void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer() && !_isTriggered)
			{
				_isTriggered = true;
				DungeonGenerator.Instance.IncrementFloorIndex();

				await PlayerManager.Instance.Player.ExitFloorAsync(this);
				PlayerManager.Instance.RespawnSequence();
			}
		}

		public void EnableMask()
		{
			Mask.enabled = true;
		}
	}
}
