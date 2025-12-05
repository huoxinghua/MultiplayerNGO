using _Project.Code.Utilities.Audio;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.MacheteItem;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class MacheteInventoryItem : BaseballBatItem
    {
        protected override void PerformMeleeAttack()
        {
            if (_itemSO is MacheteItemSO _macheteSO)
            {
                LayerMask enemyLayer = LayerMask.GetMask("Enemy");

                var player = _owner;
                var origin = player.transform.position + player.transform.forward * _macheteSO.AttackRadius;
                Collider[] hitEnemies = Physics.OverlapSphere(
                    origin,
                    _macheteSO.AttackRadius,
                    LayerMask.GetMask("Enemy"));

                /*Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward
                * _baseballBatSO.AttackDistance * 0.5f, _baseballBatSO.AttackRadius, enemyLayer);*/
                if (hitEnemies.Length > 0)
                {
                    //play hit sound??
                    AudioManager.Instance.PlayByKey3D("BaseBallBatHit", hitEnemies[0].transform.position);
                }

                foreach (Collider enemy in hitEnemies)
                {
                    /*enemy.gameObject.GetComponent<IHitable>()?.OnHit(_owner,
                    _baseballBatSO.Damage, _baseballBatSO.KnockoutPower);*/

                    var enemyNetObj = enemy.GetComponentInParent<NetworkObject>();
                    var attackerNetObj = _owner.GetComponent<NetworkObject>();

                    if (enemyNetObj != null && attackerNetObj != null)
                    {

                        RequestHitServerRpc(enemyNetObj, attackerNetObj,
                            _macheteSO.Damage, _macheteSO.KnockoutPower);
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
    }
}
