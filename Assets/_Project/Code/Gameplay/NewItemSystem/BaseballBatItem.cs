using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Utilities.Audio;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.BaseballBat;
using UnityEngine;
using Unity.Netcode;

namespace _Project.Code.Gameplay.NewItemSystem
{
    /// <summary>
    /// Clean Rewrite: Baseball bat melee weapon.
    /// Performs sphere cast attack on use, deals damage and knockout to enemies.
    /// </summary>
    public class BaseballBatItem : BaseInventoryItem
    {
        #region Initialization

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
        /// This is an alternative to parenting (which can cause NetworkTransform issues).
        /// </summary>
        private void Update()
        {
            if (!IsOwner) return;
            UpdateHeldPosition();
        }

        #endregion

        #region Item Usage - Melee Attack

        /// <summary>
        /// Primary use: Swing baseball bat to attack enemies in radius.
        /// Sends ServerRpc to execute attack on server.
        /// </summary>
        public override void UseItem()
        {
            Debug.Log($"[BaseballBat] UseItem() called - IsOwner:{IsOwner}");

            // Base class handles cooldown check and reset
            base.UseItem();

            // Owner requests attack from server
            if (IsOwner)
            {
                Debug.Log("[BaseballBat] Calling RequestAttackServerRpc()");

                // Force state change by resetting to None first, then Interact
                // This ensures OnValueChanged fires even on repeat attacks
                CurrentAnimState.Value = _Project.Code.Art.AnimationScripts.IK.IKAnimState.None;
                CurrentAnimState.Value = _Project.Code.Art.AnimationScripts.IK.IKAnimState.Interact;
                AnimTime.Value = 0f;

                RequestAttackServerRpc();
            }
            else
            {
                Debug.Log("[BaseballBat] Not owner, cannot use item");
            }
        }

        /// <summary>
        /// SERVER-ONLY: Executes melee attack on server.
        /// Sphere casts for enemies, deals damage, plays audio.
        /// </summary>
        [ServerRpc]
        private void RequestAttackServerRpc()
        {
            Debug.Log("[BaseballBat] RequestAttackServerRpc() called on server");
            PerformMeleeAttack();
        }

        /// <summary>
        /// SERVER-ONLY: Performs the actual melee attack logic.
        /// Virtual to allow child classes (Machete, SledgeHammer) to override.
        /// </summary>
        protected virtual void PerformMeleeAttack()
        {
            Debug.Log("[BaseballBat] PerformMeleeAttack() started");

            // Validate we have baseball bat SO
            if (_itemSO is not BaseballBatItemSO baseballBatSO)
            {
                Debug.LogWarning("[BaseballBatItem] ItemSO is not BaseballBatItemSO");
                return;
            }

            // Validate owner exists (server-only field)
            if (_owner == null)
            {
                Debug.LogWarning("[BaseballBatItem] Owner is null when attacking");
                return;
            }

            Debug.Log($"[BaseballBat] Attacking with radius:{baseballBatSO.AttackRadius} damage:{baseballBatSO.Damage}");

            // Sphere cast for enemies
            Vector3 origin = _owner.transform.position + _owner.transform.forward * baseballBatSO.AttackRadius;
            Collider[] hitEnemies = Physics.OverlapSphere(
                origin,
                baseballBatSO.AttackRadius,
                LayerMask.GetMask("Enemy"));

            // Play hit sound if any enemies hit
            if (hitEnemies.Length > 0)
            {
                AudioManager.Instance.PlayByKey3D("BaseBallBatHit", hitEnemies[0].transform.position);
            }

            // Get attacker NetworkObject reference for damage attribution
            NetworkObject attackerNetObj = _owner.GetComponent<NetworkObject>();
            if (attackerNetObj == null)
            {
                Debug.LogWarning("[BaseballBatItem] Owner has no NetworkObject component");
                return;
            }

            // Deal damage to all hit enemies
            foreach (Collider enemyCollider in hitEnemies)
            {
                // Get enemy's NetworkObject (might be on parent)
                NetworkObject enemyNetObj = enemyCollider.GetComponentInParent<NetworkObject>();
                if (enemyNetObj == null)
                {
                    Debug.LogWarning($"[BaseballBatItem] {enemyCollider.name} missing NetworkObject");
                    continue;
                }

                // Get enemy's IHitable interface
                IHitable hitable = enemyNetObj.GetComponent<IHitable>();
                if (hitable == null)
                {
                    Debug.LogWarning($"[BaseballBatItem] {enemyNetObj.name} missing IHitable component");
                    continue;
                }

                // Deal damage (this is already on server, so call directly)
                hitable.OnHit(attackerNetObj.gameObject, baseballBatSO.Damage, baseballBatSO.KnockoutPower);
            }
        }

        /// <summary>
        /// SERVER-ONLY: Helper method for child classes to deal damage to enemies.
        /// Used by Machete and SledgeHammer which override PerformMeleeAttack.
        /// </summary>
        /// <param name="targetRef">Enemy NetworkObject reference</param>
        /// <param name="attackerRef">Attacker NetworkObject reference</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="knockout">Knockout power</param>
        [ServerRpc(RequireOwnership = false)]
        protected void RequestHitServerRpc(NetworkObjectReference targetRef, NetworkObjectReference attackerRef,
            float damage, float knockout)
        {
            if (targetRef.TryGet(out NetworkObject targetObj))
            {
                var hitable = targetObj.GetComponent<IHitable>();
                if (hitable != null)
                {
                    hitable.OnHit(attackerRef.TryGet(out var atk) ? atk.gameObject : null, damage, knockout);
                }
                else
                {
                    Debug.LogWarning($"[ServerRpc] {targetObj.name} missing IHitable!");
                }
            }
            else
            {
                Debug.LogWarning("[ServerRpc] Failed to resolve target NetworkObjectReference");
            }
        }

        #endregion
    }
}
