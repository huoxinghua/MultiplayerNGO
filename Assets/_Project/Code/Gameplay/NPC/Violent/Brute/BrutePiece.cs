using _Project.Code.Gameplay.NewItemSystem;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    /// <summary>
    /// Clean Rewrite: Brute piece sample (enemy drop).
    /// KEY FIX: Random values generated on SERVER only, synced via NetworkVariables.
    /// This ensures all clients see the same sell values.
    /// </summary>
    public class BrutePiece : BaseInventoryItem
    {
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

                Debug.Log($"[Server] BrutePiece spawned with values T:{_tranquilValueNet.Value:F2} V:{_violentValueNet.Value:F2} M:{_miscValueNet.Value:F2}");

                // Deparent from brute ragdoll if needed
                // TryRemoveParent returns false if there's no parent, so it's safe to call
                if (NetworkObject.TryRemoveParent())
                {
                    Debug.Log("[Server] BrutePiece deparented from brute ragdoll");
                }
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

        #endregion

        #region Position Update

        /// <summary>
        /// Manually locks item position to hold transform.
        /// Called every frame when owner is holding the item.
        /// </summary>
        private void Update()
        {
            if (!IsOwner) return;
            UpdateHeldPosition();
        }

        /// <summary>
        /// Override to update both held visual and main transform.
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

            // Also update main transform position
            transform.position = CurrentHeldPosition.position;
            transform.rotation = CurrentHeldPosition.rotation;
        }

        #endregion

        #region Item Usage

        /// <summary>
        /// Brute piece has no use functionality.
        /// </summary>
        public override bool TryUse()
        {
            // No use functionality for brute piece
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
