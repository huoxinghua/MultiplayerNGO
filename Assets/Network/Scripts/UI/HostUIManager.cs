using Project.Network.SteamWork;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Network.UI
{
    public class HostUIManager : MonoBehaviour
    {
        public static HostUIManager Instance;
        [SerializeField] private GameObject panelContainer;

        [SerializeField] private GameObject multiplayerOption;
        //  [SerializeField] private GameObject publicLobbyObj;
        [SerializeField] private GameObject hostOption;
        // [SerializeField] private GameObject publicLobbyCreate;
        private bool isOpen = false;

        //room List
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private Transform lobbyListContainer;
        // [SerializeField] private GameObject lobbyList;
        [SerializeField] private GameObject Scrollview;
        [SerializeField] private SteamLobbyManager lobbyManager;
        [SerializeField] private GameObject img;
        private bool isPrivatLobbySelected;



        [SerializeField] private Toggle createLobbyToggle;
        void Start()
        {
            if (!SteamManager.Initialized) return;
            HidePanels();
            isPrivatLobbySelected = createLobbyToggle.isOn;
            Debug.Log("isPrivatLobbySelected:" + isPrivatLobbySelected);
            createLobbyToggle.onValueChanged.AddListener(OnPrivateLobbyChanged);

        }


        private void Awake()
        {
            Instance = this;
        }
        private void HidePanels()
        {
            foreach (Transform child in panelContainer.transform)
            {
                child.gameObject.SetActive(false);
            }

        }


        public void ShowMultiplayerOption()
        {
            multiplayerOption.SetActive(true);
        }
        public void ShowHostOption()
        {
            HidePanels();
            hostOption.SetActive(true);
        }

        private void ShowImage()
        {
            img.SetActive(true);
        }


        public void HideHostOption()
        {

            multiplayerOption.SetActive(false);
            isOpen = false;
        }


        public void ShowLobbyList()
        {
            SteamLobbyManager.OnLobbyListRequest?.Invoke();
           
            HidePanels();
            Scrollview.SetActive(true);
            ShowImage();
           
        }


        public void ClearLobbyList()
        {

            foreach (Transform child in lobbyListContainer)
            {
                Destroy(child.gameObject);
            }
        }


        public void OnPrivateLobbyChanged(bool value)
        {
            Debug.Log("On private lobby changed");
            isPrivatLobbySelected = value;

        }

        public void ClickCreateLobby()
        {
            //check is the private toggle if check in or not
            Debug.Log("isPrivatLobbySelected when click:" + isPrivatLobbySelected);
            if (isPrivatLobbySelected)
            {
                SteamLobbyManager.OnCreateFriendOnlyLobby?.Invoke();
                Debug.Log("create the friendOnly lobby");

            }
            else
            {
                SteamLobbyManager.OnCreatePublicLobby?.Invoke();
                Debug.Log("create the public lobby");

            }
        }

        public void GenerateLobbyList()
        {
            Debug.Log("GenerateLobbyList:"+ SteamLobbyManager.lobbyLists.Count);
            foreach (var kvp in SteamLobbyManager.lobbyLists)
            {
                CSteamID lobbyId = kvp.Key;       // Lobby ? ID
                string lobbyName = kvp.Value;

                GameObject newItem = Instantiate(lobbyItemPrefab, lobbyListContainer);

                TMP_Text text = newItem.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = lobbyName;

                Button joinButton = newItem.GetComponentInChildren<Button>();
                if (joinButton != null)
                {
                    joinButton.onClick.AddListener(() =>
                    {
                        Debug.Log("join Lobby: " + lobbyId);
                        SteamMatchmaking.JoinLobby(lobbyId);
                    });
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


