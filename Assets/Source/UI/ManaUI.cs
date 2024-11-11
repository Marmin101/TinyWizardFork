using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class ManaUI : MonoBehaviour
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
			Bar.value = _caster.Mana / _caster.MaxMana;
		}
	}
}
