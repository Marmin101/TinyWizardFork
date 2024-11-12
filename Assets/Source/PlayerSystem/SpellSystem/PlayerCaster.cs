using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(PlayerMovement))]
	public class PlayerCaster : MonoBehaviour
	{
		[SerializeField, Required]
		private Transform StaffPivot;
		[SerializeField]
		private float StaffOffset = 0.5f;
		[SerializeField]
		private float InputBufferTimeout = 0.2f;
		[SerializeField, Required]
		private VisualEffect CastingSpark;

		[SerializeField, Required, Space]
		private Transform StoredStaffsParent;
		[SerializeField]
		private float StaffMaxAngle = 30f;

		[Space, SerializeField]
		private Staff FallbackStaff;
		[SerializeField, FoldoutGroup("Starting Staffs")]
		private Staff[] StartingStaffs;

		[field: Space, SerializeField]
		public float MaxMana { get; private set; } = 100f;
		[SerializeField]
		private float ManaRegenDelay = 1f;
		[SerializeField, InfoBox("@(MaxMana / ManaRegenRate).ToString() + 's'")]
		private float ManaRegenRate = 50f;

		public Staff ActiveStaff {  get; private set; }
		public bool CanCast => Time.time >= _nextInputTime;

		public float Mana { get; private set; }

		public bool IsBasicHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		public PlayerMovement Movement { get; private set; }
		public event Action<Staff> OnStaffEquipped;

		private float _nextInputTime;
		private readonly BufferManager _inputBuffer = new();
		private readonly List<Staff> _storedStaffs = new();

		private float _manaRegenStartTime;

		private void Awake()
		{
			Movement = GetComponent<PlayerMovement>();
			var input = InputManager.Instance;

			input.OnCastStart += OnBasicStart;
			input.OnSpecialStart += OnSpecialStart;

			Debug.Assert(StartingStaffs.Length > 0);
			GameObject staff = StartingStaffs.GetRandom().gameObject.Clone();
			EquipStaff(staff.GetComponent<Staff>());

			StoreStaff(FallbackStaff);
			Mana = MaxMana;
		}

		private void Update()
		{
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Alpha0) && ActiveStaff != null && ActiveStaff.Energy > 0f)
			{
				ActiveStaff.ConsumeAllEnergy();
			}
#endif

			if (ActiveStaff != null && ActiveStaff.Energy <= 0f)
			{
				StoreStaff(ActiveStaff);
				EquipStaff(FallbackStaff);
			}

			if (Time.time > _manaRegenStartTime && Mana < MaxMana && (ActiveStaff == null || ActiveStaff.CanRegenMana))
			{
				Mana = Mathf.Min(Mana + (Time.deltaTime * ManaRegenRate), MaxMana);
			}
		}

		private void LateUpdate()
		{
			UpdateStaffTransform();

			if (PlayerManager.Instance.IsDead)
				return;

			_inputBuffer.Update();

			if (Input.GetMouseButton(0) && !IsBasicHeld && CanCast)
			{
				OnBasicStart();
			}

			if (Input.GetMouseButton(1) && !IsSpecialHeld && CanCast)
			{
				OnSpecialStart();
			}

			if (!Input.GetMouseButton(0))
			{
				OnBasicStop();
			}

			if (!Input.GetMouseButton(1))
			{
				OnSpecialStop();
			}

			// Make player face crosshair.
			var dirToCross = transform.position.DirectionTo(CrosshairManager.Instance.Position);
			transform.localScale = new Vector3(Mathf.Sign(dirToCross.x), 1f, 1f);
		}

		private void OnDestroy()
		{
			var input = InputManager.Instance;

			if (input != null)
			{
				input.OnCastStart -= OnBasicStart;
				input.OnSpecialStart -= OnSpecialStart;
			}
		}

		public void EquipStaff(Staff staff)
		{
			if (staff != ActiveStaff)
			{
				DequipActiveStaff();

				_storedStaffs.Remove(staff);

				ActiveStaff = staff;
				staff.transform.SetParent(transform, false);
				staff.SetCaster(this);

				CastingSpark.SetGradient("Color", staff.SparkGradient);
				CastingSpark.transform.SetParent(staff.Head, false);
				CastingSpark.transform.localPosition = Vector3.zero;

				FullyReplenishMana();
				OnStaffEquipped?.Invoke(staff);
			}
		}

		public void SetCooldown(float duration)
		{
			_nextInputTime = Time.time + duration;
		}

		public void Spark()
		{
			CastingSpark.Play();
		}

		public void StoreStaff(Staff staff)
		{
			if (!_storedStaffs.Contains(staff))
			{
				_storedStaffs.Add(staff);

				if (ActiveStaff == staff)
				{
					DequipActiveStaff();
				}

				staff.transform.SetParent(StoredStaffsParent, false);
				LayoutStoredStaffs();

				staff.DisableInteraction();
			}
		}

		public void LayoutStoredStaffs()
		{
			float delta = StaffMaxAngle / _storedStaffs.Count;

			for (int i = 0; i < _storedStaffs.Count; i++)
			{
				float angle = delta * i;
				angle -= StaffMaxAngle / 2f;

				var storedStaff = _storedStaffs[i];
				storedStaff.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.forward));
				storedStaff.transform.localScale = Vector3.one;
			}
		}

		public void DequipActiveStaff()
		{
			if (ActiveStaff != null)
			{
				var staff = ActiveStaff;

				staff.SetCaster(null);
				staff.transform.SetParent(null);

				ActiveStaff = null;

				IsBasicHeld = false;
				IsSpecialHeld = false;

				StoreStaff(staff);
			}
		}

		public void ConsumeMana(float amount)
		{
			if (Mana == 0f)
				return;

			Mana = Mathf.Max(0f, Mana - amount);

			if (amount > 0f)
			{
				_manaRegenStartTime = Time.time + ManaRegenDelay;
			}
		}

		public void ReplenishMana(float amount)
		{
			Mana = Mathf.Min(Mana + amount, MaxMana);
		}

		public void FullyReplenishMana()
		{
			ReplenishMana(MaxMana);
		}

		private void OnBasicStart()
		{
			if (ActiveStaff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsBasicHeld = true;
				ActiveStaff.OnBasicDown();
			}, () => CanCast && InputManager.Instance.IsCastHeld);
		}

		private void OnBasicStop()
		{
			if (ActiveStaff == null)
				return;

			if (IsBasicHeld)
			{
				IsBasicHeld = false;
				ActiveStaff.OnBasicUp();
			}
		}

		private void OnSpecialStart()
		{
			if (ActiveStaff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsSpecialHeld = true;
				ActiveStaff.OnSpecialDown();
			}, () => CanCast && InputManager.Instance.IsSpecialHeld);
		}

		private void OnSpecialStop()
		{
			if (ActiveStaff == null)
				return;

			if (IsSpecialHeld)
			{
				IsSpecialHeld = false;
				ActiveStaff.OnSpecialUp();
			}
		}

		private void UpdateStaffTransform()
		{
			if (ActiveStaff == null)
				return;

			ActiveStaff.gameObject.SetActive(PlayerManager.Instance.IsAlive);

			Vector2 cursorPos = InputManager.Instance.CursorWorldPos;

			Vector3 dir = StaffPivot.position.DirectionTo(cursorPos);
			ActiveStaff.transform.position = StaffPivot.position + (dir * StaffOffset);

			float angle = Mathf.Atan2(dir.y, dir.x).ToDegrees();
			ActiveStaff.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			if (dir.x < 0f)
				ActiveStaff.transform.localScale = new Vector3(1f, -1f, 1f);
			else
				ActiveStaff.transform.localScale = Vector3.one;
		}
	}
}
