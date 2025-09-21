using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteAttack : MonoBehaviour
{
    [SerializeField] BruteStateController _stateController;
    bool _isOnCooldown;
    [SerializeField] BruteSO _bruteSO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnAttack()
    {
        _isOnCooldown = true;
        StartCoroutine(AttackCooldown());
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
    void Update()
    {
        if(_stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            foreach (PlayerList player in PlayerList.AllPlayers) 
            { 
                
            }
        }
    }
}
