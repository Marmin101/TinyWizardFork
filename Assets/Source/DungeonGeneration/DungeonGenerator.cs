using FMOD.Studio;
using FMODUnity;
using Quinn.PlayerSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class DungeonGenerator : MonoBehaviour
	{
		enum DoorCriteria
		{
			None,
			Required,
			Banned
		}

		record RoomCriteria
		{
			public DoorCriteria North, South, East, West;

			public bool IsMatch(Room room)
			{
				// Require.
				if (North is DoorCriteria.Required && !room.HasNorthDoor) return false;
				if (South is DoorCriteria.Required && !room.HasSouthDoor) return false;
				if (East is DoorCriteria.Required && !room.HasEastDoor) return false;
				if (West is DoorCriteria.Required && !room.HasWestDoor) return false;

				// Ban.
				if (North is DoorCriteria.Banned && room.HasNorthDoor) return false;
				if (South is DoorCriteria.Banned && room.HasSouthDoor) return false;
				if (East is DoorCriteria.Banned && room.HasEastDoor) return false;
				if (West is DoorCriteria.Banned && room.HasWestDoor) return false;

				return true;
			}

			public override string ToString()
			{
				return $"N {North}, S {South}, E {East}, W {West}";
			}
		}

		[SerializeField]
		private int MaxRoomSize = 48;

		[SerializeField, RequiredListLength(MinLength = 1)]
		private FloorSO[] Floors;

		public static DungeonGenerator Instance { get; private set; }

		public FloorSO ActiveFloor { get; private set; }

		public event System.Action<FloorSO> OnFloorStart;

		private readonly Dictionary<Vector2Int, GameObject> _generatedRooms = new();
		private EventInstance _ambience, _music;

		private int _floorIndex;

		public void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		public async void Start()
		{
			PlayerManager.Instance.OnPlayerDeath += OnPlayerDeath;
			PlayerManager.Instance.OnPlayerDeathPreSceneLoad += OnPlayerDeathPreSceneLoad;

			await StartFloorAsync(Floors[0]);
		}

#if UNITY_EDITOR
		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.B))
			{
				var player = PlayerManager.Instance.Player;
				Vector2 pos = player.transform.position;

				var room = GameObject.FindGameObjectsWithTag("Room").FirstOrDefault(x => x.GetComponent<Room>().IsBossRoom);
				if (room != null)
				{
					pos = room.transform.position;
					pos += Vector2.down * 4f;
				}

				player.transform.position = pos;
			}
		}
#endif

		public void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public async void StartFloorOfCurrentIndex()
		{
			await StartFloorAsync(Floors[_floorIndex]);
		}

		public void IncrementFloorIndex()
		{
			_floorIndex++;

			if (_floorIndex >= Floors.Length)
			{
				Debug.Log("Game Finished!");
			}
		}

		public void SetFloorIndex(int i)
		{
			_floorIndex = Mathf.Min(i, Floors.Length - 1);
		}

		private async Awaitable StartFloorAsync(FloorSO floor)
		{
			UnityServices.Analytics.Instance.Push(new UnityServices.Events.DiscoveredFloorEvent()
			{
				Name = floor.name
			});

			CameraManager.Instance.Blackout();
			RuntimeManager.StudioSystem.setParameterByName("reverb", floor.Reverb);

			DestroyAllRooms();
			ActiveFloor = floor;

			// Ambience.
			if (_ambience.isValid())
			{
				_ambience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}

			if (!floor.Ambience.IsNull)
			{
				_ambience = RuntimeManager.CreateInstance(floor.Ambience);
				_ambience.start();
			}

			// Music.
			if (_music.isValid())
			{
				_music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}

			if (!floor.Music.IsNull)
			{
				RuntimeManager.StudioSystem.setParameterByName("enable-music", 0f, ignoreseekspeed: true);

				_music = RuntimeManager.CreateInstance(floor.Music);
				_music.start();
			}

			await floor.GetVariant().CloneAsync();
			OnFloorStart?.Invoke(floor);

			var fade = CameraManager.Instance.FadeIn();

			if (!floor.SkipDropSequence)
			{
				var player = PlayerManager.Instance.Player;

				if (floor.AmbientVFX != null)
				{
					player.SetAmbientVFX(floor.AmbientVFX);
				}
				else
				{
					player.ClearAmbientVFX();
				}

				await player.EnterFloorAsync();
			}

			await fade;
		}

		private void DestroyAllRooms()
		{
			foreach (var room in _generatedRooms)
			{
				Destroy(room.Value);
			}

			_generatedRooms.Clear();
		}

		private void OnPlayerDeath()
		{
			_music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_music.release();
		}

		private void OnPlayerDeathPreSceneLoad()
		{
			DestroyAllRooms();
		}
	}
}
