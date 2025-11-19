using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.HandSawItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class HandSawInventoryItem : BaseInventoryItem
    {
        NetworkVariable<float> SawTimeAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        NetworkVariable<bool> BeingUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
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
           
            SawTimeAmount = new NetworkVariable<float>(_handSawItemSO.SawTimeAmount, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
            BeingUsed = new NetworkVariable<bool>(_handSawItemSO.BeingUsed, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
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
            if (IsOwner)
            {
                UseSaw();
            }
            base.UseItem();
        }

        private void UseSaw()
        {
            Debug.Log("UseSaw");
            RequestChangeIsUsedServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            
        }

        #endregion
    }
}