using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Interactables.Network;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Code.Network.GameManagers
{
    public class PlayerListManager : NetworkSingleton<PlayerListManager>
    {
        private readonly List<ulong> _alivePlayers = new List<ulong>();
        private readonly List<ulong> _deadPlayers = new List<ulong>();
        private int alivePlayers;

        public HashSet<PlayerStateMachine> _alivePlayersObj = new
            HashSet<PlayerStateMachine>();

        protected override bool PersistBetweenScenes => true;

 
        public void RegisterPlayerObj(PlayerStateMachine player)
        {
            Debug.Log("before  alive count:" + _alivePlayers.Count);
            _alivePlayersObj.Add(player);
            Debug.Log("after  alive count:" + _alivePlayers.Count);
        }

        public void UnregisterPlayerObj(PlayerStateMachine player)
        {
            Debug.Log("UnregisterPlayerObj before  alive count:" + _alivePlayers.Count);
            _alivePlayersObj.Remove(player);
            Debug.Log("UnregisterPlayerObj after  alive count:" + _alivePlayers.Count);
        }

        public void ClearEnemiesInHub()
        {
            foreach (var enemy in FindObjectsOfType<BruteStateMachine>())
                Destroy(enemy.gameObject);

            foreach (var door in FindObjectsOfType<SwingDoors>())
                Destroy(door.gameObject);
        }

        public void OnPlayerDied(PlayerStateMachine player)
        {
            if (_alivePlayersObj.Contains(player))
            {
                _alivePlayersObj.Remove(player);
                Debug.Log("After remove alive count:" + _alivePlayersObj.Count);
            }

            if (_alivePlayersObj.Count <= 0)
            {
                GameFlowManager.Instance.ReturnToHub();
                ClearEnemiesInHub();
            }
            else
            {
                Debug.Log("Enter to spectator cam");
                SpectatorController.Instance.EnterSpectatorMode();
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            _alivePlayers.Add(clientId);
            Debug.Log("alivePlayer:" + _alivePlayers.Count);
            foreach (var player in _alivePlayers)
            {
                //  Debug.Log("player:"+ _alivePlayers.Count + player.clientId);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _alivePlayers.Remove(clientId);
            _deadPlayers.Remove(clientId);
            Debug.Log("alivePlayer:" + _alivePlayers.Count);
            foreach (var player in _alivePlayers)
            {
                //   Debug.Log("player:"+ _alivePlayers.Count + player.clientId);
            }
        }

        [ClientRpc]
        private void UpdateAliveListClientRpc(ulong[] newList)
        {
            _alivePlayers.Clear();
            _alivePlayers.AddRange(newList);
        }

        public List<ulong> GetAlivePlayers() => _alivePlayers;
    }
}