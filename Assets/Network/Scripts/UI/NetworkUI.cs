using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Network.Scripts.UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button _levelLoadButton;
        [SerializeField] private Button clientButton;
        void Start()
        {
            if (hostButton != null)
            {
                hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
            }
        
            if (clientButton != null)
            {
                clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
            }
       
        }
        public void LoadGeneratLevel()
        {
            if (_levelLoadButton != null)
            {
                _levelLoadButton.onClick.AddListener(() => NetworkManager.Singleton.SceneManager.LoadScene("SecondShowcase", LoadSceneMode.Single));
            }
        }
    }



}
