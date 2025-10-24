using _Project.ScriptableObjects.ScriptObjects.StoreSO;
using TMPro;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class StoreButton : MonoBehaviour
    {
        [SerializeField] private ItemIds _thisItemID;
        [SerializeField] private StoreSO _storeSO;
        [SerializeField] private TMP_Text _priceText;
        void Start()
        {
            _priceText.SetText($"@{_storeSO.GetItemData(_thisItemID).Cost}");
        }

    
    }
}
