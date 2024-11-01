using UnityEngine;

namespace Quinn
{
	public class Global : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Bootstrap()
		{
			var asset = Resources.Load<GameObject>("Globals");
			var instance = Instantiate(asset);

			DontDestroyOnLoad(instance);
		}
	}
}
