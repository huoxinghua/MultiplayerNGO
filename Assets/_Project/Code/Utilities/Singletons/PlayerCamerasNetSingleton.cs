using System;
using _Project.Code.Core.Patterns;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
namespace _Project.Code.Utilities.Singletons
{
    public class PlayerCamerasNetSingleton : NetworkSingleton<PlayerCamerasNetSingleton>
    {
        public NetworkList<NetworkObjectReference> PlayerCamerasNetList = new NetworkList<NetworkObjectReference>();

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddPlayerCamServerRpc(NetworkObjectReference cameraNetRef)
        {
            PlayerCamerasNetList.Add(cameraNetRef);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRemovePlayerCamServerRpc(NetworkObjectReference cameraNetRef)
        {
            PlayerCamerasNetList.Remove(cameraNetRef);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestClearCameraListServerRpc()
        {
            PlayerCamerasNetList.Clear();
        }
        [ServerRpc(RequireOwnership = false)]
        public void RequestTogglePlayerCamServerRpc(NetworkObjectReference cameraNetRef)
        {
            if (PlayerCamerasNetList.Contains(cameraNetRef))
            {
                PlayerCamerasNetList.Remove(cameraNetRef);
            }
            else
            {
                // If the reference is NOT in the list, add it (Toggle ON)
                PlayerCamerasNetList.Add(cameraNetRef);
            }
        }
        
    }
}