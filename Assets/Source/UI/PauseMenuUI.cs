using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class PauseMenuUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Canvas Canvas;
		[SerializeField, Required]
		private CanvasGroup CanvasGroup;

		[SerializeField, Required]
		private Slider SFXSlider, MusicSlider;

		public static PauseMenuUI Instance { get; private set; }
		public bool IsPaused { get; private set; }

		private Bus _sfx, _music;

		public void Awake()
		{
			Instance = this;
			CanvasGroup.alpha = 0f;

			RuntimeManager.StudioSystem.getBus("bus:/SFX", out _sfx);
			RuntimeManager.StudioSystem.getBus("bus:/Music", out _music);

			_sfx.setVolume(SFXSlider.value);
			_music.setVolume(MusicSlider.value);
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				IsPaused = !IsPaused;

				if (IsPaused)
				{
					Time.timeScale = 0f;

					Canvas.enabled = true;
					CanvasGroup.DOFade(1f, 0.1f).SetUpdate(true);

					Cursor.visible = true;
					Cursor.lockState = CursorLockMode.None;

					CrosshairManager.Instance.Hide();
				}
				else
				{
					Unpause();
				}
			}

			if (IsPaused)
			{
				_sfx.setVolume(SFXSlider.value);
				_music.setVolume(MusicSlider.value);
			}
		}

		public void Unpause_Button()
		{
			Unpause();
		}

		public async void Quit_Button()
		{
			await Wait.Seconds(0.2f);

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif

			Application.Quit();
		}

		private void Unpause()
		{
			Time.timeScale = 1f;
			CanvasGroup.DOFade(0f, 0.1f)
				.SetUpdate(true)
				.onComplete += () => Canvas.enabled = false;

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;

			CrosshairManager.Instance.Show();
		}
	}
}
