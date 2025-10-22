using Unity.Netcode;
using UnityEngine;
namespace Project.Network.PlayerController
{
    public class OwnerOnly : NetworkBehaviour
    {
        [SerializeField] private MonoBehaviour[] _componentsToDisable;
        [SerializeField] private GameObject[] _objectsToEnable;
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