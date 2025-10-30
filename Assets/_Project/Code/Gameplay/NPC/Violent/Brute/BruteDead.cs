using System.Collections.Generic;
using _Project.Code.Gameplay.Interactables;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteDead : NetworkBehaviour, IHoldToInteract
    {
        [SerializeField] Collider _interactCollider;
        private float _heldTime;
        [SerializeField] float _timeToHold;
        private bool _hasSpawned;
        bool _isInteracting;
        List<GameObject> playersInteracting = new List<GameObject>();

        [SerializeField] GameObject _brutePiecesPrefab;
        [SerializeField] GameObject _destroy;

        [SerializeField] float heightOffset;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        public void OnHold(GameObject player)
        {
            Debug.Log("PlayerInt");
            playersInteracting.Add(player);
            _isInteracting = true;
        }

        public void OnRelease(GameObject player)
        {
            playersInteracting?.Remove(player);
            if (playersInteracting.Count <= 0)
            {
                _isInteracting = false;
                _heldTime = 0;
                Debug.Log("PlayerDone");
            }
        }
        public void OnEnable()
        {
            _interactCollider.enabled = true;
        }

        /*
        public void SpawnBrutePieces()
        {
            Instantiate(_brutePiecesPrefab, transform.position + new Vector3(0, heightOffset, 0), Quaternion.identity);
        }
        */

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

         
            if (_destroy == null)
            {
                _destroy = transform.Find("SK_Brute")?.gameObject;
                Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Auto-assigned _destroy: {_destroy?.name}");
            }
        }

        void Update()
        {
            if (_isInteracting)
            {
                _heldTime += Time.deltaTime;
                Debug.Log("PlayerInting");
            }

            if (_heldTime > _timeToHold && !_hasSpawned)
            {
                _hasSpawned = true;

                if (IsServer)
                {
                    HandleBruteDeathServer();
                }
                else
                {
                    RequestBruteDeathServerRpc();
                }
               
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestBruteDeathServerRpc()
        {
            HandleBruteDeathServer();
        }
        private void HandleBruteDeathServer()
        {
            SpawnBrutePiecesServer();
            DestroyBodyServer();
        }
        private void SpawnBrutePiecesServer()
        {
            var spawnPos = transform.position + new Vector3(0, heightOffset, 0);
            var obj = Instantiate(_brutePiecesPrefab, spawnPos, Quaternion.identity);
            var netObj = obj.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.Spawn(true); 
            }
            else
            {
                Debug.LogWarning($"{_brutePiecesPrefab.name} no NetworkObject comp！");
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SpawnBrutePiecesServerRpc()
        {
            SpawnBrutePiecesServer();
        }
        private void DestroyBodyServer()
        {
            if (_destroy == null)
            {
                Debug.LogWarning("[Server] _destroy is null!");
                return;
            }
            
            Destroy(_destroy);
            DestroyBodyClientRpc();
            Debug.Log($"[Server] Notified all clients to destroy {_destroy.name}");
        }
        
        [ClientRpc]
        private void DestroyBodyClientRpc()
        {
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId}] DestroyBodyClientRpc received.");
            if (_destroy != null)
            {
                Destroy(_destroy);
                Debug.Log($"[Client] Locally destroyed {_destroy.name}");
            }
        }

    }
}