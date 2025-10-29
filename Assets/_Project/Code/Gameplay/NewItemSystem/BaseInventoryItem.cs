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
    public class BaseInventoryItem : NetworkBehaviour , IInteractable , IInventoryItem
    {
        [SerializeField] protected GameObject _heldVisual;
        //  [SerializeField] protected GameObject _heldVisualRPC;
        [SerializeField] protected BaseItemSO _itemSO;

        
        NetworkVariable<bool> _isHeld = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
        //casting type to child? to get child specific properties
        /*if (item is WeaponSO weapon)
{
        Debug.Log("Damage: " + weapon.damage);
}*/
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
                _isHeld.OnValueChanged += OnHeldStateChanged;
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
            if(OutlineEffect != null)
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
                if (_hasOwner) { OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden; return; }
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

            transform.parent = playerHoldPosition;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);
            Debug.Log("[BaseInventoryItem] PickupItem" + _currentHeldVisual.name);
        }
        
        [ServerRpc(RequireOwnership = false)]
        void PickupServerRpc()
        {
            Debug.Log("[BaseInventoryItem] PickupServerRpc");
            _isHeld.Value = true; // <-- automatically triggers OnHeldStateChanged on all clients
        }
        
        public virtual void DropItem(Transform dropPoint)
        {
            _owner = null;

            _renderer.enabled = true;

            Destroy(_currentHeldVisual);
            transform.parent = null;
            if (NetworkManager.Singleton.IsClient)
            {
                DropServerRpc();
            }
            _rb.isKinematic = false;
            _collider.enabled = true;
            transform.position = dropPoint.position;
        }
        [ServerRpc(RequireOwnership = false)]
        void DropServerRpc()
        {
            _isHeld.Value = false;
        }
        
        
        
        public virtual void UseItem()
        {

        }
        public virtual void UnequipItem()
        {

            _currentHeldVisual?.SetActive(false);
            _isInOwnerHand = false;
            Debug.Log("[BaseInventoryItem] UnequipItem" + _currentHeldVisual);
        }
        public virtual void EquipItem()
        {

            _currentHeldVisual?.SetActive(true);
            _isInOwnerHand = true;
            Debug.Log("[BaseInventoryItem] EquipItem" + _currentHeldVisual.name);
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
            return new ScienceData { RawTranquilValue = _tranquilValue, RawMiscValue = _miscValue, RawViolentValue = _violentValue, KeyName = _itemSO.ItemName };
        }
    }
}
