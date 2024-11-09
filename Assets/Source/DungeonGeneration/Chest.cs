using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Chest : MonoBehaviour
	{
		[SerializeField]
		private bool StartOpen;
		[SerializeField, Required]
		private Sprite OpenSprite;

		[Space, SerializeField]
		private EventReference OpenSound;

		public bool IsOpen { get; private set; }

		private void Awake()
		{
			if (StartOpen)
			{
				Open_Internal();
			}
		}

		public void Open()
		{
			if (!IsOpen)
			{
				Open_Internal();
				Audio.Play(OpenSound, transform.position);
			}
		}

		private void Open_Internal()
		{
			IsOpen = true;
			GetComponent<SpriteRenderer>().sprite = OpenSprite;
		}
	}
}
