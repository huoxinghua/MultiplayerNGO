using _Project.ScriptableObjects.ScriptObjects.MarketSO;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreSO", menuName = "Scriptable Objects/StoreSO")]
public class StoreSO : ScriptableObject
{
    [SerializeField]
    private List<StoreItem> itemList = new();
    private Dictionary<ItemIds, StoreItem> itemDictionary;

    private void OnEnable()
    {
        itemDictionary = new Dictionary<ItemIds, StoreItem>();
        foreach (var item in itemList)
        {
            if (!itemDictionary.ContainsKey(item.ItemID))
            {
                itemDictionary.Add(item.ItemID, item);
            }
        }
    }

    public StoreItem GetItemData(ItemIds itemID)
    {
        if (itemDictionary.TryGetValue(itemID, out var data))
        {
            return data;
        }

        Debug.LogWarning($"Item ID '{itemID}' not found.");
        return default;
    }
}
[System.Serializable]
public struct StoreItem
{
    public ItemIds ItemID;
    public int Cost;
    public BaseCartItem ItemPrefab;
    public GameObject PurchasedItemPrefab;
}
public enum ItemIds
{
    Flashlight = 0,
    BaseballBat = 1
}
