using Netcode.Transports;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Network.UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button serverButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private TMP_InputField hostIdInput;
        [SerializeField] private TMP_Text mySteamIdText;
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
                Debug.Log($"[Host] Client connected: {id}");
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
            {
                Debug.Log($"[Host] Client disconnected: {id}");
            };
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
