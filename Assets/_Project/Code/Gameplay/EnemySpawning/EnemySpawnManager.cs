using Steamworks;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private Timer _enemySpawnDelay;
    [field: SerializeField] public EnemyPrefabsSO EnemyPrefabs {get; private set; }
    [field: SerializeField] public SpawnDataSO SpawnData {get; private set; }

    private void Awake()
    {
        _enemySpawnDelay = new Timer(SpawnData.BaseRandomTimeBetweenSpawns);
        
    }
    private void Start()
    {
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
    }
    private void Update()
    {
        _enemySpawnDelay.TimerUpdate(Time.deltaTime);
        if (_enemySpawnDelay.IsComplete)
        {
            SpawnEnemy();
            _enemySpawnDelay.Reset(SpawnData.BaseRandomTimeBetweenSpawns);
        }
    }
    //currently just spawns tranquil. Fix and add random chance of each
    public void SpawnEnemy()
    {
        int randomSpawnPoint = Random.Range(0, EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
        if (EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[randomSpawnPoint].CanSpawnEnemy())
        {
            EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[randomSpawnPoint].DoSpawnEnemy(EnemyPrefabs.TranquilPrefabs[0]);
        }
    }
}
