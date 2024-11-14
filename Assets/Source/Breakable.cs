using FMODUnity;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn
{
	public class Breakable : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private Sprite[] DestroyedSprites;
		[SerializeField]
		private VisualEffect[] VFX;
		[SerializeField]
		private EventReference DamageSound, DestroySound;
		[SerializeField]
		private float DestructionDelay = 5f;

		public Team Team => Team.Environment;

		private int _hits;

		public bool TakeDamage(DamageInfo info)
		{
			if (DestroyedSprites.Length > 0)
			{
				if (_hits >= DestroyedSprites.Length)
				{
					Death();
					return true;
				}
				else
				{
					Audio.Play(DamageSound, transform.position);
				}

				GetComponent<SpriteRenderer>().sprite = DestroyedSprites[_hits];
				_hits++;
			}
			else
			{
				Death();
			}

			return true;
		}

		private void Death()
		{

			GetComponent<SpriteRenderer>().enabled = false;
			GetComponent<Collider2D>().enabled = false;

			foreach (var vfx in VFX)
			{
				vfx.Play();
			}

			Audio.Play(DestroySound, transform.position);
			Destroy(gameObject, DestructionDelay);
		}
	}
}
