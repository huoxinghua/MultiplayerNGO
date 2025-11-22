using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Art.AnimationScripts.IK;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.UsableItems;
using _Project.Code.UI.Inventory;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Gameplay.Player.RefactorInventory
{
    /// <summary>
    /// Clean Rewrite: Server-authoritative inventory system for multiplayer game.
    /// </summary>
    public class PlayerInventory : NetworkBehaviour
    {
        #region Network State (Single Source of Truth)

        /// <summary>
        /// Server-authoritative inventory contents. Contains NetworkObjectReferences to items.
        /// This is the SINGLE SOURCE OF TRUTH for inventory state.
        /// </summary>
        public NetworkList<NetworkObjectReference> InventoryNetworkRefs;

        /// <summary>
        /// Server-authoritative big item reference (non-pocketsize items held in hands).
        /// </summary>
        public NetworkVariable<NetworkObjectReference> InventoryNetworkBigItemRef = new NetworkVariable<NetworkObjectReference>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>
        /// Server-authoritative currently equipped slot index (0-4).
        /// </summary>
        public NetworkVariable<int> NetworkCurrentIndex = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        #endregion

        #region Local Cache (Rebuilt from Network State)

        /// <summary>
        /// Local cache of inventory items. Rebuilt from InventoryNetworkRefs in callbacks.
        /// DO NOT MODIFY DIRECTLY - this is a cache only!
        /// </summary>
        public BaseInventoryItem[] InventoryItems = new BaseInventoryItem[5];

        /// <summary>
        /// Local cache of big item carried. Rebuilt from InventoryNetworkBigItemRef in callbacks.
        /// DO NOT MODIFY DIRECTLY - this is a cache only!
        /// </summary>
        public BaseInventoryItem BigItemCarried { get; private set; }

        /// <summary>
        /// Local cache of current index. Rebuilt from NetworkCurrentIndex in callbacks.
        /// DO NOT MODIFY DIRECTLY - this is a cache only!
        /// </summary>
        private int _currentIndex;

        #endregion

        #region Configuration & References

        [Header("Inventory Configuration")]
        [SerializeField] [Tooltip("Number of inventory slots (should be 5)")]
        private int InventorySlots = 5;

        [Header("Transform References")]
        [SerializeField] [Tooltip("Transform on FPS model where items are parented")]
        private Transform FPSItemParent;

        [SerializeField] [Tooltip("Transform on TPS model where items are parented")]
        private Transform TPSItemParent;

        [SerializeField] [Tooltip("Transform where dropped items spawn")]
        private Transform DropTransform;

        [Header("Component References")]
        [SerializeField] private PlayerInputManager _inputManager;
        [SerializeField] private PlayerAnimation PlayerAnimation;
        [SerializeField] public PlayerIKData ThisPlayerIKData;

        [Header("UI References")]
        [SerializeField] private Image[] ItemUIDisplay = new Image[5];
        [SerializeField] private Image[] SlotUIBackground = new Image[5];

        #endregion

        #region Constants

        /// <summary>
        /// Maximum distance player can be from item to pick it up (meters).
        /// </summary>
        private const float MAX_PICKUP_RANGE = 3f;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if player is holding a big item (non-pocketsize).
        /// </summary>
        private bool _handsFull => BigItemCarried != null;

        /// <summary>
        /// Returns true if all 5 inventory slots are occupied.
        /// </summary>
        public bool InventoryFull => IsInventoryFull();

        /// <summary>
        /// Gets the FPS model's item parent transform.
        /// </summary>
        public Transform GetFPSItemParent() => FPSItemParent;

        /// <summary>
        /// Gets the TPS model's item parent transform.
        /// </summary>
        public Transform GetTPSItemParent() => TPSItemParent;

        #endregion

        #region Initialization

        private void Awake()
        {
            // Initialize NetworkList in constructor-like method
            InventoryNetworkRefs = new NetworkList<NetworkObjectReference>();

            // Validate configuration
            if (InventorySlots != 5)
            {
                Debug.LogError($"[PlayerInventory] InventorySlots must be 5, currently set to {InventorySlots}");
            }

            if (FPSItemParent == null || TPSItemParent == null || DropTransform == null)
            {
                Debug.LogError("[PlayerInventory] FPSItemParent, TPSItemParent, or DropTransform not assigned in Inspector!");
            }

            // Register input event handlers
            if (_inputManager != null)
            {
                _inputManager.OnNumPressed += HandlePressedSlot;
                _inputManager.OnUse += UseItemInHand;
                _inputManager.OnSecondaryUse += SecondaryUseItemInHand;
                _inputManager.OnDropItem += DropItem;
            }
            else
            {
                Debug.LogError("[PlayerInventoryV2] PlayerInputManager not assigned in Inspector!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Server initializes inventory with empty slots
            if (IsServer)
            {
                for (int i = 0; i < InventorySlots; i++)
                {
                    InventoryNetworkRefs.Add(default);
                }
                NetworkCurrentIndex.Value = 0;
            }

            // All clients register NetworkVariable callbacks
            InventoryNetworkRefs.OnListChanged += HandleInventoryListChange;
            NetworkCurrentIndex.OnValueChanged += HandleCurrentIndexChange;
            InventoryNetworkBigItemRef.OnValueChanged += HandleBigItemChanged;

            // Apply initial values for late joiners (these methods read network state)
            HandleBigItemChanged(default, InventoryNetworkBigItemRef.Value);
            HandleCurrentIndexChange(0, NetworkCurrentIndex.Value);
            RebuildInventoryCache();
        }

        private void OnDisable()
        {
            // Unregister input event handlers
            if (_inputManager != null)
            {
                _inputManager.OnNumPressed -= HandlePressedSlot;
                _inputManager.OnUse -= UseItemInHand;
                _inputManager.OnSecondaryUse -= SecondaryUseItemInHand;
                _inputManager.OnDropItem -= DropItem;
            }
        }

        #endregion

        #region R1: Item Pickup

        /// <summary>
        /// Called by interaction system to check if pickup is possible.
        /// This is a local check only - server does final validation.
        /// </summary>
        /// <returns>True if player has space to pickup item</returns>
        public bool TryPickupItem()
        {
            // Can't pickup if hands full AND inventory full
            if (_handsFull || InventoryFull)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Client entry point for pickup. Sends request to server.
        /// Server will validate distance, space, and execute pickup.
        /// </summary>
        /// <param name="item">The item to pick up</param>
        public void DoPickup(BaseInventoryItem item)
        {
            if (item == null || item.NetworkObject == null)
            {
                Debug.LogWarning("[PlayerInventory] DoPickup called with null item");
                return;
            }

            // Client just sends the request - server does all validation and execution
            PickupItemServerRpc(new NetworkObjectReference(item.NetworkObject), item.IsPocketSize());
        }

        /// <summary>
        /// Server-authoritative pickup. Validates distance, space, and executes pickup.
        /// Updates NetworkList/NetworkVariable which triggers callbacks on all clients.
        /// </summary>
        /// <param name="itemRef">NetworkObjectReference to the item</param>
        /// <param name="isPocketSize">Whether item goes in inventory or held in hands</param>
        /// <param name="rpcParams">RPC parameters for sender validation</param>
        [ServerRpc(RequireOwnership = false)]
        private void PickupItemServerRpc(NetworkObjectReference itemRef, bool isPocketSize, ServerRpcParams rpcParams = default)
        {
            // Validate item reference
            if (!itemRef.TryGet(out NetworkObject itemNetObj))
            {
                Debug.LogWarning($"[Server] Invalid item reference in pickup from client {rpcParams.Receive.SenderClientId}");
                return;
            }

            BaseInventoryItem item = itemNetObj.GetComponent<BaseInventoryItem>();
            if (item == null)
            {
                Debug.LogWarning("[Server] NetworkObject has no BaseInventoryItem component");
                return;
            }

            // Validate distance (prevent teleport pickup exploits)
            float distance = Vector3.Distance(transform.position, itemNetObj.transform.position);
            if (distance > MAX_PICKUP_RANGE)
            {
                Debug.LogWarning($"[Server] Client {rpcParams.Receive.SenderClientId} tried to pickup item from {distance:F2}m away (max: {MAX_PICKUP_RANGE}m)");
                return;
            }

            if (isPocketSize)
            {
                // Find available slot (prefer current slot, then first empty)
                int targetSlot = -1;

                if (!InventoryNetworkRefs[_currentIndex].TryGet(out _))
                {
                    // Current slot is empty
                    targetSlot = _currentIndex;
                }
                else
                {
                    // Find first empty slot
                    for (int i = 0; i < InventorySlots; i++)
                    {
                        if (!InventoryNetworkRefs[i].TryGet(out _))
                        {
                            targetSlot = i;
                            break;
                        }
                    }
                }

                if (targetSlot == -1)
                {
                    Debug.LogWarning("[Server] No available inventory slot");
                    return;
                }

                // Server executes pickup on the item (server-only method)
                item.PickupItem(gameObject, FPSItemParent, TPSItemParent, NetworkObject);

                // Update NetworkList - this triggers HandleInventoryListChange on all clients
                InventoryNetworkRefs[targetSlot] = itemRef;

                // If picking up to current slot, equip it; otherwise unequip
                if (targetSlot == _currentIndex)
                {
                    item.EquipItem();
                }
                else
                {
                    item.UnequipItem();
                }
            }
            else
            {
                // Big item (non-pocketsize) - unequip current slot item first
                if (InventoryNetworkRefs[_currentIndex].TryGet(out NetworkObject currentItemObj))
                {
                    BaseInventoryItem currentItem = currentItemObj.GetComponent<BaseInventoryItem>();
                    currentItem?.UnequipItem();
                }

                // Server executes pickup
                item.PickupItem(gameObject, FPSItemParent, TPSItemParent, NetworkObject);
                item.EquipItem();

                // Update NetworkVariable for big item - triggers HandleBigItemChanged on all clients
                InventoryNetworkBigItemRef.Value = itemRef;
            }
        }

        #endregion

        #region R2: Item Drop

        /// <summary>
        /// Client entry point for drop. Sends request to server.
        /// </summary>
        public void DropItem()
        {
            // Client just sends the request
            DropItemServerRpc(_currentIndex, _handsFull);
        }

        /// <summary>
        /// Server-authoritative drop. Validates and executes drop.
        /// Updates NetworkList/NetworkVariable which triggers callbacks on all clients.
        /// </summary>
        /// <param name="slotIndex">Index of slot to drop from</param>
        /// <param name="droppingBigItem">Whether dropping big item or slot item</param>
        [ServerRpc(RequireOwnership = false)]
        private void DropItemServerRpc(int slotIndex, bool droppingBigItem)
        {
            // Validate slot index
            if (slotIndex < 0 || slotIndex >= InventorySlots)
            {
                Debug.LogWarning($"[Server] Invalid slot index for drop: {slotIndex}");
                return;
            }

            if (droppingBigItem)
            {
                // Validate big item exists
                if (!InventoryNetworkBigItemRef.Value.TryGet(out NetworkObject bigItemObj))
                {
                    Debug.LogWarning("[Server] No big item to drop");
                    return;
                }

                BaseInventoryItem bigItem = bigItemObj.GetComponent<BaseInventoryItem>();
                if (bigItem == null)
                {
                    Debug.LogWarning("[Server] Big item has no BaseInventoryItem component");
                    return;
                }

                // Server executes drop (server-only methods)
                bigItem.UnequipItem();
                bigItem.DropItem(DropTransform);

                // Clear NetworkVariable - triggers HandleBigItemChanged on all clients
                InventoryNetworkBigItemRef.Value = default;
            }
            else
            {
                // Validate slot has item
                if (!InventoryNetworkRefs[slotIndex].TryGet(out NetworkObject slotItemObj))
                {
                    Debug.LogWarning($"[Server] No item in slot {slotIndex} to drop");
                    return;
                }

                BaseInventoryItem slotItem = slotItemObj.GetComponent<BaseInventoryItem>();
                if (slotItem == null)
                {
                    Debug.LogWarning("[Server] Slot item has no BaseInventoryItem component");
                    return;
                }

                // Server executes drop
                slotItem.UnequipItem();
                slotItem.DropItem(DropTransform);

                // Clear NetworkList slot - triggers HandleInventoryListChange on all clients
                InventoryNetworkRefs[slotIndex] = default;
            }
        }

        #endregion

        #region R3: Slot Switching

        /// <summary>
        /// Client entry point for slot switching. Sends request to server.
        /// </summary>
        /// <param name="slotIndex">The inventory slot index to switch to (0-4)</param>
        private void EquipSlot(int slotIndex)
        {
            // Can't switch slots while holding big item
            if (_handsFull)
            {
                return;
            }

            // Client just sends the request
            EquipSlotServerRpc(slotIndex);
        }

        /// <summary>
        /// Server-authoritative slot switching. Updates NetworkVariable which triggers callbacks.
        /// </summary>
        /// <param name="newIndex">The new slot index to equip (0-4)</param>
        [ServerRpc(RequireOwnership = false)]
        private void EquipSlotServerRpc(int newIndex)
        {
            // Validate index
            if (newIndex < 0 || newIndex >= InventorySlots)
            {
                Debug.LogWarning($"[Server] Invalid slot index for equip: {newIndex}");
                return;
            }

            // Get old and new slot items
            BaseInventoryItem oldItem = null;
            BaseInventoryItem newItem = null;

            if (InventoryNetworkRefs[NetworkCurrentIndex.Value].TryGet(out NetworkObject oldItemObj))
            {
                oldItem = oldItemObj.GetComponent<BaseInventoryItem>();
            }

            if (InventoryNetworkRefs[newIndex].TryGet(out NetworkObject newItemObj))
            {
                newItem = newItemObj.GetComponent<BaseInventoryItem>();
            }

            // Unequip old item
            oldItem?.UnequipItem();

            // Update NetworkVariable - this triggers HandleCurrentIndexChange on all clients
            NetworkCurrentIndex.Value = newIndex;

            // Equip new item
            newItem?.EquipItem();
        }

        #endregion

        #region R4: Item Usage

        /// <summary>
        /// Uses the currently held item (big item or equipped slot item).
        /// Triggers animation if item has cooldown available.
        /// </summary>
        public void UseItemInHand()
        {
            Debug.Log($"[UseItemInHand] Called - _handsFull:{_handsFull}, _currentIndex:{_currentIndex}, Item:{InventoryItems[_currentIndex]?.GetItemName()}");

            // Play interact animation if item is ready to use
            if (InventoryItems[_currentIndex] != null)
            {
                bool cooldownComplete = InventoryItems[_currentIndex].ItemCooldown.IsComplete;
                Debug.Log($"[UseItemInHand] Animation check - CooldownComplete:{cooldownComplete}, PlayerAnimation null:{PlayerAnimation == null}");

                if (cooldownComplete)
                {
                    Debug.Log("[UseItemInHand] Calling PlayerAnimation.PlayInteract()");
                    PlayerAnimation.PlayInteract();
                }
            }

            // Use big item or slot item
            if (_handsFull)
            {
                Debug.Log($"[UseItemInHand] Using big item: {BigItemCarried?.GetItemName()}");
                BigItemCarried?.UseItem();
            }
            else
            {
                Debug.Log($"[UseItemInHand] Using slot item: {InventoryItems[_currentIndex]?.GetItemName()}");
                InventoryItems[_currentIndex]?.UseItem();
            }
        }

        /// <summary>
        /// Secondary use of currently held item (hold/release pattern).
        /// </summary>
        /// <param name="isPerformed">True when button pressed, false when released</param>
        public void SecondaryUseItemInHand(bool isPerformed)
        {
            if (_handsFull)
            {
                BigItemCarried?.SecondaryUse(isPerformed);
            }
            else
            {
                InventoryItems[_currentIndex]?.SecondaryUse(isPerformed);
            }
        }

        #endregion

        #region R5: Item Selling

        /// <summary>
        /// Checks if currently holding a sellable item.
        /// </summary>
        /// <returns>True if holding sellable item</returns>
        public bool IsHoldingSample()
        {
            if (_handsFull)
            {
                return BigItemCarried != null && BigItemCarried.CanBeSold();
            }
            else
            {
                return InventoryItems[_currentIndex] != null && InventoryItems[_currentIndex].CanBeSold();
            }
        }

        /// <summary>
        /// Requests to sell currently held item. This is an ASYNC operation!
        /// The actual sale happens on server and is notified via NotifySaleClientRpc.
        /// Calling code should listen for the callback, not use return value.
        /// </summary>
        /// <returns>Pending ScienceData (will be updated via callback)</returns>
        public ScienceData TrySell()
        {
            // Client sends request to server
            SellItemServerRpc(_currentIndex, _handsFull);

            // Return pending data (calling code should use callback instead)
            return new ScienceData
            {
                RawTranquilValue = 0,
                RawViolentValue = 0,
                RawMiscValue = 0,
                KeyName = "Pending"
            };
        }

        /// <summary>
        /// Server-authoritative sell. Validates, gets values, destroys item, updates network state.
        /// </summary>
        /// <param name="slotIndex">Slot index if selling slot item</param>
        /// <param name="sellingBigItem">True if selling big item</param>
        [ServerRpc(RequireOwnership = false)]
        private void SellItemServerRpc(int slotIndex, bool sellingBigItem)
        {
            ScienceData data = new ScienceData
            {
                RawTranquilValue = 0,
                RawViolentValue = 0,
                RawMiscValue = 0,
                KeyName = "Invalid"
            };

            if (sellingBigItem)
            {
                // Validate big item exists
                if (!InventoryNetworkBigItemRef.Value.TryGet(out NetworkObject bigItemObj))
                {
                    Debug.LogWarning("[Server] No big item to sell");
                    return;
                }

                BaseInventoryItem bigItem = bigItemObj.GetComponent<BaseInventoryItem>();
                if (bigItem == null || !bigItem.CanBeSold())
                {
                    Debug.LogWarning("[Server] Big item cannot be sold");
                    return;
                }

                // Get item values before destroying
                data = bigItem.GetValueStruct();

                // Server handles sale (destroys item)
                bigItem.WasSold();

                // Clear NetworkVariable - triggers callback on all clients
                InventoryNetworkBigItemRef.Value = default;
            }
            else
            {
                // Validate slot index
                if (slotIndex < 0 || slotIndex >= InventorySlots)
                {
                    Debug.LogWarning($"[Server] Invalid slot index for sell: {slotIndex}");
                    return;
                }

                // Validate slot has item
                if (!InventoryNetworkRefs[slotIndex].TryGet(out NetworkObject slotItemObj))
                {
                    Debug.LogWarning($"[Server] No item in slot {slotIndex} to sell");
                    return;
                }

                BaseInventoryItem slotItem = slotItemObj.GetComponent<BaseInventoryItem>();
                if (slotItem == null || !slotItem.CanBeSold())
                {
                    Debug.LogWarning("[Server] Slot item cannot be sold");
                    return;
                }

                // Get item values before destroying
                data = slotItem.GetValueStruct();

                // Server handles sale (destroys item)
                slotItem.WasSold();

                // Clear NetworkList slot - triggers callback on all clients
                InventoryNetworkRefs[slotIndex] = default;
            }

            // Notify all clients of successful sale with item data
            // Note: ScienceData struct can't be passed in RPC, so pass individual values
            NotifySaleClientRpc(data.RawTranquilValue, data.RawViolentValue, data.RawMiscValue, data.KeyName);
        }

        /// <summary>
        /// Notifies all clients of successful sale with item data.
        /// Calling code (ResearchDeposit) should listen for this callback.
        /// </summary>
        [ClientRpc]
        private void NotifySaleClientRpc(float tranquilValue, float violentValue, float miscValue, string itemName)
        {
            // Reconstruct ScienceData from individual values
            ScienceData data = new ScienceData
            {
                RawTranquilValue = tranquilValue,
                RawViolentValue = violentValue,
                RawMiscValue = miscValue,
                KeyName = itemName
            };

            // Publish event for ResearchDeposit to listen to
            if (IsOwner)
            {
                Debug.Log($"[Client] Item sold: {itemName} (T:{tranquilValue}, V:{violentValue}, M:{miscValue})");
                EventBus.Instance.Publish<ItemSoldEvent>(new ItemSoldEvent { SoldItemData = data });
            }
        }

        #endregion

        #region R6: State Sync (NetworkVariable Callbacks)

        /// <summary>
        /// Callback when big item NetworkVariable changes.
        /// Rebuilds local BigItemCarried cache from network state.
        /// </summary>
        private void HandleBigItemChanged(NetworkObjectReference oldRef, NetworkObjectReference newRef)
        {
            if (newRef.TryGet(out NetworkObject itemNetObj))
            {
                BigItemCarried = itemNetObj.GetComponent<BaseInventoryItem>();
            }
            else
            {
                BigItemCarried = null;
            }
        }

        /// <summary>
        /// Callback when current index NetworkVariable changes.
        /// Rebuilds local _currentIndex cache and publishes EventBus event.
        /// </summary>
        private void HandleCurrentIndexChange(int oldIndex, int newIndex)
        {
            _currentIndex = newIndex;

            // Publish event for UI (only on owning client)
            if (IsOwner)
            {
                EventBus.Instance.Publish<InventorySlotIndexChangedEvent>(
                    new InventorySlotIndexChangedEvent { NewIndex = _currentIndex });
            }
        }

        /// <summary>
        /// Callback when inventory NetworkList changes.
        /// Rebuilds local InventoryItems cache from network state and publishes EventBus event.
        /// </summary>
        private void HandleInventoryListChange(NetworkListEvent<NetworkObjectReference> netlistEvent)
        {
            RebuildInventoryCache();

            // Publish event for UI (only on owning client)
            if (IsOwner)
            {
                EventBus.Instance.Publish<InventoryListModifiedEvent>(
                    new InventoryListModifiedEvent { NewInventory = InventoryItems });
            }
        }

        /// <summary>
        /// Rebuilds InventoryItems cache array from InventoryNetworkRefs (network state).
        /// This is the ONLY method that should modify InventoryItems array.
        /// </summary>
        private void RebuildInventoryCache()
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (i < InventoryNetworkRefs.Count && InventoryNetworkRefs[i].TryGet(out NetworkObject itemNetObj))
                {
                    InventoryItems[i] = itemNetObj.GetComponent<BaseInventoryItem>();
                }
                else
                {
                    InventoryItems[i] = null;
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if all inventory slots are occupied.
        /// </summary>
        /// <returns>True if all slots occupied</returns>
        private bool IsInventoryFull()
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == null)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Input Handlers

        /// <summary>
        /// Handles numeric key press (1-5) to switch inventory slots.
        /// </summary>
        /// <param name="index">1-based index from input (1-5)</param>
        private void HandlePressedSlot(int index)
        {
            if (_handsFull)
            {
                return;
            }

            // Convert 1-based input to 0-based index
            EquipSlot(index - 1);
        }

        #endregion
    }
}
