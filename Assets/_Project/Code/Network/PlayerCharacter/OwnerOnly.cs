using FMODUnity;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.PlayerCharacter
{
    public class OwnerOnly : NetworkBehaviour
    {
        [SerializeField] private MonoBehaviour[] _componentsToDisable;
        [SerializeField] private GameObject[] _objectsToEnable;
         private Camera _playerCamera;
        [SerializeField] private AudioListener _unityListener;
        [SerializeField] private StudioListener _fmodListener;
        [SerializeField] private Renderer _renderToDisable;
        [SerializeField] private Renderer _renderToEnable;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _playerCamera = GetComponentInChildren<Camera>();
            ApplyOwnerState(IsOwner);
          

        }

        private void ApplyOwnerState(bool isOwner)
        {
            foreach (var component in _componentsToDisable)
            {
                if (component != null)
                {
                    component.enabled = isOwner;
                }
            }

            foreach (var obj in _objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(isOwner);
                }
            }

            if (isOwner)
            {
                // Local Listener = ON
                if (_playerCamera != null) _playerCamera.enabled = true;
                if (_unityListener != null) _unityListener.enabled = true;
                if (_fmodListener != null) _fmodListener.enabled = true;
            }
            else
            {
                // Remote Listener = DESTROY (important)
                if (_playerCamera != null) _playerCamera.enabled = false;
                if (_unityListener != null) Destroy(_unityListener);
                if (_fmodListener != null) Destroy(_fmodListener);
            }
           
            _renderToEnable.enabled = isOwner;

            _renderToDisable.enabled = !isOwner;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            foreach (var obj in _objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}