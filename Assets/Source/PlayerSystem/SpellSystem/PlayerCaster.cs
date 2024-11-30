using DG.Tweening;
using FMODUnity;
using Quinn.UI;
using Quinn.UnityServices;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Quinn.PlayerSystem.SpellSystem
{
	[RequireComponent(typeof(PlayerMovement))]
	public class PlayerCaster : MonoBehaviour
	{
		[SerializeField, Required]
		private Transform StaffPivot;
		[SerializeField]
		private float StaffOffset = 0.5f;
		[SerializeField]
		private float InputBufferTimeout = 0.2f;
		[SerializeField, Required]
		private VisualEffect CastingSpark;
		[SerializeField]
		private float EnergyTransferRate = 0.5f;

		[SerializeField, Required, Space]
		private Transform StoredStaffsParent;
		[SerializeField, Unit(Units.Degree)]
		private float StaffMaxAngle = 30f;

		[Space, SerializeField]
		private Staff FallbackStaff;
		[SerializeField]
		private Staff[] AllStaffs, StartingStaffs;

		[field: Space, SerializeField]
		public float MaxMana { get; private set; } = 100f;
		[SerializeField]
		private float ManaRegenDelay = 1f;
		[SerializeField, InfoBox("@(MaxMana / ManaRegenRate).ToString() + 's'")]
		private float ManaRegenRate = 50f;

		[Space, SerializeField, Required]
		private Light2D EquipLight;
		[SerializeField, Required]
		private VisualEffect EquipVFX, EquipEnergyVFX;
		[SerializeField]
		private EventReference EquipSound;

		public Staff ActiveStaff {  get; private set; }
		// Used by EnergyUI.cs to update energy value during equipping sequence before ActiveStaff is changed.
		public Staff UIStaff { get; private set; }
		public bool CanCast => Time.time >= _nextInputTime;

		public float Mana { get; private set; }

		public bool IsBasicHeld { get; private set; }
		public bool IsSpecialHeld { get; private set; }

		public float Charge { get; private set; }

		public PlayerMovement Movement { get; private set; }
		public event Action<Staff> OnStaffEquipped;

		public event Action<float> OnManaAdded, OnManaRemoved;

		private float _nextInputTime;
		private readonly BufferManager _inputBuffer = new();
		private readonly List<Staff> _storedStaffs = new();

		private float _manaRegenStartTime;

		public void Awake()
		{
			Movement = GetComponent<PlayerMovement>();
			var input = InputManager.Instance;

			input.OnCastStart += OnBasicStart;
			input.OnSpecialStart += OnSpecialStart;

			if (!string.IsNullOrEmpty(PlayerManager.Instance.EquippedStaffGUID))
			{
				GameObject instance = StaffGUIDToPrefab(PlayerManager.Instance.EquippedStaffGUID).gameObject.Clone();
				Staff staff = instance.GetComponent<Staff>();

				EquipStaff(staff, true, true, supressAnalytics: true);
				staff.SetCaster(this);
				staff.SetEnergy(PlayerManager.Instance.EquippedStaffEnergy);

				foreach (string guid in PlayerManager.Instance.StoredStaffGUIDs)
				{
					instance = StaffGUIDToPrefab(guid).gameObject.Clone();
					staff = instance.GetComponent<Staff>();

					StoreStaff(staff);
					staff.SetCaster(this);
				}
			}
			else
			{
				Debug.Assert(StartingStaffs.Length > 0);
				GameObject staff = StartingStaffs.GetRandom().gameObject.Clone();
				EquipStaff(staff.GetComponent<Staff>(), true, true);
			}

			StoreStaff(FallbackStaff);
			Mana = MaxMana;
		}

		public void Update()
		{
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Alpha0) && ActiveStaff != null && ActiveStaff.Energy > 0f)
			{
				ActiveStaff.ConsumeAllEnergy();
			}

			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				if (ActiveStaff)
				{
					CastingSpark.transform.SetParent(null);
					Destroy(ActiveStaff.gameObject);
					ActiveStaff = null;
				}

				GameObject staff = AllStaffs.Where(x => ActiveStaff == null || x.GUID != ActiveStaff.GUID).GetRandom().gameObject.Clone(StaffPivot);
				EquipStaff(staff.GetComponent<Staff>(), true);
			}
#endif

			if (PauseMenuUI.Instance.IsPaused)
				return;

			if (ActiveStaff != null && ActiveStaff.Energy <= 0f)
			{
				StoreStaff(ActiveStaff);
				EquipStaff(FallbackStaff, true);
			}

			if (Time.time > _manaRegenStartTime && Mana < MaxMana && (ActiveStaff == null || ActiveStaff.CanRegenMana))
			{
				Mana = Mathf.Min(Mana + (Time.deltaTime * ManaRegenRate), MaxMana);
			}
		}

		public void LateUpdate()
		{
			UpdateStaffTransform();
			CrosshairManager.Instance.SetCharge(Charge);

			if (PlayerManager.Instance.IsDead)
				return;

			_inputBuffer.Update();

			if (Input.GetMouseButton(0) && !IsBasicHeld && CanCast)
			{
				OnBasicStart();
			}

			if (Input.GetMouseButton(1) && !IsSpecialHeld && CanCast)
			{
				OnSpecialStart();
			}

			if (!Input.GetMouseButton(0))
			{
				OnBasicStop();
			}

			if (!Input.GetMouseButton(1))
			{
				OnSpecialStop();
			}

			// Make player face crosshair.
			Vector2 faceDir = transform.position.DirectionTo(CrosshairManager.Instance.Position);

			// Instead make player face dash direction.
			if (Movement.IsDashing)
			{
				// Disabling this feature.
				//faceDir = Movement.DashDirection.normalized;
			}

			if (!PauseMenuUI.Instance.IsPaused)
			{
				// Do not change facing direction if input is disabled as it's a form of input.
				if (InputManager.Instance.enabled)
					transform.localScale = new Vector3(Mathf.Sign(faceDir.x), 1f, 1f);
			}
		}

		public void OnDestroy()
		{
			if (ActiveStaff != null && PlayerManager.Instance != null)
			{
				PlayerManager.Instance.EquippedStaffGUID = ActiveStaff.GUID;
				PlayerManager.Instance.EquippedStaffEnergy = ActiveStaff.Energy;

				PlayerManager.Instance.StoredStaffGUIDs = _storedStaffs
					.Select(staff => staff.GUID)
					.Where(guid => guid != FallbackStaff.GUID)
					.ToArray();
			}

			var input = InputManager.Instance;

			if (input != null)
			{
				input.OnCastStart -= OnBasicStart;
				input.OnSpecialStart -= OnSpecialStart;
			}
		}

		/// <param name="staff">The staff being equipped. This must be an instance not a prefab.</param>
		/// <param name="skipSequence">Skip the rising up animation with FX and such when a player takes a staff from a chest for instance.</param>
		/// <param name="supressNonSequenceSound">If skip sequence is false and this is true and then an equip sound will play on staff equip.</param>
		public async void EquipStaff(Staff staff, bool skipSequence = false, bool supressNonSequenceSound = false, bool supressAnalytics = false)
		{
			if (staff != ActiveStaff)
			{
				if (!supressAnalytics && staff.GUID != FallbackStaff.GUID)
				{
					Analytics.Instance.Push(new UnityServices.Events.StaffEquipEvent()
					{
						Name = staff.gameObject.name
					});
				}

				staff.OnEnergyDepleted += OnStaffEnergyDepleted;

				// Skip sequence if requested or if current staff is fallback staff.
				if (!skipSequence && (ActiveStaff != FallbackStaff))
				{
					await StaffPickUpSequence(staff);
				}
				// Avoid equip sound. The equip sound is otherwise played in the StaffPickUpSequence(Staff) method.
				else if (!supressNonSequenceSound)
				{
					Audio.Play(EquipSound, transform.position);
				}

				DequipActiveStaff();

				// If the staff being equipped was on the player's back; e.g. the fallback staff.
				_storedStaffs.Remove(staff);

				ActiveStaff = staff;
				UIStaff = staff;

				staff.transform.SetParent(transform, false);
				staff.SetCaster(this);

				FullyReplenishMana();
				SetCharge(0f);

				CastingSpark.SetGradient("Color", staff.SparkGradient);
				CastingSpark.transform.SetParent(staff.Head, false);
				CastingSpark.transform.localPosition = Vector3.zero;

				OnStaffEquipped?.Invoke(staff);
			}
		}

		public void StoreStaff(Staff staff)
		{
			if (!_storedStaffs.Contains(staff))
			{
				_storedStaffs.Add(staff);
				staff.OnEnergyDepleted -= OnStaffEnergyDepleted;

				if (ActiveStaff == staff)
				{
					DequipActiveStaff();
				}

				staff.transform.SetParent(StoredStaffsParent, false);
				LayoutStoredStaffs();

				staff.DisableInteraction();
			}
		}

		public void SetCooldown(float duration)
		{
			_nextInputTime = Time.time + duration;
		}

		public void Spark()
		{
			Debug.Assert(CastingSpark != null, "Casting spark has been destroyed! Make sure to detach it before destroying the equipped staff.");
			CastingSpark.Play();
		}

		public void LayoutStoredStaffs()
		{
			float delta = StaffMaxAngle / _storedStaffs.Count;

			for (int i = 0; i < _storedStaffs.Count; i++)
			{
				float angle = delta * i;
				angle -= StaffMaxAngle / 2f;

				var storedStaff = _storedStaffs[i];
				storedStaff.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.forward));
				storedStaff.transform.localScale = Vector3.one;
			}
		}

		public void DequipActiveStaff()
		{
			if (ActiveStaff != null)
			{
				var previousStaff = ActiveStaff;

				previousStaff.SetCaster(null);
				previousStaff.transform.SetParent(null);

				ActiveStaff = null;

				IsBasicHeld = false;
				IsSpecialHeld = false;

				StoreStaff(previousStaff);
			}
		}

		public void ConsumeMana(float amount)
		{
			if (Mana == 0f)
				return;

			Mana = Mathf.Max(0f, Mana - amount);

			if (amount > 0f)
			{
				_manaRegenStartTime = Time.time + ManaRegenDelay;
			}

			OnManaRemoved?.Invoke(amount);
		}

		public void ReplenishMana(float amount)
		{
			if (Mana >= MaxMana)
				return;

			Mana = Mathf.Min(Mana + amount, MaxMana);
			OnManaAdded?.Invoke(amount);
		}

		public void FullyReplenishMana()
		{
			ReplenishMana(MaxMana);
		}

		public void SetCharge(float percent)
		{
			Charge = percent;
		}

		private Staff StaffGUIDToPrefab(string guid)
		{
			var staff = AllStaffs.FirstOrDefault(x => x.GUID == guid);
			Debug.Assert(staff != null, "Failed to find staff. Make sure staff is in AllStaffs array in PlayerCaster!");

			return staff;
		}

		private void OnBasicStart()
		{
			if (ActiveStaff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsBasicHeld = true;
				ActiveStaff.OnBasicDown();
			}, () => CanCast && InputManager.Instance.IsCastHeld);
		}

		private void OnBasicStop()
		{
			if (ActiveStaff == null)
				return;

			if (IsBasicHeld)
			{
				IsBasicHeld = false;
				ActiveStaff.OnBasicUp();
			}
		}

		private void OnSpecialStart()
		{
			if (ActiveStaff == null)
				return;

			_inputBuffer.Buffer(InputBufferTimeout, () =>
			{
				IsSpecialHeld = true;
				ActiveStaff.OnSpecialDown();
			}, () => CanCast && InputManager.Instance.IsSpecialHeld);
		}

		private void OnSpecialStop()
		{
			if (ActiveStaff == null)
				return;

			if (IsSpecialHeld)
			{
				IsSpecialHeld = false;
				ActiveStaff.OnSpecialUp();
			}
		}

		private void UpdateStaffTransform()
		{
			if (ActiveStaff == null)
				return;

			ActiveStaff.gameObject.SetActive(PlayerManager.Instance.IsAlive);

			Vector2 cursorPos = InputManager.Instance.CursorWorldPos;

			Vector3 dir = StaffPivot.position.DirectionTo(cursorPos);
			ActiveStaff.transform.position = StaffPivot.position + (dir * StaffOffset);

			float angle = Mathf.Atan2(dir.y, dir.x).ToDegrees();
			ActiveStaff.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			if (dir.x < 0f)
				ActiveStaff.transform.localScale = new Vector3(1f, -1f, 1f);
			else
				ActiveStaff.transform.localScale = Vector3.one;
		}

		private async Awaitable StaffPickUpSequence(Staff targetStaff)
		{
			float energyToTransfer = ActiveStaff.Energy * EnergyTransferRate;

			InputManager.Instance.DisableInput();
			var hp = GetComponent<Health>();
			hp.BlockDamage(this);

			EquipLight.enabled = true;
			EquipLight.DOFade(0f, 0.5f).From();

			GetComponent<Animator>().SetTrigger("EquipSequence");

			UIStaff = targetStaff;
			DOTween.To(() => targetStaff.Energy, x => targetStaff.SetEnergy(x), targetStaff.Energy + energyToTransfer, 4f)
				.SetEase(Ease.OutCubic);

			Transform oldStaffHead = ActiveStaff.Head;
			Vector3 oldStaffHeadPos = oldStaffHead.position;
			Transform newStaffHead = targetStaff.Head;

			targetStaff.transform.DORotate(Vector3.zero, 3f);
			var tween = targetStaff.transform.DOMove(transform.position + (Vector3.up * 3f), 3f)
				.SetEase(Ease.OutCubic);

			tween.onUpdate += () =>
			{
				UpdateEnergyVFX(oldStaffHead, newStaffHead);
			};

			EquipEnergyVFX.SetGradient("Color", ActiveStaff.SparkGradient);
			EquipEnergyVFX.Play();

			await Wait.Seconds(4f);

			EquipVFX.Play();
			Audio.Play(EquipSound, transform.position);

			await EquipLight.DOFade(0f, 0.2f).AsyncWaitForCompletion();
			EquipLight.enabled = false;
			EquipEnergyVFX.Stop();

			InputManager.Instance.EnableInput();
			hp.UnblockDamage(this);
		}

		private void UpdateEnergyVFX(Transform start, Transform end)
		{
			Transform oldStaffHead = start;
			Transform newStaffHead = end;

			EquipEnergyVFX.SetVector2("Start", oldStaffHead.position);
			EquipEnergyVFX.SetVector2("End", newStaffHead.position);

			Vector2 startBias = Vector2.Lerp(oldStaffHead.position, newStaffHead.position, 0.15f);
			Vector2 endBias = Vector2.Lerp(oldStaffHead.position, newStaffHead.position, 0.85f);

			Vector2 diff = newStaffHead.position - oldStaffHead.position;
			Vector2 perp = new(-diff.x, diff.y);

			startBias += perp;
			endBias += perp;

			EquipEnergyVFX.SetVector2("StartBias", startBias);
			EquipEnergyVFX.SetVector2("EndBias", endBias);
		}

		private void OnStaffEnergyDepleted(Staff staff)
		{
			var p = PlayerManager.Instance;

			if (p.EquippedStaffGUID == staff.GUID)
			{
				p.EquippedStaffGUID = string.Empty;
			}
		}
	}
}
