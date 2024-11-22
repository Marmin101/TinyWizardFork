using FMODUnity;
using Quinn.DungeonGeneration;
using Quinn.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.AI.BehaviorTree
{
	[RequireComponent(typeof(BehaviorGraphAgent))]
	[RequireComponent(typeof(AIMovement))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(Animator))]
	public class BTAgent : MonoBehaviour, IAgent
	{
		[SerializeField]
		private EventReference FootstepSound;
		[SerializeField]
		private string DeathTrigger = "Die";
		[SerializeField]
		private VisualEffect DeathVFX;

		[field: SerializeField, FoldoutGroup("Boss")]
		public bool IsBoss { get; private set; }
		[field: SerializeField, FoldoutGroup("Boss"), ShowIf(nameof(IsBoss))]
		public string BossTitle { get; private set; } = "Boss Title";

		public Animator Animator { get; private set; }
		public Health Health { get; private set; }
		public AIMovement Movement { get; private set; }
		public Room Room { get; private set; }

		private readonly HashSet<string> _setAnimTriggers = new();

		public void Awake()
		{
			Animator = GetComponent<Animator>();
			Health = GetComponent<Health>();
			Movement = GetComponent<AIMovement>();

			Health.OnDeath += OnDeath;
		}

		public void FixedUpdate()
		{
			if (IsBoss)
			{
				float phase = Health.Percent <= 0.5f ? 1f : 0f;
				RuntimeManager.StudioSystem.setParameterByName("second-phase", phase);
			}
		}

		public void StartRoom(Room room)
		{
			Room = room;

			if (IsBoss)
			{
				BossUI.Instance.SetBoss(this);
			}
		}

		public void SetAnimationTrigger(string key)
		{
			_setAnimTriggers.Add(key);
		}

		public bool GetAnimationTrigger(string key)
		{
			bool isSet = _setAnimTriggers.Contains(key);
			_setAnimTriggers.Remove(key);

			return isSet;
		}

		public void OnFootstep_Anim()
		{
			PlayFootstepSound(GetSoundMaterialType());
		}

		private SoundMaterialType GetSoundMaterialType()
		{
			SoundMaterialType mat = SoundMaterialType.None;
			int highestPriority = -9999;

			var colliders = Physics2D.OverlapPointAll(transform.position);

			foreach (var collider in colliders)
			{
				if (collider.TryGetComponent(out SoundMaterial soundMat))
				{
					if (soundMat.Priority > highestPriority)
					{
						mat = soundMat.Material;
						highestPriority = soundMat.Priority;
					}
				}
			}

			return mat;
		}

		private void PlayFootstepSound(SoundMaterialType mat)
		{
			var instance = RuntimeManager.CreateInstance(FootstepSound);
			instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
			instance.setParameterByNameWithLabel("sound-mat", mat.ToString());

			instance.start();
			instance.release();
		}

		private void OnDeath()
		{
			Animator.SetTrigger(DeathTrigger);

			if (DeathVFX != null)
			{
				DeathVFX.Play();
			}

			if (IsBoss)
			{
				Room.KillAlLiveAgents();
				DungeonGenerator.Instance.IncrementFloorIndex();
			}
		}
	}
}
