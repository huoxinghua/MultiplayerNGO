using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Code.Network.GameManagers
{
    public class NetworkSessionReset : MonoBehaviour
    {
        /*
        public void ReturnToMainMenu()
        {
            if (NetworkManager.Singleton != null)
            {
                if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost ||
                    NetworkManager.Singleton.IsClient)
                {
                    Debug.Log("[NetworkSessionReset] Shutting down network session...");
                    NetworkManager.Singleton.Shutdown();
                }
            }
            DestroyDuplicateManagers();
            SceneManager.LoadScene("NetWorkMainMenu");
        }
        */

        /*private void DestroyDuplicateManagers()
        {
            var existingManagers = GameObject.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var m in existingManagers)
            {
                if (m.name.Contains("SteamManager") || m.name.Contains("AudioManager"))
                {
                    Destroy(m.gameObject);
                }
            }
        }*/
    }
}