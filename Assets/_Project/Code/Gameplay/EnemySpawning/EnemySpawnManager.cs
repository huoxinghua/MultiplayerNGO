using _Project.Code.Core.Patterns;
using _Project.Code.Utilities.Singletons;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using _Project.Code.Gameplay.EnemySpawning;
namespace _Project.Code.Gameplay.EnemySpawning
{
    public class EnemySpawnManager : NetworkSingleton<EnemySpawnManager>
    {
        private Timer _enemySpawnDelay;
        [field: SerializeField] public EnemyPrefabsSO EnemyPrefabs { get; private set; }
        [field: SerializeField] public SpawnDataSO SpawnData { get; private set; }
        private void Start()
        {
            if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
                return;

            Debug.Log("[EnemySpawnManager] Listing registered network prefabs...");

            foreach (var prefab in Unity.Netcode.NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
            {
                if (prefab.Prefab != null)
                    Debug.Log($"[PrefabList] {prefab.Prefab.name} => Hash: {prefab.SourcePrefabGlobalObjectIdHash}");
                else
                    Debug.LogWarning("[PrefabList] Null prefab entry detected!");
            }
        }
        public  override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            _enemySpawnDelay = new Timer(SpawnData.BaseRandomTimeBetweenSpawns);
            _enemySpawnDelay.Start();
            
            int randomSpawnPoint = Random.Range(0, EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
            SpawnViolent(randomSpawnPoint);
            randomSpawnPoint = Random.Range(0, EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
            SpawnTranquil(randomSpawnPoint);
        }
        
        private void Update()
        {
            if (!IsServer) return;
            if (_enemySpawnDelay == null) return; 
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
            Debug.Log("[enemySpawnManager]StartSpawn called count："+ EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
            
            for (int i = 0; i < EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count / 2; i++)
            {
                int randomSpawnPoint = Random.Range(0, EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
                Debug.Log("[enemySpawnManager]StartSpawn canspawn："+ EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[randomSpawnPoint].CanSpawnEnemy());
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
            EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[spawnPoint]
                .DoSpawnEnemy(EnemyPrefabs.TranquilPrefabs[enemyIndex]);
        }

        public void SpawnViolent(int spawnPoint)
        {
            if (EnemyPrefabs.ViolentPrefabs.Count < 1) return;
            int enemyIndex = Random.Range(0, EnemyPrefabs.ViolentPrefabs.Count);
            EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[spawnPoint]
                .DoSpawnEnemy(EnemyPrefabs.ViolentPrefabs[enemyIndex]);
        }

        public void SpawnHorror(int spawnPoint)
        {
            if (EnemyPrefabs.HorrorPrefabs.Count < 1) return;
            int enemyIndex = Random.Range(0, EnemyPrefabs.HorrorPrefabs.Count);
            EnemySpawnPoints.Instance.ActiveEnemySpawnPoints[spawnPoint]
                .DoSpawnEnemy(EnemyPrefabs.HorrorPrefabs[enemyIndex]);
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
}