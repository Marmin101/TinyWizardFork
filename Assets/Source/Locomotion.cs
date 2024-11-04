using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class Locomotion : MonoBehaviour
	{
		[FoldoutGroup("Knockback")]
		[SerializeField, Tooltip("Knockback can still be triggered manually even if this is false.")]
		private bool DoesKnockbackOnDamage = true;
		[InfoBox("@\"Knockback duration: \" + (KnockbackSpeed / KnockbackDecayRate).ToString() + 's'")]
		[SerializeField, FoldoutGroup("Knockback")]
		private float KnockbackSpeed = 12f;
		[SerializeField, FoldoutGroup("Knockback")]
		private float KnockbackDecayRate = 32f;

		protected Rigidbody2D Rigidbody { get; private set; }

		private float _knockbackVel;
		private Vector2 _knockbackDir;

		protected virtual void Awake()
		{
			Rigidbody = GetComponent<Rigidbody2D>();

			if (DoesKnockbackOnDamage)
			{
				GetComponent<Health>().OnDamaged += OnDamaged;
			}
		}

		private void FixedUpdate()
		{
			Vector2 vel = GetVelocity();

			if (_knockbackVel > 0f)
			{
				vel += _knockbackDir * _knockbackVel;
				_knockbackVel -= KnockbackDecayRate * Time.fixedDeltaTime;
			}

			Rigidbody.linearVelocity = vel;
		}

		public abstract Vector2 GetVelocity();

		public void Knockback(Vector2 dir)
		{
			_knockbackVel = KnockbackSpeed;
			_knockbackDir = dir.normalized;
		}

		private void OnDamaged(float damage, Vector2 dir)
		{
			Knockback(dir);
		}
	}
}
