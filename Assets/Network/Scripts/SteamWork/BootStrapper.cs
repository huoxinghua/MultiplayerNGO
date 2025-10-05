using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Network.SteamWork
{
    public class Bootstrapper : MonoBehaviour
    {
        private void Start()
        {

            SceneManager.LoadScene("NetWorkMainMenu");
        }
    }
}

