using System.Collections;
using System.Collections.Generic;
using _Project.ScriptableObjects.ScriptObjects.StoreSO.VanSO;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class DeliveryVan : NetworkBehaviour
    {
        [SerializeField] private VanSO _vanSO;
        [SerializeField] private float _vanSpeed;
        public List<BuyOrder> BuyOrders = new List<BuyOrder>();
        private List<GameObject> _itemsToSpawn = new List<GameObject>();

        [SerializeField] private Transform _spawnerPos;
        private Vector3 _vanSpawnedAtPos;
        private Vector3 _startPos;
        private float _distanceTraveled => Vector3.Distance(_startPos,transform.position);

        private float _distanceBetweenSpawns => _vanSO.DistanceForDroppingItems/_itemsToSpawn.Count;

        private int _itemsDropped = 0;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            _vanSpawnedAtPos = transform.position;
            transform.position += _vanSO.VanSpawnPoint;
            _startPos = transform.position;
        }
        public void AddBuyOrder(BuyOrder buyOrder)
        {
            BuyOrders.Add(buyOrder);
            int itemsInOrder = buyOrder.Amount;
            for (int i = 0; i < itemsInOrder; i++)
            {
                _itemsToSpawn.Add(buyOrder.ItemPrefab);
            }
        }

        public void DestroyAfterDistanceReached()
        {
            VanBegoneServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void VanBegoneServerRpc()
        {
            NetworkObject.Despawn();
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            if (!IsServer) return;
            if (_distanceTraveled >= _vanSO.DistanceToDestination - 5)
            {
                DestroyAfterDistanceReached();
            }
            transform.position = Vector3.MoveTowards(
                transform.position,
                _vanSpawnedAtPos + _vanSO.VanDestination,
                Time.deltaTime * _vanSO.VanSpeed);

            if (_itemsDropped >= _itemsToSpawn.Count) return;
            if (_distanceTraveled >= _vanSO.DistanceFromStartToDropItems && _distanceTraveled <= _vanSO.DistanceFromStartToStopDropping)
            {
                if (_distanceTraveled - _vanSO.DistanceFromStartToDropItems >= _distanceBetweenSpawns * _itemsDropped)
                {
                   DropItem();
                }
            }
        }

        private void DropItem()
        { 
            GameObject spawnedInstance = Instantiate(_itemsToSpawn[_itemsDropped]);
            spawnedInstance.transform.position = _spawnerPos.position;
            NetworkObject networkObject = spawnedInstance.GetComponent<NetworkObject>();
            networkObject.Spawn();
            _itemsDropped++;
        }
    }
}
