using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using QuickOutline.Scripts;
using Unity.Netcode;
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


        NetworkVariable<bool> IsHeld = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

// True if the item is actively held in hand (so the held visual should show)
        NetworkVariable<bool> IsInHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

// Track who owns it
        NetworkVariable<ulong> OwnerClientId = new NetworkVariable<ulong>(0);

        [SerializeField] protected Rigidbody _rb;
        [SerializeField] protected Renderer _renderer;
        [SerializeField] protected Collider _collider;
        protected Outline OutlineEffect;
        protected GameObject _owner;
        protected bool _hasOwner => _owner != null;
        protected bool _isInOwnerHand = false;

        protected GameObject _currentHeldVisual;

        //  protected GameObject _currentHeldVisualRPC;
        protected float _tranquilValue = 0;
        protected float _violentValue = 0;
        protected float _miscValue = 0;

        public override void OnNetworkSpawn()
        {
            IsHeld.OnValueChanged += OnHeldStateChanged;
            IsInHand.OnValueChanged += OnIsInHandChanged;
        }

        protected virtual void OnHeldStateChanged(bool oldHeld, bool newHeld)
        {
            _rb.isKinematic = newHeld;
            _renderer.enabled = !newHeld;
            _collider.enabled = !newHeld;
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

            //if (!inv.IsOwner)
            //return;
            if (NetworkManager.Singleton.IsClient)
            {
                // Tell the server we want to pick this up
                PickupServerRpc();
            }

            inv.TryPickupItem();
            inv.DoPickup(this);
            //if (interactingPlayer.GetComponent<PlayerInventory>().TryPickupItem())
            //{
            //    interactingPlayer.GetComponent<PlayerInventory>().DoPickup(this);
            //}
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

        public virtual void PickupItem(GameObject player, Transform playerHoldPosition)
        {
            _owner = player;
            _rb.isKinematic = true;
            _renderer.enabled = false;
            _collider.enabled = false;

            
          //  _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);
            if (IsServer)
            {
                transform.parent = playerHoldPosition;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                _currentHeldVisual = Instantiate(_heldVisual);
                var netObj = _currentHeldVisual.GetComponent<NetworkObject>();
                netObj.Spawn(true); // spawn visible to everyone
                _currentHeldVisual.transform.parent = playerHoldPosition;
            }

            if (IsOwner) RequestOwnershipServerRpc(player.GetComponent<NetworkObject>().OwnerClientId);

            Debug.Log("[BaseInventoryItem] PickupItem" + _currentHeldVisual.name);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestOwnershipServerRpc(ulong newOwnerId)
        {
            NetworkObject.ChangeOwnership(newOwnerId);
            OwnerClientId.Value = newOwnerId; // Keep the NetworkVariable in sync
        }

        [ServerRpc(RequireOwnership = false)]
        void PickupServerRpc()
        {
            IsHeld.Value = true;
            OwnerClientId.Value = NetworkManager.Singleton.LocalClientId;
        }

        [ServerRpc(RequireOwnership = false)]
        void SwitchInHandStateServerRpc(bool isInHand)
        {
            IsInHand.Value = isInHand;
        }

        public virtual void DropItem(Transform dropPoint)
        {
            _owner = null;

            _renderer.enabled = true;
            transform.parent = null;
            transform.position = dropPoint.position;

            _rb.isKinematic = false;
            _collider.enabled = true;

            // Tell server to handle network state and destruction
            if (NetworkManager.Singleton.IsClient)
            {
                DropServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void DropServerRpc()
        {
            // Mark item as no longer held
            IsHeld.Value = false;
            IsInHand.Value = false;

            // Destroy the visual for everyone
            if (_currentHeldVisual != null)
            {
                // NetworkObject.Destroy ensures it is removed on all clients
                if (_currentHeldVisual.TryGetComponent<NetworkObject>(out var netObj))
                {
                    netObj.Despawn(true); // true = destroy across network
                }
                else
                {
                    Destroy(_currentHeldVisual); // fallback
                }

                _currentHeldVisual = null;
            }

            // Optionally remove ownership
            if (NetworkObject.IsOwnedByServer)
            {
                NetworkObject.RemoveOwnership();
            }
        }


        [ServerRpc(RequireOwnership = true)]
        public void DropItemServerRpc()
        {
            NetworkObject.RemoveOwnership();
            // Optionally reset position, physics, etc.
        }

        public virtual void UseItem()
        {
        }

        public virtual void UnequipItem()
        {
            _currentHeldVisual?.SetActive(false);
            _isInOwnerHand = false;
            if (NetworkManager.Singleton.IsClient)
            {
                SwitchInHandStateServerRpc(false);
            }

            Debug.Log("[BaseInventoryItem] UnequipItem" + _currentHeldVisual);
        }

        public virtual void EquipItem()
        {
            _currentHeldVisual?.SetActive(true);
            _isInOwnerHand = true;
            if (NetworkManager.Singleton.IsClient)
            {
                SwitchInHandStateServerRpc(true);
            }

            Debug.Log("[BaseInventoryItem] EquipItem" + _currentHeldVisual.name);
        }

        protected void OnIsInHandChanged(bool oldValue, bool newValue)
        {
            if (_currentHeldVisual == null && _heldVisual != null)
            {
                // Instantiate once and parent to this object (or a temporary holder)
                _currentHeldVisual = Instantiate(_heldVisual, transform);
                _currentHeldVisual.transform.localPosition = Vector3.zero;
                _currentHeldVisual.transform.localRotation = Quaternion.identity;
            }

            if (newValue)
            {
                // Move it to the owner's hand and activate
                if (OwnerClientId.Value != 0)
                {
                    var ownerObj = NetworkManager.Singleton.ConnectedClients[OwnerClientId.Value].PlayerObject
                        .gameObject;
                    _currentHeldVisual.transform.SetParent(ownerObj.transform);
                    _currentHeldVisual.transform.localPosition = Vector3.zero;
                    _currentHeldVisual.transform.localRotation = Quaternion.identity;
                }

                _currentHeldVisual.SetActive(true);
            }
            else
            {
                // Deactivate it
                _currentHeldVisual.SetActive(false);

                // Optionally, parent it back to the world (or this object) for consistency
                _currentHeldVisual.transform.SetParent(transform);
            }
        }

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