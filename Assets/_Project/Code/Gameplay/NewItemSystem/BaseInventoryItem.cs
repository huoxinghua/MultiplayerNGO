using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player.RefactorInventory;
using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using QuickOutline.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Outline = QuickOutline.Scripts.Outline;

namespace _Project.Code.Gameplay.NewItemSystem
{
    [RequireComponent(typeof(Outline))]
    public class BaseInventoryItem : MonoBehaviour , IInteractable , IInventoryItem
    {
        [SerializeField] protected GameObject _heldVisual;
        [SerializeField] protected BaseItemSO _itemSO;

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
        protected float _tranquilValue = 0;
        protected float _violentValue = 0;
        protected float _miscValue = 0;
        private void Awake()
        {
            OutlineEffect = GetComponent<Outline>();
            if(OutlineEffect != null)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }
            
        }
        private void Update()
        {

        }
        public virtual void OnInteract(GameObject interactingPlayer)
        {
            if (interactingPlayer.GetComponent<PlayerInventory>().TryPickupItem())
            {
                interactingPlayer.GetComponent<PlayerInventory>().DoPickup(this);
            }
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

        }
        public virtual void DropItem(Transform dropPoint)
        {
            _owner = null;

            _renderer.enabled = true;

            Destroy(_currentHeldVisual);
            transform.parent = null;

            _rb.isKinematic = false;
            _collider.enabled = true;
            transform.position = dropPoint.position;
        }
        public virtual void UseItem()
        {

        }
        public virtual void UnequipItem()
        {
            _currentHeldVisual?.SetActive(false);
            _isInOwnerHand = false;
        }
        public virtual void EquipItem()
        {
            _currentHeldVisual?.SetActive(true);
            _isInOwnerHand = true;
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
