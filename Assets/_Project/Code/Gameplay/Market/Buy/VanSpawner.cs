
using System.Collections.Generic;
using UnityEngine;

public class VanSpawner : MonoBehaviour
{
    private List<BuyOrder> buyOrders = new List<BuyOrder>();
    [SerializeField] private DeliveryVan _vanPrefab;
    public void AddBuyOrders(BuyOrder buyOrder)
    {
        buyOrders.Add(buyOrder);
    }
    public void SendVan()
    {
        DeliveryVan temp = Instantiate(_vanPrefab, transform);
        foreach(var buyOrder in buyOrders)
        {
            temp.AddBuyOrder(buyOrder);
        }
        ClearBuyOrders();
    }
    public void ClearBuyOrders()
    {
        buyOrders.Clear();
    }
}
