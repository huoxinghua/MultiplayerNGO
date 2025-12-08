using System;
using _Project.Code.Gameplay.Player.MiscPlayer;
using UnityEngine;

namespace _Project.Code.UI
{
public class PauseMenuManager : MonoBehaviour
{
    private GameObject pauseMenuUI;
    [SerializeField] private PlayerInputManager playerInput;
    [SerializeField] private GameObject pauseMenuPrefab;
    private bool _isPaused = false;

    private void Start()
    {
        playerInput.OnPauseOpen+= TogglePauseMenu;
        playerInput.OnPauseClose+= TogglePauseMenu;
    }

    private void OnDisable()
    {
        playerInput.OnPauseOpen-= TogglePauseMenu;
        playerInput.OnPauseClose-= TogglePauseMenu;
    }

    public void TogglePauseMenu()
    {
        Debug.Log("pause menu open  ");
        if (!_isPaused)
            OpenPause();
        else
            ClosePause();
    }

    private void OpenPause()
    {
        _isPaused = true;

      
        if (pauseMenuUI == null)
        {
            
            pauseMenuUI = Instantiate(pauseMenuPrefab);
        }

        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        playerInput.SwitchToUIMode();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ClosePause()
    {
        Debug.Log("pause menu close  ");
        _isPaused = false;

        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        playerInput.SwitchToPlayerMode();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
}