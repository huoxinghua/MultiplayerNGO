using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Buy;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Utilities.Singletons
{
    public class WalletBankton : NetworkSingleton<WalletBankton>
    {
        public NetworkVariable<int> TotalMoneyNW = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        protected override bool AutoSpawn => false;

        #region Initialization

        

   
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("WalletBankton.OnNetworkSpawn");
            if (IsServer)
            {
                // Force the object to NOT be destroyed when the scene unloads.
                NetworkObject.DestroyWithScene = false;
            }
            TotalMoneyNW.OnValueChanged += HandleMoneyChange;
        }
        #endregion

        #region Events

        public void HandleMoneyChange(int oldAmount, int newAmount)
        {
            EventBus.EventBus.Instance.Publish<WalletUpdate>(new WalletUpdate());
        }

        //need event and a purpose

        #endregion

        #region Money

        public void AddSubMoney(int amount)
        {
            RequestAddSubMoneyServerRpc(amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddSubMoneyServerRpc(int amount)
        {
            TotalMoneyNW.Value += amount;
        }

        #endregion

      
    }
}