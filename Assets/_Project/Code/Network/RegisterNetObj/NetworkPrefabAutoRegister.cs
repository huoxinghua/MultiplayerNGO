using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.RegisterNetObj
{
    public class NetworkPrefabAutoRegister : MonoBehaviour
    {
        [Header("enemy pickable piceces Prefabs")] [SerializeField]
        private GameObject[] _pickupPrefabs;

        private IEnumerator Start()
        {
            while (NetworkManager.Singleton == null)
            {
                yield return null;
            }

            RegisterPrefabs();
        }

        private void RegisterPrefabs()
        {
            foreach (var prefab in _pickupPrefabs)
            {
                var netObj = prefab.GetComponent<NetworkObject>();
                if (netObj == null)
                {
                    continue;
                }

                NetworkManager.Singleton.AddNetworkPrefab(prefab);
            }
        }
    }
}