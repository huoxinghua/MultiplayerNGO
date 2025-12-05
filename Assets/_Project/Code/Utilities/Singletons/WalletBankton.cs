using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Buy;
using EventBus =  _Project.Code.Utilities.EventBus;
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