using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Buy;
using Unity.Netcode;

namespace _Project.Code.Utilities.Singletons
{
    public class WalletBankton : NetworkSingleton<WalletBankton>
    {
        public NetworkVariable<int> TotalMoneyNW = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public int TotalMoney { get; private set; } = 100;

        public NetworkVariable<float> ResearchProgressNW = new NetworkVariable<float>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public float CurrentResearchProgress { get; private set; } = 0;

        public NetworkVariable<float> QuotaNW = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public float ResearchQuota { get; private set; } = 250;

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
        public void HandleResearchProgressChange(float oldAmount, float newAmount)
        {
        }

        public void HandleQuotaChange(float oldAmount, float newAmount)
        {
        }

        #endregion

        #region Money

        public void AddSubMoney(int amount)
        {
            TotalMoney += amount;
            RequestAddSubMoneyServerRpc(amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddSubMoneyServerRpc(int amount)
        {
            TotalMoneyNW.Value += amount;
        }

        #endregion

        #region Research

        public void AddResearchProgress(float amount)
        {
            CurrentResearchProgress += amount;
            RequestAddResearchProgressServerRpc(amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddResearchProgressServerRpc(float amount)
        {
            ResearchProgressNW.Value += amount;
        }

        public void ResetResearchProgress()
        {
            CurrentResearchProgress = 0;
            RequestResetResearchProgressServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetResearchProgressServerRpc()
        {
            ResearchProgressNW.Value = 0;
        }

        #endregion

        #region Quota

        public void SetQuota(float quota)
        {
            ResearchQuota = quota;
            RequestSetQuotaServerRpc(quota);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetQuotaServerRpc(float quota)
        {
            QuotaNW.Value = quota;
        }

        public void AddToQuota(float amount)
        {
            ResearchQuota += amount;
            RequestAddToQuotaServerRpc(amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddToQuotaServerRpc(float amount)
        {
            QuotaNW.Value += amount;
        }

        #endregion
    }
}