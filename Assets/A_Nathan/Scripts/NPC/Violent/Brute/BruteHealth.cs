using UnityEngine;

public class BruteHealth : MonoBehaviour, IHitable
{
    float _maxHealth;
    float _currentHealth;
    float _maxConsciousness;
    float _currentConsciousness;
    [SerializeField] BruteSO _bruteSO;
    [SerializeField] Ragdoll _ragdoll;
    [SerializeField] GameObject _ragdolledObj;
    [SerializeField] BruteDead _bruteDead;
    [SerializeField] BruteStateMachine _stateMachine;
    public void Awake()
    {
        _maxHealth = _bruteSO.MaxHealth;
        _currentHealth = _maxHealth;
        _maxConsciousness = _bruteSO.MaxConsciousness;
        _currentConsciousness = _maxConsciousness;
    }
    public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
    {
        ChangeHealth(damage);
        ChangeConsciousness(knockoutPower);
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

    }
    public void ChangeHealth(float healthChange)
    {
        _currentHealth += healthChange;

        Debug.Log(_currentHealth);
        if (_currentHealth < 0)
        {
            OnDeath();
        }
    }
    public void OnDeath()
    {
        _stateMachine.OnDeath();
        _ragdoll.EnableRagdoll();
        _ragdolledObj.transform.SetParent(null);
        Destroy(gameObject);
        _bruteDead.enabled = true;
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
