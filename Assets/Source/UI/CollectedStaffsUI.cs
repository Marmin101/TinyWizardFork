using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	public class CollectedStaffsUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Transform ListParent;
		[SerializeField, Required]
		private GameObject EquippedStaffPrefab, StoredStaffPrefab;

		[Space, SerializeField]
		private StaffsSO Staffs;

		private TextMeshProUGUI _equippedStaff;
		private readonly List<TextMeshProUGUI> _staffs = new();

		public void Start()
		{
			var caster = PlayerManager.Instance.Player.GetComponent<PlayerCaster>();
			caster.OnStaffEquipped += OnStaffEquipped;

			foreach (var child in ListParent.GetChildren())
			{
				child.gameObject.Destroy();
			}

			if (caster.ActiveStaff != null)
			{
				AddStaff(caster.ActiveStaff);
			}
		}

		private void OnStaffEquipped(Staff staff)
		{
			AddStaff(staff);
		}

		private void UpdateStaffs()
		{

		}

		private void AddStaff(Staff staff)
		{
			GameObject instance;

			if (_equippedStaff != null)
			{
				if (staff.Name == "Fallback Staff")
				{ 
					// TODO: Do not store but do equip fallback staff.
				}

				// TODO: Reference saved staffs in player manager.

				instance = StoredStaffPrefab.Clone(ListParent);
				instance.transform.SetAsFirstSibling();

				var stored = instance.GetComponent<TextMeshProUGUI>();
				stored.text = _equippedStaff.text;

				_staffs.Add(stored);
				_equippedStaff.gameObject.Destroy();

				_equippedStaff = null;
			}

			instance = EquippedStaffPrefab.Clone(ListParent);
			instance.transform.SetAsFirstSibling();

			_equippedStaff = instance.GetComponent<TextMeshProUGUI>();
			_equippedStaff.text = staff.Name;
		}
	}
}
