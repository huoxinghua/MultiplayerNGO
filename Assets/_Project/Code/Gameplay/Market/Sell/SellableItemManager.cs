using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Quota;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Sell
{
    /// <summary>
    /// Tracks all sellable items spawned in the world (dead beetles, brute pieces, etc.)
    /// Despawns unheld items when players return to hub.
    /// </summary>
    public class SellableItemManager : NetworkSingleton<SellableItemManager>
    {
        private readonly Dictionary<NetworkObject, BaseInventoryItem> _trackedItems = new();

        protected override bool PersistBetweenScenes => false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                EventBus.Instance.Subscribe<OnEnterHubEvent>(this, OnEnterHub);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (EventBus.Instance != null)
            {
                EventBus.Instance.Unsubscribe<OnEnterHubEvent>(this);
            }
        }

        /// <summary>
        /// Register a sellable item to be tracked.
        /// Called by items in their OnNetworkSpawn.
        /// </summary>
        public void RegisterItem(NetworkObject netObj, BaseInventoryItem item)
        {
            if (!IsServer) return;
            if (netObj == null || item == null) return;

            if (!_trackedItems.ContainsKey(netObj))
            {
                _trackedItems.Add(netObj, item);
                Debug.Log($"[SellableItemManager] Registered item: {item.GetItemName()}");
            }
        }

        /// <summary>
        /// Unregister a sellable item (called when sold or despawned).
        /// </summary>
        public void UnregisterItem(NetworkObject netObj)
        {
            if (!IsServer) return;
            if (netObj == null) return;

            if (_trackedItems.Remove(netObj))
            {
                Debug.Log($"[SellableItemManager] Unregistered item");
            }
        }

        /// <summary>
        /// Called when players enter the hub. Despawns all unheld sellable items.
        /// </summary>
        private void OnEnterHub(OnEnterHubEvent evt)
        {
            if (!IsServer) return;
            DespawnUnheldItems();
        }

        /// <summary>
        /// Despawns all tracked items that are NOT currently held by a player.
        /// </summary>
        public void DespawnUnheldItems()
        {
            if (!IsServer) return;

            var itemsToDespawn = new List<NetworkObject>();

            foreach (var kvp in _trackedItems)
            {
                NetworkObject netObj = kvp.Key;
                BaseInventoryItem item = kvp.Value;

                if (netObj == null || !netObj.IsSpawned)
                {
                    itemsToDespawn.Add(netObj);
                    continue;
                }

                // Check if item is held - if not, mark for despawn
                if (!item.IsCurrentlyHeld)
                {
                    itemsToDespawn.Add(netObj);
                    Debug.Log($"[SellableItemManager] Despawning unheld item: {item.GetItemName()}");
                }
                else
                {
                    Debug.Log($"[SellableItemManager] Keeping held item: {item.GetItemName()}");
                }
            }

            // Despawn and remove from tracking
            foreach (var netObj in itemsToDespawn)
            {
                _trackedItems.Remove(netObj);

                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn(true);
                }
            }

            Debug.Log($"[SellableItemManager] Despawned {itemsToDespawn.Count} unheld items, {_trackedItems.Count} items remain");
        }

        /// <summary>
        /// Gets count of currently tracked items (for debugging).
        /// </summary>
        public int TrackedItemCount => _trackedItems.Count;
    }
}
