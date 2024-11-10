using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.PlayerSystem
{
	public class PlayerManager : MonoBehaviour
	{
		[SerializeField, Required, AssetsOnly]
		private GameObject PlayerGroupPrefab;
		[SerializeField]
		private Vector2 InitialSpawnOffset = new(-1f, -1f);

		public static PlayerManager Instance { get; private set; }

		public Player Player { get; private set; }
		public Health Health { get; private set; }
		public bool IsAlive => !IsDead;
		public bool IsDead => Health == null || Health.IsDead;
		public Vector2 Position => Player.transform.position;

		public event Action<Player> OnPlayerSet;
		public event Action<float> OnPlayerHealthChange;
		public event Action OnPlayerMaxHealthChange;

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private void Start()
		{
			SpawnPlayer(InitialSpawnOffset);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void SetPlayer(Player player)
		{
			Player = player;

			if (player == null)
			{
				UnsubscribeAll();
			}
			else
			{
				OnPlayerSet?.Invoke(player);
			}
		}

		public async void SpawnPlayer(Vector2 position)
		{
			var player = PlayerGroupPrefab.Clone(position);
			Health = player.GetComponentInChildren<Health>();

			var camManager = GetComponent<CameraManager>();
			camManager.EnableBlackout();

			Health.OnHealed += OnHealed;
			Health.OnDamaged += OnDamaged;
			Health.OnDeath += OnDeath;
			Health.OnMaxChange += OnMaxHealthChange;

			InputManager.Instance.EnableInput();
			await camManager.FadeIn();
		}

		private void OnHealed(float amount)
		{
			OnPlayerHealthChange?.Invoke(amount);
		}

		private void OnDamaged(float amount, Vector2 dir, GameObject source)
		{
			OnPlayerHealthChange?.Invoke(-amount);
		}

		private void OnMaxHealthChange()
		{
			OnPlayerMaxHealthChange?.Invoke();
		}

		private async void OnDeath()
		{
			UnsubscribeAll();

			Player = null;
			Health = null;

			InputManager.Instance.DisableInput();

			await CameraManager.Instance.FadeOut();
			await SceneManager.LoadSceneAsync(0);

			SpawnPlayer(new(-0.5f, -0.5f));
		}

		private void UnsubscribeAll()
		{
			if (Health != null)
			{
				Health.OnHealed -= OnHealed;
				Health.OnDamaged -= OnDamaged;
				Health.OnDeath -= OnDeath;
				Health.OnMaxChange -= OnMaxHealthChange;
			}
		}
	}
}
