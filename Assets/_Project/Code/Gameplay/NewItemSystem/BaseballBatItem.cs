using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.Utility;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.BaseballBat;
using UnityEngine;
using Unity.Netcode;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class BaseballBatItem : BaseInventoryItem
    {
        #region Setup + Update
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");
            // Now add flashlight-specific network setup
            CustomNetworkSpawn();
        }   
        private void Update()
        {
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
        }
        #endregion
        
        #region Use Logic
        public override void UseItem()
        {
            base.UseItem();

            if (IsOwner)
            {
                RequestAttackServerRpc();
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void RequestAttackServerRpc()
        {
            PerformMeleeAttack();
        }
         protected virtual void PerformMeleeAttack()
        {
         //   Debug.Log("BaseBallBatItem】:PerformMeleeAttack" +"IsServer："+IsServer+"IsHost：="+IsHost+ "IsClient:" +IsClient);
            if (_itemSO is BaseballBatItemSO _baseballBatSO)
            {
             //   Debug.Log("_itemSO is BaseballBatItemSO _baseballBatSO");
                LayerMask enemyLayer = LayerMask.GetMask("Enemy");

                var player = _owner; 
                var origin = player.transform.position + player.transform.forward * _baseballBatSO.AttackRadius; 
                Collider[] hitEnemies = Physics.OverlapSphere(
                    origin,
                    _baseballBatSO.AttackRadius,
                    LayerMask.GetMask("Enemy"));
                
                /*Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward
                    * _baseballBatSO.AttackDistance * 0.5f, _baseballBatSO.AttackRadius, enemyLayer);*/
                if (hitEnemies.Length > 0)
                {
                    //play hit sound??
                    //Debug.Log("?A?DA?");
                    AudioManager.Instance.PlayByKey3D("BaseBallBatHit", hitEnemies[0].transform.position);
                }

                foreach (Collider enemy in hitEnemies)
                {
                    Debug.Log("PerformMeleeAttack!");
                    /*enemy.gameObject.GetComponent<IHitable>()?.OnHit(_owner,
                        _baseballBatSO.Damage, _baseballBatSO.KnockoutPower);*/
                    
                    var enemyNetObj = enemy.GetComponentInParent<NetworkObject>();
                    var attackerNetObj = _owner.GetComponent<NetworkObject>();

                    if (enemyNetObj != null && attackerNetObj != null)
                    {
                       
                        RequestHitServerRpc(enemyNetObj, attackerNetObj,
                            _baseballBatSO.Damage, _baseballBatSO.KnockoutPower);
                    }
                    else
                    {
                        Debug.LogWarning($"[Bat] {enemy.name} missing NetworkObject!");
                    }

                }
            }
            else
            {
                Debug.LogWarning(" false _itemSO is BaseballBatItemSO _baseballBatSO");
            }
        }
         
        [ServerRpc(RequireOwnership = false)]
        protected void RequestHitServerRpc(NetworkObjectReference targetRef, NetworkObjectReference attackerRef,
            float damage, float knockout)
        {
            Debug.Log("RequestHitServerRpc");
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
