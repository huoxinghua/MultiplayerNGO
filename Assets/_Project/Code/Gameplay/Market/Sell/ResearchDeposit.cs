using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.ServiceLocator;
using _Project.Code.UI.Inventory;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Sell
{
    /// <summary>
    /// Research deposit station for selling items.
    /// Updated to use async callback pattern instead of synchronous TrySell return value.
    /// </summary>
    [RequireComponent(typeof(Outline))]
    public class ResearchDeposit : NetworkBehaviour, IInteractable
    {
        protected Outline OutlineEffect;

        public void Awake()
        {
            OutlineEffect = GetComponent<Outline>();
            if (OutlineEffect != null)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }

            // Subscribe to item sold event
            EventBus.Instance.Subscribe<ItemSoldEvent>(this, OnItemSold);
        }

        private void OnDisable()
        {
            // Unsubscribe from event
            EventBus.Instance.Unsubscribe<ItemSoldEvent>(this);
        }
        /// <summary>
        /// Called when player interacts with research deposit.
        /// Initiates async sale - result will arrive via ItemSoldEvent callback.
        /// </summary>
        public void OnInteract(GameObject interactingPlayer)
        {
            PlayerInventory playerInventory = interactingPlayer.GetComponent<PlayerInventory>();
            if (playerInventory != null)
            {
                if (playerInventory.IsHoldingSample())
                {
                    playerInventory.TrySell();
                }
            }
        }

        /// <summary>
        /// Callback when item sale completes.
        /// Receives ScienceData via event and processes economy transaction.
        /// </summary>
        private void OnItemSold(ItemSoldEvent saleEvent)
        {

            // Get market value from sold item data
            SampleMarketValue itemValues = EconomyManager.Instance.GetMarketValue(saleEvent.SoldItemData);

            // Process sale (adds money and research progress)
            EconomyManager.Instance.SoldItem(itemValues);

          //  Debug.Log($"[ResearchDeposit] Item sold: {saleEvent.SoldItemData.KeyName} | Money: {economyManager.PlayerMoney} | Research: {economyManager.ResearchProgress}");
        }
        public void HandleHover(bool isHovering)
        {
            if (OutlineEffect != null)
            {

                if (isHovering)
                {
                    OutlineEffect.OutlineMode = Outline.Mode.OutlineVisible;
                    OutlineEffect.OutlineWidth = 2;
                }
                else
                {
                    OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                    OutlineEffect.OutlineWidth = 0;
                }
            }
        }
    }
}
