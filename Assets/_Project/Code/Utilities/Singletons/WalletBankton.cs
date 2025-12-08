using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Buy;
using _Project.Code.Gameplay.Market.Quota;
using EventBus =  _Project.Code.Utilities.EventBus;
using Unity.Netcode;

using UnityEngine;

namespace _Project.Code.Utilities.Singletons
{
    public class WalletBankton : NetworkSingleton<WalletBankton>
    {
        public NetworkVariable<int> TotalMoneyNW = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> DaysMoneyNW = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        private int _startMoney;

        protected override bool AutoSpawn => false;

        #region Initialization

        

   
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            TotalMoneyNW.OnValueChanged += HandleMoneyChange;
            _startMoney = TotalMoneyNW.Value;
            EventBus.EventBus.Instance.Subscribe<QuotaFailedEvent>(this, HandleFailedQuota);
            EventBus.EventBus.Instance.Subscribe<SuccessfulDayEvent>(this, HandleSuccesfulDay);
            EventBus.EventBus.Instance.Subscribe<OnEnterHubEvent>(this, HandleEnteredHub);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            EventBus.EventBus.Instance.Unsubscribe<QuotaFailedEvent>(this);
            EventBus.EventBus.Instance.Unsubscribe<SuccessfulDayEvent>(this);
            EventBus.EventBus.Instance.Unsubscribe<OnEnterHubEvent>(this);
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

        private void HandleEnteredHub(OnEnterHubEvent e)
        {
            RequestClearDayServerRpc();
        }

        private void HandleFailedQuota(QuotaFailedEvent e)
        {
            RequestResetTotalMoneyServerRpc();
            RequestClearDayServerRpc();
        }

        private void HandleSuccesfulDay(SuccessfulDayEvent e)
        {
            RequestAddSubMoneyServerRpc(DaysMoneyNW.Value);
            RequestClearDayServerRpc();
        }
        public void AddToDaysProgress(int amount)
        {
            RequestAddToDayServerRpc(amount);
        }
        public void AddSubMoney(int amount)
        {
            RequestAddSubMoneyServerRpc(amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddSubMoneyServerRpc(int amount)
        {
            TotalMoneyNW.Value += amount;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetTotalMoneyServerRpc()
        {
            TotalMoneyNW.Value = _startMoney;
        }
        [ServerRpc(RequireOwnership = false)]
        public void RequestAddToDayServerRpc(int amount)
        {
            DaysMoneyNW.Value += amount;
        }
        [ServerRpc(RequireOwnership = false)]
        public void RequestClearDayServerRpc()
        {
            DaysMoneyNW.Value = 0;
        }

        #endregion

        #region Save System

        public int GetMoney() => TotalMoneyNW.Value;

        public void SetMoney(int amount)
        {
            if (!IsServer) return;
            TotalMoneyNW.Value = amount;
        }

        #endregion
    }
}