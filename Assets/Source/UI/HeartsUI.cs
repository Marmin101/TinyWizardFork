using DG.Tweening;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HeartsUI : MonoBehaviour
	{
		[SerializeField, Required]
		private GameObject HeartPrefab;
		[SerializeField, Required]
		private Sprite FullHeart, EmptyHeart;

		[SerializeField, BoxGroup("Last Heart Shake")]
		private float LastHeartAmplitude = 5f, LastHeartFrequency = 50f;

		[SerializeField, BoxGroup("HP Change Punch")]
		private float PunchScale = 1.1f, PunchDur = 0.2f;
		[SerializeField, BoxGroup("HP Change Punch")]
		private Ease PunchBigEase = Ease.Linear, PunchSmallEase = Ease.Linear;

		[SerializeField, BoxGroup("SFX")]
		private EventReference GainHeartSound;

		public void Start()
		{
			PlayerManager.Instance.OnPlayerHealthChange += OnHealthChange;
			PlayerManager.Instance.OnPlayerMaxHealthChange += OnMaxHealthChange;

			ReconstructHearts();
		}

		public void Update()
		{
			if (PlayerManager.Instance.Health != null && PlayerManager.Instance.Health.Current == 1f)
			{
				transform.localPosition = new Vector3()
				{
					x = Mathf.Sin(Time.time * LastHeartFrequency),
					y = Mathf.Cos((Time.time + Random.value) * LastHeartFrequency)
				} * LastHeartAmplitude;
			}
			else
			{
				transform.localPosition = Vector3.zero;
			}
		}

		public void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.OnPlayerHealthChange -= OnHealthChange;
				PlayerManager.Instance.OnPlayerMaxHealthChange -= OnMaxHealthChange;
			}
		}

		private async void OnHealthChange(float delta)
		{
			if (delta != 0f)
			{
				if (delta > 0f)
				{
					Audio.Play(GainHeartSound);
				}

				UpdateHearts(isHealing: delta > 0f);

				await transform.parent.DOScale(PunchScale, PunchDur / 2f)
					.SetEase(PunchBigEase)
					.AsyncWaitForCompletion();

				transform.parent.DOScale(1, PunchDur / 2f)
					.SetEase(PunchSmallEase);
			}
		}

		private void OnMaxHealthChange()
		{
			ReconstructHearts();
			UpdateHearts();
		}

		private void ReconstructHearts()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.Destroy();
			}

			int max = Mathf.RoundToInt(PlayerManager.Instance.Health.Max);

			for (int i = 0; i < max; i++)
			{
				HeartPrefab.Clone(transform);
			}
		}

		private void UpdateHearts(bool isHealing = false)
		{
			int current = Mathf.RoundToInt(PlayerManager.Instance.Health.Current);

			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);

				if (child != null)
				{
					bool isFull = i < current;

					var img = child.GetComponent<Image>();
					img.sprite = isFull ? FullHeart : EmptyHeart;

					if (isFull && isHealing)
					{
						PunchHeart(child, i);
					}
				}
			}
		}

		private async void PunchHeart(Transform heart, int index)
		{
			await Wait.Seconds(index * 0.1f);

			await heart.DOScale(2f, 0.1f).AsyncWaitForCompletion();
			heart.DOScale(1f, 0.1f);
		}
	}
}
