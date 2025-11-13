using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.Flashlight;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class FlashlightItem : BaseInventoryItem
    {
        [field: Header("Flashlight Specific")]
        [SerializeField] private Light _sceneLight;
        [SerializeField] Light _lightComponent;
        private float _currentCharge;
       [SerializeField] private bool _isFlashOn = false;
        [SerializeField] private bool _lastFlashState = true;
        private bool _hasCharge => _currentCharge >= 0;
     
        NetworkVariable<bool> FlashOnNetworkVariable = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        

       
        #region Setup + Update
        protected override void CustomNetworkSpawn()
        {
            // Call base so pickup/equip syncing still happens
            base.CustomNetworkSpawn();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");
            // Now add flashlight-specific network setup
            CustomNetworkSpawn();
            OnFlashStateChanged(!FlashOnNetworkVariable.Value,FlashOnNetworkVariable.Value);
        }
        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
            FlashOnNetworkVariable.OnValueChanged += OnFlashStateChanged;
            // immediately sync current value
            OnFlashStateChanged(!FlashOnNetworkVariable.Value, FlashOnNetworkVariable.Value);
            Debug.Log("CustomNetworkPostSpawn called!");
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
                if (FlashOnNetworkVariable.Value) _currentCharge -= flashLight.ChargeLoseRate * Time.deltaTime;
            }
            //  if (_currentCharge <= 0) _isFlashOn = false;
            if (_currentCharge <= 0) SetFlashStateServerRpc(false);
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
          
        }
        #endregion
        
        #region Pickup Logic

        public override void PickupItem(GameObject player, Transform playerHoldPosition, NetworkObject networkObject)
        {
            base.PickupItem(player, playerHoldPosition, networkObject);
            RequestFlashPickupServerRpc();
            _sceneLight.enabled = false;
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestFlashPickupServerRpc()
        {
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            _sceneLight.enabled = false;
            SendLightCompByClientRpc(new NetworkObjectReference(_currentHeldVisual.GetComponent<NetworkObject>()));
        }
        
        [ClientRpc(RequireOwnership = false)]
        void SendLightCompByClientRpc(NetworkObjectReference heldRef)
        {
            if (!heldRef.TryGet(out NetworkObject heldObj)) return;
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            _sceneLight.enabled = false;
        }
        #endregion
        
        #region Drop Logic
        public override void DropItem(Transform dropPoint)
        {
            base.DropItem(dropPoint);
            _sceneLight.enabled = FlashOnNetworkVariable.Value;
            _lightComponent.enabled = false;
        }
        

        #endregion
        
        #region Swap Logic

        public override void EquipItem()
        {
            base.EquipItem();
            if (_lightComponent == null) return;
            _lightComponent.enabled = FlashOnNetworkVariable.Value;
            //_isInOwnerHand = true;
        }
        public override void UnequipItem()
        {
            base.UnequipItem();
            if (_lightComponent == null) return;
            _lightComponent.enabled = false;
            // _isInOwnerHand = false;
        }
        #endregion
        
        #region Use Logic
        private void ToggleFlashLight()
        {
            if (!_hasCharge)
            {
                Debug.Log("FlashLight not charged");
                SetFlashStateServerRpc(false);
                return;
            }
            SetFlashStateServerRpc(!FlashOnNetworkVariable.Value);
        }
        public override void UseItem()
        {
            base.UseItem();
            ToggleFlashLight();
            Debug.Log("UseItem");
        }

        #endregion
        
        #region NetVar Logic
        protected override void OnPickedUpStateChanged(bool oldHeld, bool newHeld)
        {
            base.OnPickedUpStateChanged(oldHeld, newHeld);
        }
        [ServerRpc(RequireOwnership = false)]
        void SetFlashStateServerRpc(bool flashState)
        {
            FlashOnNetworkVariable.Value = flashState;
        }
        void OnFlashStateChanged(bool oldState, bool newState)
        {
            if (_currentHeldVisual == null)
            {
                Debug.Log("No CurrentheldVisual");
                return;
            }
            _lightComponent.enabled = newState;
        }
        #endregion
    }
}