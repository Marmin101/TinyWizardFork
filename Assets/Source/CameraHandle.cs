using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn
{
	public class CameraHandle : MonoBehaviour
	{
		[field: SerializeField, Required]
		public Camera View { get; private set; }
		[field: SerializeField, Required]
		public CinemachineCamera VirtualCamera { get; private set; }
		[field: SerializeField, Required]
		public Image Blackout { get; private set; }

		private void Awake()
		{
			CameraManager.Instance.SetCameraHandle(this);
		}
	}
}
