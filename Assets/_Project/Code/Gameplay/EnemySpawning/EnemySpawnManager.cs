using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private Timer _enemySpawnDelay;
    [field: SerializeField] public EnemyPrefabsSO EnemyPrefabs { get; private set; }
    [field: SerializeField] public SpawnDataSO SpawnData { get; private set; }

    private void Awake()
    {
        _enemySpawnDelay = new Timer(SpawnData.BaseRandomTimeBetweenSpawns);

    }
    private void Start()
    {
        StartSpawn();
    }
    private void Update()
    {
        _enemySpawnDelay.TimerUpdate(Time.deltaTime);
        if (_enemySpawnDelay.IsComplete)
        {
            StartSpawn();
            _enemySpawnDelay.Reset(SpawnData.BaseRandomTimeBetweenSpawns);
        }
    }
    //currently just spawns tranquil. Fix and add random chance of each
    public void StartSpawn()
    {
        for (int i = 0; i < EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count / 2; i++)
        {
            int randomSpawnPoint = Random.Range(0, EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
            if (EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[randomSpawnPoint].CanSpawnEnemy())
            {
                RandomEnemySelection(randomSpawnPoint);
                break;
            }
        }
    }
    public void SpawnTranquil(int spawnPoint)
    {
        if (EnemyPrefabs.TranquilPrefabs.Count < 1) return;
        int enemyIndex = Random.Range(0, EnemyPrefabs.TranquilPrefabs.Count);
        EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[spawnPoint].DoSpawnEnemy(EnemyPrefabs.TranquilPrefabs[enemyIndex]);
    }
    public void SpawnViolent(int spawnPoint)
    {
        if (EnemyPrefabs.ViolentPrefabs.Count < 1) return;
        int enemyIndex = Random.Range(0, EnemyPrefabs.ViolentPrefabs.Count);
        EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[spawnPoint].DoSpawnEnemy(EnemyPrefabs.ViolentPrefabs[enemyIndex]);
    }
    public void SpawnHorror(int spawnPoint)
    {
        if (EnemyPrefabs.HorrorPrefabs.Count < 1) return;
        int enemyIndex = Random.Range(0, EnemyPrefabs.HorrorPrefabs.Count);
        EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[spawnPoint].DoSpawnEnemy(EnemyPrefabs.HorrorPrefabs[enemyIndex]);
    }
    private void RandomEnemySelection(int spawnPoint)
    {
        float totalWeight = SpawnData.TranquilRandWeight + SpawnData.ViolentRandWeight + SpawnData.HorrorWeight;

        float randomValue = Random.value * totalWeight;

        if (randomValue < SpawnData.TranquilRandWeight)
            SpawnTranquil(spawnPoint);
        else if (randomValue < SpawnData.TranquilRandWeight + SpawnData.ViolentRandWeight)
            SpawnViolent(spawnPoint);
        else
            SpawnHorror(spawnPoint);
    }
}
