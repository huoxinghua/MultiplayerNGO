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
            Debug.Log("CustomNetworkSpawn called!");
            // Now add flashlight-specific network setup
            CustomNetworkSpawn();
            StartCoroutine(EnsureFlashSync());
        }

        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
            FlashOnNetworkVariable.OnValueChanged += OnFlashStateChanged;
            // immediately sync current value
            OnFlashStateChanged(!FlashOnNetworkVariable.Value, FlashOnNetworkVariable.Value);
            Debug.Log("CustomNetworkPostSpawn called!");
        }
    
        //temp 
        protected override void CustomNetworkSpawn()
        {
            // Call base so pickup/equip syncing still happens
            base.CustomNetworkSpawn();
            
        }

        IEnumerator EnsureFlashSync()
        {
            while (_currentHeldVisual == null)
                yield return null;
            OnFlashStateChanged(!FlashOnNetworkVariable.Value,FlashOnNetworkVariable.Value);
        }
        protected override void OnHeldStateChanged(bool oldHeld, bool newHeld)
        {
            base.OnHeldStateChanged(oldHeld, newHeld);
           
        }

        IEnumerator DelayAssign()
        {
            while (_currentHeldVisual == null)
                yield return null;
            _lightComponent = _currentHeldVisual.GetComponent<Light>();
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            _sceneLight.enabled = false;
            SendLightCompByClientRpc(new NetworkObjectReference(_currentHeldVisual.GetComponent<NetworkObject>()));
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
        }

        void OnFlashStateChanged(bool oldState, bool newState)
        {
            Debug.Log($"OnFlashStateChanged {oldState} {newState}");
            if (_currentHeldVisual == null)
            {
                Debug.Log("No CurrentheldVisual");
                return;
            }
            _lightComponent.enabled = newState;
            //_sceneLight.enabled = newState;
        }

        public override void PickupItem(GameObject player, Transform playerHoldPosition, NetworkObject networkObject)
        {
            base.PickupItem(player, playerHoldPosition, networkObject);
            RequestFlashPickupServerRpc();
            _sceneLight.enabled = false;
        }

        
        [ServerRpc(RequireOwnership = false)]
        void RequestFlashPickupServerRpc()
        {
            StartCoroutine(DelayAssign());
        }

        [ClientRpc(RequireOwnership = false)]
        void SendLightCompByClientRpc(NetworkObjectReference heldRef)
        {
            if (!heldRef.TryGet(out NetworkObject heldObj)) return;
            _lightComponent = heldObj.GetComponent<Light>();
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            _sceneLight.enabled = false;
        }
        public override void DropItem(Transform dropPoint)
        {
            base.DropItem(dropPoint);
            _sceneLight.enabled = FlashOnNetworkVariable.Value;
            _lightComponent = null;
        }

        private void ToggleFlashLight()
        {
            Debug.Log("ToggleFlashLight");
            if (!_hasCharge)
            {
                Debug.Log("FlashLight not charged");
                SetFlashStateServerRpc(false);
                return;
            }
            Debug.Log($"Last State {FlashOnNetworkVariable.Value}  -- New State{!FlashOnNetworkVariable.Value}");
            SetFlashStateServerRpc(!FlashOnNetworkVariable.Value);
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
           // _isInOwnerHand = false;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if (_lightComponent == null) return;
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            //_isInOwnerHand = true;
        }
    }
}