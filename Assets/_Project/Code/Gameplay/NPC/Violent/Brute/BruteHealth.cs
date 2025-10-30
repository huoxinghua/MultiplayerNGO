using _Project.Code.Art.RagdollScripts;
using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using UnityEngine;
using Unity.Netcode;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteHealth : NetworkBehaviour, IHitable
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
            if (!IsServer) return;
            _stateMachine.OnDeath();
            _ragdoll.EnableRagdoll();
            _ragdolledObj.transform.SetParent(null);
            DetachRagdollServerRpc();
            var netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
               
                if (IsServer)
                    netObj.Despawn(true); 
            }
            else
            {
               
                Destroy(gameObject);
            }
            _bruteDead.enabled = true;
        }
        
        [ServerRpc]
        void DetachRagdollServerRpc()
        {
            DetachRagdollClientRpc();
        }

        [ClientRpc]
        void DetachRagdollClientRpc()
        {
            _ragdoll.EnableRagdoll();
            _ragdolledObj.transform.SetParent(null);
        }
    }
}
