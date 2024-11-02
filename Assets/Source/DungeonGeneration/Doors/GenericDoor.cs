using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.DungeonGeneration.Doors
{
	public class GenericDoor : Door
	{
		[SerializeField, Required]
		private GameObject OpenChild, ClosedChild;

		private void Awake()
		{
			OpenChild.SetActive(true);
			ClosedChild.SetActive(false);
		}

		protected override void OnOpen()
		{
			OpenChild.SetActive(true);
			ClosedChild.SetActive(false);
		}

		protected override void OnClose()
		{
			ClosedChild.SetActive(true);
			OpenChild.SetActive(false);
		}
	}
}
