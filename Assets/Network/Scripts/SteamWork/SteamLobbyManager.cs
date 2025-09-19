using Netcode.Transports;
using Project.Network.UI;
using Steamworks;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Network.SteamWork
{
    public class SteamLobbyManager : MonoBehaviour
    {
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyMatchList_t> lobbyList;

        private CSteamID currentLobbyId;
        private ELobbyType lastLobbyType; 
        [SerializeField] private TMP_InputField roomNameInput;

        public static Action<string, CSteamID> OnLobbyFound; 
        void Start()
        {
            Debug.Log("SteamLobbyManager start");
            if (!SteamManager.Initialized) return;

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyList = Callback<LobbyMatchList_t>.Create(OnLobbyListReceived);
        }

        private void OnLobbyListReceived(LobbyMatchList_t param)
        {

            int count = (int)Mathf.Min(param.m_nLobbiesMatching, 5); 

            for (int i = 0; i < count; i++)
            {
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);

                string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");
                Debug.Log("Lobby: " + lobbyName + " ID: " + lobbyId);
                if (string.IsNullOrEmpty(lobbyName))
                    lobbyName = "Unnamed Lobby";

                OnLobbyFound?.Invoke(lobbyName, lobbyId);
              
            }
        }
        public void OnClickJoinCrewButton()
        {
            HostUIManager.Instance.ClearLobbyList(); 
            SteamMatchmaking.RequestLobbyList();    
            Debug.Log("Requesting lobby list...");
        }
      
        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("[SteamLobbyManager] GameLobbyJoinRequested: " + callback.m_steamIDLobby);

         
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }
        public void CreateFriendOnlyLobby()
        {
            Debug.Log("Creat friend only Lobby");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }
        public void CreatePrivateLobby()
        {
            Debug.Log("Creat Lobby");
            lastLobbyType = ELobbyType.k_ELobbyTypePrivate;
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 4);
        }
        public void ClickHostPublic()
        {
            if (SteamManager.Initialized)
            {
                lastLobbyType = ELobbyType.k_ELobbyTypePublic;
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
                Debug.Log("[SteamLobbyManager] Creating PUBLIC lobby...");
            }
            else
            {
                Debug.LogError("Steam is not initialized!");
            }
        }
      
        public void ClickJoinFriendButton()
        {
            if (SteamManager.Initialized)
            {
                SteamFriends.ActivateGameOverlay("Friends");
                Debug.Log("Opened Steam Friends Overlay to join a friend");
            }
        }
        public void ClickSinglePlayerButton()
        {
            //here need switch steam transport to unity transport
            //NetworkManager.Singleton.SceneManager.LoadScene("NetWorkGymP2P", LoadSceneMode.Single);
            //NetworkManager.Singleton.StartHost();
            

        }
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                Debug.Log("[SteamLobbyManager] OnLobbyCreated Lobby created: " + currentLobbyId);
                string roomName = "defaultRoom";

                if (roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text))
                    roomName = roomNameInput.text;

                if (lastLobbyType == ELobbyType.k_ELobbyTypePublic)
                {
                    SteamMatchmaking.SetLobbyData(currentLobbyId, "name", roomName);
                    Debug.Log("[SteamLobbyManager] room: " + roomName+ "ID:"+ currentLobbyId);
                    OnLobbyFound?.Invoke("roomName", currentLobbyId);
                   
                }
                if (lastLobbyType == ELobbyType.k_ELobbyTypeFriendsOnly || lastLobbyType == ELobbyType.k_ELobbyTypePrivate)
                {
                   // SteamFriends.ActivateGameOverlay("Friends");
                    SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyId);
                    Debug.Log("ActivateGameOverlayInviteDialog" + currentLobbyId);
                }
               
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
                NetworkManager.Singleton.SceneManager.LoadScene("NetWorkGymP2P", LoadSceneMode.Single);
            }

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