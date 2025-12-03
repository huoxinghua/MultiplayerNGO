using _Project.Code.Art.RagdollScripts;
using _Project.Code.Core.Patterns;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.ObjectManager
{
    public class NetworkRelay : NetworkSingleton<NetworkRelay>
    {
        private string _targetName;
        [ClientRpc]
        public void DestroyCorpseClientRpc(string corpseName,ulong parentId)
        {
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
        
    }
}