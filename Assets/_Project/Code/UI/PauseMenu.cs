using System;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using UnityEngine;

namespace _Project.Code.UI
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject ControlsMenu;
        public GameObject ResumeButton;
        public GameObject ControlsButton;
        public GameObject QuitButton;
        public GameObject PauseTitle;


        public void Start()
        {
            DisplayPauseMenu();
        }

        public void DisplayPauseMenu()
        {
            PauseTitle.SetActive(true);
            ControlsMenu.SetActive(false);
            ResumeButton.SetActive(true);
            ControlsButton.SetActive(true);
            QuitButton.SetActive(true);
        }

        public void DisplayControlsMenu()
        {
            PauseTitle.SetActive(false);
            ControlsMenu.SetActive(true);
            ResumeButton.SetActive(false);
            ControlsButton.SetActive(false);
            QuitButton.SetActive(false);
        }

        public void OnClickResume()
        {
            PauseTitle.SetActive(false);
            ControlsMenu.SetActive(false);
            ResumeButton.SetActive(false);
            ControlsButton.SetActive(false);
            QuitButton.SetActive(false);
            EventBus.Instance.Publish(new PauseMenuResumeEvent{});
        }
        public void OnClickQuitGame()
        {
            if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsListening)
            {
                Unity.Netcode.NetworkManager.Singleton.Shutdown();
            }

            Application.Quit();


#if UNITY_EDITOR

            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

    }

    public struct PauseMenuResumeEvent : IEvent
    {
        
    }
        
}