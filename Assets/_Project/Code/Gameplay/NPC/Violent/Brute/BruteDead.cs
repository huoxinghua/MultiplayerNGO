using System.Collections.Generic;
using _Project.Code.Art.RagdollScripts;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Network.ObjectManager;
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
        private ulong _parentId;
        private Vector3 _deathPosition;
        [SerializeField] private NetworkObject parentNetworkObject;

        public void OnHold(GameObject player)
        {
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
            }
        }

        public void OnEnable()
        {
            _interactCollider.enabled = true;
        }


        void Update()
        {
            if (_isInteracting)
            {
                _heldTime += Time.deltaTime;
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
            if (_destroy != null)
            {
                _deathPosition = _destroy.transform.position;
            }
            else
            {
                _deathPosition = transform.position;
            }

            SpawnBrutePiecesServer();
            DestroyBodyServer();
        }

        private void SpawnBrutePiecesServer()
        {
            var spawnPos = _deathPosition + new Vector3(0, heightOffset, 0);
            
            // Instantiate the pieces container locally to get child positions
            var tempContainer = Instantiate(_brutePiecesPrefab, spawnPos, Quaternion.identity);
            
            // Collect children first (can't modify collection while iterating)
            var children = new List<Transform>();
            foreach (Transform child in tempContainer.transform)
            {
                children.Add(child);
            }
            
            // Spawn each child piece - NetworkObject may be on child or nested inside
            foreach (var child in children)
            {
                // First check if NetworkObject is directly on the child
                var childNetObj = child.GetComponent<NetworkObject>();

                // If not found, check if it's nested inside (first child with NetworkObject)
                if (childNetObj == null)
                {
                    childNetObj = child.GetComponentInChildren<NetworkObject>();
                }

                if (childNetObj != null)
                {
                    // Detach the NetworkObject's transform before spawning
                    childNetObj.transform.SetParent(null);
                    childNetObj.transform.position = child.position;
                    childNetObj.transform.rotation = child.rotation;
                    childNetObj.Spawn(true);
                }
                else
                {
                    Debug.LogWarning($"[BruteDead] Child {child.name} has no NetworkObject (checked children too)!");
                }
            }
            
            // Destroy the empty container (it was just used for positioning)
            Destroy(tempContainer);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnBrutePiecesServerRpc()
        {
            SpawnBrutePiecesServer();
            var netObj = GetComponentInParent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn();
            }
        }

        private void DestroyBodyServer()
        {
            if (_destroy == null)
            {
                return;
            }

            var rag = GetComponentInParent<Ragdoll>();
            if (IsServer && NetworkRelay.Instance != null)
            {
                NetworkRelay.Instance.DestroyCorpseClientRpc("SK_Brute", rag.ParentId);
            }

            RequestDespawnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestDespawnServerRpc()
        {
            parentNetworkObject.Despawn(true);
        }
    }
}