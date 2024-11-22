using FMODUnity;
using Quinn.DungeonGeneration;
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
		[SerializeField]
		private EventReference DeathMusicCue;

		public static PlayerManager Instance { get; private set; }

		public Player Player { get; private set; }
		public Health Health { get; private set; }
		public bool IsAlive => !IsDead;
		public bool IsDead => Health == null || Health.IsDead;
		public Vector2 Position => Player.transform.position;

		public event Action<Player> OnPlayerSet;
		public event Action<float> OnPlayerHealthChange;
		public event Action OnPlayerMaxHealthChange;

		public event Action OnPlayerDeath;
		public event Action OnPlayerDeathPreSceneLoad;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		public void Start()
		{
			SpawnPlayer(InitialSpawnOffset);
		}

		public void OnDestroy()
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

		public void SpawnPlayer(Vector2 position)
		{
			var player = PlayerGroupPrefab.Clone(position);
			Health = player.GetComponentInChildren<Health>();

			var camManager = GetComponent<CameraManager>();
			camManager.Blackout();

			Health.OnHealed += OnHealed;
			Health.OnDamaged += OnDamaged;
			Health.OnDeath += OnDeath;
			Health.OnMaxChange += OnMaxHealthChange;

			InputManager.Instance.EnableInput();
		}

		public async void RespawnSequence()
		{
			await SceneManager.LoadSceneAsync(1);

			SpawnPlayer(InitialSpawnOffset);
			DungeonGenerator.Instance.StartRandomFloor();
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

			OnPlayerDeath?.Invoke();

			InputManager.Instance.DisableInput();
			Audio.Play(DeathMusicCue);

			await CameraManager.Instance.DeathFadeOut();
			OnPlayerDeathPreSceneLoad?.Invoke();

			RespawnSequence();
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
