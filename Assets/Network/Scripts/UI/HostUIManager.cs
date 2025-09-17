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
        [SerializeField] private GameObject hostOption;
        [SerializeField] private GameObject publicOption;
        private bool isOpen = false;
        private bool ispublicOpen = false;
        //room List
        [SerializeField] private GameObject lobbyItemPrefab;
        [SerializeField] private Transform lobbyListContainer;
        [SerializeField] private GameObject lobbyList;
        [SerializeField] private SteamLobbyManager lobbyManager;
        [SerializeField] private GameObject img;

        void Start()
        {
            HideHostOption();
            HidePublicOption();
            lobbyList.SetActive(false);
            if (!SteamManager.Initialized) return;
            img.SetActive(false);

        }
        private void ShowImage()
        {
            img.SetActive(true);
        }
        private void Awake()
        {
            Instance = this;
        }
        public void ShowHostOption()
        {
            if (!isOpen)
            {
                hostOption.SetActive(true);
                ShowImage();
            }
        }
        public void HideHostOption()
        {

            hostOption.SetActive(false);
            isOpen = false;
        }
        public void ShowPublicOption()
        {
            if (!ispublicOpen)
            {
                HideHostOption();
                publicOption.SetActive(true);
                ShowImage();
            }
        }
        public void HidePublicOption()
        {

            publicOption.SetActive(false);

        }
        public void ShowLobbyList()
        {

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


