using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Quinn.UI
{
	public class LeaderboardUI : MonoBehaviour
	{
		[SerializeField, Required]
		private TMP_InputField NameField;
		[SerializeField, Required]
		private TextMeshProUGUI Stats, Score, Total;
		[SerializeField, Required]
		private Button SubmitButton;
		[SerializeField, Required]
		private Transform LeaderboardContent;
		[SerializeField, Required]
		private GameObject ScorePrefab;

		[SerializeField]
		private float StaffScore = 100f, PathScore = 200f;
		[SerializeField]
		private float DurationBaseScore = 100f, DurationPenalty = 1f;

		private const string LeaderboardID = "score";
		private bool _isSubmitted;

		private float _score;

		public void Awake()
		{
			UpdateLeaderobardUI();

			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			CrosshairManager.Instance.Hide();
			Time.timeScale = 1f;

			int staffCount = PlayerManager.Instance.DiscoveredStaffCount;
			int pathCount = PlayerManager.Instance.PathsFound;
			float duration = PlayerManager.Instance.GameplayDuration;

			Stats.text =
				$"{staffCount} Staffs Collected\n" +
				$"{pathCount} Paths Discovered\n" +
				$"{duration:0}s Start to Finish";

			float staffScore = StaffScore * staffCount;
			float pathScore = PathScore * pathCount;
			float timeScore = DurationBaseScore - (DurationPenalty * duration);

			Score.text = 
				$"{staffCount}x Staffs = {GetSign(staffScore)}{staffScore:0}\n" +
				$"{pathCount}x Paths = {GetSign(pathScore)}{pathScore:0}\n" +
				$"{duration:0}s Time = {GetSign(timeScore, true)}{timeScore:0}";

			_score = staffScore + pathScore + timeScore;
			Total.text = $"Total Score: {_score:0}!";
		}

		public async void SubmitScore_Button()
		{
			if (string.IsNullOrWhiteSpace(NameField.text))
			{
				return;
			}

			if (!_isSubmitted)
			{
				_isSubmitted = true;

				SubmitButton.interactable = false;
				NameField.interactable = false;
			}

			await AuthenticationService.Instance.UpdatePlayerNameAsync(NameField.text);

			await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardID, _score);

			UpdateLeaderobardUI();
		}

		private string GetSign(float value, bool ignoreNegative = false)
		{
			string sign = string.Empty;
			if (value > 0f) sign = "+";
			else if (value < 0f && !ignoreNegative) sign = "-";

			return sign;
		}

		private async void UpdateLeaderobardUI()
		{
			LeaderboardContent.DestroyChildren();

			var page = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardID);
			var entries = page.Results;

			int index = 1;
			foreach (var entry in entries)
			{
				string name = entry.PlayerName[..entry.PlayerName.IndexOf('#')];
				double score = entry.Score;

				var tm = ScorePrefab.Clone(LeaderboardContent).GetComponent<TextMeshProUGUI>();
				tm.text = $"{index}. [{score:0}] {name}";

				index++;
			}
		}
	}
}
