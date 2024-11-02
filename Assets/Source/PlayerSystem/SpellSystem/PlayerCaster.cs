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
		}

		private void Update()
		{
			UpdateStaffTransform();
		}

		public void SetStaff(Staff staff)
		{
			Staff = staff;
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

			Draw.Rect(cursorPos, Vector2.one * 0.5f, Color.yellow);
		}
	}
}
