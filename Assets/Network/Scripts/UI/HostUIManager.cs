using _Project.Code.Core.Patterns;
using Network.Scripts.SteamWork;
using Steamworks;
using Steamworks.NET;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Scripts.UI
{
    public class HostUIManager : Singleton<HostUIManager>
    {

        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private GameObject _multiplayerOption;
        [SerializeField] private GameObject _hostOption;
        private bool _isOpenHostOption = false;
        //room List
        [SerializeField] private GameObject _lobbyItemPrefab;
        [SerializeField] private Transform _lobbyListContainer;
        [SerializeField] private GameObject _scrollview;
        private bool isPrivatLobbySelected;
        [SerializeField] private Toggle _createLobbyToggle;
        //reference
        [SerializeField] private SteamLobbyManager _lobbyManager;
        //visual
        [SerializeField] private GameObject _filterImg;
        //host
        [SerializeField] private TMP_InputField _roomNameInput;
        public string RoomName { get; private set; }
        void Start()
        {
            if (!SteamManager.Initialized) return;
            //give the server name with a deafault name
            var servar = SteamFriends.GetPersonaName().ToString();
            _roomNameInput.text = servar + "'s Game ";

            HidePanels();
            isPrivatLobbySelected = _createLobbyToggle.isOn;
            _createLobbyToggle.onValueChanged.AddListener(OnPrivateLobbyChanged);
        }
        
        private void HidePanels()
        {
            foreach (Transform child in _panelContainer.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        public void ShowMultiplayerOption()
        {
            _multiplayerOption.SetActive(true);
        }

        public void ShowHostOption()
        {
            HidePanels();
            _hostOption.SetActive(true);
        }

        private void ShowImage()
        {
            _filterImg.SetActive(true);
        }

        public void HideHostOption()
        {
            _multiplayerOption.SetActive(false);
            _isOpenHostOption = false;
        }

        public void ShowLobbyList()
        {
            SteamLobbyManager.RaiseLobbyListRequest();

            HidePanels();
            _scrollview.SetActive(true);
            ShowImage();

        }

        public void ClearLobbyList()
        {
            foreach (Transform child in _lobbyListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void OnPrivateLobbyChanged(bool value)
        {
            isPrivatLobbySelected = value;
        }

        public void ClickCreateLobby()
        {
            DisplayInputServerName();
            //check is the private toggle if check in or not
            if (isPrivatLobbySelected)
            {
                SteamLobbyManager.RaiseCreateFriendOnlyLobby();
            }
            else
            {
                SteamLobbyManager.RaiseCreatePublicLobby();
            }
        }

        public void DisplayInputServerName()
        {
            if (_roomNameInput != null && !string.IsNullOrEmpty(_roomNameInput.text))
            {
                RoomName = _roomNameInput.text;
            }
        }

        public void GenerateLobbyList()
        {
            foreach (var kvp in SteamLobbyManager.LobbyLists)
            {
                // KeyValuePair:kvp
                CSteamID lobbyId = kvp.Key; 
                string lobbyName = kvp.Value;

                GameObject newItem = Instantiate(_lobbyItemPrefab, _lobbyListContainer);
                var lobbyItem = newItem.GetComponent<LobbyItemUI>();
                if (SteamLobbyManager.Instance != null)
                {

                    lobbyItem.SetData(lobbyName, -1, lobbyId);
                }

                Button joinButton = newItem.GetComponentInChildren<Button>();
                if (joinButton != null)
                {
                    joinButton.onClick.AddListener(() =>
                    {
                        SteamMatchmaking.JoinLobby(lobbyId);
                    });
                }
            }
        }

        public void UpdateLobbyPing(CSteamID lobbyId, int ping)
        {
            foreach (Transform child in _lobbyListContainer)
            {
                var item = child.GetComponent<LobbyItemUI>();
                if (item != null && item.LobbyId == lobbyId)
                {
                    item.SetPing(ping);
                    break;
                }
            }
        }

        public void QuitGame()
        {
            Debug.Log("Quit button pressed. Exiting game...");
            if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsListening)
            {
                Unity.Netcode.NetworkManager.Singleton.Shutdown();
            }

            Application.Quit();



#if UNITY_EDITOR

            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


    }

}


