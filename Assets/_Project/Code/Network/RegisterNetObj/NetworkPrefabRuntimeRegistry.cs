using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.RegisterNetObj
{
    public static class NetworkPrefabRuntimeRegistry
    {
        public static void EnsurePrefabRegistered(GameObject prefab)
        {
            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                UnityEngine.Debug.Log("[NetworkPrefabRuntimeRegistry] NetworkManager not found!");
                return;
            }

            var networkPrefabs = manager.NetworkConfig.Prefabs;


            foreach (var p in networkPrefabs.Prefabs)
            {
                if (p.Prefab == prefab)
                    return;
            }


            networkPrefabs.Add(new NetworkPrefab { Prefab = prefab });
        }
    }
}