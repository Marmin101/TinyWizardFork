using DG.Tweening;
using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class RandomStaff : MonoBehaviour
	{
		[SerializeField]
		private bool AvoidPlayersActiveStaff = true;
		[SerializeField, AssetsOnly]
		private GameObject[] Staffs;

		private void Awake()
		{
			GameObject[] filtered;
			Staff activeStaff = PlayerManager.Instance.Player.GetComponent<PlayerCaster>().ActiveStaff;

			if (activeStaff != null && AvoidPlayersActiveStaff)
			{
				filtered = Staffs.Where(x => x.GetComponent<Staff>().GUID != activeStaff.GUID).ToArray();
			}
			else
			{
				filtered = Staffs;
			}

			var instance = Staffs.GetRandom().Clone(transform);
			instance.GetComponent<Staff>().OnPickedUp += () =>
			{
				transform.DOKill();
				transform.DetachChildren();
				Destroy(gameObject);
			};
		}
	}
}
