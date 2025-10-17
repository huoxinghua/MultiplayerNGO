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
}
