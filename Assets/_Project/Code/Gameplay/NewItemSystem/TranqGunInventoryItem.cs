using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.TranqGunItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class TranqGunInventoryItem : BaseInventoryItem
    {
        NetworkVariable<bool> IsUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private TranqGunItemSO _tranqGunItemSO;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is TranqGunItemSO tranqGunItemSO)
            {
                _tranqGunItemSO = tranqGunItemSO;
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
                ShootGun();
            }
        }

        private void ShootGun()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            Debug.Log("ShootGun");
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


