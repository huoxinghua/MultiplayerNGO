using _Project.Code.Art.RagdollScripts;
using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

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
            if(!IsServer)return;
            _currentHealth += healthChange;

            if (_currentHealth < 0)
            {
                OnDeath();
            }
        }

        public void OnDeath()
        {
           RequestOnDeathServerRpc();
           DetachRagdollServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestOnDeathServerRpc()
        {
            DisableVisualClientRPC();
        }

        [ClientRpc]
        void DisableVisualClientRPC()
        {
            var mesh = GetComponent<MeshRenderer>();
            if (mesh != null)
                mesh.enabled = false;

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.enabled = false;
            }

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.enabled = false;
            }

            if (_stateMachine != null)
            {
                _stateMachine.OnDeath();
                _stateMachine.enabled = false;
            }

            if (_ragdoll != null)
                _ragdoll.EnableRagdoll();

            if (_bruteDead != null)
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
        }
    }
}
