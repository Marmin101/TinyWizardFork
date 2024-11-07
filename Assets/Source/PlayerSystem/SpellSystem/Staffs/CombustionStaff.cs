using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class CombustionStaff : Staff
	{
		[SerializeField, Unit(Units.Second)]
		private float ChargeRate = 1f;
		[SerializeField, Unit(Units.Second)]
		private float MaxCharge = 4f;
		[SerializeField]
		private float BaseChargeToDamage = 51f;
		[SerializeField]
		private AnimationCurve ChargeToDamageFactor;
		[SerializeField]
		private float MinChargeThreshold = 1f;
		[SerializeField]
		private float DamageRadius = 1f;
		[SerializeField]
		private float BaseSparkCooldown = 0.3f;
		[SerializeField]
		private float CastCooldown = 0.1f;
		[SerializeField]
		private GameObject CombustionVFX;
		[SerializeField]
		private float VFXLifespan = 3f;
		[SerializeField]
		private EventReference SmallExplosionSound, MediumExplosionSound, LargeExplosionSound;

		private bool _isCharging;
		private float _charge;

		private void FixedUpdate()
		{
			if (_isCharging && Caster != null)
			{
				_charge += Time.fixedDeltaTime * ChargeRate;
				_charge = Mathf.Min(_charge, MaxCharge);

				float cooldown = BaseSparkCooldown;
				float percent = _charge / MaxCharge;
				if (percent > 0.3f) cooldown *= 0.4f;
				else if (percent > 0.8f) cooldown *= 0.1f;
				Cooldown.Call(this, BaseSparkCooldown, Caster.Spark);
			}
		}

		public override void OnBasicDown()
		{
			_isCharging = true;
		}

		public override void OnBasicUp()
		{
			Caster.SetCooldown(CastCooldown);

			if (_charge >= MinChargeThreshold)
			{
				Vector2 pos = CrosshairManager.Instance.Position;
				float chargePercent = (_charge - MinChargeThreshold) / (MaxCharge - MinChargeThreshold);

				var vfx = CombustionVFX.Clone(pos);
				vfx.Destroy(VFXLifespan);

				EventReference sound;
				if (chargePercent > 0.8f)
					sound = LargeExplosionSound;
				else if (chargePercent > 0.3f)
					sound = MediumExplosionSound;
				else
					sound = SmallExplosionSound;

				Audio.Play(sound, pos);

				foreach (var collider in Physics2D.OverlapCircleAll(pos, DamageRadius))
				{
					if (collider.TryGetComponent(out Health health) && health.Team != Team.Player)
					{
						float dmg = ChargeToDamageFactor.Evaluate(chargePercent);
						dmg *= BaseChargeToDamage;

						health.TakeDamage(dmg, pos.DirectionTo(health.transform.position), Team.Player, Caster.gameObject);
					}
				}
			}

			_isCharging = false;
			_charge = 0f;
		}
	}
}
