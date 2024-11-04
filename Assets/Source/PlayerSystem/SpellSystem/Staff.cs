using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(Collider2D))]
	public abstract class Staff : MonoBehaviour, IInteractable
	{
		[field: SerializeField, Required]
		protected Transform Head { get; private set; }

		protected PlayerCaster Caster { get; private set; }

		public void Interact(Player player)
		{
			var caster = player.GetComponent<PlayerCaster>();
			caster.SetStaff(this);
		}

		public void SetCaster(PlayerCaster caster)
		{
			Caster = caster;

			// Avoid being interacted with while being held.
			GetComponent<Collider2D>().enabled = false;
		}

		public virtual void OnCastStart() { }
		public virtual void OnCastStop() { }

		public virtual void OnSpecialStart() { }
		public virtual void OnSpecialStop() { }
	}
}
