using FMODUnity;
using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.PlayerSystem.SpellSystem.Staffs
{
	public class BasicStaff : Staff
	{
		[SerializeField, FoldoutGroup("SFX")]
		private EventReference BasicCastSound;
		[SerializeField, ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("SFX")]
		private EventReference BasicFinisherCastSound;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("SFX")]
		private EventReference SpecialCastSound;

		[SerializeField, Required, FoldoutGroup("Basic")]
		private Missile BasicMissile;
		[Space, SerializeField, FoldoutGroup("Basic")]
		private float CastCooldown = 0.2f;
		[SerializeField, FoldoutGroup("Basic")]
		private float CastChainCooldown = 0.6f;
		[SerializeField, FoldoutGroup("Basic")]
		private float CastKnockbackSpeed = 10f;
		[SerializeField, FoldoutGroup("Basic")]
		private MissileSpawnBehavior CastBehavior = MissileSpawnBehavior.Direct;
		[SerializeField, HideIf("@CastBehavior == MissileSpawnBehavior.Direct"), FoldoutGroup("Basic")]
		private float CastSpread = 0f;
		[SerializeField, FoldoutGroup("Basic")]
		private int CastCount = 1;
		[SerializeField, ShowIf("@CastCount > 1"), FoldoutGroup("Basic")]
		private float CastInterval = 0f;

		[Space, SerializeField, FoldoutGroup("Basic Finisher")]
		private bool HasFinalCastingChain = true;
		[SerializeField, ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("Basic Finisher")]
		private int FinalChainMissileCount = 3;
		[SerializeField, ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("Basic Finisher")]
		private MissileSpawnBehavior FinalChainBehavior = MissileSpawnBehavior.SpreadRandom;
		[SerializeField, HideIf("@FinalChainBehavior == MissileSpawnBehavior.Direct"), ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("Basic Finisher")]
		private float FinalChainMissileSpread = 45f;
		[SerializeField, ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("Basic Finisher")]
		private float ChainWindowDuration = 0.4f;
		[SerializeField, ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("Basic Finisher")]
		private float ChainFinalKnockbackSpeed = 14f;
		[SerializeField, ShowIf(nameof(HasFinalCastingChain)), FoldoutGroup("Basic Finisher")]
		[Tooltip("This can be null to use the basic normal missile.")]
		private Missile ChainFinalMissileOverride;

		[Space, SerializeField, FoldoutGroup("Special")]
		private bool HasSpecial = true;
		[SerializeField, Required, FoldoutGroup("Special")]
		private Missile SpecialMissile;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float SpecialCooldown = 1f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float SpecialChargeTime = 1f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float SpecialKnockbackSpeed = 10f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float ChargingMoveSpeedFactor = 0.5f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private int SpecialCount = 1;
		[SerializeField, ShowIf("@SpecialCount > 1"), ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float SpecialInterval = 0f;
		[SerializeField, ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private MissileSpawnBehavior SpecialBehavior = MissileSpawnBehavior.Direct;
		[SerializeField, HideIf("@SpecialBehavior == MissileSpawnBehavior.Direct"), ShowIf(nameof(HasSpecial)), FoldoutGroup("Special")]
		private float SpecialSpread = 0f;

		private float _largeMissileTime;
		private int _castChainCount;
		private float _chainTimeoutTime;

		private bool _isMovePenaltyApplied;

		private void FixedUpdate()
		{
			if (_castChainCount < FinalChainMissileCount && _castChainCount > 0 && Time.time > _chainTimeoutTime)
			{
				_castChainCount = 0;
				Caster.SetCooldown(CastChainCooldown);
			}

			if (_isMovePenaltyApplied && Time.time > _largeMissileTime)
			{
				_isMovePenaltyApplied = false;
				Caster.Movement.RemoveSpeedModifier(this);
			}
		}

		public override void OnCastStart()
		{
			_castChainCount++;

			// Finisher cast.
			if (_castChainCount >= FinalChainMissileCount && HasFinalCastingChain)
			{
				Caster.SetCooldown(CastChainCooldown);
				_castChainCount = 0;

				var missile = ChainFinalMissileOverride != null ? ChainFinalMissileOverride : BasicMissile;

				MissileManager.Instance.SpawnMissile(Caster.gameObject, missile, Head.position, GetDir(),
					FinalChainMissileCount, FinalChainBehavior, FinalChainMissileSpread);
				Caster.Movement.Knockback(-GetDir(), ChainFinalKnockbackSpeed);

				Audio.Play(BasicFinisherCastSound, Head.position);
			}
			// Normal cast.
			else
			{
				Caster.SetCooldown(CastCooldown);
				MissileManager.Instance.SpawnMissile(Caster.gameObject, BasicMissile, Head.position, GetDir(),
					CastCount, CastInterval, CastBehavior, CastSpread);

				_chainTimeoutTime = Time.time + ChainWindowDuration + CastCooldown;
				Caster.Movement.Knockback(-GetDir(), CastKnockbackSpeed);

				Audio.Play(BasicCastSound, Head.position);
			}
		}

		public override void OnSpecialStart()
		{
			if (!HasSpecial)
				return;

			Caster.SetCooldown(SpecialCooldown);
			_largeMissileTime = Time.time + SpecialChargeTime;

			Caster.Movement.ApplySpeedModifier(this, ChargingMoveSpeedFactor);
			_isMovePenaltyApplied = true;
			_castChainCount = 0;
		}

		public override void OnSpecialStop()
		{
			if (!HasSpecial)
				return;

			var prefab = Time.time > _largeMissileTime ? SpecialMissile : BasicMissile;
			MissileManager.Instance.SpawnMissile(Caster.gameObject, prefab, Head.position, GetDir(),
				SpecialCount, SpecialInterval, SpecialBehavior, SpecialSpread);

			Caster.Movement.Knockback(-GetDir(), SpecialKnockbackSpeed);
			Audio.Play(SpecialCastSound, Head.position);

			Caster.Movement.RemoveSpeedModifier(this);
		}

		private Vector2 GetDir()
		{
			return CrosshairManager.Instance.DirectionToCrosshair(Head.position);
		}
	}
}
