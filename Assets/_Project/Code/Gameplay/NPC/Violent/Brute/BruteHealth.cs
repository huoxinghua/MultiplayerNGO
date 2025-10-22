using _Project.Code.Art.RagdollScripts;
using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteHealth : MonoBehaviour, IHitable
    {
        private float _maxHealth;
        private float _currentHealth;
        private float _maxConsciousness;
        private float _currentConsciousness;
        [SerializeField] private BruteSO _bruteSO;
        [SerializeField] private Ragdoll _ragdoll;
        [SerializeField] private GameObject _ragdolledObj;
        [SerializeField] private BruteDead _bruteDead;
        [SerializeField] private BruteStateMachine _stateMachine;
        public void Awake()
        {
            _maxHealth = _bruteSO.MaxHealth;
            _currentHealth = _maxHealth;
            _maxConsciousness = _bruteSO.MaxConsciousness;
            _currentConsciousness = _maxConsciousness;
        }
        public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
        {
            ChangeHealth(-damage);
            ChangeConsciousness(-knockoutPower);
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
    }
}
