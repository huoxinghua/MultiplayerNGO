using System;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using UnityEngine;

namespace _Project.Code.UI.Menu
{
    public class PauseMenuManager : MonoBehaviour
    {
        //this code attached with player,so keep Monobehaviour
        private GameObject pauseMenuUI;
        [SerializeField] private PlayerInputManager playerInput;
        [SerializeField] private GameObject pauseMenuPrefab;
        private bool _isPaused = false;

        private void Start()
        {
            playerInput.OnPauseOpen+= TogglePauseMenu;
            playerInput.OnPauseClose+= TogglePauseMenu;
            EventBus.Instance.Subscribe<PauseMenuResumeEvent>(this,ClosePause);
        }

        private void OnDisable()
        {
            playerInput.OnPauseOpen-= TogglePauseMenu;
            playerInput.OnPauseClose-= TogglePauseMenu;
        }

        public void TogglePauseMenu()
        {
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
            var pauseMenu = pauseMenuUI.GetComponent<PauseMenu>();
            pauseMenu.DisplayPauseMenu();
            Time.timeScale = 0f;
            playerInput.SwitchToUIMode();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ClosePause()
        {
            Debug.Log("pause menu close  ");
            _isPaused = false;

            pauseMenuUI.SetActive(false);

            Time.timeScale = 1f;
            playerInput.SwitchToPlayerMode();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void ClosePause(PauseMenuResumeEvent evt)
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