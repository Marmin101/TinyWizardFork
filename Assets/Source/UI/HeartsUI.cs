using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HeartsUI : MonoBehaviour
	{
		[SerializeField, Required]
		private GameObject HeartPrefab;
		[SerializeField, Required]
		private Sprite FullHeart, HalfHeart, EmptyHeart;

		private void Start()
		{
			PlayerManager.Instance.OnPlayerHealthChange += OnHealthChange;
			PlayerManager.Instance.OnPlayerMaxHealthChange += OnMaxHealthChange;
		}

		private void OnDestroy()
		{
			if (PlayerManager.Instance != null)
			{
				PlayerManager.Instance.OnPlayerHealthChange -= OnHealthChange;
				PlayerManager.Instance.OnPlayerMaxHealthChange -= OnMaxHealthChange;
			}
		}

		private void OnHealthChange(float delta)
		{
			UpdateHearts();
		}

		private void OnMaxHealthChange()
		{
			ReconstructHearts();
			UpdateHearts();
		}

		private void UpdateHearts()
		{
			int current = Mathf.RoundToInt(PlayerManager.Instance.Health.Current);

			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);

				if (child != null)
				{
					var img = child.GetComponent<Image>();
					img.sprite = i < current ? FullHeart : EmptyHeart;
				}
			}
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
	}
}
