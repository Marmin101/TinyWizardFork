using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(Collider2D))]
	public abstract class Staff : MonoBehaviour, IInteractable
	{
		[field: SerializeField, Required]
		public Transform Head { get; private set; }
		[field: SerializeField]
		public Gradient SparkGradient { get; private set; }
		[field: SerializeField]
		public float MaxEnergy { get; private set; } = 500f;

		public float Energy { get; private set; }

		protected PlayerCaster Caster { get; private set; }

		protected bool CanCast
		{
			get
			{
				if (Caster == null || Energy <= 0f)
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

		protected virtual void Awake()
		{
			Energy = MaxEnergy;
		}

		public void Interact(Player player)
		{
			var caster = player.GetComponent<PlayerCaster>();
			caster.EquipStaff(this);
		}

		public void SetCaster(PlayerCaster caster)
		{
			Caster = caster;

			if (caster == null)
				GetComponent<Collider2D>().enabled = true;
			else
				GetComponent<Collider2D>().enabled = false;
		}

		public void DisableInteraction()
		{
			GetComponent<Collider2D>().enabled = false;
		}

		public virtual void OnBasicDown() { }
		public virtual void OnBasicUp() { }

		public virtual void OnSpecialDown() { }
		public virtual void OnSpecialUp() { }

		public void ConsumeEnergy(float amount)
		{
			Energy = Mathf.Max(0f, Energy - amount);
		}

		public void ConsumeAllEnergy()
		{
			Energy = 0f;
		}
	}
}
