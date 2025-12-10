using System.Collections;
using _Project.Code.Gameplay.NPC.Violent.Brute;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.RegisterNetObj
{
    public class NetworkPrefabAutoRegister : MonoBehaviour
    {
        [Header("enemy pickable piceces Prefabs")] 
        [SerializeField] private BruteSO _bruteSo;

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
            foreach (var prefab in _bruteSo.BrutePiecePrefabs)
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