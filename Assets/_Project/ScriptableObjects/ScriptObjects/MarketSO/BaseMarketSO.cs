using System.Collections.Generic;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.MarketSO
{
    [CreateAssetMenu(fileName = "BaseMarketSO", menuName = "Market/BaseMarketSO")]
    public class BaseMarketSO : ScriptableObject
    {
        [SerializeField]
        private List<MarketData> itemList = new();
        [field: SerializeField] public float ScienceToMoneyMult {  get; private set; }
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
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.Log("empty for somereason");
                return default;
            }
            if (itemDictionary.TryGetValue(itemID, out var data))
            {
                return data;
            }

            Debug.LogWarning($"Item ID '{itemID}' not found.");
            return default;
        }
    }
}
