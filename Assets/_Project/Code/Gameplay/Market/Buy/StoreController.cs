using System.Collections.Generic;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Singletons;
using _Project.ScriptableObjects.ScriptObjects.StoreSO;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class StoreController : NetworkBehaviour
    {
        [SerializeField] private CartManager _cartManager;
        [SerializeField] private TMP_Text _walletMoney;
        [SerializeField] private VanSpawner _vanSpawner;
        private List<BuyOrder> _buyOrders = new List<BuyOrder>();
        public void OnEnable()
        {
            EventBus.Instance.Subscribe<WalletUpdate>(this, HandleWalletUpdateEvent);
            UpdateCashText();
        }
        public void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WalletUpdate>(this);
        }
        public void HandleWalletUpdateEvent(WalletUpdate wallet)
        {
            UpdateCashText();
        }
        public void UpdateCashText()
        {
            _walletMoney.SetText($"Cash: @{WalletBankton.Instance.TotalMoneyNW.Value}");
        }
        public void HandleStoreClicked(int itemID)
        {
            _cartManager.AddToCart((ItemIds)itemID);
        }
        public void AddBuyOrder(BuyOrder newBuyOrder)
        {
            _buyOrders.Add(newBuyOrder);
        }
        public void ClearBuyOrders()
        {
            _buyOrders.Clear();
        }
        public void SpawnItemsFromBuyOrder()
        {
            foreach(var item in _buyOrders)
            {
                _vanSpawner.AddBuyOrders(item);     
            }
            _vanSpawner.SendVan();
        }
    }
    public struct WalletUpdate : IEvent
    {

    }
    public struct BuyOrder
    {
        public int Amount;
        public GameObject ItemPrefab;

    }
}