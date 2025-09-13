using Netcode.Transports;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace Project.Network.UI.SteamTransport
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

            /*
                        if (hostButton != null)
                        {
                            hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
                        }
                        if (serverButton != null)
                        {
                            serverButton.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
                        }
                        if (clientButton != null)
                        {
                            Debug.Log("client start");
                            clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
                        }
            */



            if (hostButton != null)
            {
                hostButton.onClick.AddListener(() =>
                {
                    Debug.Log(">>> Clicked Host");
                    Debug.Log("IsListening before = " + NetworkManager.Singleton.IsListening);

                    bool ok = NetworkManager.Singleton.StartHost();
                    Debug.Log("StartHost() returned = " + ok);

                    if (ok)
                    {
                        Debug.Log("Host actually started!");
                        if (mySteamIdText != null && SteamManager.Initialized)
                        {
                            var myId = SteamUser.GetSteamID().m_SteamID;
                            mySteamIdText.text = "My SteamID: " + myId;
                        }
                    }
                    else
                    {
                        Debug.LogError("StartHost() FAILED");
                    }
                });
            }

            if (serverButton != null)
            {
                serverButton.onClick.AddListener(() =>
                {
                    if (!NetworkManager.Singleton.IsListening)
                    {
                        NetworkManager.Singleton.StartServer();
                    }
                });
            }

            if (clientButton != null)
            {
                clientButton.onClick.AddListener(() =>
                {
                    if (!NetworkManager.Singleton.IsListening)
                    {
                        var transport = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

                        if (hostIdInput != null && !string.IsNullOrEmpty(hostIdInput.text))
                        {
                            transport.ConnectToSteamID = ulong.Parse(hostIdInput.text);
                            Debug.Log("[Client] Target Host SteamID = " + transport.ConnectToSteamID);
                            var myId = SteamUser.GetSteamID().m_SteamID;
                            Debug.Log("client id is :" + myId);
                        }
                        else
                        {
                            Debug.LogError("[Client] No Host SteamID provided!");
                            return;
                        }

                        NetworkManager.Singleton.StartClient();

                    }
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
