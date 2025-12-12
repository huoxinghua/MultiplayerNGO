using System.Collections;
using _Project.Code.Network.GameManagers;
using _Project.Code.Network.Level;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;


namespace _Project.Code.Gameplay.Player.MiscPlayer
{
    public class PlayerCameraActivator : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;
        public bool IsPlayerCamReady { get; private set; } = false;

        public override void OnNetworkSpawn()
        {
            Debug.Log("PlayerCameraActivator spawn" + IsOwner);
            if (!IsOwner) return;

            playerCamera.enabled = IsOwner;
          
            //StartCoroutine(NotifyCameraReadyNextFrame());
            

        }
        private IEnumerator NotifyCameraReadyNextFrame()
        {
            yield return null; 
            CameraFallbackManager.Instance.LocalPlayerCameraReady();
            IsPlayerCamReady = true;
        }
    }

  
}
