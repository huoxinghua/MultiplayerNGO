using _Project.Code.Art.RagdollScripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.ObjectManager
{
    public class NetworkRelay : NetworkBehaviour
    {
        public static NetworkRelay Instance;
        private string _targetName;
     
        private void Awake()
        {
            Instance = this;
        }

        [ClientRpc]
        public void DestroyCorpseClientRpc(string corpseName,ulong parentId)
        {
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Received DestroyCorpseClientRpc: corpseName={corpseName}, ParentID={parentId}");
                var ragdolls = FindObjectsOfType<Ragdoll>();
                var found = false;
                foreach (var rag in ragdolls)
                {
                    if (rag == null) continue;

                    if (rag.ParentId == parentId)
                    {
                        Object.Destroy(rag.gameObject);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogWarning("Client  not found ");
                }
        }
        
        [ClientRpc]
        public void SetInPlayerHandClientRpc(string corpseName,ulong parentId,NetworkObjectReference ownerPlayer)
        {
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Received DestroyCorpseClientRpc: corpseName={corpseName}, ParentID={parentId}");
            var ragdolls = FindObjectsOfType<Ragdoll>();
            var found = false;
            foreach (var rag in ragdolls)
            {
                if (rag == null) continue;

                if (rag.ParentId == parentId)
                {
                   // rag.gameObject.transform.SetParent(ownerPlayer);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning("Client  not found ");
            }
        }
    }
}