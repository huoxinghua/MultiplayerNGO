using System.Collections;
using _Project.Code.Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.TestRPC
{
    public class SpawnOnTrigger : NetworkBehaviour
    {
         [SerializeField] private GameObject prefabToSpawn;
        private bool hasSpawned = false;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<PlayerHealth>()) return;
            if(IsOwner)
            {
                RequestSpawnServerRpc();
            }
        }
        [ServerRpc]
        private void RequestSpawnServerRpc(ServerRpcParams rpcParams = default)
        {
            if(hasSpawned)return;
            // who request
            ulong senderId = rpcParams.Receive.SenderClientId;
            GameObject spawned = Instantiate(prefabToSpawn,transform.position + Vector3.up * 2, Quaternion.identity);
            //sync to all clients
            var netObj = spawned.GetComponent<NetworkObject>();
            netObj.Spawn();
            hasSpawned = true;
        }

        private IEnumerator DespawnAfterSeconds(NetworkObject netObj, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(); 
            }
        }
    }
}
