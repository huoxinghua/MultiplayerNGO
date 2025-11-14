using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.Debugger
{
    public class NetworkPrefabHashDebugger : MonoBehaviour
    {
        private void Start()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null)
            {
                UnityEngine.Debug.LogWarning("No NetworkManager found!");
                return;
            }

            UnityEngine.Debug.Log($"[NetworkPrefabHashDebugger] Listing all registered NetworkPrefabs...");
            foreach (var entry in manager.NetworkConfig.Prefabs.Prefabs)
            {
                if (entry.Prefab != null && entry.Prefab.TryGetComponent<NetworkObject>(out var netObj))
                {
                    UnityEngine.Debug.Log($"Prefab: {entry.Prefab.name} | Hash: {netObj.PrefabIdHash}");
                }
            }
        }
    }
}