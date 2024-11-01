using System;
using UnityEngine;

namespace Quinn
{
	public class Health : MonoBehaviour
	{
		[field: SerializeField]
		public float Max { get; private set; } = 100f;
		[field: SerializeField]
		public Team Team { get; private set; }

		public float Current { get; private set; }
		public bool IsDead {  get; private set; }

		public event Action<float> OnHealed, OnDamaged;
		public event Action OnDeath;

		private void Awake()
		{
			Current = Max;
		}

		public void Heal(float health)
		{
			if (IsDead)
				return;

			Current += health;
			Current = Mathf.Min(Current, Max);

			OnHealed?.Invoke(health);
		}

		public void TakeDamage(float damage)
		{
			if (IsDead)
				return;

			Current -= damage;
			Current = Mathf.Max(0, Current);

			OnDamaged?.Invoke(damage);

			if (Current == 0f)
			{
				IsDead = true;
				OnDeath?.Invoke();
			}
		}
	}
}
