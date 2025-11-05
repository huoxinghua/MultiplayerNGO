using Unity.Netcode;
using UnityEngine;

namespace Network.Scripts.PlayerController
{
    public class OwnerOnly : NetworkBehaviour
    {
        [SerializeField] private MonoBehaviour[] _componentsToDisable;
        [SerializeField] private GameObject[] _objectsToEnable;
        [SerializeField] private Camera _cameraToDisable;
        [SerializeField] private AudioListener _audioListenerToDisable;
        [SerializeField] private Renderer _renderToDisable;
        [SerializeField] private Renderer _renderToEnable;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
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
            
            /*    foreach (var obj in _objectsToDisable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(!isOwner);
                    }
                }*/
            _cameraToDisable.enabled = isOwner;
            _audioListenerToDisable.enabled = isOwner;
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