using Quinn.PlayerSystem;
using UnityEngine;

namespace Quinn
{
	public class HealingPool : MonoBehaviour
	{
		[SerializeField]
		private float HealInterval = 1f;
		[SerializeField]
		private float HealAmount = 1f;
		[SerializeField]
		private float SpeedFactor = 0.3f;

		private float _nextHealTime;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				_nextHealTime = Time.time + HealInterval;
				PlayerManager.Instance.Movement.ApplySpeedModifier(this, SpeedFactor);
				PlayerManager.Instance.Player.EnablePuddleMask();
			}
		}

		public void OnTriggerStay2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				if (Time.time > _nextHealTime)
				{
					var hp = PlayerManager.Instance.Health;
					hp.Heal(HealAmount);

					_nextHealTime = Time.time + HealInterval;
				}
			}
		}

		public void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				PlayerManager.Instance.Movement.RemoveSpeedModifier(this);
				PlayerManager.Instance.Player.DisablePuddleMask();
			}
		}
	}
}
