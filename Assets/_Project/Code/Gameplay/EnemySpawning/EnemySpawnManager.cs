using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Utilities.Singletons;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using _Project.Code.Gameplay.EnemySpawning;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.Code.Network.RegisterNetObj;
using Unity.Netcode;

namespace _Project.Code.Gameplay.EnemySpawning
{
    public class EnemySpawnManager : NetworkSingleton<EnemySpawnManager>
    {
        private Timer _enemySpawnDelay;
        [field: SerializeField] public EnemyPrefabsSO EnemyPrefabs { get; private set; }
        [field: SerializeField] public SpawnDataSO SpawnData { get; private set; }
        private readonly HashSet<NetworkObject> _aliveEnemies = new();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _enemySpawnDelay = new Timer(SpawnData.BaseRandomTimeBetweenSpawns);
            _enemySpawnDelay.Start();
            var points = EnemySpawnPoints.Instance.ActiveEnemySpawnPoints;
            if (points.Count == 0)
            {
                Debug.LogWarning("[EnemySpawnManager] OnNetworkSpawn: no spawn points");
                return;
            }

            int idx = Random.Range(0, points.Count);
            SpawnViolent(points[idx]);

            idx = Random.Range(0, points.Count);
            SpawnTranquil(points[idx]);
        }

        private bool IsPointSafe(EnemySpawnPoint sp)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObj = client.PlayerObject;
                if (playerObj == null) continue;

                var pNet = playerObj.GetComponent<PlayerNetworkPos>();
                if (pNet == null) continue;

                float distance = Vector3.Distance(sp.transform.position, pNet.ServerPosition.Value);

                if (distance <= sp.SafeDistance)
                    return false;
            }
            return true;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (_enemySpawnDelay == null) return;
            _enemySpawnDelay.TimerUpdate(Time.deltaTime);
            if (_enemySpawnDelay.IsComplete)
            {
                StartSpawn();
                _enemySpawnDelay.Reset(
                    Random.Range(SpawnData.BaseMinTimeBetweenSpawns,
                        SpawnData.BaseMaxTimeBetweenSpawns));
            }
        }

        //currently just spawns tranquil. Fix and add random chance of each
        public void StartSpawn()
        {
            if (!IsServer) return;
            if (_aliveEnemies.Count >= SpawnData.MaxTotalEnemies)
            {
                Debug.Log("[EnemySpawnManager] Enemy cap reached.");
                return;
            }
            var allPoints = EnemySpawnPoints.Instance.ActiveEnemySpawnPoints;

            if (allPoints.Count == 0)
            {
                Debug.LogWarning("[EnemySpawnManager] No spawn points at all!");
                return;
            }

            List<EnemySpawnPoint> candidates = new List<EnemySpawnPoint>();

            foreach (var sp in allPoints)
            {
                if (IsPointSafe(sp))
                    candidates.Add(sp);
            }

            if (candidates.Count == 0)
            {
                Debug.Log("[EnemySpawnManager] No valid spawn points near players.");
                return;
            }

            int spawnCount = Random.Range(SpawnData.MinSpawnPerWave,SpawnData.MaxSpawnPerWave);

            for (int i = 0; i < spawnCount; i++)
            {
                int idx = Random.Range(0, candidates.Count);

                RandomEnemySelection(candidates[idx]);
            }
        }

        public void SpawnTranquil(EnemySpawnPoint spawnPoint)
        {
            if (!IsServer) return;
            if (EnemyPrefabs.TranquilPrefabs.Count < 1) return;
            int enemyIndex = Random.Range(0, EnemyPrefabs.TranquilPrefabs.Count);
            DoSpawnEnemy(EnemyPrefabs.TranquilPrefabs[enemyIndex], spawnPoint);
         
            
        }

        public void SpawnViolent(EnemySpawnPoint spawnPoint)
        {
            if (!IsServer) return;
            if (EnemyPrefabs.ViolentPrefabs.Count < 1) return;
            int enemyIndex = Random.Range(0, EnemyPrefabs.ViolentPrefabs.Count);
            DoSpawnEnemy(EnemyPrefabs.ViolentPrefabs[enemyIndex], spawnPoint);
        }

        public void SpawnHorror(EnemySpawnPoint spawnPoint)
        {
            if (!IsServer) return;
            if (EnemyPrefabs.HorrorPrefabs.Count < 1) return;
            int enemyIndex = Random.Range(0, EnemyPrefabs.HorrorPrefabs.Count);
            DoSpawnEnemy(EnemyPrefabs.HorrorPrefabs[enemyIndex], spawnPoint);
        }

        private void DoSpawnEnemy(GameObject enemyPF, EnemySpawnPoint spawnPoint)
        {

            if (!IsServer) return;

            if (enemyPF == null)
            {
                Debug.LogError("[EnemySpawnManager] DoSpawnEnemy called with null enemyPF");
                return;
            }

            if (spawnPoint == null)
            {
                Debug.LogError("[EnemySpawnManager] DoSpawnEnemy called with null spawnPoint");
                return;
            }

            var spawnTransform = spawnPoint.GetSpawnPoint();
            Vector3 pos = spawnTransform.position;
            Quaternion rot = spawnTransform.rotation;

            NetworkPrefabRuntimeRegistry.EnsurePrefabRegistered(enemyPF);

            GameObject temp = Object.Instantiate(enemyPF, pos, rot);

            var netObj = temp.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("[EnemySpawnManager] Enemy prefab is missing NetworkObject component!");
                Object.Destroy(temp);
                return;
            }

            netObj.Spawn();
            _aliveEnemies.Add(netObj); 
            // handle the remove
        }

        private void RandomEnemySelection(EnemySpawnPoint sp)
        {
            float totalWeight = SpawnData.TranquilRandWeight + SpawnData.ViolentRandWeight + SpawnData.HorrorWeight;
            float randomValue = Random.value * totalWeight;

            if (randomValue < SpawnData.TranquilRandWeight)
            {
                SpawnTranquil(sp);
                
            }
              
            else if (randomValue < SpawnData.TranquilRandWeight + SpawnData.ViolentRandWeight)
            {
                SpawnViolent(sp);
            }

            else
            {
                SpawnHorror(sp);
            }
             
        }
    }
}