using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Quota;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Utilities.ServiceLocator;
using _Project.Code.Utilities.Singletons;
using _Project.ScriptableObjects.ScriptObjects.MarketSO;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Sell
{
    public class EconomyManager : NetworkSingleton<EconomyManager>
    {
        [SerializeField] private BaseMarketSO BaseMarketSO;
        [SerializeField] private ScienceToMoneySO _scienceToMoneySO;
        protected override bool AutoSpawn => false;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                // Force the object to NOT be destroyed when the scene unloads.
                NetworkObject.DestroyWithScene = false;
            }
        }

        public SampleMarketValue GetMarketValue(ScienceData itemData)
        {
            MarketData marketData = BaseMarketSO.GetItemData(itemData.KeyName);
            return new SampleMarketValue
            {
                TranquilMarketValue = Mathf.Lerp(marketData.MinTranquilValue, marketData.MaxTranquilValue, itemData.RawTranquilValue),
                ViolentMarketValue = Mathf.Lerp(marketData.MinTranquilValue, marketData.MaxTranquilValue, itemData.RawViolentValue),
                MiscMarketValue = Mathf.Lerp(marketData.MinTranquilValue, marketData.MaxTranquilValue, itemData.RawMiscValue)
            };
        }
        public void SoldItem(SampleMarketValue values)
        {
            QuotaManager.Instance.RequestAddDayProgressServerRpc(values.TranquilMarketValue + values.ViolentMarketValue + values.MiscMarketValue);
            WalletBankton.Instance.AddSubMoney((int)(values.TranquilMarketValue * _scienceToMoneySO.TranquilMoneyModifier +
                                  values.ViolentMarketValue * _scienceToMoneySO.ViolentMoneyModifier +
                                  values.MiscMarketValue * _scienceToMoneySO.MiscMoneyModifier));
        }
    }
    public struct SampleMarketValue
    {
        public float TranquilMarketValue;
        public float ViolentMarketValue;
        public float MiscMarketValue;
    }
}