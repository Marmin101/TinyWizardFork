using UnityEngine;

namespace Quinn
{
	[ExecuteAlways]
	[RequireComponent(typeof(BoxCollider2D))]
	public class DrawDebugBox : MonoBehaviour
	{
		[SerializeField]
		private Color Color = Color.white;

		private BoxCollider2D _box;

		private void OnDrawGizmos()
		{
			if (_box == null)
			{
				_box = GetComponent<BoxCollider2D>();
			}

			Gizmos.color = Color;
			Gizmos.DrawWireCube(_box.bounds.center, _box.bounds.size);
		}
	}
}
