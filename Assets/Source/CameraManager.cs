using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn
{
	public class CameraManager : MonoBehaviour
	{
		public static CameraManager Instance { get; private set; }

		[SerializeField]
		private float FadeToBlackDuration = 0.3f;
		[SerializeField]
		private float FadeFromBlackDuration = 0.2f;
		[SerializeField]
		private Ease FadeToBlackEase = Ease.InCubic;
		[SerializeField]
		private Ease FadeFromBlackEase = Ease.OutCubic;

		private CameraHandle _handle;
		private CinemachineCamera _followCam;
		private Image _blackout;

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private async void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				float time = Time.time;
				await TransitionAsync(() => Time.time > time + 1f);
			}
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void SetCameraHandle(CameraHandle handle)
		{
			_handle = handle;

			_followCam = _handle.VirtualCamera;
			_blackout = _handle.Blackout;
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

		public async Awaitable TransitionAsync(System.Func<bool> canFadeIn)
		{
			_blackout.enabled = true;
			var color = _blackout.color;
			color.a = 0f;
			_blackout.color = color;

			await _blackout.DOFade(1f, FadeToBlackDuration)
				.SetEase(FadeToBlackEase)
				.AsyncWaitForCompletion();

			while(!canFadeIn())
			{
				await System.Threading.Tasks.Task.Yield();
			}

			_blackout.DOFade(0f, FadeFromBlackDuration)
				.SetEase(FadeFromBlackEase)
				.onComplete += () =>
				{
					_blackout.enabled = false;
				};
		}
		public async Awaitable TransitionAsync()
		{
			await TransitionAsync(() => true);
		}
	}
}
