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
        NetworkVariable<float> SpeedBoostAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        NetworkVariable<float> EffectDuration = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone,
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
            
            CustomNetworkSpawn();
            IsUsed = new NetworkVariable<bool>(_syringeItemSo.IsUsed, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
            SpeedBoostAmount = new NetworkVariable<float>(_syringeItemSo.SpeedBoostAmount, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
            EffectDuration = new NetworkVariable<float>(_syringeItemSo.EffectDuration, NetworkVariableReadPermission.Everyone,
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
            if (IsUsed.Value) return;
            if (IsOwner)
            {
                InjectSyringe();
            }
            base.UseItem();
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