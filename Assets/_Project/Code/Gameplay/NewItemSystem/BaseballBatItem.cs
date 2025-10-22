using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.Utility;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.BaseballBat;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class BaseballBatItem : BaseInventoryItem
    {

        private Timer _attackCooldownTimer = new Timer(1);
        private bool _canAttack = true;
        private float attackTime = 2f;
        private void Update()
        {
            if (_hasOwner)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            _attackCooldownTimer.TimerUpdate(Time.deltaTime);
            if (_attackCooldownTimer.IsComplete)
            {
                _canAttack = true;
            }
        }
        void PerformMeleeAttack()
        {
            if (_itemSO is BaseballBatItemSO _baseballBatSO)
            {
                LayerMask enemyLayer = LayerMask.GetMask("Enemy");

                Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward
                    * _baseballBatSO.AttackDistance * 0.5f, _baseballBatSO.AttackRadius, enemyLayer);
                if (hitEnemies.Length > 0)
                {
                    //play hit sound??
                    //Debug.Log("?A?DA?");
                    AudioManager.Instance.PlayByKey3D("BaseBallBatHit", hitEnemies[0].transform.position);
                }

                foreach (Collider enemy in hitEnemies)
                {
                    enemy.gameObject.GetComponent<IHitable>()?.OnHit(_owner,
                        _baseballBatSO.Damage, _baseballBatSO.KnockoutPower);
                    // Debug.Log(enemy.gameObject.name);
                    //  enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
                }
            }
        }


        public override void UseItem()
        {
            if (_canAttack)
            {
                PerformMeleeAttack();
                _attackCooldownTimer.Reset(attackTime);
                _canAttack = false;
            }
        }

    }
}
