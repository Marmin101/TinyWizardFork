using Quinn.PlayerSystem;
using Unity.Cinemachine;
using UnityEngine;

namespace Quinn
{
	public class CameraManager : MonoBehaviour
	{
		public static CameraManager Instance { get; private set; }

		private CinemachineCamera _followCam;

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;

			PlayerManager.Instance.OnPlayerSet += OnPlayerSet;
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void EnableFollowCamera()
		{
			Debug.Assert(_followCam != null, "Cannot enable follow camera because it is null");
			_followCam.enabled = true;
		}

		public void DisableFollowCamera()
		{
			Debug.Assert(_followCam != null, "Cannot disable follow camera because it is null");
			_followCam.enabled = false;
		}

		private void OnPlayerSet()
		{
			Transform playerGroup = PlayerManager.Instance.Player.transform.root;
			_followCam = playerGroup.GetComponentInChildren<CinemachineCamera>();
		}
	}
}
