using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class Room : MonoBehaviour
	{
		[SerializeField]
		private EventReference OpenSound, LockSound;
		[SerializeField, Required]
		private Trigger RoomTrigger;
		[SerializeField, Required]
		private CinemachineCamera RoomCamera;

		[SerializeField, BoxGroup("Doors"), ValidateInput("@HasNorthDoor || HasEastDoor || HasSouthDoor || HasWestDoor")]
		private Door NorthDoor, SouthDoor, EastDoor, WestDoor;

		public bool IsLocked { get; private set; }
		public bool IsConquered { get; private set; } = true; // TODO: Implement.

		public bool HasNorthDoor => NorthDoor != null;
		public bool HasEastDoor => EastDoor != null;
		public bool HasSouthDoor => SouthDoor != null;
		public bool HasWestDoor => WestDoor != null;

		public Vector2Int RoomGridIndex { get; set; }

		private readonly HashSet<Door> _doors = new();

		private void Awake()
		{
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

		public void Open()
		{
			if (!IsLocked)
				return;

			IsLocked = false;
			Audio.Play(OpenSound, transform.position);

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

		private void OnPlayerTriggerEnter(Collider2D collider)
		{
			if (collider.CompareTag("Player"))
			{
				RoomCamera.enabled = true;
				RoomCamera.Target.TrackingTarget = CameraManager.Instance.CameraTarget;

				if (!IsConquered)
				{
					Lock();
				}
			}
		}

		private void OnPlayerTriggerExit(Collider2D collider)
		{
			if (collider.CompareTag("Player"))
			{
				RoomCamera.enabled = false;
				GenerateRoomAtPlayer(collider);
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
	}
}
