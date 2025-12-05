using System;
using _Project.Code.Art.AnimationScripts.IK;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;
using Timer = _Project.Code.Utilities.Utility.Timer;

namespace _Project.Code.Gameplay.NewItemSystem
{
    [RequireComponent(typeof(Outline))]
    public class BaseInventoryItem : NetworkBehaviour, IInteractable, IInventoryItem
    {
        #region Serialized Fields

        [Header("Item Configuration")]
        [SerializeField] 
        protected BaseItemSO _itemSO;

        [Header("Visual Components - Pre-assigned Children")]
        [SerializeField] 
        protected GameObject _fpsHeldVisualChild;

        [SerializeField] 
        protected GameObject _tpsHeldVisualChild;

        protected BaseHeldVisual _fpsHeldVisualScript;
        protected BaseHeldVisual _tpsHeldVisualScript;
        protected IKAnimState _queuedItemAnimState;
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


        private NetworkVariable<bool> IsPickedUp = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public bool IsCurrentlyHeld => IsPickedUp.Value;



        private NetworkVariable<bool> IsInHand = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public NetworkVariable<IKAnimState> CurrentAnimState = new NetworkVariable<IKAnimState>(
            IKAnimState.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);


        public NetworkVariable<float> AnimTime = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        #endregion

        #region Internal Item State

        private enum UsageState
        {
            Idle,
            InUse,
            Cooldown
        }

        private UsageState _usageState = UsageState.Idle;

        #endregion

        #region Server-Only State


        protected GameObject _owner;


        protected Transform CurrentHeldPosition;


        protected bool _hasOwner => _owner != null;

        #endregion

        #region Component References


        protected Outline OutlineEffect;


        private PlayerInventory _currentPlayerInventory;

        #endregion

        #region Item Values (For Selling)


        protected float _tranquilValue = 0;


        protected float _violentValue = 0;


        protected float _miscValue = 0;

        #endregion

        #region Cooldown System


        public Timer ItemCooldown = new Timer(0);

        #endregion
        
      
        #region Initialization

        protected virtual void Awake()
        {
            // Initialize cooldown duration (but don't start timer yet - only starts on first use)
            if (_itemSO != null)
            {
                ItemCooldown.Reset(_itemSO.ItemCooldown);
                ItemCooldown.ForceComplete(); // Mark as complete so item can be used immediately
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] _itemSO is null in Awake()");
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
            CurrentAnimState.OnValueChanged += OnAnimStateChanged;

            // Apply initial state for late joiners
            OnPickedUpStateChanged(!IsPickedUp.Value, IsPickedUp.Value);
            OnChangedInHandState(!IsInHand.Value, IsInHand.Value);
        }
        

        protected virtual void LateUpdate() 
        {
            // Update cooldown timer
            ItemCooldown.TimerUpdate(Time.deltaTime);

            // Complete usage when BOTH cooldown AND animation are complete
            if (_usageState == UsageState.InUse && ItemCooldown.IsComplete)
            {
                // Check if FPS animation is complete (owner sees FPS)
                bool animComplete = _fpsHeldVisualScript?.HeldIKInteractable?.IsInteractComplete ?? true;


                if (animComplete)
                {
                    CompleteUsage();
                }
            }
        }

        #endregion

        #region Position Update (Override in Child Classes)

        protected virtual void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null)
            {
                return;
            }
           // transform.position = CurrentHeldPosition.position;
            //transform.rotation = CurrentHeldPosition.rotation;
        }

        #endregion

        #region IInteractable Implementation (Player Interaction)

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
            CurrentHeldPosition = fpsItemParent;

            // Transfer ownership to player
            NetworkObject.ChangeOwnership(networkObjectForPlayer.OwnerClientId);

            // Position at hand (NetworkObjects can't be parented to non-NetworkObjects)
            transform.position = fpsItemParent.position;
            transform.rotation = fpsItemParent.rotation;

            // Distribute player inventory reference and setup held visuals on server
            DistributeHeldVisualReferenceClientRpc(new NetworkObjectReference(networkObjectForPlayer));
            SetupHeldVisualsServerSide(networkObjectForPlayer);

            // Set picked up state - callback will handle physics/rendering on all clients
            IsPickedUp.Value = true;
        }

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
        }

        #endregion

        #region Server-Only Drop Logic

        public virtual void DropItem(Vector3 dropPosition)
        {
            // Guard: This method should ONLY run on server
            if (!IsServer)
            {
                Debug.LogError("[BaseInventoryItem] DropItem() called on client! This should only run on server.");
                return;
            }
            // Position item at drop point
            transform.position = dropPosition;
            _rb.position = dropPosition;

            // Server-only logic
            _owner = null;

            // Remove ownership (becomes server-owned)
            NetworkObject.RemoveOwnership();

            // Reset held visuals back to item on server
            ResetHeldVisualsServerSide();
            

            // Set picked up state to false - callback will handle physics/rendering on all clients
            IsPickedUp.Value = false;
        }



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
        }

        [ClientRpc(RequireOwnership = false)]
        private void HideHeldVisualsClientRpc()
        {
            if (_fpsHeldVisualScript != null)
            {
                _fpsHeldVisualScript.IKUnequipped();
            }
            if (_tpsHeldVisualScript != null)
            {
                _tpsHeldVisualScript.IKUnequipped();
            }

            if (_fpsHeldVisualChild != null) _fpsHeldVisualChild.SetActive(false);
            if (_tpsHeldVisualChild != null) _tpsHeldVisualChild.SetActive(false);

            _fpsHeldVisualScript = null;
            _tpsHeldVisualScript = null;
            _currentPlayerInventory = null;
        }

        #endregion

        #region Server-Only Equip/Unequip Logic

        public virtual void EquipItem()
        {
            SetInHandServerRpc(true);
        }

        public virtual void UnequipItem()
        {
            SetInHandServerRpc(false);
        }

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

        protected virtual void OnPickedUpStateChanged(bool oldState, bool newState)
        {
            // When picked up: disable collision, enable kinematic, hide renderer
            // When dropped: enable collision, disable kinematic, show renderer
            _collider.enabled = !newState;
            _rb.isKinematic = newState;
            _renderer.enabled = !newState;
           // transform.position = new Vector3(0, 0, 0);
        }

        private void OnChangedInHandState(bool oldState, bool newState)
        {
            if (_currentPlayerInventory == null)
            {
                return;
            }

            bool isOwner = _currentPlayerInventory.IsOwner;

            if (newState) // Equipped
            {
                if (isOwner)
                {
                    if (_fpsHeldVisualChild != null) _fpsHeldVisualChild.SetActive(true);
                    if (_tpsHeldVisualChild != null) _tpsHeldVisualChild.SetActive(false);
                }
                else
                {
                    if (_fpsHeldVisualChild != null) _fpsHeldVisualChild.SetActive(false);
                    if (_tpsHeldVisualChild != null) _tpsHeldVisualChild.SetActive(true);
                }

                if (_fpsHeldVisualScript != null)
                {
                    _fpsHeldVisualScript.SetRendererActive(true);
                    _fpsHeldVisualScript.IKEquipped(_currentPlayerInventory.ThisPlayerIKData.FPSIKController, isFPS: true);
                }

                if (_tpsHeldVisualScript != null)
                {
                    _tpsHeldVisualScript.SetRendererActive(true);
                    _tpsHeldVisualScript.IKEquipped(_currentPlayerInventory.ThisPlayerIKData.TPSIKController, isFPS: false);
                }

                if (IsOwner)
                {
                    CurrentAnimState.Value = IKAnimState.Idle;
                    AnimTime.Value = 0f;
                }
            }
            else // Unequipped
            {
                if (_fpsHeldVisualChild != null) _fpsHeldVisualChild.SetActive(false);
                if (_tpsHeldVisualChild != null) _tpsHeldVisualChild.SetActive(false);

                if (_fpsHeldVisualScript != null)
                {
                    _fpsHeldVisualScript.SetRendererActive(false);
                    _fpsHeldVisualScript.IKUnequipped();
                }

                if (_tpsHeldVisualScript != null)
                {
                    _tpsHeldVisualScript.SetRendererActive(false);
                    _tpsHeldVisualScript.IKUnequipped();
                }
            }
        }

        private void OnAnimStateChanged(IKAnimState oldState, IKAnimState newState)
        {
            if (newState == IKAnimState.None) return;

            if (_fpsHeldVisualScript != null && _fpsHeldVisualScript.HeldIKInteractable != null)
            {
                _fpsHeldVisualScript.HeldIKInteractable.SetAnimState(newState, true);
            }

            if (_tpsHeldVisualScript != null && _tpsHeldVisualScript.HeldIKInteractable != null)
            {
                _tpsHeldVisualScript.HeldIKInteractable.SetAnimState(newState, false);
            }
        }

        #endregion

        #region Movement Sync

        public void NotifyMovementChanged(bool isMoving, bool isRunning, bool isCrouching)
        {
            if (!IsOwner) return;
            _queuedItemAnimState = DetermineAnimationFromMovement(isMoving, isRunning, isCrouching);
            if (_usageState == UsageState.InUse)
            {
                return;
            }

            

            if (CurrentAnimState.Value != _queuedItemAnimState)
            {
                CurrentAnimState.Value = _queuedItemAnimState;
            }
        }

        private IKAnimState DetermineAnimationFromMovement(bool isMoving, bool isRunning, bool isCrouching)
        {
            if (!isMoving) return IKAnimState.Idle;
            if (isRunning) return IKAnimState.Run;
            if (isCrouching) return IKAnimState.CrouchWalk;
            return IKAnimState.Walk;
        }

        #endregion

        #region Item Usage (Override in Child Classes)

        public virtual bool TryUse()
        {
            if (!CanUse()) return false;

            StartUsage();
            return true;
        }

        protected virtual bool CanUse()
        {
            if (!IsOwner) return false;
            if (_usageState != UsageState.Idle) return false;

            bool noCooldown = _itemSO.ItemCooldown == 0;
            bool cooldownReady = ItemCooldown.IsComplete;
            return noCooldown || cooldownReady;
        }

        protected virtual void StartUsage()
        {
            _usageState = UsageState.InUse;
            ItemCooldown.Reset(_itemSO.ItemCooldown);

            CurrentAnimState.Value = IKAnimState.Interact;

            ExecuteUsageLogic();
        }

        protected virtual void ExecuteUsageLogic()
        {
        }

        protected void CompleteUsage()
        {
            _usageState = UsageState.Idle;
            CurrentAnimState.Value = _queuedItemAnimState;
        }

        public virtual void SecondaryUse(bool isPerformed)
        {
            // Base implementation does nothing - override in child classes
        }

        #endregion

        #region IInventoryItem Interface Implementation
        
        public virtual string GetItemName()
        {
            return _itemSO != null ? _itemSO.ItemName : "Unknown Item";
        }
        
        public virtual bool IsPocketSize()
        {
            return _itemSO != null && _itemSO.IsPocketSize;
        }
        
        public virtual GameObject GetHeldVisual()
        {
            return _heldVisual;
        }
        
        public virtual Sprite GetUIImage()
        {
            return _itemSO != null ? _itemSO.ItemUIImage : null;
        }
        
        public virtual bool CanBeSold()
        {
            return _itemSO != null && _itemSO.CanBeSold;
        }

        #endregion

        #region Selling Logic

        public virtual void WasSold()
        {
            if (_currentHeldVisual != null)
            {
                Destroy(_currentHeldVisual);
            }
            Destroy(gameObject);
        }

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
