using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Buy
{
    public class VanSpawner : NetworkBehaviour
    {
        private List<BuyOrder> buyOrders = new List<BuyOrder>();
        [SerializeField] private DeliveryVan _vanPrefab;
        public void AddBuyOrders(BuyOrder buyOrder)
        {
            if (!IsServer) return;
            buyOrders.Add(buyOrder);
        }
        public void SendVan()
        {
            if (IsServer)
            {
                DeliveryVan temp = Instantiate(_vanPrefab, transform);
                temp.GetComponent<NetworkObject>().Spawn();
                foreach(var buyOrder in buyOrders)
                {
                    temp.AddBuyOrder(buyOrder);
                }
                ClearBuyOrders();
            }
            
        }
        public void ClearBuyOrders()
        {
            if (!IsServer) return;
            buyOrders.Clear();
        }
    }
}
