using _Project.Code.Network.GameManagers;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerHealth
{
    public class PlayerHealth : NetworkBehaviour, IPlayerHealth
    {
        [SerializeField] float _maxHealth;
        private NetworkVariable<float> _currentHealth = new NetworkVariable<float>();
        private NetworkVariable<bool> _isDead = new NetworkVariable<bool>();
        public bool IsDead => _isDead.Value;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _currentHealth.Value = _maxHealth;
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsServer) return;
            if (_isDead.Value) return;

            _currentHealth.Value -= damage;
            if (_currentHealth.Value <= 0f)
            {
                _currentHealth.Value = 0f;
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            if (_isDead.Value) return;
            
            _isDead.Value = true;
        
            var sm = GetComponent<PlayerStateMachine.PlayerStateMachine>();
            ulong deadClientId = OwnerClientId;
            PlayerListManager.Instance.OnPlayerDied(deadClientId);
            if (sm != null)
            {
                sm.TransitionTo(sm.DeadState);
               
            }
            HandleDeathClientRpc(deadClientId);
        }

        [ClientRpc]
        private void HandleDeathClientRpc(ulong deadClientId)
        {
            if (NetworkManager.Singleton.LocalClientId != deadClientId)
                return;

            var sm = GetComponent<PlayerStateMachine.PlayerStateMachine>();

            if (sm != null)
                sm.TransitionTo(sm.DeadState);
        }
    }
}