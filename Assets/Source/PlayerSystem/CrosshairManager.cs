using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem
{
	public class CrosshairManager : MonoBehaviour
	{
		public static CrosshairManager Instance { get; private set; }

		[SerializeField, Required]
		private GameObject CrosshairPrefab;

		private Transform _crosshair;

		public Vector2 Position => Camera.main.ScreenToWorldPoint(Input.mousePosition);

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;

			_crosshair = CrosshairPrefab.Clone(transform).transform;
			GetComponent<InputManager>().HideCursor();
		}

		private void LateUpdate()
		{
			_crosshair.position = Position;
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public Vector2 DirectionToCrosshair(Vector2 from)
		{
			return from.DirectionTo(Position);
		}
	}
}
