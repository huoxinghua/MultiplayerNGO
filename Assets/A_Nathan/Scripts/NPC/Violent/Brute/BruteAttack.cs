using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteAttack : MonoBehaviour
{
    [SerializeField] BruteStateController _stateController;
    bool _isOnCooldown;
    [SerializeField] BruteSO _bruteSO;
    float attackDistance => _bruteSO.AttackDistance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnAttack(GameObject playerAttacked)
    {
        _isOnCooldown = true;
        AttemptAttack(playerAttacked);
        StartCoroutine(AttackCooldown());
    }
    public void AttemptAttack(GameObject playerAttacked)
    {
        
    }
    public void OnAnimationEnd()
    {
        _isOnCooldown = false;
    }

    //temp coroutine until animation
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(2f);
        OnAnimationEnd();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!_isOnCooldown && _stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            foreach (PlayerList player in PlayerList.AllPlayers) 
            {
                if (Vector3.Distance(player.transform.position, transform.position) < attackDistance)
                {
                    OnAttack(player.gameObject);
                    return;
                }
            }
        }
    }
}
