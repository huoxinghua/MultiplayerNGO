using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.HandSawItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class HandSawInventoryItem : BaseInventoryItem
    {
        NetworkVariable<bool> IsUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private HandSawItemSO _handSawItemSO;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is HandSawItemSO handSawItemSO)
            {
                _handSawItemSO = handSawItemSO;
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
                UseSaw();
            }
        }

        private void UseSaw()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            Debug.Log("UseSaw");
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