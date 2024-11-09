using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.MissileSystem
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Missile : MonoBehaviour
	{
		[SerializeField, BoxGroup("FX")]
		private EventReference SpawnSound, HitSound, FizzleOutSound;

		[SerializeField, BoxGroup("Core")]
		private float DirectSpeed = 8f;
		[SerializeField, BoxGroup("Core")]
		private float DirectDamage = 1f;
		[SerializeField, BoxGroup("Core")]
		private Team Team = Team.Monster;
		[SerializeField, BoxGroup("Core"), Unit(Units.Second)]
		private float Lifespan = 10f;
		[SerializeField]
		private bool UsesCustomKnockbackSpeed;
		[SerializeField, ShowIf(nameof(UsesCustomKnockbackSpeed))]
		private float CustomKnockbackSpeed;

		[SerializeField, BoxGroup("Core"), Space]
		private GameObject SpawnOnDeath;
		[SerializeField, BoxGroup("Core"), ShowIf(nameof(SpawnOnDeath))]
		private float DestroySpawnedDelay = 3f;

		[SerializeField, BoxGroup("Core"), Space]
		private VisualEffect[] DelayDestructionOnDeath;
		[Space, SerializeField, BoxGroup("Core"), ShowIf("@DelayDestructionOnDeath.Length > 0"), Unit(Units.Second)]
		private float DestructionDelay = 3f;

		[SerializeField]
		private bool HasSplashDamage;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private float BaseSplashDamage = 1f;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private float SplashRadius = 1f;
		[SerializeField, FoldoutGroup("Splash Damage"), ShowIf(nameof(HasSplashDamage))]
		private AnimationCurve SplashDamageFalloff;

		[SerializeField]
		private bool DoesOscillate;
		[SerializeField, FoldoutGroup("Oscillate"), ShowIf(nameof(DoesOscillate))]
		private float OscillateAmplitude = 0.5f;
		[SerializeField, FoldoutGroup("Oscillate"), ShowIf(nameof(DoesOscillate))]
		private float OscillateFrequency = 0.5f;
		[SerializeField, FoldoutGroup("Oscillate"), ShowIf(nameof(DoesOscillate))]
		private bool RandomizeOscillation;

		private Rigidbody2D _rb;
		private GameObject _owner;

		private float _endLifeTime;
		private Vector2 _velocity;
		private Vector2 _baseDir;

		private float _oscillateOffset;

		private void Awake()
		{
			_rb = GetComponent<Rigidbody2D>();
			_oscillateOffset = RandomizeOscillation ? Random.value : 0f;
		}

		private void FixedUpdate()
		{
			_velocity = Vector2.zero;

			_velocity += UpdateDirect();
			_velocity += UpdateOscillate();

			_rb.linearVelocity = _velocity;

			if (Time.time > _endLifeTime)
			{
				OnLifespanEnd();
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.TryGetComponent(out Health health))
			{
				float? knockbackSpeed = null;
				if (UsesCustomKnockbackSpeed)
				{
					knockbackSpeed = CustomKnockbackSpeed;
				}

				if (health.TakeDamage(DirectDamage, _rb.linearVelocity.normalized, Team, _owner, knockbackSpeed))
					OnImpact();
			}
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
			{
				OnImpact();
			}
		}

		public void Initialize(Vector2 dir, GameObject owner)
		{
			_baseDir = dir.normalized;
			_endLifeTime = Time.time + Lifespan;
			_owner = owner;

			Audio.Play(SpawnSound);
		}

		private void OnImpact()
		{
			Audio.Play(HitSound, transform.position);

			OnDeath();
			TriggerSplash();
			Destroy(gameObject);
		}

		private void OnLifespanEnd()
		{
			Audio.Play(FizzleOutSound, transform.position);

			OnDeath();
			TriggerSplash();
			Destroy(gameObject);
		}

		private void TriggerSplash()
		{
			if (HasSplashDamage)
			{
				var colliders = Physics2D.OverlapCircleAll(transform.position, SplashRadius);
				foreach (var collider in colliders)
				{
					if (collider.TryGetComponent(out Health health))
					{
						float dst = transform.position.DistanceTo(collider.transform.position);
						float dmg = SplashDamageFalloff.Evaluate(dst / SplashRadius) * BaseSplashDamage;
						health.TakeDamage(dmg, transform.position.DirectionTo(collider.transform.position), Team, gameObject);
					}
				}
			}
		}

		private Vector2 UpdateDirect()
		{
			return _baseDir * DirectSpeed;
		}

		private Vector2 UpdateOscillate()
		{
			if (!DoesOscillate)
				return Vector2.zero;

			float time = Time.time + _oscillateOffset;

			Vector2 oscDir = new Vector2(-_baseDir.y, _baseDir.x);
			return Mathf.Sin(time * OscillateFrequency) * OscillateAmplitude * oscDir;
		}

		private void OnDeath()
		{
			if (SpawnOnDeath != null)
			{
				var instance = SpawnOnDeath.Clone(transform.position);
				Destroy(instance, DestroySpawnedDelay);
			}

			foreach (var obj in DelayDestructionOnDeath)
			{
				obj.transform.SetParent(null, true);
				obj.gameObject.Destroy(DestructionDelay);
			}
		}
	}
}
