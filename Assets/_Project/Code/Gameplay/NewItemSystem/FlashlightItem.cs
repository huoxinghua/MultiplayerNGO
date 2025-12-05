using _Project.ScriptableObjects.ScriptObjects.ItemSO.Flashlight;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    /// <summary>
    /// Clean Rewrite: Flashlight item that can be toggled on/off with battery charge.
    /// KEY FIX: Server drains charge in Update() instead of RPC spam every frame.
    /// </summary>
    public class FlashlightItem : BaseInventoryItem
    {
        #region Serialized Fields

        [Header("Flashlight Specific")]
        [SerializeField] [Tooltip("Light component on world item (visible when dropped)")]
        private Light _sceneLight;

        [SerializeField] [Tooltip("Light component on held visual for TPS (visible when held)")]
        private Light _tpsLightComponent;
        [SerializeField] [Tooltip("Light component on held visual for FPS (visible when held)")]
        private Light _fpsLightComponent;

        private bool _hasLightComponent = false;
        private bool HasLightComponent
        {
            get
            {
                return _hasLightComponent && (_tpsLightComponent != null && _fpsLightComponent != null);
            }
            set
            {
                _hasLightComponent = value;
                if(_fpsLightComponent != null) _fpsLightComponent.enabled = value;
                if(_tpsLightComponent != null) _tpsLightComponent.enabled = value;
               

            }
        }
        #endregion

        #region Network State

        /// <summary>
        /// Server-authoritative: Is flashlight currently on?
        /// </summary>
        private NetworkVariable<bool> FlashOnNetworkVariable = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>
        /// Server-authoritative: Current battery charge (0 to MaxCharge).
        /// </summary>
        private NetworkVariable<float> _currentChargeNetVar = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        #endregion

        #region Private Fields

        /// <summary>
        /// Typed reference to FlashItemSO for easy access to flashlight-specific data.
        /// </summary>
        private FlashItemSO _flashItemSO;

        /// <summary>
        /// Returns true if flashlight has charge remaining.
        /// </summary>
        private bool _hasCharge => _currentChargeNetVar.Value > 0;

        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();

            // Disable scene light by default
            if (_sceneLight != null)
            {
                _sceneLight.enabled = false;
            }

            // Cache typed SO reference
            if (_itemSO is FlashItemSO flashItemSO)
            {
                _flashItemSO = flashItemSO;
            }
            else
            {
                Debug.LogError("[FlashlightItem] ItemSO is not FlashItemSO!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Server initializes charge to max
            if (IsServer)
            {
                _currentChargeNetVar.Value = _flashItemSO != null ? _flashItemSO.MaxCharge : 100f;
            }

            // Register NetworkVariable callbacks
            FlashOnNetworkVariable.OnValueChanged += OnFlashStateChanged;
            _currentChargeNetVar.OnValueChanged += OnChargeValueChanged;

            // Apply initial values for late joiners
            OnFlashStateChanged(!FlashOnNetworkVariable.Value, FlashOnNetworkVariable.Value);
        }

        #endregion

        #region Update - Charge Drain (Server-Only)

        /// <summary>
        /// CRITICAL FIX: Server drains charge directly in Update(), not via RPC spam!
        /// Also updates item position when held by owner.
        /// </summary>
        protected override void LateUpdate()
        {
            base.LateUpdate();

            // SERVER-ONLY: Drain charge when flashlight is on
            // This prevents 60+ RPCs per second that old version had!
            if (IsServer && FlashOnNetworkVariable.Value && _hasCharge)
            {
                _currentChargeNetVar.Value -= _flashItemSO.ChargeLoseRate * Time.deltaTime;

                // Clamp to 0
                if (_currentChargeNetVar.Value < 0)
                {
                    _currentChargeNetVar.Value = 0;
                }
            }

            // Owner updates held position
            if (IsOwner)
            {
                UpdateHeldPosition();
            }
        }

        #endregion

        #region Pickup/Drop/Equip Override

        /// <summary>
        /// Override pickup to disable scene light when picked up.
        /// </summary>
        public override void PickupItem(GameObject player, Transform fpsItemParent, Transform tpsItemParent, NetworkObject networkObjectForPlayer)
        {
            base.PickupItem(player, fpsItemParent, tpsItemParent, networkObjectForPlayer);

            // Server-only: Disable scene light, enable held light if flashlight is on
            if (IsServer)
            {
                if (_sceneLight != null)
                {
                    _sceneLight.enabled = false;
                }
                HasLightComponent = FlashOnNetworkVariable.Value;
                
                // Sync light state to all clients
                SyncLightStateClientRpc(FlashOnNetworkVariable.Value, false);
            }
        }

        /// <summary>
        /// Override drop to enable scene light when dropped.
        /// </summary>
        public override void DropItem(Vector3 dropPosition)
        {
            base.DropItem(dropPosition);

            // Server-only: Enable scene light if flashlight is on, disable held light
            if (IsServer)
            {
                if (_sceneLight != null)
                {
                    _sceneLight.enabled = FlashOnNetworkVariable.Value;
                }

                HasLightComponent = false;

                // Sync light state to all clients
                SyncLightStateClientRpc(false, FlashOnNetworkVariable.Value);
            }
        }

        /// <summary>
        /// Override equip to enable held light when equipped.
        /// </summary>
        public override void EquipItem()
        {
            base.EquipItem();

            // Server-only: Enable held light if flashlight is on
            if (IsServer)
            {
                HasLightComponent = FlashOnNetworkVariable.Value; 
                SyncLightStateClientRpc(FlashOnNetworkVariable.Value, false);
            }
        }

        /// <summary>
        /// Override unequip to disable held light when unequipped.
        /// </summary>
        public override void UnequipItem()
        {
            base.UnequipItem();

            // Server-only: Disable held light
            if (IsServer)
            {
                HasLightComponent = false;
                SyncLightStateClientRpc(false, false);
            }
        }

        /// <summary>
        /// Syncs light component states to all clients.
        /// </summary>
        /// <param name="heldLightState">State of held light</param>
        /// <param name="sceneLightState">State of scene light</param>
        [ClientRpc]
        private void SyncLightStateClientRpc(bool heldLightState, bool sceneLightState)
        {
            HasLightComponent = heldLightState;


            if (_sceneLight != null)
            {
                _sceneLight.enabled = sceneLightState;
            }
        }

        #endregion

        #region Item Usage - Toggle Flashlight
        protected override bool CanUse()
        {
            if (!IsOwner) return false;
            return true;
        }
        protected override void StartUsage()
        {
            ToggleFlashLight();
        }

        /// <summary>
        /// Toggles flashlight on/off state.
        /// Sends ServerRpc to update NetworkVariable.
        /// </summary>
        private void ToggleFlashLight()
        {
            // Can't turn on if no charge
            if (!FlashOnNetworkVariable.Value && !_hasCharge)
            {
                return;
            }

            // Request toggle from server
            SetFlashStateServerRpc(!FlashOnNetworkVariable.Value);
        }

        /// <summary>
        /// SERVER-ONLY: Sets flashlight on/off state.
        /// </summary>
        [ServerRpc]
        private void SetFlashStateServerRpc(bool flashState)
        {
            FlashOnNetworkVariable.Value = flashState;
        }

        #endregion

        #region NetworkVariable Callbacks

        /// <summary>
        /// Callback when flashlight on/off state changes.
        /// Enables/disables light components based on state.
        /// </summary>
        private void OnFlashStateChanged(bool oldState, bool newState)
        {
            // Update light component based on whether item is picked up
            HasLightComponent = newState;
        }

        /// <summary>
        /// Callback when charge value changes.
        /// Turns off flashlight if charge reaches 0.
        /// </summary>
        private void OnChargeValueChanged(float oldValue, float newValue)
        {
            // Turn off flashlight if charge depleted
            if (newValue <= 0 && FlashOnNetworkVariable.Value)
            {
                // Only server can change network state
                if (IsServer)
                {
                    FlashOnNetworkVariable.Value = false;
                }
            }

            // Clamp to max charge
            if (newValue > _flashItemSO.MaxCharge)
            {
                if (IsServer)
                {
                    _currentChargeNetVar.Value = _flashItemSO.MaxCharge;
                }
            }
        }

        #endregion

        #region Public API (For Charging Stations)

        /// <summary>
        /// Adds charge to flashlight. Can be called by charging stations.
        /// </summary>
        /// <param name="chargeAmount">Amount of charge to add</param>
        public void AddCharge(float chargeAmount)
        {
            if (IsServer)
            {
                _currentChargeNetVar.Value = Mathf.Min(_currentChargeNetVar.Value + chargeAmount, _flashItemSO.MaxCharge);
            }
            else
            {
                AddChargeServerRpc(chargeAmount);
            }
        }

        /// <summary>
        /// SERVER-ONLY: Adds charge to flashlight.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void AddChargeServerRpc(float chargeAmount)
        {
            _currentChargeNetVar.Value = Mathf.Min(_currentChargeNetVar.Value + chargeAmount, _flashItemSO.MaxCharge);
        }

        /// <summary>
        /// Gets current charge value (read-only).
        /// </summary>
        public float GetCurrentCharge()
        {
            return _currentChargeNetVar.Value;
        }

        /// <summary>
        /// Gets max charge value (read-only).
        /// </summary>
        public float GetMaxCharge()
        {
            return _flashItemSO != null ? _flashItemSO.MaxCharge : 100f;
        }

        #endregion
    }
}
