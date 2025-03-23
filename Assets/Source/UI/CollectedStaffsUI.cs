﻿using DG.Tweening;
using Quinn.AI;
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

        private PlayerCaster _caster;
        private readonly List<TextMeshProUGUI> _staffs = new();
        private string _activeStaffGUID;

        public void Start()
        {
            foreach (var child in ListParent.GetChildren())
            {
                child.gameObject.Destroy();
            }

            _caster = PlayerManager.Instance.Player.GetComponent<PlayerCaster>();
        }

        public void FixedUpdate()
        { 
            if (_caster.EquippedStaff != null && _caster.EquippedStaff.GUID != _activeStaffGUID)
            {
                _activeStaffGUID = _caster.EquippedStaff.GUID;
                UpdateStaffs();
            }
        }

        private void UpdateStaffs()
        {
            _staffs.ForEach(staff => staff.gameObject.Destroy());
            _staffs.Clear();

            var manager = PlayerManager.Instance;

            if (!string.IsNullOrEmpty(manager.EquippedStaffGUID))
            {
                var equipped = CloneItem(isEquipped: true);
                equipped.text = Staffs.GetStaff(manager.EquippedStaffGUID).Name;
                //Changes the text for the item that comes next
                if (Staffs.GetStaff(manager.StoredStaffGUID).Name != null)
                {
                    equipped = CloneItem(isEquipped: false);

                    equipped.text = Staffs.GetStaff(manager.StoredStaffGUID).Name;
                }
                
            }
        }

        private TextMeshProUGUI CloneItem(bool isEquipped)
        {
            var prefab = isEquipped ? EquippedStaffPrefab : StoredStaffPrefab;

            var instance = prefab.Clone(ListParent).GetComponent<TextMeshProUGUI>();
            _staffs.Add(instance);

            return instance;
        }
    }
}
