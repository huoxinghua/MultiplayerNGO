using UnityEngine;

public class ControlsDisplay : MonoBehaviour
{
    public GameObject PauseMenu;

    void Start()
    {
        PauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PauseMenu.SetActive(false);
    }
}
