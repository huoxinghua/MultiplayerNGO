using _Project.Code.Network.GameManagers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Code.Network.UI
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
            if (NetworkManager.Singleton.StartHost())
            {
                GameFlowManager.Instance.ShowLoadMenu();
             GameFlowManager.Instance.LoadScene(_loadSceneName);
            }
            else
            {
                Debug.LogError("Failed to start host");
            }
        }

        private void OnClientClicked()
        {
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
