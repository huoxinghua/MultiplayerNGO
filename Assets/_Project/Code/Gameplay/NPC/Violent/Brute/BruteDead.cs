using System.Collections.Generic;
using _Project.Code.Gameplay.Interactables;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteDead : MonoBehaviour,IHoldToInteract
    {
        [SerializeField] Collider _interactCollider;
        private float _heldTime;
        [SerializeField] float _timeToHold;
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
            if(playersInteracting.Count <= 0)
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
        public void SpawnBrutePieces()
        {
            Instantiate(_brutePiecesPrefab,transform.position + new Vector3(0,heightOffset,0),Quaternion.identity);
        }
        // Update is called once per frame
        void Update()
        {
            if (_isInteracting)
            {
                _heldTime += Time.deltaTime;
                Debug.Log("PlayerInting");
            }
        
            if(_heldTime > _timeToHold)
            {
                SpawnBrutePieces();
                Destroy(_destroy);
            }
        }
    }
}
