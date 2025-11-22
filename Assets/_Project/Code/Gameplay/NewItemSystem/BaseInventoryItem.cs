using _Project.Code.Art.AnimationScripts.IK;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;
using Timer = _Project.Code.Utilities.Utility.Timer;

namespace _Project.Code.Gameplay.NewItemSystem
{
    /// <summary>
    /// Clean Rewrite: Base class for all inventory items in multiplayer game.
    /// Implements server-authoritative pickup/drop/equip with proper NetworkVariable patterns.
    /// All visual/physics changes driven by NetworkVariable callbacks for perfect sync.
    /// </summary>
    [RequireComponent(typeof(Outline))]
    public class BaseInventoryItem : NetworkBehaviour, IInteractable, IInventoryItem
    {
        #region Serialized Fields

        [Header("Item Configuration")]
        [SerializeField] [Tooltip("ScriptableObject containing item data")]
        protected BaseItemSO _itemSO;

        [Header("Visual Components - Pre-assigned Children")]
        [SerializeField] [Tooltip("FPS held visual child GameObject (pre-assigned on item prefab)")]
        protected GameObject _fpsHeldVisualChild;

        [SerializeField] [Tooltip("TPS held visual child GameObject (pre-assigned on item prefab)")]
        protected GameObject _tpsHeldVisualChild;

        protected BaseHeldVisual _fpsHeldVisualScript;
        protected BaseHeldVisual _tpsHeldVisualScript;

        // Legacy fields for backward compatibility
        [Header("Legacy - Deprecated")]
        [SerializeField] protected GameObject _heldVisual;
        protected GameObject _currentHeldVisual;

        [Header("Physics Components")]
        [SerializeField] protected Rigidbody _rb;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Collider _collider;

        #endregion

        #region Network State (Server-Authoritative)

        /// <summary>
        /// Server-authoritative: Is this item picked up by a player?
        /// When true: physics disabled, renderer hidden, collider disabled.
        /// </summary>
        private NetworkVariable<bool> IsPickedUp = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>
        /// Server-authoritative: Is this item currently equipped in player's hand?
        /// When true: held visual shown, IK applied.
        /// </summary>
        private NetworkVariable<bool> IsInHand = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        #endregion

        #region Server-Only State

        /// <summary>
        /// Server-only: GameObject of player who owns this item.
        /// Only valid on server, null on clients.
        /// </summary>
        protected GameObject _owner;

        /// <summary>
        /// Server-only: Transform where item should be held.
        /// Only valid on server, null on clients.
        /// </summary>
        protected Transform CurrentHeldPosition;

        /// <summary>
        /// Returns true if item has an owner (server-only).
        /// </summary>
        protected bool _hasOwner => _owner != null;

        #endregion

        #region Component References

        /// <summary>
        /// Outline effect for hover indication.
        /// </summary>
        protected Outline OutlineEffect;

        /// <summary>
        /// Player inventory reference (set via ClientRpc when picked up).
        /// </summary>
        private PlayerInventory _currentPlayerInventory;

        #endregion

        #region Item Values (For Selling)

        /// <summary>
        /// Item's tranquil research value (generated on server for enemy drops).
        /// </summary>
        protected float _tranquilValue = 0;

        /// <summary>
        /// Item's violent research value (generated on server for enemy drops).
        /// </summary>
        protected float _violentValue = 0;

        /// <summary>
        /// Item's miscellaneous research value (generated on server for enemy drops).
        /// </summary>
        protected float _miscValue = 0;

        #endregion

        #region Cooldown System

        /// <summary>
        /// Cooldown timer for item usage.
        /// Initialized from _itemSO.ItemCooldown in Awake().
        /// </summary>
        public Timer ItemCooldown = new Timer(0);

        #endregion

        #region Initialization

        protected virtual void Awake()
        {
            // Initialize cooldown duration (but don't start timer yet - only starts on first use)
            if (_itemSO != null)
            {
                Debug.Log($"[{gameObject.name}] BEFORE ForceComplete - SODuration:{_itemSO.ItemCooldown}");
                ItemCooldown.Reset(_itemSO.ItemCooldown);
                ItemCooldown.ForceComplete(); // Mark as complete so item can be used immediately
                Debug.Log($"[{gameObject.name}] AFTER ForceComplete - IsComplete:{ItemCooldown.IsComplete}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] _itemSO is null in Awake()");
            }

            // Validate held visual prefab assignment
            if (_heldVisual == null)
            {
                Debug.LogWarning($"[{gameObject.name}] _heldVisual not assigned in Inspector");
            }

            // Initialize outline effect
            OutlineEffect = GetComponent<Outline>();
            if (OutlineEffect != null)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }

            // Hide held visuals initially (only shown when picked up and equipped)
            if (_fpsHeldVisualChild != null) _fpsHeldVisualChild.SetActive(false);
            if (_tpsHeldVisualChild != null) _tpsHeldVisualChild.SetActive(false);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Register NetworkVariable callbacks
            IsPickedUp.OnValueChanged += OnPickedUpStateChanged;
            IsInHand.OnValueChanged += OnChangedInHandState;

            // Apply initial state for late joiners
            OnPickedUpStateChanged(!IsPickedUp.Value, IsPickedUp.Value);
            OnChangedInHandState(!IsInHand.Value, IsInHand.Value);
        }

        protected virtual void LateUpdate()
        {
            // Update cooldown timer
            ItemCooldown.TimerUpdate(Time.deltaTime);
        }

        #endregion

        #region Position Update (Override in Child Classes)

        /// <summary>
        /// Override this in child classes to update item position when held.
        /// Call this in Update() only if IsOwner.
        /// Used to manually lock position to hand transform (alternative to parenting).
        /// </summary>
        protected virtual void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null)
            {
                return;
            }

            transform.position = CurrentHeldPosition.position;
            transform.rotation = CurrentHeldPosition.rotation;
        }

        #endregion

        #region IInteractable Implementation (Player Interaction)

        /// <summary>
        /// Called when player interacts with this item in the world.
        /// Attempts to pick up the item if player has space.
        /// </summary>
        /// <param name="interactingPlayer">GameObject of player interacting</param>
        public virtual void OnInteract(GameObject interactingPlayer)
        {
            PlayerInventory inv = interactingPlayer.GetComponent<PlayerInventory>();
            if (inv == null)
            {
                Debug.LogWarning("[BaseInventoryItem] Interacting player has no PlayerInventory component");
                return;
            }

            // Check if player has space (local check, server does final validation)
            if (inv.TryPickupItem())
            {
                inv.DoPickup(this);
            }
        }

        /// <summary>
        /// Called when player hovers over (looks at) this item.
        /// Enables/disables outline effect for visual feedback.
        /// </summary>
        /// <param name="isHovering">True if player is looking at item</param>
        public virtual void HandleHover(bool isHovering)
        {
            if (OutlineEffect == null)
            {
                return;
            }

            // Don't show outline if item is picked up
            if (_hasOwner)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                return;
            }

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

        #endregion

        #region Server-Only Pickup Logic

        /// <summary>
        /// SERVER-ONLY: Executes pickup logic on server.
        /// Sets ownership, updates network state, spawns dual held visuals.
        /// Visual/physics changes handled by OnPickedUpStateChanged callback.
        /// </summary>
        /// <param name="player">GameObject of player picking up item</param>
        /// <param name="fpsItemParent">Transform on FPS model where item is parented</param>
        /// <param name="tpsItemParent">Transform on TPS model where item is parented</param>
        /// <param name="networkObjectForPlayer">Player's NetworkObject for ownership transfer</param>
        public virtual void PickupItem(GameObject player, Transform fpsItemParent, Transform tpsItemParent, NetworkObject networkObjectForPlayer)
        {
            // Guard: This method should ONLY run on server
            if (!IsServer)
            {
                Debug.LogError("[BaseInventoryItem] PickupItem() called on client! This should only run on server.");
                return;
            }

            // Server-only logic
            _owner = player;
            CurrentHeldPosition = fpsItemParent; // Keep for backward compatibility (parenting to FPS)

            // Transfer ownership to player
            NetworkObject.ChangeOwnership(networkObjectForPlayer.OwnerClientId);

            // Parent item to FPS transform (primary parent)
            transform.SetParent(fpsItemParent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Distribute player inventory reference and setup held visuals on server
            DistributeHeldVisualReferenceClientRpc(new NetworkObjectReference(networkObjectForPlayer));
            SetupHeldVisualsServerSide(networkObjectForPlayer);

            // Set picked up state - callback will handle physics/rendering on all clients
            IsPickedUp.Value = true;
        }

        /// <summary>
        /// Distributes player inventory reference to all clients.
        /// Needed so item can access IK data when equipped.
        /// </summary>
        /// <param name="playerObjRef">NetworkObjectReference to player</param>
        [ClientRpc(RequireOwnership = false)]
        private void DistributeHeldVisualReferenceClientRpc(NetworkObjectReference playerObjRef)
        {
            if (playerObjRef.TryGet(out NetworkObject playerNetObj))
            {
                if (playerNetObj.TryGetComponent<PlayerInventory>(out PlayerInventory playerInventory))
                {
                    _currentPlayerInventory = playerInventory;
                }
                else
                {
                    Debug.LogWarning("[BaseInventoryItem] Player NetworkObject has no PlayerInventory component");
                }
            }
        }

        /// <summary>
        /// Spawns FPS and TPS held visual instances on all clients.
        /// Sets visibility based on ownership (FPS for owner, TPS for others).
        /// </summary>
        /// <param name="playerObjRef">NetworkObjectReference to player</param>
        private void SetupHeldVisualsServerSide(NetworkObject playerNetObj)
        {
            if (!IsServer) return;

            PlayerInventory playerInv = playerNetObj.GetComponent<PlayerInventory>();
            if (playerInv == null)
            {
                Debug.LogError($"[{gameObject.name}] Player NetworkObject has no PlayerInventory component");
                return;
            }

            // Validate pre-assigned children
            if (_fpsHeldVisualChild == null || _tpsHeldVisualChild == null)
            {
                Debug.LogError($"[{gameObject.name}] FPS or TPS held visual children not assigned in Inspector!");
                return;
            }

            // Get hold transforms from player
            Transform fpsParent = playerInv.GetFPSItemParent();
            Transform tpsParent = playerInv.GetTPSItemParent();

            if (fpsParent == null || tpsParent == null)
            {
                Debug.LogError($"[{gameObject.name}] FPS or TPS ItemParent is null on player");
                return;
            }

            // Reparent FPS held visual to player's FPS hand
            _fpsHeldVisualChild.transform.SetParent(fpsParent);
            _fpsHeldVisualChild.transform.localPosition = Vector3.zero;
            _fpsHeldVisualChild.transform.localRotation = Quaternion.identity;
            _fpsHeldVisualScript = _fpsHeldVisualChild.GetComponent<BaseHeldVisual>();

            // Reparent TPS held visual to player's TPS hand
            _tpsHeldVisualChild.transform.SetParent(tpsParent);
            _tpsHeldVisualChild.transform.localPosition = Vector3.zero;
            _tpsHeldVisualChild.transform.localRotation = Quaternion.identity;
            _tpsHeldVisualScript = _tpsHeldVisualChild.GetComponent<BaseHeldVisual>();

            // Sync visibility to all clients
            SyncHeldVisualVisibilityClientRpc(new NetworkObjectReference(playerNetObj));

            Debug.Log($"[{gameObject.name}] Reparented held visuals to player hands");
        }

        [ClientRpc(RequireOwnership = false)]
        private void SyncHeldVisualVisibilityClientRpc(NetworkObjectReference playerRef)
        {
            if (!playerRef.TryGet(out NetworkObject playerNetObj))
            {
                Debug.LogError($"[{gameObject.name}] Failed to resolve player NetworkObject in SyncHeldVisualVisibilityClientRpc");
                return;
            }

            PlayerInventory playerInv = playerNetObj.GetComponent<PlayerInventory>();
            if (playerInv == null)
            {
                Debug.LogError($"[{gameObject.name}] Player has no PlayerInventory component");
                return;
            }

            // Get player's hand transforms
            Transform fpsParent = playerInv.GetFPSItemParent();
            Transform tpsParent = playerInv.GetTPSItemParent();

            if (fpsParent == null || tpsParent == null)
            {
                Debug.LogError($"[{gameObject.name}] FPS or TPS ItemParent is null on player");
                return;
            }

            // Reparent FPS held visual to player's FPS hand (on ALL clients)
            if (_fpsHeldVisualChild != null)
            {
                _fpsHeldVisualChild.transform.SetParent(fpsParent);
                _fpsHeldVisualChild.transform.localPosition = Vector3.zero;
                _fpsHeldVisualChild.transform.localRotation = Quaternion.identity;
                _fpsHeldVisualScript = _fpsHeldVisualChild.GetComponent<BaseHeldVisual>();
            }

            // Reparent TPS held visual to player's TPS hand (on ALL clients)
            if (_tpsHeldVisualChild != null)
            {
                _tpsHeldVisualChild.transform.SetParent(tpsParent);
                _tpsHeldVisualChild.transform.localPosition = Vector3.zero;
                _tpsHeldVisualChild.transform.localRotation = Quaternion.identity;
                _tpsHeldVisualScript = _tpsHeldVisualChild.GetComponent<BaseHeldVisual>();
            }

            // Set visibility based on ownership
            bool isOwner = playerNetObj.IsOwner;
            _fpsHeldVisualChild.SetActive(isOwner);   // Owner sees FPS
            _tpsHeldVisualChild.SetActive(!isOwner);  // Others see TPS

            // Ensure renderers start disabled (will be enabled when equipped)
            if (_fpsHeldVisualScript != null) _fpsHeldVisualScript.SetRendererActive(false);
            if (_tpsHeldVisualScript != null) _tpsHeldVisualScript.SetRendererActive(false);

            Debug.Log($"[{gameObject.name}] Reparented and synced held visuals - FPS active: {isOwner}, TPS active: {!isOwner}");
        }

        #endregion

        #region Server-Only Drop Logic

        /// <summary>
        /// SERVER-ONLY: Executes drop logic on server.
        /// Clears ownership, updates network state, destroys held visuals, spawns item in world.
        /// Visual/physics changes handled by OnPickedUpStateChanged callback.
        /// </summary>
        /// <param name="dropPoint">Transform where item should be dropped</param>
        public virtual void DropItem(Transform dropPoint)
        {
            // Guard: This method should ONLY run on server
            if (!IsServer)
            {
                Debug.LogError("[BaseInventoryItem] DropItem() called on client! This should only run on server.");
                return;
            }

            // Server-only logic
            _owner = null;

            // Remove ownership (becomes server-owned)
            NetworkObject.RemoveOwnership();

            // Unparent from player
            transform.SetParent(null);

            // Position item at drop point
            if (dropPoint != null)
            {
                transform.position = dropPoint.position;
                transform.rotation = dropPoint.rotation;
            }

            // Reset held visuals back to item on server
            ResetHeldVisualsServerSide();

            // Clear held position reference on all clients
            ClearHeldPositionClientRpc();

            // Set picked up state to false - callback will handle physics/rendering on all clients
            IsPickedUp.Value = false;
        }

        /// <summary>
        /// Clears CurrentHeldPosition on all clients.
        /// </summary>
        [ClientRpc(RequireOwnership = false)]
        private void ClearHeldPositionClientRpc()
        {
            CurrentHeldPosition = null;
        }

        /// <summary>
        /// Reparents held visuals back to item and hides them.
        /// </summary>
        private void ResetHeldVisualsServerSide()
        {
            if (!IsServer) return;

            if (_fpsHeldVisualChild == null || _tpsHeldVisualChild == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Held visual children are null, cannot reset");
                return;
            }

            // Reparent FPS held visual back to item
            _fpsHeldVisualChild.transform.SetParent(transform);
            _fpsHeldVisualChild.transform.localPosition = Vector3.zero;
            _fpsHeldVisualChild.transform.localRotation = Quaternion.identity;

            // Reparent TPS held visual back to item
            _tpsHeldVisualChild.transform.SetParent(transform);
            _tpsHeldVisualChild.transform.localPosition = Vector3.zero;
            _tpsHeldVisualChild.transform.localRotation = Quaternion.identity;

            // Hide both visuals on all clients
            HideHeldVisualsClientRpc();

            Debug.Log($"[{gameObject.name}] Reset held visuals back to item");
        }

        [ClientRpc(RequireOwnership = false)]
        private void HideHeldVisualsClientRpc()
        {
            if (_fpsHeldVisualChild != null) _fpsHeldVisualChild.SetActive(false);
            if (_tpsHeldVisualChild != null) _tpsHeldVisualChild.SetActive(false);

            _fpsHeldVisualScript = null;
            _tpsHeldVisualScript = null;
        }

        #endregion

        #region Server-Only Equip/Unequip Logic

        /// <summary>
        /// Equips this item (shows in hand, applies IK).
        /// Sends ServerRpc to set IsInHand to true.
        /// </summary>
        public virtual void EquipItem()
        {
            SetInHandServerRpc(true);
        }

        /// <summary>
        /// Unequips this item (hides from hand, removes IK).
        /// Sends ServerRpc to set IsInHand to false.
        /// </summary>
        public virtual void UnequipItem()
        {
            SetInHandServerRpc(false);
        }

        /// <summary>
        /// SERVER-ONLY: Sets IsInHand NetworkVariable.
        /// NetworkVariable callback will handle visual/IK changes on all clients.
        /// </summary>
        /// <param name="inHand">True to equip, false to unequip</param>
        [ServerRpc(RequireOwnership = false)]
        private void SetInHandServerRpc(bool inHand)
        {
            // Validate item is picked up before equipping
            if (!IsPickedUp.Value && inHand)
            {
                Debug.LogWarning("[Server] Attempting to equip item that isn't picked up");
                return;
            }

            IsInHand.Value = inHand;
        }

        #endregion

        #region NetworkVariable Callbacks (Visual/Physics Sync)

        /// <summary>
        /// Callback when IsPickedUp changes.
        /// Enables/disables physics and rendering based on pickup state.
        /// Runs on all clients for perfect sync.
        /// </summary>
        /// <param name="oldState">Previous pickup state</param>
        /// <param name="newState">New pickup state</param>
        protected virtual void OnPickedUpStateChanged(bool oldState, bool newState)
        {
            // When picked up: disable collision, enable kinematic, hide renderer
            // When dropped: enable collision, disable kinematic, show renderer
            _collider.enabled = !newState;
            _rb.isKinematic = newState;
            _renderer.enabled = !newState;
        }

        /// <summary>
        /// Callback when IsInHand changes.
        /// Shows/hides FPS and TPS held visuals and applies/removes IK to each.
        /// Runs on all clients for perfect sync.
        /// </summary>
        /// <param name="oldState">Previous equipped state</param>
        /// <param name="newState">New equipped state</param>
        private void OnChangedInHandState(bool oldState, bool newState)
        {
            // Validate player inventory reference exists
            if (_currentPlayerInventory == null)
            {
                Debug.LogWarning($"[{gameObject.name}] CurrentPlayerInventory not set yet (waiting for ClientRpc)");
                return;
            }

            // Apply or remove IK for both FPS and TPS visuals
            if (newState) // Equipped
            {
                // Setup FPS held visual
                if (_fpsHeldVisualScript != null)
                {
                    _fpsHeldVisualScript.SetRendererActive(true);
                    _fpsHeldVisualScript.IKEquipped(_currentPlayerInventory.ThisPlayerIKData.FPSIKController, isFPS: true);
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] FPS held visual script is null");
                }

                // Setup TPS held visual
                if (_tpsHeldVisualScript != null)
                {
                    _tpsHeldVisualScript.SetRendererActive(true);
                    _tpsHeldVisualScript.IKEquipped(_currentPlayerInventory.ThisPlayerIKData.TPSIKController, isFPS: false);
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] TPS held visual script is null");
                }
            }
            else // Unequipped
            {
                // Cleanup FPS held visual
                if (_fpsHeldVisualScript != null)
                {
                    _fpsHeldVisualScript.SetRendererActive(false);
                    _fpsHeldVisualScript.IKUnequipped();
                }

                // Cleanup TPS held visual
                if (_tpsHeldVisualScript != null)
                {
                    _tpsHeldVisualScript.SetRendererActive(false);
                    _tpsHeldVisualScript.IKUnequipped();
                }
            }
        }

        #endregion

        #region Item Usage (Override in Child Classes)

        /// <summary>
        /// Primary use of item. Override in child classes for item-specific behavior.
        /// Base implementation handles cooldown check and reset.
        /// Child classes should call base.UseItem() first, then add custom logic.
        /// </summary>
        public virtual void UseItem()
        {
            // Check cooldown - early return if not ready
            if (!TryUseItem())
            {
                Debug.Log($"[{gameObject.name}] UseItem blocked - item on cooldown");
                return;
            }

            // Reset cooldown timer for next use
            ItemCooldown.Reset(_itemSO.ItemCooldown);
        }

        /// <summary>
        /// Checks if item can be used (cooldown complete).
        /// </summary>
        /// <returns>True if item can be used</returns>
        protected virtual bool TryUseItem()
        {
            bool noCooldown = _itemSO.ItemCooldown == 0;
            bool isComplete = ItemCooldown.IsComplete;
            bool result = noCooldown || isComplete;
            Debug.Log($"[{gameObject.name}] TryUseItem - SOCooldown:{_itemSO.ItemCooldown}, NoCooldown:{noCooldown}, IsComplete:{isComplete}, Result:{result}");
            return result;
        }

        /// <summary>
        /// Secondary use of item (hold/release pattern).
        /// Override in child classes for item-specific behavior.
        /// </summary>
        /// <param name="isPerformed">True when button pressed, false when released</param>
        public virtual void SecondaryUse(bool isPerformed)
        {
            // Base implementation does nothing - override in child classes
        }

        #endregion

        #region IInventoryItem Interface Implementation

        /// <summary>
        /// Gets item name from ScriptableObject.
        /// </summary>
        public virtual string GetItemName()
        {
            return _itemSO != null ? _itemSO.ItemName : "Unknown Item";
        }

        /// <summary>
        /// Gets whether item is pocket-size (goes in inventory) or big (held in hands).
        /// </summary>
        public virtual bool IsPocketSize()
        {
            return _itemSO != null && _itemSO.IsPocketSize;
        }

        /// <summary>
        /// Gets held visual GameObject.
        /// </summary>
        public virtual GameObject GetHeldVisual()
        {
            return _heldVisual;
        }

        /// <summary>
        /// Gets UI sprite for inventory display.
        /// </summary>
        public virtual Sprite GetUIImage()
        {
            return _itemSO != null ? _itemSO.ItemUIImage : null;
        }

        /// <summary>
        /// Gets whether item can be sold.
        /// </summary>
        public virtual bool CanBeSold()
        {
            return _itemSO != null && _itemSO.CanBeSold;
        }

        #endregion

        #region Selling Logic

        /// <summary>
        /// SERVER-ONLY: Called when item is sold.
        /// Destroys held visual and item GameObject.
        /// </summary>
        public virtual void WasSold()
        {
            if (_currentHeldVisual != null)
            {
                Destroy(_currentHeldVisual);
            }
            Destroy(gameObject);
        }

        /// <summary>
        /// Gets item's science values for selling.
        /// </summary>
        /// <returns>ScienceData struct with item values</returns>
        public virtual ScienceData GetValueStruct()
        {
            return new ScienceData
            {
                RawTranquilValue = _tranquilValue,
                RawViolentValue = _violentValue,
                RawMiscValue = _miscValue,
                KeyName = GetItemName()
            };
        }

        #endregion
    }
}
