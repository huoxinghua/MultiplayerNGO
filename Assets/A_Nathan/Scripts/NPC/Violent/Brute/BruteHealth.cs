using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BruteHealth : MonoBehaviour , IHitable
{
    float _maxHealth;
    float _currentHealth;
    float _maxConsciousness;
    float _currentConsciousness;
    [SerializeField] BruteSO _bruteSO;
    [SerializeField] BruteStateController _stateController;
    public void Awake()
    {
        _maxHealth = _bruteSO.MaxHealth;
        _currentHealth = _maxHealth;
        _maxConsciousness = _bruteSO.MaxConsciousness;
        _currentConsciousness = _maxConsciousness;
    }
    public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
    {
        if(_stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            ChangeHealth(-damage);
            ChangeConsciousness(-knockoutPower);
        }
        else if(_stateController.GetAttentionState() == BruteAttentionStates.Unaware)
        {
            _stateController.StartChasePlayer(attackingPlayer);
        }
    }
    public void ChangeConsciousness(float consciousnessChange)
    {
        _currentConsciousness += consciousnessChange;
        if (_currentConsciousness < 0)
        {
            OnKnockOut();
        }
    }
    public void OnKnockOut()
    {
        //  rb.isKinematic = false;
        //set state to KO. Ragdoll, disable movement
        _stateController.TransitionToAttentionState(BruteAttentionStates.KnockedOut);
    }
    public void ChangeHealth(float healthChange)
    {
        _currentHealth += healthChange;
        if (_currentHealth < 0)
        {
            OnDeath();
        }
    }
    public void OnDeath()
    {
        //set state to Dead. Ragdoll, disable movement
        //rb.isKinematic = false;
        _stateController.TransitionToAttentionState(BruteAttentionStates.Dead);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
