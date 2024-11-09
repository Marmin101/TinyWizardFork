using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class EnergyUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Slider Bar;

		private PlayerCaster _caster;

		private void Start()
		{
			_caster = PlayerManager.Instance.Player.GetComponent<PlayerCaster>();
		}

		private void FixedUpdate()
		{
			if (_caster != null)
			{
				var staff = _caster.ActiveStaff;

				if (staff != null)
				{
					Bar.value = staff.Energy / staff.MaxEnergy;
				}
			}
		}
	}
}
