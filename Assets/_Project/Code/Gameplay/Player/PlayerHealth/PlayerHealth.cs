using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;
using PlayerInputManager = _Project.Code.Gameplay.Player.MiscPlayer.PlayerInputManager;
using PlayerLook = _Project.Code.Gameplay.Player.MiscPlayer.PlayerLook;

namespace _Project.Code.Gameplay.Player.PlayerHealth
{
    public class PlayerHealth : NetworkBehaviour, IPlayerHealth
    {
        [SerializeField] float _maxHealth;

        private NetworkVariable<float> _currentHealth = new NetworkVariable<float>();

        private NetworkVariable<bool> _isDead = new NetworkVariable<bool>();

        public bool IsDead => _isDead.Value;
        private PlayerLook _playerLook;
        private PlayerInputManager _playerInput;
        private PlayerInputManagerSpectator _spectatorInput;

        public override void OnNetworkSpawn()
        {
            _currentHealth.Value = _maxHealth;
            _playerLook = GetComponent<PlayerLook>();
        }

        public void TakeDamage(float damage)
        {
            if (!IsServer) return; //only the server can deal the health
            if (_isDead.Value) return;

            _currentHealth.Value -= damage;
            if (_currentHealth.Value <= 0f)
            {
                _currentHealth.Value = 0f;
                HandleDeath();
                /*
                // NotifyDeathClientRpc();*/
            }
        }

        private void HandleDeath()
        {
            if (_isDead.Value)
                return;

            _isDead.Value = true;
            Debug.Log($"[Server] Player {OwnerClientId} died");
            HandleDeathClientRpc();
            NetworkObject.Despawn(true);
        }

        [ClientRpc]
        private void HandleDeathClientRpc()
        {
            if (IsOwner)
            {
                var playerInput = GetComponent<PlayerInputManager>();
                playerInput.SwitchToSpectatorMode();
                EventBus.Instance.Publish<PlayerDiedEvent>(new PlayerDiedEvent { deadPlayer = this.gameObject });
            }
        }
    }

    public struct PlayerDiedEvent : IEvent
    {
        public GameObject deadPlayer;
    }
}