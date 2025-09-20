using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    private void Start()
    {
      
        SceneManager.LoadScene("NetWorkMainMenu");
    }
}
