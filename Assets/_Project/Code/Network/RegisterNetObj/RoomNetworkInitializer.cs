using Unity.Netcode;

namespace _Project.Code.Network.RegisterNetObj
{
    public class RoomNetworkInitializer : NetworkBehaviour
    {
        public override void  OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                foreach (var netObj in GetComponentsInChildren<NetworkObject>())
                {
                    NetworkPrefabRuntimeRegistry.EnsurePrefabRegistered(netObj.gameObject);
                    if (!netObj.IsSpawned)
                        netObj.Spawn();
                }
            }
        }
    }
}
