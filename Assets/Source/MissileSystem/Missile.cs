using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.MissileSystem
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Missile : MonoBehaviour
	{
		[SerializeField, BoxGroup("Core")]
		private float DirectSpeed = 8f;
		[SerializeField, BoxGroup("Core")]
		private float DirectDamage = 1f;
		[SerializeField, BoxGroup("Core")]
		private float Lifespan = 10f;

		[SerializeField/*, FoldoutGroup("Splash Damage")*/]
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

		private Rigidbody2D _rb;

		private float _endLifeTime;
		private Vector2 _velocity;
		private Vector2 _baseDir;

		private void Awake()
		{
			_rb = GetComponent<Rigidbody2D>();
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
				health.TakeDamage(DirectDamage);
				OnImpact();
			}
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
			{
				OnImpact();
			}
		}

		public void Initialize(Vector2 dir)
		{
			_baseDir = dir.normalized;
			_endLifeTime = Time.time + Lifespan;
		}

		private void OnImpact()
		{
			TriggerSplash();
			Destroy(gameObject);
		}

		private void OnLifespanEnd()
		{
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
						health.TakeDamage(SplashDamageFalloff.Evaluate(dst / SplashRadius) * BaseSplashDamage);
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

			Vector2 oscDir = new Vector2(-_baseDir.y, _baseDir.x);
			return Mathf.Sin(Time.time * OscillateFrequency) * OscillateAmplitude * oscDir;
		}
	}
}
