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
            if (!IsServer) RequestStoreButtonClickedServerRpc(itemID);
            else _cartManager.AddToCart((ItemIds)itemID);
            
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestStoreButtonClickedServerRpc(int itemID)
        {
            _cartManager.AddToCart((ItemIds)itemID);
        }
        public void AddBuyOrder(BuyOrder newBuyOrder)
        {
            if (!IsServer) return;
            _buyOrders.Add(newBuyOrder);
        }
        public void ClearBuyOrders()
        {
            if (!IsServer) return;
            _buyOrders.Clear();
        }
        public void SpawnItemsFromBuyOrder()
        {
            if (!IsServer) return;
            foreach(var item in _buyOrders)
            {
                _vanSpawner.AddBuyOrders(item);     
            }
            _vanSpawner.SendVan();
            ClearBuyOrders();
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