using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.JarItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class JarItem : BaseInventoryItem
    {
        NetworkVariable<bool> HasCollected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        NetworkVariable<float> CollectedAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private JarItemSO _jarItemSO;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is JarItemSO jarItemSO)
            {
                _jarItemSO = jarItemSO;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // Now add flashlight-specific network setup
      
            HasCollected = new NetworkVariable<bool>(_jarItemSO.HasCollected, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
            CollectedAmount = new NetworkVariable<float>(_jarItemSO.CollectedAmount, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
        }

        private void Update()
        {
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
        }

        #endregion

        #region UseLogic

        protected override bool CanUse()
        {
            if (HasCollected.Value) return false;
            return base.CanUse();
        }

        protected override void ExecuteUsageLogic()
        {
            if (IsOwner)
            {
                UseJar();
            }
        }

        private void UseJar()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            RequestChangeIsUsedServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            HasCollected.Value = true;
        }

        #endregion
    }
}