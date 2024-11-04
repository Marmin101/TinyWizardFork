using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Quinn.PlayerSystem
{
	[RequireComponent(typeof(Animator))]
	public class Player : MonoBehaviour
	{
		[SerializeField]
		private float InteractionRadius = 1f;
		[SerializeField]
		private EventReference FootstepSound;
		[SerializeField]
		private float StartFootstepCooldown = 0.2f;

		private Animator _animator;
		private bool _wasMoving;
		private float _nextStartFootstepSoundAllowedTime;

		private void Awake()
		{
			_animator = GetComponent<Animator>();

			PlayerManager.Instance.SetPlayer(this);
			InputManager.Instance.OnInteract += OnInteract;
		}

		private void Update()
		{
			bool isMoving = InputManager.Instance.MoveDirection.sqrMagnitude > 0f;
			_animator.SetBool("IsMoving", isMoving);

			if (isMoving && !_wasMoving && Time.time > _nextStartFootstepSoundAllowedTime)
			{
				_nextStartFootstepSoundAllowedTime = Time.time + StartFootstepCooldown;
				OnFootstep_Anim();
			}

			_wasMoving = isMoving;
		}

		private void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.SetPlayer(null);
			}
		}

		public void OnFootstep_Anim()
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
	}
}
