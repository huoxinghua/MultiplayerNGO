using System.Collections.Generic;
using _Project.Code.Utilities.Singletons;
using _Project.ScriptableObjects.ScriptObjects.StoreSO;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class CartManager : NetworkBehaviour
    {
        [SerializeField] private StoreSO StoreSO;
        [SerializeField] private Transform CartSorter;
        [SerializeField] List<BaseCartItem> CurrentItemsInCart = new List<BaseCartItem>();
        [SerializeField] private TMP_Text _cartTotal;
        [SerializeField] private StoreController _storeController;
        public void AddToCart(ItemIds itemID)
        {
            foreach (var item in CurrentItemsInCart)
            {
                if (item.ThisItemId == itemID)
                {
                    item.HandleAddButton();
                }
            }
            /*if (isInList) return;*/
            //BaseCartItem temp = Instantiate(StoreSO.GetItemData(itemID).ItemPrefab, CartSorter);
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
            if (newTotal > WalletBankton.Instance.TotalMoneyNW.Value)
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
            if(!IsServer)
            {
              RequestBuyCartServerRpc();  
                
            }
            else
            {
                int cartTotal = GetCartTotal();
                if (cartTotal > WalletBankton.Instance.TotalMoneyNW.Value) return;
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
           
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestBuyCartServerRpc()
        {
            int cartTotal = GetCartTotal();
            if (cartTotal > WalletBankton.Instance.TotalMoneyNW.Value) return;
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
                item.HandleRemoveFromCart();
            }
            UpdateTotalText();
        }
        public void RemoveFromCart(BaseCartItem item)
        {
            UpdateTotalText();
        }
    }
}
