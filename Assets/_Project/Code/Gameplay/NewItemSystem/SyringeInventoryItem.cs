using _Project.ScriptableObjects.ScriptObjects.ItemSO.SyringeItem;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    /// <summary>
    /// Clean Rewrite: Syringe item that provides temporary speed boost.
    /// Single-use item - becomes unusable after injection.
    /// </summary>
    public class SyringeInventoryItem : BaseInventoryItem
    {
        #region Network State

        /// <summary>
        /// Server-authoritative: Has this syringe been used?
        /// Once used, cannot be used again.
        /// </summary>
        private NetworkVariable<bool> IsUsed = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        #endregion

        #region Private Fields

        /// <summary>
        /// Typed reference to SyringeItemSO for easy access to syringe-specific data.
        /// </summary>
        private SyringeItemSO _syringeItemSO;

        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();

            // Cache typed SO reference
            if (_itemSO is SyringeItemSO syringeItemSO)
            {
                _syringeItemSO = syringeItemSO;
            }
            else
            {
                Debug.LogError("[SyringeInventoryItem] ItemSO is not SyringeItemSO!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // Base class handles all callback registration
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

        #endregion

        #region Item Usage - Inject Syringe

        /// <summary>
        /// Primary use: Inject syringe to apply effect.
        /// Can only be used once - syringe becomes unusable after injection.
        /// </summary>
        protected override bool CanUse()
        {
            // Check if already used
            if (IsUsed.Value)
            {
                Debug.Log("[SyringeInventoryItem] Syringe already used");
                return false;
            }

            return base.CanUse();
        }

        protected override void ExecuteUsageLogic()
        {
            // Owner requests injection from server
            if (IsOwner)
            {
                InjectSyringeServerRpc();
            }
        }

        /// <summary>
        /// SERVER-ONLY: Injects syringe and applies effect to player.
        /// Marks syringe as used.
        /// </summary>
        [ServerRpc]
        private void InjectSyringeServerRpc()
        {
            // Validate not already used
            if (IsUsed.Value)
            {
                return;
            }

            // Mark as used
            IsUsed.Value = true;

            // TODO: Apply effect to player
            // This would typically:
            // 1. Get player's movement controller
            // 2. Apply speed boost for EffectDuration
            // 3. Visual feedback (injection animation, particle effects)

            // For now, just log
            Debug.Log($"[Server] Syringe injected! Speed boost: {_syringeItemSO.SpeedBoostAmount} for {_syringeItemSO.EffectDuration}s");

            // TODO: Start coroutine or timer to remove effect after EffectDuration
            // TODO: Visual feedback via ClientRpc
        }

        #endregion
    }
}
