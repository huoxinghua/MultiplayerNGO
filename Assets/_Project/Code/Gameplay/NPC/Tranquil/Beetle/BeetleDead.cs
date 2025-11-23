using _Project.Code.Gameplay.NewItemSystem;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.BeetleSample;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    /// <summary>
    /// Clean Rewrite: Dead beetle sample (enemy drop).
    /// KEY FIX: Random values generated on SERVER only, synced via NetworkVariables.
    /// This ensures all clients see the same sell values.
    /// </summary>
    public class BeetleDead : BaseInventoryItem
    {
        #region Serialized Fields

        [Header("Beetle Specific")]
        [SerializeField] [Tooltip("Beetle skeleton mesh")]
        private GameObject _beetleSkele;

        [SerializeField] private BeetleItemSO _beetleSO;

        #endregion

        #region Network State - Synchronized Random Values

        /// <summary>
        /// Server-authoritative: Tranquil research value (generated on server).
        /// NetworkVariable ensures all clients see the same value.
        /// </summary>
        private NetworkVariable<float> _tranquilValueNet = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>
        /// Server-authoritative: Violent research value (generated on server).
        /// </summary>
        private NetworkVariable<float> _violentValueNet = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>
        /// Server-authoritative: Miscellaneous research value (generated on server).
        /// </summary>
        private NetworkVariable<float> _miscValueNet = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        #endregion

        #region Initialization

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // SERVER ONLY: Generate random values on spawn
            if (IsServer)
            {
                _tranquilValueNet.Value = Random.Range(0f, 1f);
                _violentValueNet.Value = Random.Range(0f, 1f);
                _miscValueNet.Value = Random.Range(0f, 1f);

                Debug.Log($"[Server] BeetleDead spawned with values T:{_tranquilValueNet.Value:F2} V:{_violentValueNet.Value:F2} M:{_miscValueNet.Value:F2}");
            }

            // All clients cache values locally for easy access
            _tranquilValue = _tranquilValueNet.Value;
            _violentValue = _violentValueNet.Value;
            _miscValue = _miscValueNet.Value;

            // Register callbacks to update local cache when values change
            _tranquilValueNet.OnValueChanged += (oldVal, newVal) => _tranquilValue = newVal;
            _violentValueNet.OnValueChanged += (oldVal, newVal) => _violentValue = newVal;
            _miscValueNet.OnValueChanged += (oldVal, newVal) => _miscValue = newVal;
        }

        private void OnEnable()
        {
            // Ensure collider is enabled when spawned in world
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        #endregion

        #region Position Update

        /// <summary>
        /// Manually locks item position and beetle skeleton to hold transform.
        /// Called every frame when owner is holding the item.
        /// </summary>
        private void Update()
        {
            if (!IsOwner) return;
            UpdateHeldPosition();
        }

        /// <summary>
        /// Override to also update beetle skeleton position.
        /// </summary>
        protected override void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null)
            {
                return;
            }

            // Update held visual position
            _currentHeldVisual.transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.rotation = CurrentHeldPosition.rotation;

            // Also update beetle skeleton position
            if (_beetleSkele != null)
            {
                _beetleSkele.transform.position = CurrentHeldPosition.position;
                _beetleSkele.transform.rotation = CurrentHeldPosition.rotation;
            }
        }

        #endregion

        #region Item Usage

        /// <summary>
        /// Beetle corpse has no use functionality.
        /// </summary>
        public override bool TryUse()
        {
            // No use functionality for dead beetle
            return false;
        }

        #endregion

        #region Selling Override

        /// <summary>
        /// Override GetValueStruct to use NetworkVariable values.
        /// This ensures sell values are consistent across all clients.
        /// </summary>
        public override ScienceData GetValueStruct()
        {
            return new ScienceData
            {
                RawTranquilValue = _tranquilValueNet.Value,
                RawViolentValue = _violentValueNet.Value,
                RawMiscValue = _miscValueNet.Value,
                KeyName = GetItemName()
            };
        }

        #endregion
    }
}
