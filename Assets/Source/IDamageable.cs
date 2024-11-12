using UnityEngine;

namespace Quinn
{
	public interface IDamageable
	{
		public Team Team { get; }

		public bool TakeDamage(float damage, Vector2 dir, Team sourceTeam, GameObject source, float? customKnockback = null)
		{
			var info = new DamageInfo()
			{
				Damage = damage,
				Direction = dir,
				SourceTeam = sourceTeam,
				Source = source,
				UsesCustomKnockbackSpeed = customKnockback.HasValue
			};

			if (customKnockback.HasValue)
			{
				info.CustomKnockbackSpeed = customKnockback.Value;
			}

			return TakeDamage(info);
		}
		public bool TakeDamage(DamageInfo info);
	}
}
