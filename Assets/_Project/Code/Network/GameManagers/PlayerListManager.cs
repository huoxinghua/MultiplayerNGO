using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.GameManagers
{
    public class PlayerListManager : NetworkSingleton<PlayerListManager>
    {
        private readonly List<ulong> _alivePlayers = new List<ulong>();
        private readonly List<ulong> _deadPlayers = new List<ulong>();
        
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