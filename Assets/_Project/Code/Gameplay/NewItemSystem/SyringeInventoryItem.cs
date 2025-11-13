using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.SyringeItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class SyringeInventoryItem : BaseInventoryItem
    {
        NetworkVariable<bool> IsUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private SyringeItemSO _syringeItemSo;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is SyringeItemSO syringeItemSO)
            {
                _syringeItemSo = syringeItemSO;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");
            // Now add flashlight-specific network setup
            CustomNetworkSpawn();
        }

        private void Update()
        {
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
        }

        #endregion

        #region UseLogic

        public override void UseItem()
        {
            base.UseItem();
            if (IsUsed.Value) return;
            if (IsOwner)
            {
                InjectSyringe();
            }
        }

        private void InjectSyringe()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            Debug.Log("InjectSyringe");
            RequestChangeIsUsedServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            IsUsed.Value = true;
        }

        #endregion
    }
}