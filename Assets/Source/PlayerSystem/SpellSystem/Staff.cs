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

		protected bool CanCast
		{
			get
			{
				if (Caster == null)
					return false;

				return Caster.CanCast;
			}
		}
		protected bool IsBasicHeld
		{
			get
			{
				if (Caster == null)
					return false;

				return Caster.IsBasicHeld;
			}
		}
		protected bool IsSpecialHeld
		{
			get
			{
				if (Caster == null)
					return false;

				return Caster.IsSpecialHeld;
			}
		}

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

		public virtual void OnBasicDown() { }
		public virtual void OnBasicUp() { }

		public virtual void OnSpecialDown() { }
		public virtual void OnSpecialUp() { }
	}
}
