using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Scripts.UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button serverButton;
        [SerializeField] private Button clientButton;
        void Start()
        {
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
                clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
            }
        }
    }



}
