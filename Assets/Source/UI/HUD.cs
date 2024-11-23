using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.UI
{
	public class HUD : MonoBehaviour
	{
		[SerializeField, Required]
		private CanvasGroup Group;
		[SerializeField]
		private float FadeDuration = 1f;

		public static HUD Instance { get; private set; }

		public void Awake()
		{
			Instance = this;
		}

		public void OnDestroy()
		{
			Group.DOKill();
		}

		public void Hide()
		{
			Group.alpha = 0f;
		}

		public void FadeIn()
		{
			Group.DOFade(1f, FadeDuration);
		}
	}
}
