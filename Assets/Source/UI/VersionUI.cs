using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	public class VersionUI : MonoBehaviour
	{
		private void Awake()
		{
			GetComponent<TextMeshProUGUI>().text = Application.version;
		}
	}
}
