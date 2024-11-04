using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.UI
{
	[RequireComponent(typeof(Canvas))]
	public class HUD : MonoBehaviour
	{
		[field: SerializeField, Required]
		public HeartsUI Hearts { get; private set; }
	}
}
