using System;
using _Project.ScriptableObjects.ScriptObjects.StoreSO;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class BaseCartItem : NetworkBehaviour
    {
        [SerializeField] protected TMP_Text QuantityText;
        [SerializeField] protected ItemIds ItemIds;
        [field: SerializeField] protected CartManager CartManager;
        [SerializeField] protected StoreSO StoreSO;

        [SerializeField] protected GameObject UIVisual;

        // protected int _quantity;
        public NetworkVariable<int> Quantity = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        protected int CurrentPrice => StoreSO.GetItemData(ItemIds).Cost * Quantity.Value;
        public ItemIds ThisItemId => ItemIds;
        private bool _hasInitialized = false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Quantity.OnValueChanged += HandleQuantityChange;
            Debug.Log("OnNetworkSpawn");
            HandleQuantityChange(0, 0);
            _hasInitialized = true;
        }

        private void OnEnable()
        {
            if (!_hasInitialized )
            {
                Quantity.OnValueChanged += HandleQuantityChange;
                Debug.Log("EnableHax");
                _hasInitialized = true;
                if (IsServer)
                {
                    HandleQuantityChange(0, 0);
                    return;
                }
            }
            HandleQuantityChange(0, Quantity.Value);
        }

        private void HandleQuantityChange(int oldQuantity, int newQuantity)
        {
            if (newQuantity == 0)
            {
                UIVisual.SetActive(false);
                Transform parentTransform = transform.parent;
                int lastIndex = parentTransform.childCount - 1;
                transform.SetSiblingIndex(lastIndex);
                Debug.Log("WhyNoHide?");
            }
            else
            {
                UIVisual.SetActive(true);
                Debug.Log("WhyNoShow?");
                QuantityText.SetText(newQuantity.ToString());
                CartManager.UpdateTotalText();
            }

            if (oldQuantity == 0 && newQuantity != 0)
            {
                transform.SetSiblingIndex(0);
            }
        }

        public virtual void HandleAddButton()
        {
            RequestAddServerRpc();
            /*if (_quantity >= 10) return;
            _quantity++;
            QuantityText.SetText(_quantity.ToString());
            CartManager.UpdateTotalText();*/
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddServerRpc()
        {
            if (Quantity.Value >= 10) return;
            Quantity.Value++;
        }

        public virtual void HandleSubtractButton()
        {
            RequestSubtractServerRpc();
            /*if (_quantity <= 1) return;
            _quantity--;
            QuantityText.SetText(_quantity.ToString());
            CartManager.UpdateTotalText();*/
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSubtractServerRpc()
        {
            if (Quantity.Value <= 1) return;
            Quantity.Value--;
        }

        public virtual void HandleRemoveFromCart()
        {
            RequestRemoveFromCartServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRemoveFromCartServerRpc()
        {
            Quantity.Value = 0;
            CartManager.RemoveFromCart(this);
        }

        public virtual int GetCurrentPrice()
        {
            return CurrentPrice;
        }

        public virtual int GetQuantity()
        {
            return Quantity.Value;
        }
    }
}