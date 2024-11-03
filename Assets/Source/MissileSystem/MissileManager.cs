using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.MissileSystem
{
	public class MissileManager : MonoBehaviour
	{
		[SerializeField, Required]
		private Missile TestingMissile;

		public static MissileManager Instance { get; private set; }

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private async void Start()
		{
			while (true)
			{
				await Awaitable.WaitForSecondsAsync(1f);
				SpawnMissile(TestingMissile, new(5f, 0f), new(-2f, 1f));
			}
		}

		private void OnDestroy()
		{
			if (Instance == null)
				Instance = null;
		}

		public void SpawnMissile(Missile prefab, Vector2 origin, Vector2 dir)
		{
			var instance = prefab.gameObject.Clone(origin);
			instance.GetComponent<Missile>().Initialize(dir);
		}
	}
}
