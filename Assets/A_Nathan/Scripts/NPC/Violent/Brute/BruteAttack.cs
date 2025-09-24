using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteAttack : MonoBehaviour
{
    [SerializeField] BruteStateController _stateController;
    bool _isOnCooldown;
    [SerializeField] BruteSO _bruteSO;
    [SerializeField] BruteMovement _bruteMovement;
    [SerializeField] BruteAnimation _bruteAnimation;
    float attackDistance => _bruteSO.AttackDistance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnAttack(GameObject playerAttacked)
    {
        _bruteAnimation.PlayAttack();
        _isOnCooldown = true;
        _bruteMovement.StopForAttack();
        StartCoroutine(AttackCooldown(playerAttacked));
    }
    public void AttemptAttack(GameObject playerAttacked)
    {
        if(Vector3.Distance(playerAttacked.transform.position, transform.position) < attackDistance)
        {
            if (playerAttacked.TryGetComponent(out IPlayerHealth damageable))
            {
                Debug.Log("HurtPlayer");
                damageable.TakeDamage(_bruteSO.Damage); // or whatever damage amount
            }
        }
    }
    public void OnAnimationEnd()
    {
        _isOnCooldown = false;
        Debug.Log("HurtPlayer");
    }

    //temp coroutine until animation
    IEnumerator AttackCooldown(GameObject playerAttacked)
    {
        yield return new WaitForSeconds(1f);
        AttemptAttack(playerAttacked);
        yield return new WaitForSeconds(1f);
        _bruteMovement.ResumeAfterAttack();
        OnAnimationEnd();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_isOnCooldown && (_stateController.GetAttentionState() == BruteAttentionStates.Hurt || _stateController.GetBehaviourState() == BruteBehaviourStates.Chase))
        {
            foreach (PlayerList player in PlayerList.AllPlayers) 
            {
                if (Vector3.Distance(player.transform.position, transform.position) < attackDistance)
                {
                    OnAttack(player.gameObject);

                    break;
                }
            }
        }
    }
}
