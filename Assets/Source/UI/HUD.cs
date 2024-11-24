using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	public class HUD : MonoBehaviour
	{
		[SerializeField, Required]
		private CanvasGroup Group;
		[SerializeField]
		private float FadeDuration = 1f;

		[SerializeField, Required, Space]
		private TextMeshProUGUI Dialogue;
		[SerializeField]
		private float WriteInterval = 0.01f;
		[SerializeField]
		private EventReference WriteSound;

		public static HUD Instance { get; private set; }

		private CancellationTokenSource _cancelDialogue = new();

		public void Awake()
		{
			Instance = this;
			Dialogue.alpha = 0f;
		}

		public void OnDestroy()
		{
			Group.DOKill();
			_cancelDialogue.Cancel();
		}

		public void Hide()
		{
			Group.alpha = 0f;
		}

		public void FadeIn()
		{
			Group.DOFade(1f, FadeDuration);
		}

		public async void WriteDialogue(string text)
		{
			_cancelDialogue.Cancel();
			_cancelDialogue = new();

			Dialogue.text = string.Empty;

			if (Dialogue.alpha == 0f) {
				Dialogue.DOFade(1f, 0.1f);
			}

			var builder = new StringBuilder();

			for (int i = 0; i < text.Length; i++)
			{
				if (_cancelDialogue.IsCancellationRequested)
				{
					return;
				}

				builder.Append(text[i]);
				Dialogue.text = builder.ToString();

				Audio.Play(WriteSound);
				await Wait.Seconds(WriteInterval, _cancelDialogue.Token);
			}
		}

		public void HideDialogue()
		{
			Dialogue.DOFade(0f, 0.1f);
		}
	}
}
