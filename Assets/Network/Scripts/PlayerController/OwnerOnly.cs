using Unity.Netcode;
using UnityEngine;
namespace Project.Network.PlayerController
{
    public class OwnerOnly : NetworkBehaviour
    {
        [SerializeField] private MonoBehaviour[] componentsToDisable;
        [SerializeField] private GameObject[] objectsToEnable;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                ApplyOwnerState(false);
            }
            else
            {
                ApplyOwnerState(true);
            }
        }
        private void ApplyOwnerState(bool isOwner)
        {
            foreach (var component in componentsToDisable)
            {
                if (component != null)
                {
                    component.enabled = isOwner;
                }
            }

            foreach (var obj in objectsToEnable)
                if (obj != null) obj.SetActive(isOwner);
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

        }
    }
}