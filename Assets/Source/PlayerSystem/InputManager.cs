using System;
using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class InputManager : MonoBehaviour
	{
		public static InputManager Instance { get; private set; }

		public Vector2 MoveDirection { get; private set; }
		public Vector2 CursorWorldPos { get; private set; }

		public event Action OnInteract;
		public event Action OnDash;
		public event Action OnCastStart, OnCastStop;
		public event Action OnSpecialStart, OnSpecialStop;

		private void Awake()
		{
			Debug.Assert(Instance == null, "There are more than one instances of InputManager!");
			Instance = this;

			//HideCursor();
		}

		private void Update()
		{
			MoveDirection = new Vector2()
			{
				x = Input.GetAxisRaw("Horizontal"),
				y = Input.GetAxisRaw("Vertical")
			}.normalized;

			CursorWorldPos = Camera.main.WorldToScreenPoint(Input.mousePosition);

			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
			{
				OnDash?.Invoke();
			}

			if (Input.GetMouseButtonDown(0))
			{
				OnCastStart?.Invoke();
			}
			else if (Input.GetMouseButtonUp(0))
			{
				OnCastStop?.Invoke();
			}

			if (Input.GetMouseButtonDown(1))
			{
				OnSpecialStart?.Invoke();
			}
			else if (Input.GetMouseButtonUp(1))
			{
				OnSpecialStop?.Invoke();
			}

			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
			{
				OnInteract?.Invoke();
			}
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public void EnableInput() => enabled = true;
		public void DisableInput() => enabled = false;

		public void ShowCursor()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

		public void HideCursor()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;
		}
	}
}
