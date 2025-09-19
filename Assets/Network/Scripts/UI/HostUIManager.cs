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
        [SerializeField] private GameObject publicLobbyCreate;
        private bool isOpen = false;
       
        //room List
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private Transform lobbyListContainer;
        [SerializeField] private GameObject lobbyList;
        [SerializeField] private SteamLobbyManager lobbyManager;
        [SerializeField] private GameObject img;

        void Start()
        {
            if (!SteamManager.Initialized) return;
            HidePanels();
 
        }

          private void Awake()
        {
            Instance = this;
        }
        private void HidePanels()
        {
            foreach(Transform child in panelContainer.transform)
            {
                child.gameObject.SetActive(false);
            }
            
        }
        public void ShowPublicLobbyCreate()
        {
            HidePanels();
            publicLobbyCreate.SetActive(true);
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
            HidePanels();
            lobbyList.SetActive(true);
            ShowImage();

        }

        private void OnEnable()
        {
            SteamLobbyManager.OnLobbyFound += CreateLobbyButton;
        }

        private void OnDisable()
        {
            SteamLobbyManager.OnLobbyFound -= CreateLobbyButton;
        }

        public void ClearLobbyList()
        {

            foreach (Transform child in lobbyListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void CreateLobbyButton(string lobbyName, CSteamID lobbyId)
        {
            ShowLobbyList();

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


