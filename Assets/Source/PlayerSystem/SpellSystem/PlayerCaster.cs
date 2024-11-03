using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem
{
	public class PlayerCaster : MonoBehaviour
	{
		[SerializeField, Required]
		private Transform StaffPivot;
		[SerializeField]
		private float StaffOffset = 0.5f;
		[SerializeField]
		private Staff TestingStaff;

		public Staff Staff {  get; private set; }
		public bool CanInput => Time.time >= _nextInputTime;

		public bool IsCastHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		private float _nextInputTime;

		private void Awake()
		{
			var input = InputManager.Instance;

			input.OnCastStart += OnCastStart;
			input.OnCastStart += OnCastStop;

			input.OnSpecialStart += OnSpecialStart;
			input.OnSpecialStart += OnSpecialStop;

			if (TestingStaff != null)
			{
				GameObject staff = TestingStaff.gameObject.Clone();
				SetStaff(staff.GetComponent<Staff>());
			}
		}

		private void Update()
		{
			UpdateStaffTransform();
		}

		public void SetStaff(Staff staff)
		{
			Staff = staff;
			staff.SetCaster(this);
		}

		public void SetCooldown(float duration)
		{
			_nextInputTime = Time.time + duration;
		}

		private void OnCastStart()
		{
			IsCastHeld = true;
		}

		private void OnCastStop()
		{
			IsCastHeld = false;
		}

		private void OnSpecialStart()
		{
			IsSpecialHeld = true;
		}

		private void OnSpecialStop()
		{
			IsSpecialHeld = false;
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
