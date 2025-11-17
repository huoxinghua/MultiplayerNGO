using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.ServiceLocator;
using _Project.Code.UI.Inventory;
using QuickOutline.Scripts;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Sell
{
    /// <summary>
    /// Research deposit station for selling items.
    /// Updated to use async callback pattern instead of synchronous TrySell return value.
    /// </summary>
    [RequireComponent(typeof(Outline))]
    public class ResearchDeposit : MonoBehaviour, IInteractable
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

        private void OnDestroy()
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
                    // Validate EconomyManager exists before initiating sale
                    if (!ServiceLocator.TryGet<EconomyManager>(out var economy))
                    {
                        Debug.LogWarning("[ResearchDeposit] EconomyManager not found in ServiceLocator");
                        return;
                    }

                    // Initiate async sale - result will arrive via OnItemSold callback
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
            if (!ServiceLocator.TryGet<EconomyManager>(out var economyManager))
            {
                Debug.LogWarning("[ResearchDeposit] EconomyManager not found when processing sale");
                return;
            }

            // Get market value from sold item data
            SampleMarketValue itemValues = economyManager.GetMarketValue(saleEvent.SoldItemData);

            // Process sale (adds money and research progress)
            economyManager.SoldItem(itemValues);

            Debug.Log($"[ResearchDeposit] Item sold: {saleEvent.SoldItemData.KeyName} | Money: {economyManager.PlayerMoney} | Research: {economyManager.ResearchProgress}");
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
