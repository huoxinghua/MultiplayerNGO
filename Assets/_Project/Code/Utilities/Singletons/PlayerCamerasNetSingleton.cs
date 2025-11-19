using _Project.Code.Core.Patterns;
using Unity.Netcode;

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
        public void RequestClearCameraList()
        {
            PlayerCamerasNetList.Clear();
        }
    }
}