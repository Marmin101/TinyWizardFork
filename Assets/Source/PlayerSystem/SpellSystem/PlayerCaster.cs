using Sirenix.OdinInspector;
using System;
using Unity.VisualScripting;
using UnityEngine;

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
		private Staff TestingStaff;
		[SerializeField]
		private float InputBufferTimeout = 0.2f;

		public Staff Staff {  get; private set; }
		public bool CanInput => Time.time >= _nextInputTime;

		public bool IsCastHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		public PlayerMovement Movement { get; private set; }

		private float _nextInputTime;
		private readonly BufferManager _inputBuffer = new();

		private void Awake()
		{
			Movement = GetComponent<PlayerMovement>();
			var input = InputManager.Instance;

			input.OnCastStart += OnCastStart;
			input.OnSpecialStart += OnSpecialStart;

			if (TestingStaff != null)
			{
				GameObject staff = TestingStaff.gameObject.Clone();
				SetStaff(staff.GetComponent<Staff>());
			}
		}

		private void LateUpdate()
		{
			UpdateStaffTransform();
			_inputBuffer.Update();

			if (!Input.GetMouseButton(0))
			{
				OnCastStop();
			}

			if (!Input.GetMouseButton(1))
			{
				OnSpecialStop();
			}
		}

		public void SetStaff(Staff staff)
		{
			Staff = staff;
			staff.transform.SetParent(transform, false);
			staff.SetCaster(this);
		}

		public void SetCooldown(float duration)
		{
			_nextInputTime = Time.time + duration;
		}

		private void OnCastStart()
		{
			if (Staff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsCastHeld = true;
				Staff.OnCastStart();
			}, () => CanInput && Input.GetMouseButton(0));
		}

		private void OnCastStop()
		{
			if (Staff == null)
				return;

			if (IsCastHeld)
			{
				IsCastHeld = false;
				Staff.OnCastStop();
			}
		}

		private void OnSpecialStart()
		{
			if (Staff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsSpecialHeld = true;
				Staff.OnSpecialStart();
			}, () => CanInput && Input.GetMouseButton(1));
		}

		private void OnSpecialStop()
		{
			if (Staff == null)
				return;

			if (IsSpecialHeld)
			{
				IsSpecialHeld = false;
				Staff.OnSpecialStop();
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
