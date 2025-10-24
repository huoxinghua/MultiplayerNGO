using _Project.ScriptableObjects.ScriptObjects.ItemSO.Flashlight;
using QuickOutline.Scripts;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class FlashlightItem : BaseInventoryItem
    {
        [SerializeField] private Light _sceneLight;
        private float _currentCharge;
        [SerializeField] private bool _isFlashOn = false;
        [SerializeField] private bool _lastFlashState = true;
        private bool _hasCharge => _currentCharge >= 0;
        private Light _lightComponent;
        private void Awake()
        {
            _sceneLight.enabled = false;

            if (_itemSO is FlashItemSO flashLight)
            {
                _currentCharge = flashLight.MaxCharge;
            }
            OutlineEffect = GetComponent<Outline>();
            if (OutlineEffect != null)
            {
                OutlineEffect.OutlineMode = Outline.Mode.OutlineHidden;
                OutlineEffect.OutlineWidth = 0;
            }
        }
        private void Update()
        {
            if(_itemSO is FlashItemSO flashLight)
            {
                if (_isFlashOn) _currentCharge -= flashLight.ChargeLoseRate * Time.deltaTime;
            }
        
            if (_currentCharge <= 0) _isFlashOn = false;
            if (_currentHeldVisual == null) return;
            if (_hasOwner)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            if (_currentHeldVisual.activeInHierarchy && _lastFlashState != _isFlashOn)
            {
                _lightComponent.enabled = _isFlashOn;
                _lastFlashState = _isFlashOn;
            }
        }
        public override void PickupItem(GameObject player, Transform playerHoldPosition)
        {
            base.PickupItem(player, playerHoldPosition);
            _lightComponent = _currentHeldVisual.GetComponent<Light>();
            _lightComponent.enabled = _isFlashOn;
            _sceneLight.enabled = false;
        }
        public override void DropItem(Transform dropPoint)
        {
            base.DropItem(dropPoint);
            _sceneLight.enabled = _isFlashOn;
            _lightComponent = null;
        }
        private void ToggleFlashLight()
        {

            if (!_hasCharge)
            {
                _isFlashOn = false;
                return;
            }
            _isFlashOn = !_isFlashOn;
        }
        public override void UseItem()
        {
            base.UseItem();
            ToggleFlashLight();
        }
        public override void UnequipItem()
        {
            base.UnequipItem();
            _lightComponent.enabled = false;
            _isInOwnerHand = false;
        }
        public override void EquipItem()
        {
            base.EquipItem();
            _lightComponent.enabled = _isFlashOn;
            _isInOwnerHand = true;
        }

    }
}
