using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Interactables.Network;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.Code.Utilities.EventBus;
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
        public NetworkList<ulong> AlivePlayers = new NetworkList<ulong>();
 
        public void RegisterPlayerObj(PlayerStateMachine player)
        {
            if (!IsServer) return;

            ulong cid = player.OwnerClientId;

            if (!_alivePlayersObj.Contains(player))
                _alivePlayersObj.Add(player);

            if (!AlivePlayers.Contains(cid))
                AlivePlayers.Add(cid);
             
            Debug.Log("RegisterPlayerObj after  alive count:" + AlivePlayers.Count);

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
        public void OnPlayerDied(ulong deadClientId)
        {
            
            
            //right
            if (!IsServer)
                return;

            Debug.Log($"[Server] Player died: {deadClientId}");

    
            if (AlivePlayers.Contains(deadClientId))
                AlivePlayers.Remove(deadClientId);

         

            if (AlivePlayers.Count <= 0)
            {
                GameFlowManager.Instance.ReturnToHub();
                ClearEnemiesInHub();
                EventBus.Instance?.Publish(new AllPlayerDiedEvent { });
            }
            else
            {
                Debug.Log("Enter to spectator cam");
                SendEnterSpectatorClientRpc(deadClientId);
            }


          
        }
        [ClientRpc]
        private void SendEnterSpectatorClientRpc(ulong targetClientId)
        {
        
            if (NetworkManager.Singleton.LocalClientId != targetClientId)
                return;

            Debug.Log("[Client] I died → entering spectator mode");
            SpectatorController.Instance.EnterSpectatorMode();
        }
        /*
        public void OnPlayerDied(ulong deadClientId)
        {
            Debug.Log($"[Server] Player died: {deadClientId}");

            AlivePlayers.Remove(deadClientId);
            /*if (_alivePlayersObj.Contains(player))
            {
                _alivePlayersObj.Remove(player);
                Debug.Log("After remove alive count:" + _alivePlayersObj.Count);
            }#1#

            if (_alivePlayersObj.Count <= 0)
            {
                GameFlowManager.Instance.ReturnToHub();
                ClearEnemiesInHub();
                EventBus.Instance?.Publish(new AllPlayerDiedEvent { });
            }
            else
            {
                Debug.Log("Enter to spectator cam");
                SpectatorController.Instance.EnterSpectatorMode();
            }
        }
        */

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
    public class AllPlayerDiedEvent : IEvent
    {
       
    }
}