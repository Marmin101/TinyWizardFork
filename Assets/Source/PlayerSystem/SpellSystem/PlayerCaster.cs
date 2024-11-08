using Sirenix.OdinInspector;
using System;
using Unity.VisualScripting;
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
		private Staff StartingStaff;
		[SerializeField]
		private float InputBufferTimeout = 0.2f;
		[SerializeField, Required]
		private VisualEffect CastingSpark;

		public Staff Staff {  get; private set; }
		public bool CanCast => Time.time >= _nextInputTime;

		public bool IsBasicHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		public PlayerMovement Movement { get; private set; }
		public event Action<Staff> OnStaffSet;

		private float _nextInputTime;
		private readonly BufferManager _inputBuffer = new();

		private void Awake()
		{
			Movement = GetComponent<PlayerMovement>();
			var input = InputManager.Instance;

			input.OnCastStart += OnBasicStart;
			input.OnSpecialStart += OnSpecialStart;

			if (StartingStaff != null)
			{
				GameObject staff = StartingStaff.gameObject.Clone();
				SetStaff(staff.GetComponent<Staff>());
			}
		}

		private void LateUpdate()
		{
			UpdateStaffTransform();
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
		}

		public void SetStaff(Staff staff)
		{
			if (Staff != null)
			{
				Staff.SetCaster(null);
				Staff.transform.SetParent(null);
			}

			Staff = staff;
			staff.transform.SetParent(transform, false);
			staff.SetCaster(this);

			CastingSpark.SetGradient("Color", staff.SparkGradient);
			CastingSpark.transform.SetParent(staff.Head, false);
			CastingSpark.transform.localPosition = Vector3.zero;

			OnStaffSet?.Invoke(staff);
		}

		public void SetCooldown(float duration)
		{
			_nextInputTime = Time.time + duration;
		}

		public void Spark()
		{
			CastingSpark.Play();
		}

		private void OnBasicStart()
		{
			if (Staff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsBasicHeld = true;
				Staff.OnBasicDown();
			}, () => CanCast && Input.GetMouseButton(0));
		}

		private void OnBasicStop()
		{
			if (Staff == null)
				return;

			if (IsBasicHeld)
			{
				IsBasicHeld = false;
				Staff.OnBasicUp();
			}
		}

		private void OnSpecialStart()
		{
			if (Staff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsSpecialHeld = true;
				Staff.OnSpecialDown();
			}, () => CanCast && Input.GetMouseButton(1));
		}

		private void OnSpecialStop()
		{
			if (Staff == null)
				return;

			if (IsSpecialHeld)
			{
				IsSpecialHeld = false;
				Staff.OnSpecialUp();
			}
		}

		private void UpdateStaffTransform()
		{
			if (Staff == null)
				return;

			Vector2 cursorPos = InputManager.Instance.CursorWorldPos;

			Vector3 dir = StaffPivot.position.DirectionTo(cursorPos);
			Staff.transform.position = StaffPivot.position + (dir * StaffOffset);

			float angle = Mathf.Atan2(dir.y, dir.x).ToDegrees();
			Staff.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			if (dir.x < 0f)
				Staff.transform.localScale = new Vector3(1f, -1f, 1f);
			else
				Staff.transform.localScale = Vector3.one;
		}
	}
}
