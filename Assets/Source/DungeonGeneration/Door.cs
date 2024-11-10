using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.DungeonGeneration
{
	public abstract class Door : MonoBehaviour
	{
		[SerializeField]
		private VisualEffect OpenVFX, CloseVFX;

		public bool IsOpened { get; private set; } = true;

		public void Open()
		{
			if (!IsOpened)
			{
				IsOpened = true;
				OnOpen();

				if (OpenVFX != null)
				{
					OpenVFX.Play();
				}
			}
		}

		public void Close()
		{
			if (IsOpened)
			{
				IsOpened = false;
				OnClose();

				if (CloseVFX != null)
				{
					CloseVFX.Play();
				}
			}
		}

		protected virtual void OnOpen() { }
		protected virtual void OnClose() { }
	}
}
