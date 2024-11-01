using System;
using UnityEngine;

namespace Quinn
{
	public class InputManager : MonoBehaviour
	{
		public static InputManager Instance { get; private set; }

		public Vector2 MoveInput { get; private set; }
		public event Action OnDash;

		private void Awake()
		{
			Debug.Assert(Instance == null, "There are more than one instances of InputManager!");
			Instance = this;
		}

		private void Update()
		{
			MoveInput = new Vector2()
			{
				x = Input.GetAxisRaw("Horizontal"),
				y = Input.GetAxisRaw("Vertical")
			}.normalized;

			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
			{
				OnDash?.Invoke();
			}
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public void EnableInput() => enabled = true;
		public void DisableInput() => enabled = false;
	}
}
