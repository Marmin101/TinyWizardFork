using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public abstract class Door : MonoBehaviour
	{
		public bool IsOpened { get; private set; } = true;

		public void Open()
		{
			if (!IsOpened)
			{
				IsOpened = true;
				OnOpen();
			}
		}

		public void Close()
		{
			if (IsOpened)
			{
				IsOpened = false;
				OnClose();
			}
		}

		protected virtual void OnOpen() { }
		protected virtual void OnClose() { }
	}
}
