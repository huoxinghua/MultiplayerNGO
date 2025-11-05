using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.GameManagers
{
    public class PlayerListManager : NetworkBehaviour
    {
        public static PlayerListManager Instance { get; private set; }

        private readonly List<ulong> _alivePlayers = new List<ulong>();
        private readonly List<ulong> _deadPlayers = new List<ulong>();

        private void Awake()
        {
            Instance = this;
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
            Debug.Log("alive in playerlisht:"+_alivePlayers.Count);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _alivePlayers.Remove(clientId);
            _deadPlayers.Remove(clientId);
        }

        /*
        [ServerRpc(RequireOwnership = false)]
        public void ReportDeathServerRpc(ulong playerId)
        {
            if (!_alivePlayers.Contains(playerId)) return;
            _alivePlayers.Remove(playerId);
            _deadPlayers.Add(playerId);

            UpdateAliveListClientRpc(_alivePlayers.ToArray());
        }
        */

        [ClientRpc]
        private void UpdateAliveListClientRpc(ulong[] newList)
        {
            _alivePlayers.Clear();
            _alivePlayers.AddRange(newList);
        }

        public List<ulong> GetAlivePlayers() => _alivePlayers;
    }
}