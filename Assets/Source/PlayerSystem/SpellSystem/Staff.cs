using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(Collider2D))]
	public abstract class Staff : MonoBehaviour, IInteractable
	{
		[field: SerializeField, BoxGroup("ID", showLabel: false)]
		public string GUID { get; private set; }

		[field: SerializeField, Required, BoxGroup("Base", showLabel: false)]
		public Transform Head { get; private set; }
		[field: SerializeField, BoxGroup("Base", showLabel: false)]
		public Gradient SparkGradient { get; private set; }
		[field: SerializeField, BoxGroup("Base", showLabel: false)]
		public float MaxEnergy { get; private set; } = 500f;

		public float Energy { get; private set; }
		public bool CanRegenMana { get; protected set; } = true;

		public event Action OnPickedUp;
		public event Action<Staff> OnEnergyDepleted;

		protected PlayerCaster Caster { get; private set; }

		protected bool CanCastExcludingCost
		{
			get
			{
				if (Caster == null || Energy <= 0f || Caster.Mana <= 0f)
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

		private SortingGroup _group;

		protected virtual void Awake()
		{
			Debug.Assert(!string.IsNullOrWhiteSpace(GUID), $"Staff '{gameObject.name}' is missing a GUID!");

			Energy = MaxEnergy;

			_group = GetComponentInChildren<SortingGroup>();
			_group.enabled = false;

			OnEnergyDepleted += _ =>
			{
				UnityServices.Analytics.Instance.Push(new UnityServices.Events.StaffBrokeEvent()
				{
					Name = gameObject.name
				});
			};
		}

		[Button(SdfIconType.ArrowRepeat, "Generate"), BoxGroup("ID")]
		public void GenerateGUID()
		{
			GUID = System.Guid.NewGuid().ToString();
		}

		public void Interact(Player player)
		{
			var caster = player.GetComponent<PlayerCaster>();
			caster.EquipStaff(this);

			OnPickedUp?.Invoke();
		}

		public void SetCaster(PlayerCaster caster)
		{
			Caster = caster;

			// Storing or destroying.
			if (caster == null)
			{
				GetComponent<Collider2D>().enabled = true;
				_group.enabled = false;

				Head.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

				if (Head.TryGetComponent(out Light2D light))
				{
					light.enabled = false;
				}
			}
			// Equipping.
			else
			{
				GetComponent<Collider2D>().enabled = false;
				_group.enabled = true;

				Head.GetComponent<SpriteRenderer>().color = Color.white;
			}
		}

		public void DisableInteraction()
		{
			GetComponent<Collider2D>().enabled = false;
		}

		public virtual void OnBasicDown() { }
		public virtual void OnBasicUp() { }

		public virtual void OnSpecialDown() { }
		public virtual void OnSpecialUp() { }

		public void SetEnergy(float energy)
		{
			Energy = Mathf.Max(0f, energy);
		}

		public void ConsumeEnergy(float amount)
		{
			Energy = Mathf.Max(0f, Energy - amount);

			if (Energy == 0f)
			{
				OnEnergyDepleted?.Invoke(this);
			}
		}

		public void ConsumeAllEnergy()
		{
			Energy = 0f;
			OnEnergyDepleted?.Invoke(this);
		}

		public void ConsumeMana(float amount)
		{
			Caster.ConsumeMana(amount);
		}

		protected bool CanAffordCost(float amount)
		{
			return Caster.Mana >= amount;
		}
	}
}
