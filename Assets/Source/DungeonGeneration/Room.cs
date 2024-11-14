using FMODUnity;
using Quinn.AI;
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
		private bool DisableMusic;
		[SerializeField]
		private float PostConquerDelay = 1f;

		[Space, SerializeField]
		private Collider2D MissileBlocker;

		public bool IsLocked { get; private set; }
		public bool IsConquered { get; private set; }
		public bool HasStarted { get; private set; }

		public bool HasNorthDoor => NorthDoor != null || NoDoors;
		public bool HasEastDoor => EastDoor != null || NoDoors;
		public bool HasSouthDoor => SouthDoor != null || NoDoors;
		public bool HasWestDoor => WestDoor != null || NoDoors;

		public event System.Action OnRoomConquered;

		public Vector2Int RoomGridIndex { get; set; }

		private readonly HashSet<Door> _doors = new();
		private readonly HashSet<AIAgent> _liveAgents = new();
		private readonly HashSet<StaticAgent> _staticAgents = new();

		public void Awake()
		{
			IsConquered = StartConquered;

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

			if (agent is AIAgent liveAgent)
			{
				_liveAgents.Add(liveAgent);
				liveAgent.Health.OnDeath += () => OnAgentDeath(liveAgent);
			}
			else if (agent is StaticAgent staticAgent)
			{
				_staticAgents.Add(staticAgent);
			}
		}

		private void OnPlayerTriggerEnter(Collider2D collider)
		{
			if (collider.CompareTag("Player"))
			{
				RoomCamera.enabled = true;
				RoomCamera.Target.TrackingTarget = CameraManager.Instance.CameraTarget;

				Pathfinder.Instance.SetNavmesh(Navmesh);
				RoomManager.Instance.SetActiveRoom(this);

				if (!IsConquered)
				{
					StartRoomEncounter();
				}

				if (DisableMusic)
				{
					RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f);
				}
			}
		}

		private void OnPlayerTriggerExit(Collider2D collider)
		{
			if (collider.CompareTag("Player"))
			{
				RoomCamera.enabled = false;
				GenerateRoomAtPlayer(collider);

				if (DisableMusic)
				{
					RuntimeManager.StudioSystem.setParameterByName("enable-music", 1f);
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
			HasStarted = true;
			Lock();

			if (MissileBlocker != null)
				Destroy(MissileBlocker.gameObject);

			for (int i = 0; i < AgentsParent.childCount; i++)
			{
				Transform child = AgentsParent.GetChild(i);

				if (child.TryGetComponent(out IAgent agent))
				{
					RegisterAgent(agent);
				}
			}
		}

		private async void OnAgentDeath(AIAgent agent)
		{
			_liveAgents.Remove(agent);

			if (_liveAgents.Count == 0)
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
	}
}
