using System.Collections.Generic;
using _Project.Code.Gameplay.EnemySpawning;
using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteHeart : NetworkBehaviour, IHitable
    {
        private BruteStateMachine _controller;
        private List<PlayerList> _players = PlayerList.AllPlayers;

        [SerializeField] private float _heartBeatFrequency;

        [Header("Heart Defense")]
        [SerializeField] private float _health;
        [SerializeField] private float _playerCheckFrequency;
        [SerializeField] private float _defendDistance;

        private Timer _proximityCheckTimer;
        private Timer _heartBeatTimer;

        private void Awake()
        {
            _heartBeatTimer = new Timer(_heartBeatFrequency);
            _proximityCheckTimer = new Timer(_playerCheckFrequency);
            _heartBeatTimer.Start();
            _proximityCheckTimer.Start();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer && EnemySpawnManager.Instance != null)
            {
                EnemySpawnManager.Instance.RegisterEnemyRelatedObject(NetworkObject);
            }
        }

        public void SetStateController(BruteStateMachine stateController)
        {
            _controller = stateController;
        }

        private void Update()
        {
            _heartBeatTimer.TimerUpdate(Time.deltaTime);
            _proximityCheckTimer.TimerUpdate(Time.deltaTime);

            // Heartbeat sound (all clients)
            if (_heartBeatTimer.IsComplete)
            {
                _heartBeatTimer.Reset();
                AudioManager.Instance.PlayByKey3D("BruteHeartBeat", transform.position);
            }

            // Proximity check (server only)
            if (!IsServer || _controller == null) return;

            if (_proximityCheckTimer.IsComplete)
            {
                _proximityCheckTimer.Reset();
                CheckPlayerProximity();
            }
        }

        private void CheckPlayerProximity()
        {
            foreach (var player in _players)
            {
                if (Vector3.Distance(player.transform.position, transform.position) <= _defendDistance)
                {
                    _controller.HandleDefendHeart(player.gameObject);
                }
            }
        }

        public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
        {
            _health -= damage;
            if (_health < 0)
            {
                _controller.TransitionTo(_controller.BruteHurtIdleState);
                var netObj = GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn(true);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer && EnemySpawnManager.Instance != null)
            {
                EnemySpawnManager.Instance.UnregisterEnemyRelatedObject(NetworkObject);
            }

            if (_controller != null)
                _controller.OnHeartDestroyed();
        }
    }
}
