using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace Project.Network.SteamWork
{
    public class SteamLobbyManager : MonoBehaviour
    {
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;

        private CSteamID currentLobbyId;

        void Start()
        {
            if (!SteamManager.Initialized) return;

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        }
        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("[SteamLobbyManager] GameLobbyJoinRequested: " + callback.m_steamIDLobby);

         
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }
        public void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                Debug.Log("[SteamLobbyManager] OnLobbyCreated Lobby created: " + currentLobbyId);

                SteamFriends.ActivateGameOverlay("Friends");
                SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyId);
                Debug.Log("ActivateGameOverlayInviteDialog" + currentLobbyId);
            }
            else
            {
                Debug.LogError("[SteamLobbyManager] OnLobbyCreated Failed to create lobby");
                return;
            }


            bool ok = NetworkManager.Singleton.StartHost();
            Debug.Log("StartHost() returned = " + ok);


            if (ok)
            {
                Debug.Log("Host actually started!");
                if (currentLobbyId != null && SteamManager.Initialized)
                {
                    var myId = SteamUser.GetSteamID().m_SteamID;
                    Debug.Log("start host success , my id is" + myId);
                }
                else
                {
                    Debug.Log("start host failed.");
                }
            }
            else
            {
                Debug.LogError("StartHost() FAILED");
            }
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {

            CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            Debug.Log("[SteamLobbyManager] OnLobbyEntered Joined lobby: " + lobbyId);

            CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            Debug.Log("[SteamLobbyManager] OnLobbyEntered Lobby host stream is: " + hostId);

            Debug.Log("[SteamLobbyManager] OnLobbyEntered NetworkManager.Singleton.IsListening =" + NetworkManager.Singleton.IsListening);


            var myId = SteamUser.GetSteamID();
            if (myId.m_SteamID != hostId.m_SteamID)
            {
                var transport = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

                if (lobbyId != null)
                {
                    transport.ConnectToSteamID = hostId.m_SteamID;
                    Debug.Log("[SteamLobbyManager] OnLobbyEntered Client Target Host SteamID = " + transport.ConnectToSteamID);

                    Debug.Log("[SteamLobbyManager] OnLobbyEntered my client id is :" + myId);
                }
                else
                {
                    Debug.LogError("[SteamLobbyManager] OnLobbyEntered : No Host SteamID provided!");
                    return;
                }

                NetworkManager.Singleton.StartClient();

            }
        }

        public void InviteFriends()
        {
            if (currentLobbyId.IsValid())
            {
                SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyId);
                Debug.Log("[SteamLobbyManager] InviteFriends Opened invite dialog");
            }
            else
            {
                Debug.LogWarning("[SteamLobbyManager] InviteFriends No lobby created yet!");
            }
        }
    }
}