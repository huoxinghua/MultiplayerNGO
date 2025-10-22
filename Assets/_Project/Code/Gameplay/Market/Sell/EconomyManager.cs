using _Project.Code.Core.ServiceLocator;
using UnityEngine;

public class EconomyManager : MonoBehaviourService
{
    [SerializeField] BaseMarketSO BaseMarketSO;
    public int PlayerMoney { get; private set; }
    public float ResearchProgress { get; private set; }
    public float ResearchForQuota { get; private set; }
    void Awake()
    {

        ServiceLocator.Register<EconomyManager>(this);
    }
/*    public float GetTranquilMarketValue(float lerpValue , string keyName)
    {
        MarketData data = BaseMarketSO.GetItemData(keyName);
        return Mathf.Lerp(data.MinTranquilValue,data.MaxTranquilValue,lerpValue);
    }
    public float GetViolentMarketValue(float lerpValue, string keyName)
    {
        MarketData data = BaseMarketSO.GetItemData(keyName);
        return Mathf.Lerp(data.MinTranquilValue, data.MaxTranquilValue, lerpValue);
    }
    public float GetMiscMarketValue(float lerpValue, string keyName)
    {
        MarketData data = BaseMarketSO.GetItemData(keyName);
        return Mathf.Lerp(data.MinTranquilValue, data.MaxTranquilValue, lerpValue);
    }*/
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
        ResearchProgress += values.TranquilMarketValue + values.ViolentMarketValue + values.MiscMarketValue;

    }
}
public struct SampleMarketValue
{
    public float TranquilMarketValue;
    public float ViolentMarketValue;
    public float MiscMarketValue;
}