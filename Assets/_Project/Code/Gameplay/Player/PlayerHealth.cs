using _Project.Code.Gameplay.FirstPersonController;
using _Project.Code.Network.GameManagers;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Singletons;
using Network.Scripts.PlayerController;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerInputManager = _Project.Code.Gameplay.FirstPersonController.PlayerInputManager;
using PlayerLook = _Project.Code.Gameplay.FirstPersonController.PlayerLook;

namespace _Project.Code.Gameplay.Player
{
    public class PlayerHealth : NetworkBehaviour, IPlayerHealth
    {
        float _currentHealth;
        [SerializeField] float _maxHealth;
        private bool _isDead;
        public bool IsDead => _isDead;
        private PlayerLook _playerLook;
        private PlayerInputManager _playerInput;
        private PlayerInputManagerSpectator _spectatorInput;
        public void Awake()
        {
            _currentHealth = _maxHealth;
            _playerLook = GetComponent<PlayerLook>();
         
        }
        public void TakeDamage(float damage)
        {
            if (_currentHealth <= 0) return;
            _currentHealth -= damage;
            if (_currentHealth < 0)
            {
                Debug.Log("Player is DEAD");
                _isDead = true;

                if (IsOwner)
                {
                   
                    /*var playerInputSpectator = GetComponent<PlayerInputManagerSpectator>();
                    playerInputSpectator.enabled = true;*/
                    var playerInput = GetComponent<PlayerInputManager>();
                    playerInput.SwitchToSpectatorMode();
                   // SpectatorController.Instance.EnterSpectatorMode();
                   HandleDeathServer();

                
                  //  HandleDeathServer();
                    EventBus.Instance.Publish<PlayerDiedEvent>(new PlayerDiedEvent{deadPlayer =this.gameObject});
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

        private void DisableComponent()
        {
            var stateMachine = GetComponent<PlayerStateMachine.PlayerStateMachine>();
            stateMachine.enabled = false;
            var renders = GetComponentsInChildren<Renderer>();
            foreach (var render in renders)
            {
                render.enabled = false;
            }

            var playerLook = GetComponent<PlayerLook>();
            playerLook.enabled = false;
        }

        [ClientRpc]
        private void NotifyDeathClientRpc()
        {
            if (IsOwner)
            {
                
             //   EventBus.Instance.Publish<PlayerDiedEvent>(new PlayerDiedEvent{NameOfPlayer = "sad"});
            }
        }
    }

    public struct PlayerDiedEvent : IEvent
    {
        public GameObject deadPlayer;
    }
}