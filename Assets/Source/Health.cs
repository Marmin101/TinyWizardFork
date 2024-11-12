using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn
{
	public class Health : MonoBehaviour
	{
		[SerializeField]
		private EventReference HurtSound, DeathSound, HealSound;
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

		[Space, SerializeField]
		private Slider HPBar;

		public float Current { get; private set; }
		public bool IsDead { get; private set; }
		public bool IsImmune => _isHurtImmune || _damageBlockers.Count > 0;

		public event Action<float> OnHealed;
		public event Action<float, Vector2, GameObject> OnDamaged;
		public event Action<DamageInfo> OnDamagedExpanded;
		public event Action OnDeath;
		public event Action OnMaxChange;

		private bool _isHurtImmune;
		private readonly HashSet<object> _damageBlockers = new();
		private float _nextHurtImmunityEndTime;

		private void Awake()
		{
			Current = Max;
		}

		private void Start()
		{
			if (HPBar != null)
			{
				HPBar.transform.parent.SetParent(null, true);
			}
		}

		private void FixedUpdate()
		{
			if (HPBar != null)
			{
				HPBar.gameObject.SetActive(Current < Max);
				HPBar.value = Current / Max;
			}
		}

		private void OnDestroy()
		{
			DestroyHPBar();
		}

		public void Heal(float health)
		{
			if (IsDead)
				return;

			Current += health;
			Current = Mathf.Min(Current, Max);

			OnHealed?.Invoke(health);

			Audio.Play(HealSound, transform.position);
		}

		public void FullHeal()
		{
			Heal(Max - Current);
		}

		public bool TakeDamage(float damage, Vector2 dir, Team sourceTeam, GameObject source, float? customKnockback = null)
		{
			var info = new DamageInfo()
			{
				Damage = damage,
				Direction = dir,
				SourceTeam = sourceTeam,
				Source = source,
				UsesCustomKnockbackSpeed = customKnockback.HasValue
			};

			if (customKnockback.HasValue)
			{
				info.CustomKnockbackSpeed = customKnockback.Value;
			}

			return TakeDamage(info);
		}
		public bool TakeDamage(DamageInfo info)
		{
			if (Time.time >= _nextHurtImmunityEndTime)
				_isHurtImmune = false;

			if (IsDead || info.SourceTeam == Team || IsImmune)
				return false;

			Audio.Play(HurtSound, transform.position);

			Current -= info.Damage;
			Current = Mathf.Max(0, Current);

			OnDamaged?.Invoke(info.Damage, info.Direction.normalized, info.Source);
			OnDamagedExpanded?.Invoke(info);

			if (Current == 0f)
			{
				Audio.Play(DeathSound, transform.position);

				IsDead = true;
				OnDeath?.Invoke();

				DestroyHPBar();
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

		public void SetMax(float max)
		{
			Max = max;
			OnMaxChange?.Invoke();
		}

		private async void AnimateHurtVFX()
		{
			foreach (var renderer in Renderers)
			{
				Debug.Assert(renderer != null, $"Health damaged FX found a null sprite renderered! Make sure '{gameObject.name}' has no missing references on its Health component.");

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
				if (Renderers.Length == 0) 
					return 0f;

				var renderer = Renderers[0];

				if (renderer == null)
					return 0f;

				return renderer.material.GetFloat("_Flash");
			}
			void Set(float value)
			{
				if (this == null)
					return;

				foreach (var renderer in Renderers)
				{
					if (renderer == null)
						return;

					renderer.material.SetFloat("_Flash", value);
				}
			}

			if (Renderers.Length > 0 && this != null)
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

		private void DestroyHPBar()
		{
			if (HPBar != null)
			{
				HPBar.transform.parent.gameObject.Destroy();
			}
		}
	}
}
