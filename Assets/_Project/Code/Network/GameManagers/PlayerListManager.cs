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
        private HashSet<PlayerStateMachine> _alivePlayersObj = new
            HashSet<PlayerStateMachine>();
        protected override bool PersistBetweenScenes => true;

        public void RegisterPlayer(PlayerStateMachine player)
        {
            alivePlayers++;
        }
        public void RegisterPlayerObj(PlayerStateMachine player)
        {
            _alivePlayersObj.Add(player);
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
            _alivePlayersObj.Remove(player);

            if (_alivePlayersObj.Count <= 0)
            {

                GameFlowManager.Instance.ReturnToHub();
                ClearEnemiesInHub();
            }
            else
            {
              
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
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _alivePlayers.Remove(clientId);
            _deadPlayers.Remove(clientId);
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