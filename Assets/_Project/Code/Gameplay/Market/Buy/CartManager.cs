using System.Collections.Generic;
using _Project.Code.Utilities.Singletons;
using _Project.ScriptableObjects.ScriptObjects.StoreSO;
using TMPro;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class CartManager : MonoBehaviour
    {
        [SerializeField] private StoreSO StoreSO;
        [SerializeField] private Transform CartSorter;
        private List<BaseCartItem> CurrentItemsInCart = new List<BaseCartItem>();
        [SerializeField] private TMP_Text _cartTotal;
        [SerializeField] private StoreController _storeController;
        public void AddToCart(ItemIds itemID)
        {
            bool isInList = false;
            foreach (var item in CurrentItemsInCart)
            {
                if (item.ThisItemId == itemID)
                {
                    isInList = true;
                    item.HandleAddButton();
                }
            }
            if (isInList) return;
            BaseCartItem temp = Instantiate(StoreSO.GetItemData(itemID).ItemPrefab, CartSorter);
            temp.transform.parent = CartSorter;
            CurrentItemsInCart.Add(temp);
            temp.OnAddToCart(this);


        }
        public void OnEnable()
        {
            UpdateTotalText();
        }
        public int GetCartTotal()
        {
            int newTotal = 0;
            foreach (var item in CurrentItemsInCart)
            {
                newTotal += item.GetCurrentPrice();
            }
            return newTotal;
        }
        public void UpdateTotalText()
        {
            int newTotal = GetCartTotal();
            _cartTotal.SetText($"Total: @{newTotal}");
            if (newTotal > WalletBankton.Instance.TotalMoney)
            {
                _cartTotal.color = Color.red;
            }
            else
            {
                _cartTotal.color = Color.white;
            }
        }
        public void HandleBuyCart()
        {
            int cartTotal = GetCartTotal();
            if (cartTotal > WalletBankton.Instance.TotalMoney) return;
            else
            {
                WalletBankton.Instance.AddSubMoney(-cartTotal);
                foreach (var item in CurrentItemsInCart)
                {
                    _storeController.AddBuyOrder(
                        new BuyOrder
                        {
                            Amount = item.GetQuantity(),
                            ItemPrefab = StoreSO.GetItemData(item.ThisItemId).PurchasedItemPrefab
                        });
                
                }
                _storeController.SpawnItemsFromBuyOrder();
                RemoveFullCart();
            }
        }
        public void RemoveFullCart()
        {
            foreach (var item in CurrentItemsInCart)
            {
                Destroy(item.gameObject);
            }
            CurrentItemsInCart.Clear();
            UpdateTotalText();
        }
        public void RemoveFromCart(BaseCartItem item)
        {
            CurrentItemsInCart?.Remove(item);
            Destroy(item.gameObject);
            UpdateTotalText();
        }
    }
}
