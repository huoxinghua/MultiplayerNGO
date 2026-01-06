using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;
namespace _Project.Code.Network.Level
{
    public class CameraFallbackManager :MonoBehaviour
    {
        
        public static CameraFallbackManager Instance;

        [SerializeField] private Camera fallbackCamera;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            EnableFallbackCamera();
        }

        public void LocalPlayerCameraReady()
        {
            DisableFallbackCamera();
        }

        public void EnableFallbackCamera()
        {
            fallbackCamera.enabled = true;
            fallbackCamera.gameObject.SetActive(true);
        }

        public void DisableFallbackCamera()
        {
            fallbackCamera.enabled = false;
            fallbackCamera.gameObject.SetActive(false);
        }
    }
}
