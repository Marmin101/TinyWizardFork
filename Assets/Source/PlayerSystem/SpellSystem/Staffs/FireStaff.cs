using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class FireStaff : Staff
	{
		[SerializeField]
		private float CastInterval = 0.2f;
		[SerializeField]
		private float CastChainCooldown = 0.6f;
		[SerializeField]
		private float CastKnockbackSpeed = 10f;

		[Space, SerializeField]
		private int FinalChainMissileCount = 3;
		[SerializeField]
		private float FinalChainMissileSpread = 45f;
		[SerializeField]
		private float ChainWindowDuration = 0.4f;
		[SerializeField]
		private float ChainFinalKnockbackSpeed = 14f;

		[Space, SerializeField]
		private float SpecialCooldown = 1f;
		[SerializeField]
		private float SpecialChargeTime = 1f;
		[SerializeField]
		private float SpecialKnockbackSpeed = 10f;
		[SerializeField]
		private float ChargingMoveSpeedFactor = 0.5f;

		[Space, SerializeField, Required]
		private Missile CastPrefab, SpecialPrefab;

		private float _largeMissileTime;
		private int _castChainCount;
		private float _chainTimeoutTime;

		private void Update()
		{
			if (_castChainCount < 3 && _castChainCount > 0 && Time.time > _chainTimeoutTime)
			{
				_castChainCount = 0;
				Caster.SetCooldown(CastChainCooldown);

			}
		}

		public override void OnCastStart()
		{
			_castChainCount++;

			if (_castChainCount >= 3)
			{
				Caster.SetCooldown(CastChainCooldown);
				_castChainCount = 0;

				MissileManager.Instance.SpawnMissile(CastPrefab, Head.position, GetDir(), FinalChainMissileCount, MissileSpawnBehavior.SpreadRandom, FinalChainMissileSpread);
				Caster.Movement.Knockback(-GetDir(), ChainFinalKnockbackSpeed);
			}
			else
			{
				Caster.SetCooldown(CastInterval);
				MissileManager.Instance.SpawnMissile(CastPrefab, Head.position, GetDir());

				_chainTimeoutTime = Time.time + ChainWindowDuration + CastInterval;
				Caster.Movement.Knockback(-GetDir(), CastKnockbackSpeed);
			}
		}

		public override void OnSpecialStart()
		{
			Caster.SetCooldown(SpecialCooldown);
			_largeMissileTime = Time.time + SpecialChargeTime;

			Caster.Movement.ApplySpeedModifier(this, ChargingMoveSpeedFactor);
		}

		public override void OnSpecialStop()
		{
			var prefab = Time.time > _largeMissileTime ? SpecialPrefab : CastPrefab;
			MissileManager.Instance.SpawnMissile(prefab, Head.position, GetDir());

			Caster.Movement.RemoveSpeedModifier(this);
			Caster.Movement.Knockback(-GetDir(), SpecialKnockbackSpeed);
		}

		private Vector2 GetDir()
		{
			return CrosshairManager.Instance.DirectionToCrosshair(Head.position);
		}
	}
}
