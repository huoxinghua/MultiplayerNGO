using System.Collections.Generic;
using UnityEngine;

public class BeetleHealth : MonoBehaviour,IHitable
{
    //add players who attacked to list

    [SerializeField] BeetleSO beetleSO;
    public List<GameObject> hostilePlayers = new List<GameObject>();
    [SerializeField] BeetleMove beetleMove;
    [SerializeField] BeetleState beetleState;
    float _maxHealth;
    float _currentHealth;
    float _maxConsciousness;
    float _currentConsciousness;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _maxHealth = beetleSO.MaxHealth;
        _currentHealth = _maxHealth;
        _maxConsciousness = beetleSO.MaxConsciousness;
        _currentConsciousness = _maxConsciousness;
    }
    public bool IsPlayerHostile(GameObject playerToCheck)
    {
        bool isHostile = false;
        foreach(var hostilePlayer  in hostilePlayers)
        {
            if(playerToCheck == hostilePlayer)
            {
                isHostile = true;
            }
        }
        return isHostile;
    }
    public void ChangeHealth(float healthChange)
    {
        _currentHealth += healthChange;
        if (_currentHealth < 0)
        {
            OnDeath();
        }
    }
    public void OnKnockOut()
    {
        beetleState.TransitionToState(BeetleStates.KnockedOut);
    }
    public void OnDeath()
    {
        beetleState.TransitionToState(BeetleStates.Dead);
    }
   /* public void OnKnockout()
    {
        beetleState.TransitionToState(BeetleStates.KnockedOut);
    }*/
    public void ChangeConsciousness(float consciousnessChange)
    {
        _currentConsciousness += consciousnessChange;
        if(_currentConsciousness < 0)
        {
            OnKnockOut();
        }
    }
    public void OnHit(GameObject attacker, float damage, float knockoutPower)
    {
        if (attacker.layer == 6)
        {
            bool isInList = false;
            foreach(var player in hostilePlayers)
            {
                if (player == attacker)
                {
                    isInList = true;
                }
            }
            if (!isInList) hostilePlayers.Add(attacker);
        }
        beetleMove.RunFromPlayer(attacker.transform);
        beetleState.TransitionToState(BeetleStates.RunAway);
        ChangeHealth(-damage);
        ChangeConsciousness(-knockoutPower);
     //   Debug.Log("Was hit");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
