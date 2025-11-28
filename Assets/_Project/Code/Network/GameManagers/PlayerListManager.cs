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
        protected override bool PersistBetweenScenes => true;
        public NetworkList<ulong> AlivePlayers = new NetworkList<ulong>();
 
        public void RegisterPlayerObj(PlayerStateMachine player)
        {
            if (!IsServer) return;

            ulong cid = player.OwnerClientId;

            if (!AlivePlayers.Contains(cid))
                AlivePlayers.Add(cid);
            Debug.Log("RegisterPlayerObj after  alive count:" + AlivePlayers.Count);

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

            if (!IsServer)
                return;

    
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
            SpectatorController.Instance.EnterSpectatorMode();
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
            AlivePlayers.Add(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            AlivePlayers.Remove(clientId);
        }
        
    }
    public class AllPlayerDiedEvent : IEvent
    {
       
    }
}