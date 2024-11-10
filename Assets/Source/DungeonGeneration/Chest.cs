using DG.Tweening;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Chest : MonoBehaviour, IInteractable
	{
		[SerializeField]
		private bool StartOpen;
		[SerializeField]
		private bool IsInteractable = true;
		[SerializeField, Required]
		private Sprite OpenSprite;

		[SerializeField, Required]
		private GameObject ContentHandle;
		[SerializeField]
		private float AnimationOffset = 1f;
		[SerializeField]
		private float AnimationDuration = 2f;
		[SerializeField]
		private Ease AnimationEasing = Ease.Linear;

		[SerializeField, Space, Required]
		private StudioEventEmitter Ambience;

		[Space, SerializeField]
		private EventReference OpenSound;
		[SerializeField]
		private bool DisableAmbienceOnOpen = true;

		public bool IsOpen { get; private set; }
		public int Priority => -100;

		private void Awake()
		{
			if (StartOpen)
			{
				Open_Internal();
				AnimateContent(true);
			}
		}

		private void Start()
		{
			ContentHandle.transform.GetChild(0).gameObject.SetActive(false);
		}

		public void Open()
		{
			if (!IsOpen)
			{
				Open_Internal();
				Audio.Play(OpenSound, transform.position);
				AnimateContent();
			}
		}

		private void Open_Internal()
		{
			IsOpen = true;
			GetComponent<SpriteRenderer>().sprite = OpenSprite;

			if (DisableAmbienceOnOpen)
			{
				Ambience.Stop();
			}
		}

		public void Interact(Player player)
		{
			if (!IsOpen && IsInteractable)
			{
				Open();
			}
		}

		private void AnimateContent(bool skip = false)
		{
			Debug.Assert(ContentHandle.transform.childCount > 0, "Chest's content handle is missing a child!");

			Transform child = ContentHandle.transform.GetChild(0);
			child.gameObject.SetActive(true);

			float yPos = child.position.y + AnimationOffset;
			var tween = child.DOMoveY(yPos, AnimationDuration)
				.SetEase(AnimationEasing);

			tween.onUpdate += () =>
			{
				if (ContentHandle.transform.childCount == 0)
				{
					child.DOKill();
				}
			};

			if (skip)
			{
				tween.Complete();
			}
		}
	}
}
