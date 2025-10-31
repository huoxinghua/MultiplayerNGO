using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.Flashlight;
using QuickOutline.Scripts;
using Unity.Netcode;
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
        [SerializeField] Light _lightComponent;


        NetworkVariable<bool> FlashOnNetworkVariable = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CustomNetworkSpawn();
        }

        protected override void CustomNetworkSpawn()
        {
            // Call base so pickup/equip syncing still happens
            base.CustomNetworkSpawn();

            // Now add flashlight-specific network setup
            FlashOnNetworkVariable.OnValueChanged += OnFlashStateChanged;
        }


        protected override void OnHeldStateChanged(bool oldHeld, bool newHeld)
        {
            base.OnHeldStateChanged(oldHeld, newHeld);
            if (newHeld)
            {
                if (_currentHeldVisual == null)
                {
                    StartCoroutine(DelayAssign(newHeld));
                }
                else
                {
                    _lightComponent = _currentHeldVisual.GetComponent<Light>();
                    _lightComponent.enabled = FlashOnNetworkVariable.Value;
                    _sceneLight.enabled = false;
                }
               
            }
            else
            {
                _lightComponent = null;
                _sceneLight.enabled = FlashOnNetworkVariable.Value;
            }
        }

        IEnumerator DelayAssign(bool newHeld)
        {
            while (_currentHeldVisual == null)
                yield return null;
            _lightComponent = _currentHeldVisual.GetComponent<Light>();
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            _sceneLight.enabled = false;
        }
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
            if (_itemSO is FlashItemSO flashLight)
            {
                if (_isFlashOn) _currentCharge -= flashLight.ChargeLoseRate * Time.deltaTime;
            }

            
            if (_currentCharge <= 0) _isFlashOn = false;
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
          
        }

        [ServerRpc(RequireOwnership = false)]
        void SetFlashStateServerRpc(bool flashState)
        {
            Debug.Log("setting flash state");
            FlashOnNetworkVariable.Value = flashState;
            
            OnFlashStateChanged(!flashState, flashState);
        }

        void OnFlashStateChanged(bool oldState, bool newState)
        {
            Debug.Log($"OnFlashStateChanged {oldState} {newState}");
            if (_currentHeldVisual == null)
            {
                Debug.Log("No CurrentheldVisual");
                return;
            }

            if (!_currentHeldVisual.activeInHierarchy)
            {
                Debug.Log("Not active but exists");
                return;
            }

            _lightComponent.enabled = newState;
            //_sceneLight.enabled = newState;
        }

        public override void PickupItem(GameObject player, Transform playerHoldPosition, NetworkObject networkObject)
        {
            base.PickupItem(player, playerHoldPosition, networkObject);

            _sceneLight.enabled = false;
        }

        /*[ServerRpc(RequireOwnership = false)]
        void RequestFlashPickupServerRpc()
        {

        }*/
        public override void DropItem(Transform dropPoint)
        {
            base.DropItem(dropPoint);
            _sceneLight.enabled = _isFlashOn;
            _lightComponent = null;
        }

        private void ToggleFlashLight()
        {
            Debug.Log("ToggleFlashLight");
            if (!_hasCharge)
            {
                _isFlashOn = false;
                SetFlashStateServerRpc(false);
                return;
            }
            Debug.Log($"Last State {FlashOnNetworkVariable.Value}  -- New State{!FlashOnNetworkVariable.Value}");
            SetFlashStateServerRpc(!FlashOnNetworkVariable.Value);
            _isFlashOn = !_isFlashOn;
        }

        public override void UseItem()
        {
            base.UseItem();
            ToggleFlashLight();
            Debug.Log("UseItem");
        }

        public override void UnequipItem()
        {
            base.UnequipItem();
            if (_lightComponent == null) return;
            _lightComponent.enabled = false;
            _isInOwnerHand = false;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if (_lightComponent == null) return;
            _lightComponent.enabled = _isFlashOn;
            _isInOwnerHand = true;
        }
    }
}