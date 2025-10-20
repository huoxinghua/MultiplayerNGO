using _Project.Code.Core.ServiceLocator;
using UnityEngine;

public class EconomyManager : MonoBehaviourService
{
    [SerializeField] BaseMarketSO BaseMarketSO;
    void Awake()
    {

        ServiceLocator.Register<EconomyManager>(this);
    }
    public float GetTranquilMarketValue(float lerpValue , string keyName)
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
    
}
public struct SampleMarketValue
{
    public float TranquilMarketValue;
    public float ViolentMarketValue;
    public float MiscMarketValue;
}