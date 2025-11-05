using _Project.Code.Gameplay.NewItemSystem;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.BeetleSample;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleDead : BaseInventoryItem
    {
        [SerializeField] private Transform _beetleTransform;
        [SerializeField] private GameObject _beetleSkele;
        [SerializeField] private BeetleItemSO _beetleSO;
    
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");

            CustomNetworkSpawn();
        }
        public void Awake()
        {
            _tranquilValue = Random.Range(0f, 1f);
            _violentValue = Random.Range(0f, 1f);
            _miscValue = Random.Range(0f, 1f);
        }
        private void Update()
        {
            if (!IsOwner) return;
            UpdateHeldPosition();

        }

        protected override void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null) return;
            _currentHeldVisual.transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.rotation = CurrentHeldPosition.rotation;
            _beetleSkele.transform.position = CurrentHeldPosition.position;
            _beetleSkele.transform.rotation = CurrentHeldPosition.rotation;
        }
        public override void PickupItem(GameObject player, Transform playerHoldPosition, NetworkObject networkObject)
        {
            base.PickupItem(player, playerHoldPosition, networkObject);
        }
        public override void DropItem(Transform dropPoint)
        {
            base.DropItem(dropPoint);
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
