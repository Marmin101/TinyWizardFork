using Quinn.AI;
using UnityEngine;

namespace Quinn
{
	public class Player : MonoBehaviour
	{
		private void Start()
		{
			AIGlobal.Instance.SetPlayer(this);
		}

		private void OnDestroy()
		{
			AIGlobal.Instance.SetPlayer(null);
		}
	}
}
