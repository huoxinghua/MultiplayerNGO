using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryVan : MonoBehaviour
{
    [SerializeField] private float _vanSpeed;
    [SerializeField] private float _timeBeforeSpawning;
    public List<BuyOrder> BuyOrders = new List<BuyOrder>();
    private List<GameObject> _itemsToSpawn = new List<GameObject>();
    [SerializeField] private float _timeFrameForSpawning;
    [SerializeField] private float _timeAfterSpawnDestroy;
    [SerializeField] private Transform _spawnPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LifeCycle());
    }
    public void AddBuyOrder(BuyOrder buyOrder)
    {
        BuyOrders.Add(buyOrder);
    }
    IEnumerator LifeCycle()
    {
        yield return new WaitForSeconds(_timeBeforeSpawning);
        StartSpawning();
        yield return new WaitForSeconds(_timeFrameForSpawning);
        yield return new WaitForSeconds(_timeAfterSpawnDestroy);
        DestroyAfterSeconds();
    }
    public void StartSpawning()
    {
        float timeBetweenSpawns = 0;
        int totalItemsToSpawn = 0;
        foreach(BuyOrder buyOrder in BuyOrders)
        {
            totalItemsToSpawn += buyOrder.Amount;
            for(int i = 0; i < buyOrder.Amount; i++)
            {
                _itemsToSpawn.Add(buyOrder.ItemPrefab);
            }
        }
        timeBetweenSpawns = _timeFrameForSpawning / totalItemsToSpawn;
        StartCoroutine(SpawnDelay(timeBetweenSpawns));
    }
    public void DestroyAfterSeconds()
    {
        Destroy(gameObject);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += Vector3.left * _vanSpeed;
    }
    IEnumerator SpawnDelay(float timeBetweenSpawns)
    {
        while (_itemsToSpawn.Count > 0)
        {
            Debug.Log("Spawning");
            GameObject temp = Instantiate(_itemsToSpawn[0], _spawnPos.position, Quaternion.identity);
            temp.transform.parent = null;
            _itemsToSpawn.RemoveAt(0);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}
