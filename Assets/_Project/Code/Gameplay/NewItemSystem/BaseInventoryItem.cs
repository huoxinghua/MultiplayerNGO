using System.Collections;
using System.Collections.Generic;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using QuickOutline.Scripts;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Outline = QuickOutline.Scripts.Outline;

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

        [SerializeField] Transform CurrentHeldPosition;
        protected bool _hasOwner => _owner != null;
        protected bool _isInOwnerHand = false;

        [SerializeField] protected GameObject _currentHeldVisual;

        public ulong OwnerID;

        //  protected GameObject _currentHeldVisualRPC;
        protected float _tranquilValue = 0;
        protected float _violentValue = 0;
        protected float _miscValue = 0;

        /*public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CustomNetworkSpawn();
            Debug.Log($"OnNetworkSpawn() called on {GetType().Name}");
        }*/

        /// <summary>
        /// for custom changes to onnetworkspawn
        /// </summary>
        protected virtual void CustomNetworkSpawn()
        {
            IsPickedUp.OnValueChanged += OnHeldStateChanged;
            IsInHand.OnValueChanged += OnChangedInHandState;

            OnChangedInHandState(!IsInHand.Value, IsInHand.Value);
            OnHeldStateChanged(!IsPickedUp.Value, IsPickedUp.Value);
        }

        protected void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null) return;
            _currentHeldVisual.transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.rotation = CurrentHeldPosition.rotation;
            transform.position = CurrentHeldPosition.position;
            transform.rotation = CurrentHeldPosition.rotation;
        }

        /*protected void SetPositionInUpdate()
        {
            if (_currentHeldVisual != null&& CurrentHeldPosition != null)
            {

            }
        }*/
        protected virtual void OnHeldStateChanged(bool oldHeld, bool newHeld)
        {
            _collider.enabled = !newHeld;
            _rb.isKinematic = newHeld;
            _renderer.enabled = !newHeld;
            
        }

        private void Awake()
        {
            OutlineEffect = GetComponent<Outline>();
            if (OutlineEffect != null)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }
        }

        public virtual void OnInteract(GameObject interactingPlayer)
        {
            var inv = interactingPlayer.GetComponent<PlayerInventory>();
            if (inv == null) return;

            inv.TryPickupItem();
            inv.DoPickup(this);
        }

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

        public virtual void PickupItem(GameObject player, Transform playerHoldPosition,
            NetworkObject networkObjectForPlayer)
        {
            _owner = player;
            _rb.isKinematic = true;
            _renderer.enabled = false;
            _collider.enabled = false;
            CurrentHeldPosition = networkObjectForPlayer.transform.GetChild(0).GetChild(1);
           
            if (!IsServer)
            {
                // Non-server clients request pickup
                Debug.Log("RequestSpawnServerRpc");
                RequestSpawnServerRpc();
                RequestPickupServerRpc(new NetworkObjectReference(networkObjectForPlayer));
            }
            else
            {
                // Host/server logic
                PickupServerRpc();
                _currentHeldVisual = Instantiate(_heldVisual);
                _currentHeldVisual.GetComponent<NetworkObject>().Spawn();
                AssignHeldVisualClientRpc(new NetworkObjectReference(_currentHeldVisual.GetComponent<NetworkObject>()));
                StartCoroutine(WaitForHeld(player,playerHoldPosition, networkObjectForPlayer));
                
            }
        }

        IEnumerator WaitForHeld(GameObject player, Transform playerHoldPosition, NetworkObject networkObjectForPlayer)
        {
            while (_currentHeldVisual == null)
            {
                yield return null;
            }
            DoPickup(player, playerHoldPosition, networkObjectForPlayer);
        }
        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnServerRpc()
        {
            Debug.Log("RequestSpawnServerRpc");
            _currentHeldVisual = Instantiate(_heldVisual);
            _currentHeldVisual.GetComponent<NetworkObject>().Spawn();
            AssignHeldVisualClientRpc(new NetworkObjectReference(_currentHeldVisual.GetComponent<NetworkObject>()));
        }
        private void DoPickup(GameObject player, Transform playerHoldPosition, NetworkObject networkObjectForPlayer)
        {
            // Instantiate held visual

            NetworkObject netObj = _currentHeldVisual?.GetComponent<NetworkObject>();

            // Spawn on network
           // netObj.Spawn();
            Debug.Log($"New test first ownerships - > playerID {networkObjectForPlayer.OwnerClientId}  old flash id {NetworkObject.OwnerClientId}");
            // Assign ownership to the player
            netObj?.ChangeOwnership(networkObjectForPlayer.OwnerClientId);
            NetworkObject.ChangeOwnership(networkObjectForPlayer.OwnerClientId);
            // Parent and position held visual
            
            Debug.Log($"New test first ownerships - > playerID {networkObjectForPlayer.OwnerClientId}  new flash id {NetworkObject.OwnerClientId}");
           // HandleHeldPosition(player.GetComponent<NetworkObject>(), playerHoldPosition);

            // Inform clients to assign their local reference
            
        }

        #region Pickup RPCs

        [ServerRpc(RequireOwnership = false)]
        private void RequestPickupServerRpc(NetworkObjectReference playerRef)
        {
            if (!playerRef.TryGet(out NetworkObject playerObj)) return;

            _owner = playerObj.gameObject;

            // Use the same method for server/host
            StartCoroutine(WaitForHeld(playerObj.gameObject, playerObj.transform.GetChild(0).GetChild(1), playerObj));
            PickupServerRpc();
        }

        [ClientRpc (RequireOwnership = false)]
        private void AssignHeldVisualClientRpc(NetworkObjectReference heldVisualRef)
        {
            if(IsServer) return;
            Debug.Log($"Assigning held visual {heldVisualRef}");
            StartCoroutine(AssignHeldVisualDelayed(heldVisualRef));
        }

        private IEnumerator AssignHeldVisualDelayed(NetworkObjectReference heldVisualRef)
        {
            NetworkObject netObj = null;
            while (!heldVisualRef.TryGet(out netObj))
                yield return null;
                Debug.Log("Should have worked?");
                if(_currentHeldVisual == null)
                _currentHeldVisual = netObj.gameObject;
        }


        private void HandleHeldPosition(NetworkObject playerObj, Transform playerHoldPosition)
        {
            _currentHeldVisual.transform.SetPositionAndRotation(CurrentHeldPosition.position,
                CurrentHeldPosition.rotation);
            transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.rotation = CurrentHeldPosition.rotation;
            OnChangedInHandState(false, false);
        }

        [ServerRpc(RequireOwnership = false)]
        void PickupServerRpc()
        {
            IsPickedUp.Value = true;
            // <-- automatically triggers OnHeldStateChanged on all clients
        }

        #endregion

        public virtual void DropItem(Transform dropPoint)
        {
            _owner = null;
            _renderer.enabled = true;
            //_currentHeldVisual.GetComponent<NetworkObject>().Despawn();
            Destroy(_currentHeldVisual);

            if (NetworkManager.Singleton.IsClient)
            {
                DropServerRpc();
            }

            if (!IsServer)
            {
                RequestDropServerRpc();
            }
            else
            {
                NetworkObject.RemoveOwnership();
                NetworkObject.TryRemoveParent();
            }

            _rb.isKinematic = false;
            _collider.enabled = true;
           // transform.position = dropPoint.position;
        }


        #region DropRPCS

        [ServerRpc(RequireOwnership = false)]
        void DropServerRpc()
        {
            IsPickedUp.Value = false;
            if (_currentHeldVisual != null)
            {
                var netObj = _currentHeldVisual.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
                
                _currentHeldVisual = null;
                NetworkObject.TryRemoveParent();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDropServerRpc()
        {
            NetworkObject.RemoveOwnership();
            Debug.Log($"[ServerRpc] Owner set to {NetworkObject.OwnerClientId}");
        }

        #endregion

        public virtual void UseItem()
        {
        }

        public virtual void UnequipItem()
        {
            SetInHandServerRpc(false);
            /*_currentHeldVisual?.SetActive(false);
            _isInOwnerHand = false;*/
            Debug.Log("[BaseInventoryItem] UnequipItem" + _currentHeldVisual);
        }

        public virtual void EquipItem()
        {
            SetInHandServerRpc(true);
            /*_currentHeldVisual?.SetActive(true);
            _isInOwnerHand = true;*/
            Debug.Log("[BaseInventoryItem] EquipItem" + _currentHeldVisual.name);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetInHandServerRpc(bool inHand)
        {
            IsInHand.Value = inHand;
        }

        private void OnChangedInHandState(bool oldState, bool newState)
        {
            /*Debug.Log(_currentHeldVisual);
            Debug.Log($"CurrentHeldVisual: {_currentHeldVisual.name}");*/
            if (_currentHeldVisual == null)
            {
                StartCoroutine(WaitOnCurrentHeldVisual(oldState, newState));
                return;
            }
            _currentHeldVisual?.SetActive(newState);
            _isInOwnerHand = newState;
        }

        IEnumerator WaitOnCurrentHeldVisual(bool oldState, bool newState)
        {
            yield return new WaitUntil(() => _currentHeldVisual != null);
            OnChangedInHandState(!IsInHand.Value,IsInHand.Value);
        }
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
    }
}