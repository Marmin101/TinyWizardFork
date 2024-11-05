using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	[RequireComponent(typeof(Canvas))]
	public class HUD : MonoBehaviour
	{
		[SerializeField, Required]
		private TextMeshProUGUI Version;

		private void Awake()
		{
			Version.text = Application.version;
		}
	}
}
