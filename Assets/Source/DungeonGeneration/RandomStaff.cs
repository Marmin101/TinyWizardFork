using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class RandomStaff : MonoBehaviour
	{
		[SerializeField, AssetsOnly]
		private GameObject[] Staffs;

		private void Awake()
		{
			Staffs.GetRandom().Clone(transform);
		}
	}
}
