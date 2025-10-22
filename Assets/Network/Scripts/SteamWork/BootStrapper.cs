using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network.Scripts.SteamWork
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Start()
        {

            SceneManager.LoadScene("NetWorkMainMenu");
        }
    }
}

