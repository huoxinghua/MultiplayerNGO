
using _Project.Code.Gameplay.NewItemSystem;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteHeartInteractable : BaseInventoryItem
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private Collider _secondCollider;
        public void Awake()
        {
            _tranquilValue = Random.Range(0f, 1f);
            _violentValue = Random.Range(0f, 1f);
            _miscValue = Random.Range(0f, 1f);
        }
        public override void PickupItem(GameObject player, Transform playerHoldPosition)
        {
            _owner = player;
            _rb.isKinematic = true;
            _renderer.enabled = false;
            _collider.enabled = false;
            _secondCollider.enabled = false;
            transform.parent.parent = playerHoldPosition;
            transform.parent.localPosition = Vector3.zero;
            transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
            _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);

        }
        public override void DropItem(Transform dropPoint)
        {
            _owner = null;

            _renderer.enabled = true;

            Destroy(_currentHeldVisual);
            transform.parent.parent = null;

            _rb.isKinematic = false;
            _collider.enabled = true;
            _secondCollider.enabled = true;
            transform.parent.position = dropPoint.position;
        }
        public override void WasSold()
        {
            Destroy(_currentHeldVisual);
            Destroy(transform.parent.gameObject);
        }
        // Update is called once per frame
        void Update()
        {
            if (_currentHeldVisual == null) return;
            if (_hasOwner)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
   
        }
    }
}
