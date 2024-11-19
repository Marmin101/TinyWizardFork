using FMOD.Studio;
using FMODUnity;
using Quinn.AI;
using Quinn.AI.BehaviorTree;
using Quinn.AI.Pathfinding;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Quinn.DungeonGeneration
{
	public class Room : MonoBehaviour
	{
		[SerializeField]
		private EventReference LockSound, UnlockSound;

		[SerializeField, Required, Space]
		private Trigger RoomTrigger;
		[SerializeField, Required]
		private CinemachineCamera RoomCamera;
		[SerializeField, Required]
		private Tilemap Navmesh;
		[field: SerializeField, Required]
		public BoxCollider2D PathfindBounds { get; private set; }
		[SerializeField, Required]
		private Transform AgentsParent;

		[SerializeField, Space]
		private bool StartConquered;

		[SerializeField, BoxGroup("Doors")]
		private bool NoDoors;
		[SerializeField, BoxGroup("Doors"), ValidateInput("@ NoDoors || HasNorthDoor || HasEastDoor || HasSouthDoor || HasWestDoor")]
		private Door NorthDoor, SouthDoor, EastDoor, WestDoor;
		[SerializeField]
		private Chest Chest;
		[SerializeField]
		private float PostConquerDelay = 1f;

		[Space, SerializeField, HideIf(nameof(HasCustomMusic))]
		private bool DisableMusic;
		[SerializeField]
		private bool HasCustomMusic;
		[SerializeField, ShowIf(nameof(HasCustomMusic))]
		private EventReference CustomMusic;

		[Space, SerializeField]
		private Collider2D MissileBlocker;

		[Space, SerializeField]
		private FloorExit Exit;

		public bool IsLocked { get; private set; }
		public bool IsConquered { get; private set; }
		public bool IsStarted { get; private set; }

		public bool HasNorthDoor => NorthDoor != null || NoDoors;
		public bool HasEastDoor => EastDoor != null || NoDoors;
		public bool HasSouthDoor => SouthDoor != null || NoDoors;
		public bool HasWestDoor => WestDoor != null || NoDoors;

		public event System.Action OnRoomConquered;

		public Vector2Int RoomGridIndex { get; set; }

		private readonly HashSet<Door> _doors = new();
		private readonly HashSet<IAgent> _liveAgent = new();
		private readonly HashSet<StaticAgent> _staticAgents = new();

		private EventInstance _customMusic;

		public void Awake()
		{
			IsConquered = StartConquered;
			IsStarted = StartConquered;

			if (IsStarted && IsConquered)
			{
				RegisterAllAgents();
			}

			if (StartConquered && MissileBlocker != null)
			{
				Destroy(MissileBlocker.gameObject);
			}

			if (HasNorthDoor)
				_doors.Add(NorthDoor);
			if (HasSouthDoor)
				_doors.Add(SouthDoor);
			if (HasWestDoor)
				_doors.Add(WestDoor);
			if (HasEastDoor)
				_doors.Add(EastDoor);

			RoomTrigger.OnTriggerEnter += OnPlayerTriggerEnter;
			RoomTrigger.OnTriggerExit += OnPlayerTriggerExit;
		}

		public void Unlock()
		{
			if (!IsLocked)
				return;

			IsLocked = false;
			Audio.Play(UnlockSound, transform.position);

			foreach (var door in _doors)
			{
				door.Open();
			}
		}

		public void Lock()
		{
			if (IsLocked)
				return;

			IsLocked = true;
			Audio.Play(LockSound, transform.position);

			foreach (var door in _doors)
			{
				door.Close();
			}
		}

		public void RegisterAgent(IAgent agent)
		{
			agent.StartRoom(this);

			if (agent is MonoBehaviour mono && mono.TryGetComponent(out Health health))
			{
				_liveAgent.Add(agent);
				health.OnDeath += () => OnAgentDeath(agent);
			}
			else if (agent is StaticAgent staticAgent)
			{
				_staticAgents.Add(staticAgent);
			}
		}

		private void OnPlayerTriggerEnter(Collider2D collider)
		{
			if (collider.IsPlayer())
			{
				RoomCamera.enabled = true;
				RoomCamera.Target.TrackingTarget = CameraManager.Instance.CameraTarget;

				Pathfinder.Instance.SetNavmesh(Navmesh);
				RoomManager.Instance.SetActiveRoom(this);

				if (!IsConquered)
				{
					StartRoomEncounter();
				}

				if (DisableMusic || HasCustomMusic)
				{
					RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f);
				}

				if (HasCustomMusic)
				{
					_customMusic = RuntimeManager.CreateInstance(CustomMusic);
					_customMusic.start();
				}
			}
		}

		private void OnPlayerTriggerExit(Collider2D collider)
		{
			if (collider.IsPlayer())
			{
				RoomCamera.enabled = false;
				GenerateRoomAtPlayer(collider);

				if (DisableMusic || HasCustomMusic)
				{
					RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);
				}

				if (HasCustomMusic)
				{
					_customMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					_customMusic.release();
				}
			}
		}

		private void GenerateRoomAtPlayer(Collider2D collider)
		{
			var generator = DungeonGenerator.Instance;
			if (generator == null)
				return;

			Vector2 playerExitDir = transform.position.DirectionTo(collider.transform.position);
			if (playerExitDir.IsVertical())
			{
				if (playerExitDir.y > 0f)
					generator.GenerateRoomAt(RoomGridIndex.x, RoomGridIndex.y + 1);
				else
					generator.GenerateRoomAt(RoomGridIndex.x, RoomGridIndex.y - 1);
			}
			else
			{
				if (playerExitDir.x > 0f)
					generator.GenerateRoomAt(RoomGridIndex.x + 1, RoomGridIndex.y);
				else
					generator.GenerateRoomAt(RoomGridIndex.x - 1, RoomGridIndex.y);
			}
		}

		private void StartRoomEncounter()
		{
			IsStarted = true;
			Lock();

			if (MissileBlocker != null)
				Destroy(MissileBlocker.gameObject);

			RegisterAllAgents();
		}

		private async void OnAgentDeath(IAgent agent)
		{
			_liveAgent.Remove(agent);

			if (_liveAgent.Count == 0)
			{
				IsConquered = true;

				foreach (var staticAgent in _staticAgents)
				{
					staticAgent.CeaseFire();
				}

				OnRoomConquered?.Invoke();

				await Wait.Seconds(PostConquerDelay);

				if (Chest != null)
				{
					Chest.Unlock();
				}

				Unlock();
			}
		}

		private void RegisterAllAgents()
		{
			for (int i = 0; i < AgentsParent.childCount; i++)
			{
				Transform child = AgentsParent.GetChild(i);

				if (child.TryGetComponent(out IAgent agent))
				{
					RegisterAgent(agent);
				}
			}
		}
	}
}
