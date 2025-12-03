using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteChopped : NetworkBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Awake()
        {
            /*foreach(Collider children in gameObject.GetComponentsInChildren<Collider>())
            {
                children.transform.parent = null;
            }*/
            if(IsServer)
            StartCoroutine(WaitForNoChildren());
        }

        private IEnumerator WaitForNoChildren()
        {
            yield return new WaitUntil(()=>gameObject.transform.childCount == 0);
            
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestDespawnServerRpc()
        {
            NetworkObject.Despawn();

        }
    }
}
