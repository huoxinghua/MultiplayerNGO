using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Code.Art.AnimationScripts.IK;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Outline = QuickOutline.Scripts.Outline;
using Timer = _Project.Code.Utilities.Utility.Timer;

namespace _Project.Code.Gameplay.NewItemSystem
{
    [RequireComponent(typeof(Outline))]
    public class BaseInventoryItem : NetworkBehaviour, IInteractable, IInventoryItem
    {
        [SerializeField] protected GameObject _heldVisual;

        //  [SerializeField] protected GameObject _heldVisualRPC;
        [SerializeField] protected BaseItemSO _itemSO;

        //To tell server and clients item is in player inventory. Do not render, collide, etc
        NetworkVariable<bool> IsPickedUp = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        //to tell server and clients item is in player hand. Render held version.
        NetworkVariable<bool> IsInHand = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        #region Casting to type demo

        //casting type to child? to get child specific properties
        /*if (item is WeaponSO weapon)
{
        Debug.Log("Damage: " + weapon.damage);
}*/

        #endregion
        
        [SerializeField] protected Rigidbody _rb;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Collider _collider;
        protected Outline OutlineEffect;
        protected GameObject _owner;

        [SerializeField] protected Transform CurrentHeldPosition;
        protected bool _hasOwner => _owner != null;
        protected bool _isInOwnerHand = false;

        [SerializeField] protected GameObject _currentHeldVisual;
        protected BaseHeldVisual CurrentHeldVisualScript { get; private set; }

        
        private PlayerInventory _currentPlayerInventory;
        public ulong OwnerID;

        //  protected GameObject _currentHeldVisualRPC;
        protected float _tranquilValue = 0;
        protected float _violentValue = 0;
        protected float _miscValue = 0;

        public Timer ItemCooldown = new Timer(0);

        #region SetupAndUpdates

        private void LateUpdate()
        {
            ItemCooldown.TimerUpdate(Time.deltaTime);
        }
        protected virtual void CustomNetworkSpawn()
        {
            IsPickedUp.OnValueChanged += OnPickedUpStateChanged;
            IsInHand.OnValueChanged += OnChangedInHandState;

            OnChangedInHandState(!IsInHand.Value, IsInHand.Value);
            OnPickedUpStateChanged(!IsPickedUp.Value, IsPickedUp.Value);
        }
        
        /// <summary>
        /// Call on children in update. Will keep item position locked similar to parenting
        /// </summary>
        protected virtual void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null) return;
            transform.position = CurrentHeldPosition.position;
            transform.rotation = CurrentHeldPosition.rotation;
        }
        /// <summary>
        /// Changes to IsPickedUp state. Not associated with in hand, just in inventory
        /// </summary>
        /// <param name="oldHeld">Last pickedup state</param>
        /// <param name="newHeld">new/current pickedup state</param>
        protected virtual void OnPickedUpStateChanged(bool oldHeld, bool newHeld)
        {
            _collider.enabled = !newHeld;
            _rb.isKinematic = newHeld;
            _renderer.enabled = !newHeld;
        }

        protected virtual void Awake()
        {
            ItemCooldown.Start();
            CurrentHeldVisualScript = _currentHeldVisual.GetComponent<BaseHeldVisual>();
            OutlineEffect = GetComponent<Outline>();
            if (OutlineEffect != null)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }
        }

        #endregion
        
        #region Interaction

        /// <summary>
        /// Called when player interacts with the scene object. Used to handle pickup
        /// </summary>
        /// <param name="interactingPlayer">The player obj that has interacted</param>
        public virtual void OnInteract(GameObject interactingPlayer)
        {
            var inv = interactingPlayer.GetComponent<PlayerInventory>();
            if (inv == null) return;

            inv.TryPickupItem();
            inv.DoPickup(this);
        }

        /// <summary>
        /// Called on change to player "hovering"/looking directly at the scene object. Changes outline based on hovering or not
        /// </summary>
        /// <param name="isHovering">Is the player hovering</param>
        public virtual void HandleHover(bool isHovering)
        {
            if (OutlineEffect != null)
            {
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
        }

        #endregion
        
        #region PickupLogic

        /// <summary>
        /// Call to inform the item of pickup. Only happens if inventory has space for item
        /// </summary>
        /// <param name="player"> For the players gameobject</param>
        /// <param name="playerHoldPosition">The position on the player used for holding items</param>
        /// <param name="networkObjectForPlayer"> The network object for ease of reference and sync</param>
        public virtual void PickupItem(GameObject player, Transform playerHoldPosition,
            NetworkObject networkObjectForPlayer)
        {
            //setting required variables for pickup
            _owner = player;
            _rb.isKinematic = true;
            _renderer.enabled = false;
            _collider.enabled = false;
            CurrentHeldPosition = networkObjectForPlayer.transform.GetChild(0).GetChild(1);
           
            if (!IsServer)
            {
                // Non-server clients request pickup
                Debug.Log("RequestSpawnServerRpc");
                RequestHandleCurrentVisServerRpc(networkObjectForPlayer);
                RequestPickupServerRpc(new NetworkObjectReference(networkObjectForPlayer));
            }
            else
            {
                // Host/server logic
                RequestSetIsPickedUpServerRpc();
                HandleCurrentHeldVisualClientRPC(new NetworkObjectReference(networkObjectForPlayer));
                HandleOwnershipChange(networkObjectForPlayer);
            }
        }
        
        
        /// <summary>
        ///  Changes ownership of the item when picked up. This one only sets parent to the player interacting
        /// </summary>
        /// <param name="networkObjectForPlayer">net obj of the player</param>
        private void HandleOwnershipChange(NetworkObject networkObjectForPlayer)
        {
            // Assign ownership to the player
            NetworkObject.ChangeOwnership(networkObjectForPlayer.OwnerClientId);
        }
        
        #region Pickup RPCs
        
        /// <summary>
        /// Requests server to call matching client RPC - Relays the networkobj reference
        /// </summary>
        /// <param name="netObjRef">The player networkobj reference</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestHandleCurrentVisServerRpc(NetworkObjectReference netObjRef)
        {
            HandleCurrentHeldVisualClientRPC(netObjRef);
        }
        
        /// <summary>
        /// Requests change in ownership to the player picking up - Calls another server RPC (probably could do the other RPC logic in here?)
        /// </summary>
        /// <param name="playerRef">The networkobj reference for player</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestPickupServerRpc(NetworkObjectReference playerRef)
        {
            if (!playerRef.TryGet(out NetworkObject playerObj)) return;
            _owner = playerObj.gameObject;
            // Use the same method for server/host
            HandleOwnershipChange(playerObj);
            RequestSetIsPickedUpServerRpc();
        }
        
        /// <summary>
        /// Requests the server to change IsPickedUp to true
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        void RequestSetIsPickedUpServerRpc()
        {
            IsPickedUp.Value = true;
            // <-- automatically triggers OnHeldStateChanged on all clients
        }
        
        /// <summary>
        /// Distributes _currentPlayerInventory to all clients. Sets it to the playerNetObj Refs PlayerInventory
        /// </summary>
        /// <param name="playerObjRef">The networkobj reference for player</param>
        [ClientRpc (RequireOwnership = false)]
        private void HandleCurrentHeldVisualClientRPC(NetworkObjectReference playerObjRef)
        {
            NetworkObject playerNetObj = null;
            if (playerObjRef.TryGet(out playerNetObj))
            {
                PlayerInventory playerInventory = null;
                if (playerNetObj.TryGetComponent<PlayerInventory>(out playerInventory))
                {
                    _currentPlayerInventory =  playerInventory;
                }
                
            }
        }
        #endregion
        #endregion
        
        #region DropLogic
        
        
        /// <summary>
        /// Call to inform item to drop. Clears Variables associated with being held or in inventory, Informs RPCS, and enables physics
        /// </summary>
        /// <param name="dropPoint">Point on player to drop item at</param>
        public virtual void DropItem(Transform dropPoint)
        {
            _owner = null;
            _renderer.enabled = true;
            if (NetworkManager.Singleton.IsClient)
            {
                RequestIsPickedUpDropServerRpc();
            }
            if (!IsServer)
            {
                RequestDropServerRpc();
            }
            else
            {
                NetworkObject.RemoveOwnership();
                DistributeDropClientRPC();
            }
            _rb.isKinematic = false;
            _collider.enabled = true;
        }


        #region DropRPCS
        
        /// <summary>
        /// Sets network variable for IsPickUp to false
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RequestIsPickedUpDropServerRpc()
        {
            IsPickedUp.Value = false;
        }
        /// <summary>
        /// Removes ownership for the items network object 
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RequestDropServerRpc()
        {
            NetworkObject.RemoveOwnership();
            DistributeDropClientRPC();
            Debug.Log($"[ServerRpc] Owner set to {NetworkObject.OwnerClientId}");
        }

        [ClientRpc(RequireOwnership = false)]
        private void DistributeDropClientRPC()
        {
            CurrentHeldPosition = null;
        }
        #endregion
        #endregion
        
        #region SwappingLogic
        /// <summary>
        /// Handles unequipping of an item. The function itself requests a server RPC to sync
        /// </summary>
        public virtual void UnequipItem()
        {
            SetInHandServerRpc(false);
            Debug.Log("[BaseInventoryItem] UnequipItem" + _currentHeldVisual);
        }

        /// <summary>
        /// Handles equipping of an item. The function itself requests a server RPC to sync
        /// </summary>
        public virtual void EquipItem()
        {
            SetInHandServerRpc(true);
        }

        /// <summary>
        /// Assigns the network variable IsInHand. Only call when item should leave or enter hand 
        /// </summary>
        /// <param name="inHand">Set IsInHand to inHand</param>
        [ServerRpc(RequireOwnership = false)]
        private void SetInHandServerRpc(bool inHand)
        {
            IsInHand.Value = inHand;
        }

        /// <summary>
        /// Called by switches to IsInHand network variable. Its a way to sync and display heldvisual and IK
        /// </summary>
        /// <param name="oldState">last value of IsInHand</param>
        /// <param name="newState">new/current value of IsInHand</param>
        private void OnChangedInHandState(bool oldState, bool newState)
        {
            if (_currentHeldVisual == null)
            {
                Debug.Log("SHOULDNT BE POSSIBLE TO PRINT THIS! IF SEEING THIS MAKE SURE YOU ASSIGNED CURRENT HELD VISUAL");
                return;
            }
            //I am sorry Sean
            if (_currentPlayerInventory == null)
            {
                Debug.Log("Welp, it was needed :(");
                StartCoroutine(WaitOnCurrentPlayerInventory(oldState, newState));
                return;
            }
            if (newState == false)
            {
                CurrentHeldVisualScript.IKUnequipped();
            }
            else
            {
                CurrentHeldVisualScript.IKEquipped(_currentPlayerInventory.ThisPlayerIKData);
            }
            CurrentHeldVisualScript.SetRendererActive(newState);
            _isInOwnerHand = newState;
        }

        /// <summary>
        /// Waits for the _currentPlayerInventory. Bad but necessary
        /// </summary>
        /// <param name="oldState">Should reflect OnChangedInHandState</param>
        /// <param name="newState">Should reflect OnChangedInHandState</param>
        /// <returns>Nothing, just waits</returns>
        IEnumerator WaitOnCurrentPlayerInventory(bool oldState, bool newState)
        {
            yield return new WaitUntil(() => _currentPlayerInventory != null);
            OnChangedInHandState(!IsInHand.Value, IsInHand.Value);
        }
        
        #endregion
        
        #region Junk
//junk code temp storage
        /*
        IEnumerator WaitForHeld(GameObject player, Transform playerHoldPosition, NetworkObject networkObjectForPlayer)
        {
            while (_currentHeldVisual == null)
            {
                yield return null;
            }

        }*/

        /*private IEnumerator AssignHeldVisualDelayed(NetworkObjectReference heldVisualRef)
        {
            NetworkObject netObj = null;
            while (!heldVisualRef.TryGet(out netObj))
                yield return null;
                Debug.Log("Should have worked?");
                if(_currentHeldVisual == null)
                _currentHeldVisual = netObj.gameObject;
        }*/
        /*private void HandleHeldPosition(NetworkObject playerObj, Transform playerHoldPosition)
        {
            _currentHeldVisual.transform.SetPositionAndRotation(CurrentHeldPosition.position,
                CurrentHeldPosition.rotation);
            transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.rotation = CurrentHeldPosition.rotation;
            OnChangedInHandState(false, false);
        }*/
        #endregion
        
        #region UseFunctions
        /// <summary>
        /// The base Use. This exists for children, and handles cooldown. If item has no cooldown, cooldown is ignored
        /// </summary>
        public virtual void UseItem()
        {
            if (_itemSO.ItemCooldown != 0)
            {
                if (ItemCooldown.IsComplete)
                {
                    ItemCooldown.Reset(_itemSO.ItemCooldown);
                }
                else
                {
                    return;
                }
            }
        }
        /// <summary>
        /// Base for secondary use. As of now no secondary use cooldown. They will tend to involve holding the key
        /// </summary>
        /// <param name="isPerformed">True = input pressed - False = input released</param>
        public virtual void SecondaryUse(bool isPerformed)
        {
            
        }
        #endregion
        
        #region Getters

        public virtual string GetItemName()
        {
            return _itemSO.ItemName;
        }

        public virtual bool IsPocketSize()
        {
            return _itemSO.IsPocketSize;
        }

        public virtual GameObject GetHeldVisual()
        {
            return _heldVisual;
        }

        /*   public virtual GameObject GetHeldVisualRPC()
           {
               return _currentHeldVisualRPC;
           }*/
        public virtual Image GetUIImage()
        {
            return _itemSO.ItemUIImage;
        }

        public virtual bool CanBeSold()
        {
            return _itemSO.CanBeSold;
        }

        #endregion

        #region SellingLogic
        public virtual void WasSold()
        {
            Destroy(_currentHeldVisual);
            Destroy(gameObject);
        }

        public virtual ScienceData GetValueStruct()
        {
            return new ScienceData
            {
                RawTranquilValue = _tranquilValue, RawMiscValue = _miscValue, RawViolentValue = _violentValue,
                KeyName = _itemSO.ItemName
            };
        }
        #endregion
    }
}