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

        private Timer _attackCooldownTimer = new Timer(1);
        private bool _canAttack = true;
        private float attackTime = 2f;
        private void Update()
        {
            
            /*if (_hasOwner)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }*/
            _attackCooldownTimer.TimerUpdate(Time.deltaTime);
            if (_attackCooldownTimer.IsComplete)
            {
                _canAttack = true;
            }
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
        }
     
        void PerformMeleeAttack()
        {
            Debug.Log("BaseBallBatItem】:PerformMeleeAttack" +"IsServer："+IsServer+"IsHost：="+IsHost+ "IsClient:" +IsClient);
            if (_itemSO is BaseballBatItemSO _baseballBatSO)
            {
                Debug.Log("_itemSO is BaseballBatItemSO _baseballBatSO");
                LayerMask enemyLayer = LayerMask.GetMask("Enemy");

                var player = _owner; 
                var origin = player.transform.position + player.transform.forward * 0.8f; 
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
        private void RequestHitServerRpc(NetworkObjectReference targetRef, NetworkObjectReference attackerRef,
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

        public override void UseItem()
        {
            Debug.Log("BaseBallBatItem】:UseItem" +"IsServer："+IsServer+"IsHost：="+IsHost+ "IsClient:"+IsClient);
            if (_canAttack)
            {
                if (IsOwner)
                {
                    RequestAttackServerRpc();
                }
                //PerformMeleeAttack();
               
                _attackCooldownTimer.Reset(attackTime);
                _canAttack = false;
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void RequestAttackServerRpc()
        {
            Debug.Log("[ServerRpc] BaseballBat Attack Received by Server --- FROM " + OwnerClientId);

            PerformMeleeAttack();
        }

    }
}
