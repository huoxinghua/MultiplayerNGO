using System.Collections.Generic;
using UnityEngine;

public class BeetleHealth : MonoBehaviour,IHitable
{
    //add players who attacked to list

    [SerializeField] private BeetleSO _beetleSO;
    public List<GameObject> HostilePlayers = new List<GameObject>();
    [SerializeField] private BeetleMove _beetleMove;
    [SerializeField] private BeetleState _beetleState;
    private float _maxHealth;
    private float _currentHealth;
    private float _maxConsciousness;
    private float _currentConsciousness;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _maxHealth = _beetleSO.MaxHealth;
        _currentHealth = _maxHealth;
        _maxConsciousness = _beetleSO.MaxConsciousness;
        _currentConsciousness = _maxConsciousness;
    }
    public bool IsPlayerHostile(GameObject playerToCheck)
    {
        bool isHostile = false;
        foreach(var hostilePlayer  in HostilePlayers)
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
        _beetleState.TransitionToState(BeetleStates.KnockedOut);
    }
    public void OnDeath()
    {
        _beetleState.TransitionToState(BeetleStates.Dead);
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
            foreach(var player in HostilePlayers)
            {
                if (player == attacker)
                {
                    isInList = true;
                }
            }
            if (!isInList) HostilePlayers.Add(attacker);
        }
        _beetleMove.RunFromPlayer(attacker.transform);
        _beetleState.TransitionToState(BeetleStates.RunAway);
        ChangeHealth(-damage);
        ChangeConsciousness(-knockoutPower);
     //   Debug.Log("Was hit");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
