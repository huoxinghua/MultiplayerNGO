using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.TwoManHandSawItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class TwoManHandSawInventoryItem : BaseInventoryItem
    {
        NetworkVariable<float> SawTimeAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        NetworkVariable<bool> PlayerCloseBy = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private TwoManHandSawItemSO _twoManHandSawItemSO;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is TwoManHandSawItemSO twoManHandSawItemSO)
            {
                _twoManHandSawItemSO = twoManHandSawItemSO;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");
            // Now add flashlight-specific network setup
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
            if (PlayerCloseBy.Value) return;
            if (IsOwner)
            {
                UseTwoManSaw();
            }
        }

        private void UseTwoManSaw()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            Debug.Log("UseTwoManSaw");
            RequestChangeIsUsedServerRpc();
            SawTimeAmount = new NetworkVariable<float>(_twoManHandSawItemSO.SawTimeAmount, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
            PlayerCloseBy = new NetworkVariable<bool>(_twoManHandSawItemSO.PlayerCloseBy, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            
        }

        #endregion
    }
}