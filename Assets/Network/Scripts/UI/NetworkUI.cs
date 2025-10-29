using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Network.Scripts.UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private string _loadSceneName;
        [SerializeField] private Button _clientButton;
        private void Start()
        {
            if (_hostButton != null)
            {
                _hostButton.onClick.AddListener(OnHostClicked);
            }

            if (_clientButton != null)
            {
                _clientButton.onClick.AddListener(OnClientClicked);
            }
        }

        private void OnHostClicked()
        {
            Debug.Log("Host button clicked");

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started!");
               
                NetworkManager.Singleton.SceneManager.LoadScene(_loadSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("Failed to start host");
            }
        }

        private void OnClientClicked()
        {
            Debug.Log("Client button clicked");

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started!");
             
            }
            else
            {
                Debug.LogError("Failed to start client");
            }
        }

    }



}
