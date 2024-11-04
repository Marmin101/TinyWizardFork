using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Quinn
{
	public class Health : MonoBehaviour
	{
		[field: SerializeField]
		public float Max { get; private set; } = 100f;
		[field: SerializeField]
		public Team Team { get; private set; }
		[SerializeField]
		private SpriteRenderer[] Renderers;

		[SerializeField, FoldoutGroup("Hurt Flash")]
		private float FlashInDuration = 0.1f, FlashHoldDuration = 0.05f, FlashOutDuration = 0.1f;
		[SerializeField, FoldoutGroup("Hurt Flash")]
		private Ease FlashInEase = Ease.Linear, FlashOutEase = Ease.Linear;

		[SerializeField, Space]
		private bool IsImmuneOnHurt;
		[SerializeField, FoldoutGroup("Hurt Immunity"), ShowIf(nameof(IsImmuneOnHurt))]
		private float HurtImmunityDuration = 2f;
		[SerializeField, FoldoutGroup("Hurt Immunity"), ShowIf(nameof(IsImmuneOnHurt))]
		[InfoBox("@\"Total blink animation time: \" + (BlinkCount * BlinkInterval * 2f).ToString() + 's'")]
		private int BlinkCount = 3;
		[SerializeField, FoldoutGroup("Hurt Immunity"), ShowIf(nameof(IsImmuneOnHurt))]
		private float BlinkInterval = 0.1f;

		public float Current { get; private set; }
		public bool IsDead { get; private set; }
		public bool IsImmune => _isHurtImmune || _damageBlockers.Count > 0;

		public event Action<float> OnHealed, OnDamaged;
		public event Action OnDeath;

		private bool _isHurtImmune;
		private readonly HashSet<object> _damageBlockers = new();
		private float _nextHurtImmunityEndTime;

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

		public void FullHeal()
		{
			Heal(Max - Current);
		}

		public bool TakeDamage(float damage, Team sourceTeam)
		{
			if (Time.time >= _nextHurtImmunityEndTime)
				_isHurtImmune = false;

			if (IsDead || sourceTeam == Team || IsImmune)
				return false;

			Current -= damage;
			Current = Mathf.Max(0, Current);

			OnDamaged?.Invoke(damage);

			if (Current == 0f)
			{
				IsDead = true;
				OnDeath?.Invoke();
			}

			if (IsImmuneOnHurt && !IsDead)
			{
				_isHurtImmune = true;
				_nextHurtImmunityEndTime = Time.time + HurtImmunityDuration;
			}

			AnimateHurtVFX();
			return true;
		}

		public void BlockDamage(object key)
		{
			_damageBlockers.Add(key);
		}
		public void UnblockDamage(object key)
		{
			_damageBlockers.Remove(key);
		}

		private async void AnimateHurtVFX()
		{
			foreach (var renderer in Renderers)
			{
				renderer.enabled = true;
				renderer.material.SetFloat("_Flash", 0f);
			}

			await AnimateFlash();

			if (IsImmuneOnHurt && !IsDead)
			{
				AnimateBlink();
			}
		}

		private async Awaitable AnimateFlash()
		{
			float Get()
			{
				return Renderers[0].material.GetFloat("_Flash");
			}
			void Set(float value)
			{
				foreach (var renderer in Renderers)
				{
					renderer.material.SetFloat("_Flash", value);
				}
			}

			if (Renderers.Length > 0)
			{
				await DOTween.To(Get, Set, 1f, FlashInDuration)
					.SetEase(FlashInEase)
					.AsyncWaitForCompletion();

				await Awaitable.WaitForSecondsAsync(FlashHoldDuration);

				await DOTween.To(Get, Set, 0f, FlashOutDuration)
					.SetEase(FlashOutEase)
					.AsyncWaitForCompletion();
			}
		}

		private async void AnimateBlink()
		{
			for (int i = 0; i < BlinkCount; i++)
			{
				await Awaitable.WaitForSecondsAsync(BlinkInterval);

				foreach (var renderer in Renderers)
				{
					renderer.enabled = false;
				}

				await Awaitable.WaitForSecondsAsync(BlinkInterval);

				foreach (var renderer in Renderers)
				{
					renderer.enabled = true;
				}
			}
		}
	}
}
