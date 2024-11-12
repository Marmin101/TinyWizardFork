using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.PlayerSystem
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Health))]
	public class PlayerMovement : Locomotion
	{
		[SerializeField]
		private float MoveSpeed = 6f;

		[SerializeField, BoxGroup("Dash")]
		private float DashSpeed = 12f;
		[SerializeField, BoxGroup("Dash")]
		private float DashDistance = 4f;
		[SerializeField, BoxGroup("Dash")]
		private float DashCooldown = 0.2f;
		[SerializeField, BoxGroup("Dash")]
		private EventReference DashSound;

		public bool IsDashing { get; private set; }
		public bool CanDash { get; set; } = true;

		private Animator _animator;
		private Health _health;

		private float _nextDashTime;
		private float _dashEndTime;
		private Vector2 _dashDir = Vector2.down;

		protected override void Awake()
		{
			base.Awake();

			_animator = GetComponent<Animator>();
			_health = GetComponent<Health>();

			InputManager.Instance.OnDash += OnDash;
		}

		private void Update()
		{
			float scale = Rigidbody.linearVelocity.magnitude / MoveSpeed;
			if (IsDashing) scale = 1f;
			_animator.SetFloat("SpeedScale", scale);
		}

		private void OnDestroy()
		{
			if (InputManager.Instance != null)
				InputManager.Instance.OnDash -= OnDash;
		}

		public override Vector2 GetVelocity()
		{
			Vector2 vel = Vector2.zero;
			Vector2 moveDir = InputManager.Instance.MoveDirection;

			if (IsDashing)
			{
				vel += _dashDir * DashSpeed;

				if (Time.time > _dashEndTime)
				{
					IsDashing = false;
					OnDashStop();
				}
			}
			else
			{
				vel += MoveSpeed * moveDir;

				if (moveDir.sqrMagnitude > 0f)
				{
					_dashDir = moveDir;
				}
			}

			return vel;
		}

		private void OnDash()
		{
			if (CanDash && !IsDashing && Time.time > _nextDashTime)
			{
				IsDashing = true;
				_health.BlockDamage(this);

				Audio.Play(DashSound, transform.position);

				float dashDur = DashDistance / DashSpeed;
				_dashEndTime = Time.time + dashDur;

				_nextDashTime = Time.time + DashCooldown;
			}
		}

		private void OnDashStop()
		{
			_health.UnblockDamage(this);
		}
	}
}
