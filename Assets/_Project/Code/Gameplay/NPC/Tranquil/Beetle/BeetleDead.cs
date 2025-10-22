using _Project.Code.Gameplay.NewItemSystem;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.BeetleSample;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleDead : BaseInventoryItem
    {
        [SerializeField] private Transform _beetleTransform;
        [SerializeField] private GameObject _beetleSkele;
        [SerializeField] private BeetleItemSO _beetleSO;
    

        public void Awake()
        {
            _tranquilValue = Random.Range(0f, 1f);
            _violentValue = Random.Range(0f, 1f);
            _miscValue = Random.Range(0f, 1f);
        }
        private void Update()
        {
            if (_hasOwner)
            {
                _beetleSkele.transform.localPosition = Vector3.zero;
                _beetleSkele.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
        public override void PickupItem(GameObject player, Transform playerHoldPosition)
        {
            _owner = player;
            _rb.isKinematic = true;
            _renderer.enabled = false;
            _collider.enabled = false;
            _beetleSkele.transform.parent = playerHoldPosition;
            _beetleSkele.transform.localPosition = Vector3.zero;
            _beetleSkele.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);
        }
        public override void DropItem(Transform dropPoint)
        {
            _owner = null;
            _renderer.enabled = true;
            Destroy(_currentHeldVisual);
            _beetleSkele.transform.parent = null;
            _rb.isKinematic = false;
            _collider.enabled = true;
            _beetleSkele.transform.position = dropPoint.position;
        }
        public override void UseItem()
        {

        }
        public void OnEnable()
        {
            _collider.enabled = true; 
        }
    }
}
