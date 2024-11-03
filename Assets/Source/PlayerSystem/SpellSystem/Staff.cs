using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(Collider2D))]
	public abstract class Staff : MonoBehaviour, IInteractable
	{
		protected PlayerCaster Caster { get; private set; }

		public void Interact(Player player)
		{
			var caster = player.GetComponent<PlayerCaster>();
			caster.SetStaff(this);
		}

		public void SetCaster(PlayerCaster caster)
		{
			Caster = caster;
		}
	}
}
