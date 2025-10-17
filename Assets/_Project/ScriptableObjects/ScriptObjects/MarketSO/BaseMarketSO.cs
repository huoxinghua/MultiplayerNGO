using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseMarketSO", menuName = "Scriptable Objects/BaseMarketSO")]
public class BaseMarketSO : ScriptableObject
{
    [SerializeField]
    private List<MarketData> itemList = new();

    private Dictionary<string, MarketData> itemDictionary;

    private void OnEnable()
    {
        itemDictionary = new Dictionary<string, MarketData>();
        foreach (var item in itemList)
        {
            if (!itemDictionary.ContainsKey(item.itemID))
            {
                itemDictionary.Add(item.itemID, item);
            }
        }
    }

    public MarketData GetItemData(string itemID)
    {
        if (itemDictionary.TryGetValue(itemID, out var data))
        {
            return data;
        }

        Debug.LogWarning($"Item ID '{itemID}' not found.");
        return default;
    }
}
