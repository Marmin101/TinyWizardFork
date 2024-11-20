using Quinn.AI.BehaviorTree;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class BossUI : MonoBehaviour
	{
		[SerializeField, Required]
		private GameObject ElementsParent;
		[SerializeField, Required]
		private TextMeshProUGUI Title;
		[SerializeField, Required]
		private Slider HPBar;

		private BTAgent _boss;

		public void FixedUpdate()
		{
			if (_boss != null)
			{
				HPBar.value = _boss.Health.Percent;
			}
		}

		public void SetBoss(BTAgent agent)
		{
			_boss = agent;
			Title.text = agent.BossTitle;
			ElementsParent.SetActive(true);

			agent.Health.OnDeath += OnBossDeath;
		}

		private void OnBossDeath()
		{
			_boss = null;
			ElementsParent.SetActive(false);
		}
	}
}
