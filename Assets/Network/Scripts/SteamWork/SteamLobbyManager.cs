using Netcode.Transports;
using Project.Network.UI;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Network.SteamWork
{
    public class SteamLobbyManager : MonoBehaviour
    {
        public static SteamLobbyManager Instance { get; private set; }
        protected Callback<LobbyCreated_t> _lobbyCreated;
        protected Callback<LobbyEnter_t> _lobbyEntered;
        protected Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;
        protected Callback<LobbyMatchList_t> _lobbyList;
        protected Callback<LobbyDataUpdate_t> _lobbyDataUpdate;

        public static event Action OnCreateFriendOnlyLobby;
        public static event Action OnCreatePublicLobby;
        public static event Action OnGetLobbyList;
        public static event Action OnLobbyListRequest;
        public static event Action<string, CSteamID> OnLobbyFound;

        private static CSteamID _currentLobbyId;
        private static ELobbyType _lastLobbyType;

        public static Dictionary<CSteamID, string> LobbyLists = new Dictionary<CSteamID, string>();
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            OnCreateFriendOnlyLobby += CreateFriendOnlyLobby;
            OnCreatePublicLobby += ClickHostPublic;
            OnLobbyListRequest += GetLobbyList;
        }

        private void OnDisable()
        {
            OnCreateFriendOnlyLobby -= CreateFriendOnlyLobby;
            OnCreatePublicLobby -= ClickHostPublic;
            OnLobbyListRequest -= GetLobbyList;
        }

        void Start()
        {
            if (!SteamManager.Initialized) return;

            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            _lobbyList = Callback<LobbyMatchList_t>.Create(OnLobbyListReceived);
            _lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        }

        public static void RaiseCreateFriendOnlyLobby()
        {
            OnCreateFriendOnlyLobby?.Invoke();
        }

        public static void RaiseLobbyListRequest()
        {
            OnLobbyListRequest?.Invoke();
        }
        public static void RaiseCreatePublicLobby()
        {
            OnCreatePublicLobby?.Invoke();
        }

        private void OnLobbyDataUpdate(LobbyDataUpdate_t param)
        {
            CSteamID lobbyId = new CSteamID(param.m_ulSteamIDLobby);

            string hostLocStr = SteamMatchmaking.GetLobbyData(lobbyId, "host_location");

            if (!string.IsNullOrEmpty(hostLocStr))
            {
                SteamNetworkPingLocation_t hostLoc;
                if (SteamNetworkingUtils.ParsePingLocationString(hostLocStr, out hostLoc))
                {
                    StartCoroutine(TryPingHost(lobbyId, hostLoc));
                }
            }
        }

        private IEnumerator TryPingHost(CSteamID lobbyId, SteamNetworkPingLocation_t hostLoc)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                yield break;//if just the host 
            }
            for (int i = 0; i < 5; i++)
            {
                int ping = SteamNetworkingUtils.EstimatePingTimeFromLocalHost(ref hostLoc);
                if (ping >= 0 && HostUIManager.Instance != null)
                {
                    //Debug.Log($"[Client] Ping to host: {ping} ms");
                    HostUIManager.Instance.UpdateLobbyPing(lobbyId, ping);

                    yield break;
                }

              //  Debug.Log($"[Client] Ping still -1, retry {i + 1}...");
                yield return new WaitForSeconds(2f);
            }

          //  Debug.LogWarning("[Client] Failed to get valid ping after retries.");
        }
        public void OnLobbyListReceived(LobbyMatchList_t param)
        {
            LobbyLists.Clear();
            int count = (int)param.m_nLobbiesMatching;
            for (int i = 0; i < count; i++)
            {

                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                SteamMatchmaking.RequestLobbyData(lobbyId);
                string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");

                if (string.IsNullOrEmpty(lobbyName))
                {
                    // the lobbyName default: "steam name 's name"; set in host UI Manager start function
                    continue;
                }

                if (!LobbyLists.ContainsKey(lobbyId))
                {
                    LobbyLists.Add(lobbyId, lobbyName);
                }

                else
                {
                    LobbyLists[lobbyId] = lobbyName;
                }
            }
            HostUIManager.Instance.GenerateLobbyList();
        }
        private void GetLobbyList()
        {
            // SteamMatchmaking.AddRequestLobbyListStringFilter("tag", "XHTest", ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.RequestLobbyList();
            //Debug.Log("GetLobbyList ...");
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        public void CreateFriendOnlyLobby()
        {

            Debug.Log("Creat friend only Lobby");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
            _lastLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        }
        public void CreatePrivateLobby()
        {
            Debug.Log("Creat private Lobby");

            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 4);
        }
        public void ClickHostPublic()
        {
            if (SteamManager.Initialized)
            {

                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
                _lastLobbyType = ELobbyType.k_ELobbyTypePublic;
                //Debug.Log("[SteamLobbyManager] Creating PUBLIC lobby...");
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

        public static string LastCreatedLobbyName;
        public static CSteamID LastCreatedLobbyId;
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                //Debug.Log("[SteamLobbyManager] OnLobbyCreated Lobby created: " + _currentLobbyId);
                string roomName = HostUIManager.Instance.RoomName;
              

                if (_lastLobbyType == ELobbyType.k_ELobbyTypePublic)
                {
                    SteamMatchmaking.SetLobbyData(_currentLobbyId, "name", roomName);
                    SteamMatchmaking.SetLobbyData(_currentLobbyId, "tag", "XHTest");
                    //Debug.Log("[SteamLobbyManager] room when create: " + roomName + "ID:" + _currentLobbyId);

                    LastCreatedLobbyName = roomName;
                    LastCreatedLobbyId = _currentLobbyId;
                    GetLobbyList();

                    SteamNetworkPingLocation_t myLoc;
                    SteamNetworkingUtils.GetLocalPingLocation(out myLoc);

                    string locStr;
                    SteamNetworkingUtils.ConvertPingLocationToString(ref myLoc, out locStr, 256);
                    SteamMatchmaking.SetLobbyData(_currentLobbyId, "host_location", locStr);

                    // Debug.Log($"Lobby created! Host ping location: {locStr}");
                    StartCoroutine(UpdateHostLocation());
                }
                if (_lastLobbyType == ELobbyType.k_ELobbyTypeFriendsOnly)
                {

                    SteamMatchmaking.SetLobbyData(_currentLobbyId, "name", roomName);
                    SteamMatchmaking.SetLobbyData(_currentLobbyId, "tag", "XHTest");
                    SteamFriends.ActivateGameOverlayInviteDialog(_currentLobbyId);
                    Debug.Log("ActivateGameOverlayInviteDialog" + _currentLobbyId);
                    SteamFriends.ActivateGameOverlay("Friends");
                }

            }
            else
            {
                Debug.LogError("[SteamLobbyManager] OnLobbyCreated Failed to create lobby");
                return;
            }

            Invoke("LoadGamePlayScene", 2f);
        }
        public void LoadGamePlayScene()
        {
            // Debug.Log("LoadGamePlayScene");
            bool ok = NetworkManager.Singleton.StartHost();
            if (NetworkManager.Singleton != null)
            {
                //NetworkManager.Singleton.SceneManager.LoadScene("LevelGenerationSync", LoadSceneMode.Single);
                NetworkManager.Singleton.SceneManager.LoadScene("NetWorkLobby", LoadSceneMode.Single);
                //NetworkManager.Singleton.SceneManager.LoadScene("NWLevelTest", LoadSceneMode.Single);
                
            }

            if (ok)
            {
                //Debug.Log("Host actually started!");
                if (_currentLobbyId != null && SteamManager.Initialized)
                {
                    var myId = SteamUser.GetSteamID().m_SteamID;
                    //Debug.Log("start host success , my id is" + myId);
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

        private IEnumerator UpdateHostLocation()
        {
            while (true)
            {
                if (_currentLobbyId.IsValid())
                {
                    SteamNetworkPingLocation_t myLoc;
                    SteamNetworkingUtils.GetLocalPingLocation(out myLoc);

                    string locStr;
                    SteamNetworkingUtils.ConvertPingLocationToString(ref myLoc, out locStr, 256);
                    SteamMatchmaking.SetLobbyData(_currentLobbyId, "host_location", locStr);

                    // Debug.Log("[Host] Updated host_location: " + locStr);
                }
                yield return new WaitForSeconds(10f); // update once per 10s
            }
        }
        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            var myId = SteamUser.GetSteamID();
            if (myId.m_SteamID != hostId.m_SteamID)
            {
                var transport = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

                if (lobbyId != null)
                {
                    transport.ConnectToSteamID = hostId.m_SteamID;
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
            if (_currentLobbyId.IsValid())
            {
                SteamFriends.ActivateGameOverlayInviteDialog(_currentLobbyId);
                Debug.Log("[SteamLobbyManager] InviteFriends Opened invite dialog");
            }
            else
            {
                Debug.LogWarning("[SteamLobbyManager] InviteFriends No lobby created yet!");
            }
        }
    }
}