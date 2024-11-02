using System.Collections.Generic;
using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class Player : MonoBehaviour
	{
		[SerializeField]
		private float InteractionRadius = 1f;

		private void Awake()
		{
			PlayerManager.Instance.SetPlayer(this);
			InputManager.Instance.OnInteract += OnInteract;
		}

		private void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.SetPlayer(null);
			}
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
