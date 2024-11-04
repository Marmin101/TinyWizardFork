using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HeartsUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Transform HeartsGroup;
		[SerializeField, Required]
		private GameObject HeartPrefab;
		[SerializeField, Required]
		private Sprite FullHeart, HalfHeart, EmptyHeart;

		private void Start()
		{
			PlayerManager.Instance.OnPlayerHealthChange += OnHealthChange;
		}

		private void OnHealthChange(float delta)
		{
			UpdateHearts();
		}

		private void UpdateHearts()
		{
			int max = Mathf.RoundToInt(PlayerManager.Instance.Health.Max);
			int current = Mathf.RoundToInt(PlayerManager.Instance.Health.Current);

			for (int i = 0; i < HeartsGroup.childCount; i++)
			{
				var img = HeartsGroup.GetChild(i).GetComponent<Image>();
				img.sprite = i < current ? FullHeart : EmptyHeart;
			}
		}

		private void ReconstructHearts()
		{
			for (int i = 0; i < HeartsGroup.childCount; i++)
			{
				HeartsGroup.GetChild(i).gameObject.Destroy();
			}

			int max = Mathf.RoundToInt(PlayerManager.Instance.Health.Max);

			for (int i = 0; i < max; i++)
			{
				HeartPrefab.Clone(HeartsGroup);
			}
		}
	}
}
