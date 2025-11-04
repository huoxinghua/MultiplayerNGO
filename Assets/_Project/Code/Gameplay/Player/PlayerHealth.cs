using _Project.Code.Network.GameManagers;
using _Project.Code.Utilities.Singletons;
using Network.Scripts.PlayerController;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerLook = _Project.Code.Gameplay.FirstPersonController.PlayerLook;

namespace _Project.Code.Gameplay.Player
{
    public class PlayerHealth : NetworkBehaviour, IPlayerHealth
    {
        float _currentHealth;
        [SerializeField] float _maxHealth;
        private bool _isDead;
        public bool IsDead => _isDead;

        public void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (_currentHealth <= 0) return;
            Debug.Log("Player TakeDamage" + damage);
            _currentHealth -= damage;
            Debug.Log("Player currenthealth" + _currentHealth);
            if (_currentHealth < 0)
            {
                //temp for now
                Debug.Log("Player is DEAD");
                _isDead = true;

                if (IsOwner)
                {
                    SpectatorController.Instance.EnterSpectatorMode();
                    HandleDeathServer();
                }

              //  CurrentPlayers.Instance.RemovePlayer(gameObject, OwnerClientId);

                // PlayerListManager.Instance.ReportDeathServerRpc(OwnerClientId);
                // NotifyDeathClientRpc();
            }
        }

        private void HandleDeathServer()
        {
            Debug.Log($"[Server] Player {OwnerClientId} died");
            //SpawnCorpseAt transform.position;
            RequestDespawnDeadPlayerServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDespawnDeadPlayerServerRpc()
        {
            NetworkObject.Despawn(true);
        }

        [ClientRpc]
        private void NotifyDeathClientRpc()
        {
            if (IsOwner)
            {
                GetComponent<PlayerLook>().enabled = false;
                FindObjectOfType<SpectatorController>().EnterSpectatorMode();
            }
        }
    }
}