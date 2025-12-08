using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject ControlsMenu;
    public GameObject ResumeButton;
    public GameObject ControlsButton;
    public GameObject QuitButton;

    public void Start()
    {
        ControlsMenu.SetActive(false);
        ResumeButton.SetActive(true);
        ControlsButton.SetActive(true);
        QuitButton.SetActive(true);
    }

    public void DisplayPauseMenu()
    {
        ControlsMenu.SetActive(false);
        ResumeButton.SetActive(true);
        ControlsButton.SetActive(true);
        QuitButton.SetActive(true);
    }

    public void DisplayControlsMenu()
    {
        ControlsMenu.SetActive(true);
        ResumeButton.SetActive(false);
        ControlsButton.SetActive(false);
        QuitButton.SetActive(false);
    }

    public void ClosePauseMenu()
    {
        ControlsMenu.SetActive(false);
        ResumeButton.SetActive(false);
        ControlsButton.SetActive(false);
        QuitButton.SetActive(false);
    }

}
