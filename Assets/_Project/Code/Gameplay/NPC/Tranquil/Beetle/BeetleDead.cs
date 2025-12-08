using _Project.Code.Art.RagdollScripts;
using _Project.Code.Gameplay.Market.Sell;
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
        private Transform _ragdollRoot;

        [SerializeField] private BeetleItemSO _beetleSO;
        [SerializeField] private Ragdoll _ragdoll;

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

                // Register with SellableItemManager for cleanup on hub entry
                SellableItemManager.Instance?.RegisterItem(NetworkObject, this);
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

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer)
            {
                SellableItemManager.Instance?.UnregisterItem(NetworkObject);
            }
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
            /*if (_currentHeldVisual == null || CurrentHeldPosition == null)
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
            }*/
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

        #region Pickup Override

        public override void PickupItem(GameObject player, Transform fpsItemParent, Transform tpsItemParent,
            NetworkObject networkObjectForPlayer)
        {
            _ragdoll.DisableAllRigidbodies();
            base.PickupItem(player, fpsItemParent, tpsItemParent, networkObjectForPlayer);
        }

        #endregion

        #region  Drop Override

        public override void DropItem(Vector3 dropPosition)
        {
            // Set positions BEFORE base drop logic
            transform.position = dropPosition;
            if (_ragdollRoot != null)
            {
                _ragdollRoot.position = dropPosition;
            }

            // Call base drop logic (this sets IsPickedUp = false, triggering callbacks)
            base.DropItem(dropPosition);

            // Enable ragdoll on all clients via ClientRpc
            EnableRagdollClientRpc(dropPosition);
        }

        [ClientRpc]
        private void EnableRagdollClientRpc(Vector3 dropPosition)
        {
            // Ensure positions are synced before enabling physics
            transform.position = dropPosition;
            if (_ragdollRoot != null)
            {
                _ragdollRoot.position = dropPosition;
            }

            // Reset all ragdoll bone velocities
            foreach (var rb in _ragdollRoot.GetComponentsInChildren<Rigidbody>())
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Enable ragdoll physics
            _ragdoll.EnableRagdoll();

            // Re-enable pickup collider after ragdoll setup
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        #endregion

    }
}
