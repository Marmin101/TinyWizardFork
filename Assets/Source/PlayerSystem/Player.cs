using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Quinn.DungeonGeneration;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem
{
	[RequireComponent(typeof(Animator))]
	public class Player : MonoBehaviour
	{
		[SerializeField]
		private float InteractionRadius = 1f;

		[SerializeField, Space]
		private EventReference FootstepSound;
		[SerializeField]
		private float StartFootstepCooldown = 0.2f;
		[SerializeField, Required]
		private VisualEffect FootstepVFX;
		[SerializeField, FoldoutGroup("Footstep Colors")]
		private Color StoneColor, CarpetColor, HealingPuddleColor;

		[SerializeField, Space]
		private VisualEffect LandVFX;

		[SerializeField, Space]
		private EventReference HurtSnapshot;
		[SerializeField]
		private float HurtDuration = 2f;
		[SerializeField, Required]
		private Light2D PlayerLight;

		[Space, SerializeField, Required]
		private SpriteMask PuddleMask;
		[SerializeField, Required]
		private VisualEffect PuddleVFX;

		private Animator _animator;
		private bool _wasMoving;
		private float _nextStartFootstepSoundAllowedTime;

		private EventInstance _hurtSnapshot;
		private float _playerLightIntensity;

		public void Awake()
		{
			_animator = GetComponent<Animator>();

			PlayerManager.Instance.SetPlayer(this);
			InputManager.Instance.OnInteract += OnInteract;

			_hurtSnapshot = RuntimeManager.CreateInstance(HurtSnapshot);
			GetComponent<Health>().OnDamagedExpanded += OnHurt;

			_playerLightIntensity = PlayerLight.intensity;
		}

		public void Update()
		{
			bool isMoving = InputManager.Instance.MoveDirection.sqrMagnitude > 0f;
			_animator.SetBool("IsMoving", isMoving);

			if (isMoving && !_wasMoving && Time.time > _nextStartFootstepSoundAllowedTime)
			{
				_nextStartFootstepSoundAllowedTime = Time.time + StartFootstepCooldown;
				PlayFootstepSound(GetSoundMaterialType());
			}

			_wasMoving = isMoving;
		}

		public void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.SetPlayer(null);
			}

			if (InputManager.Instance != null)
			{
				InputManager.Instance.OnInteract -= OnInteract;
			}

			_hurtSnapshot.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_hurtSnapshot.release();

			transform.DOKill();
		}

		public void EnablePuddleMask()
		{
			PuddleMask.enabled = true;
		}

		public void DisablePuddleMask()
		{
			PuddleMask.enabled = false;
		}

		public void OnFootstep_Anim()
		{
			var mat = GetSoundMaterialType();
			PlayFootstepSound(mat);

			var floorColor = SoundMaterialToColor(mat);

			if (mat is SoundMaterialType.HealingPuddle)
			{
				PuddleVFX.Play();
			}
			else
			{
				FootstepVFX.SetVector4("Color", floorColor);
				FootstepVFX.Play();
			}
		}

		public void OnLand_Anim()
		{
			var mat = GetSoundMaterialType();
			var floorColor = SoundMaterialToColor(mat);

			LandVFX.SetGradient("Color", new Gradient() { colorKeys = new GradientColorKey[] { new(floorColor, 0f) }, alphaKeys = new GradientAlphaKey[] { new(1f, 0f) } });
			LandVFX.Play();
		}

		public async Awaitable EnterFloorAsync()
		{
			InputManager.Instance.DisableInput();

			var collider = GetComponent<Collider2D>();
			collider.enabled = false;

			_animator.SetTrigger("EnterSequence");
			await Wait.Seconds(1f, destroyCancellationToken);

			collider.enabled = true;

			if (InputManager.Instance != null)
				InputManager.Instance.EnableInput();
		}

		public async Awaitable ExitFloorAsync(FloorExit exit)
		{
			InputManager.Instance.DisableInput();

			var collider = GetComponent<Collider2D>();
			collider.enabled = false;

			float dur = 0.5f;
			float halfTime = Time.time + (dur / 2f);

			var tween = transform.DOJump(exit.transform.position, 2f, 1, dur)
				.SetEase(Ease.Linear)
				.OnUpdate(() =>
				{
					if (Time.time > halfTime)
					{
						exit.EnableMask();
					}
				});

			await tween.AsyncWaitForCompletion();

			var fade = CameraManager.Instance.FadeOut();
			transform.DOMoveY(transform.position.y - 2.2f, 20f)
				.SetEase(Ease.Linear)
				.SetSpeedBased();

			await fade;
		}

		private Color SoundMaterialToColor(SoundMaterialType mat) => mat switch
		{
			SoundMaterialType.None => Color.black,
			SoundMaterialType.Stone => StoneColor,
			SoundMaterialType.Carpet => CarpetColor,
			SoundMaterialType.HealingPuddle => HealingPuddleColor,
			_ => throw new NotImplementedException(),
		};

		private SoundMaterialType GetSoundMaterialType()
		{
			SoundMaterialType mat = SoundMaterialType.None;
			int highestPriority = -9999;

			var colliders = Physics2D.OverlapPointAll(transform.position);

			foreach (var collider in colliders)
			{
				if (collider.TryGetComponent(out SoundMaterial soundMat))
				{
					if (soundMat.Priority > highestPriority)
					{
						mat = soundMat.Material;
						highestPriority = soundMat.Priority;
					}
				}
			}

			return mat;
		}

		private void PlayFootstepSound(SoundMaterialType mat)
		{
			var instance = RuntimeManager.CreateInstance(FootstepSound);
			instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
			instance.setParameterByNameWithLabel("sound-mat", mat.ToString());

			instance.start();
			instance.release();
		}

		private void OnInteract()
		{
			var colliders = Physics2D.OverlapCircleAll(transform.position, InteractionRadius);
			var interactables = new HashSet<IInteractable>();

			foreach (var collider in colliders)
			{
				var component = collider.GetComponent(typeof(IInteractable));

				if (component is IInteractable interactable)
				{
					interactables.Add(interactable);
				}
			}

			IInteractable bestInteractable = null;
			int highestPriority = -99999;

			foreach (var interactable in interactables)
			{
				int priority = interactable.Priority;

				if (priority > highestPriority)
				{
					bestInteractable = interactable;
					highestPriority = priority;
				}
			}

			bestInteractable?.Interact(this);
		}

		private async void OnHurt(DamageInfo info)
		{
			if (!info.IsLethal)
			{
				var seq = DOTween.Sequence();
				float brightness = _playerLightIntensity;

				seq.Append(PlayerLight.DOFade(brightness * 0.5f, 0.5f));
				seq.AppendInterval(HurtDuration);
				seq.Append(PlayerLight.DOFade(brightness / 0.5f, 1.5f));

				seq.Play();

				_hurtSnapshot.start();
				await Wait.Seconds(HurtDuration);
				_hurtSnapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}
		}
	}
}
