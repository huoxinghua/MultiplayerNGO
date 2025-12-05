using System.Collections;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Utilities.Singletons;
using UnityEngine;
using _Project.Code.Gameplay.EnemySpawning;
using _Project.Code.Gameplay.NPC;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.Code.Network.RegisterNetObj;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using Timer = _Project.Code.Utilities.Utility.Timer;
using _Project.Code.Gameplay.Market.Quota;
using _Project.Code.Gameplay.Market.Sell;

namespace _Project.Code.Gameplay.EnemySpawning
{
    public class EnemySpawnManager : NetworkSingleton<EnemySpawnManager>
    {
        private Timer _enemySpawnDelay;
        [field: SerializeField] public EnemyPrefabsSO EnemyPrefabs { get; private set; }
        [field: SerializeField] public SpawnDataSO SpawnData { get; private set; }
        [field: SerializeField] public BaseSpawnSO TranquilSpawnSo { get; private set; }
        [field: SerializeField] public BaseSpawnSO ViolentSpawnSo { get; private set; }
        [field: SerializeField] public BaseSpawnSO HorrorSpawnSo { get; private set; }

        private readonly HashSet<NetworkObject> _aliveEnemies = new();

        // new spawn system
        private List<NetworkObject> _spawnedTranquils = new List<NetworkObject>();
        private List<NetworkObject> _spawnedViolents = new List<NetworkObject>();
        private List<NetworkObject> _spawnedDolls = new List<NetworkObject>();
        private int _tranquilsSpawned = 0;
        private int _violentsSpawned = 0;
        private int _HorrorsSpawned = 0;

        private Timer SpawnAttemptTimer;
        private int _totalAttempts = 0;

        //reset on successful spawn, add when fail
        private int _currentTranquilAttempts = 0;
        private int _currentViolentAttempts = 0;

        private int _currentHorrorAttempts = 0;

        //constant value for base of natural log
        private float _eulersNum => Mathf.Exp(1); // (maybe just make it a const)

        private float currentTranquilSpawnChance =>
            GetTrueSpawnChance(TranquilSpawnSo, _currentTranquilAttempts, _tranquilsSpawned);
        private float currentViolentSpawnChance =>
            GetTrueSpawnChance(ViolentSpawnSo, _currentViolentAttempts, _violentsSpawned);
        private float currentHorrorSpawnChance =>
            GetTrueSpawnChance(HorrorSpawnSo, _currentHorrorAttempts, _HorrorsSpawned);
        //new spawn system end
        private readonly HashSet<NetworkObject> _aliveEnemiesRelatedObjs = new();

        private bool _hasSpawned = false;
        private float GetTrueSpawnChance(BaseSpawnSO spwnSO, int AttemptsSinceSpawn, int TotalSpawns)
        {
            Debug.Log("GetTrueSpawnChance  spawn So"+ spwnSO.name);
            // 1. Chance to spawn plus additive change
            float incrementalChance = (spwnSO.BaseSpawnChance + (spwnSO.IncreaseByAttempts * AttemptsSinceSpawn));

            // 2. Multiplier for Spawn Chance
            float chanceMult = (spwnSO.MultAtZeroTime + (spwnSO.MultByTime * _totalAttempts));

           // 3. Additive chance multiplied by the multipler (Step 1 *  Step 2)
            float positivesPercentChanceToSpawn = (incrementalChance * chanceMult);


            //Determines when and how fast spawn chance should fall off. Multiply by positive chance (step 3)
            float changeByTotalSpawns =
                (1 + Mathf.Pow(_eulersNum, ((-spwnSO.SpeedOfMajorLoss) * spwnSO.SpawnsBeforeMajorLoss))) / (1 +
                    Mathf.Pow(_eulersNum, ((spwnSO.SpeedOfMajorLoss) * (TotalSpawns - spwnSO.SpawnsBeforeMajorLoss))));

            //Multiplying positivesPercentChanceToSpawn with changeByTotalSpawns gives the "true spawn chance"
            float trueSpawnChance = positivesPercentChanceToSpawn * changeByTotalSpawns;

            // clamps the trueSpawnChance to ensure its under 100 but above the min
            float finalSpawnChance = Mathf.Clamp(trueSpawnChance, spwnSO.MinClampSpawn, 100);

            return finalSpawnChance;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _hasSpawned = true;
            _enemySpawnDelay = new Timer(SpawnData.BaseRandomTimeBetweenSpawns);
            _enemySpawnDelay.Start();
            
            SpawnAttemptTimer = new Timer(SpawnData.SpawnAttemptRate);
            SpawnAttemptTimer.Start();
            StartCoroutine(WaitForEnemySpawnPoint());
            EventBus.Instance.Subscribe<OnEnterHubEvent>(this, DespawnAllEnemies);

        }

        private IEnumerator WaitForEnemySpawnPoint()
        {
            yield return new WaitUntil(() => EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count > 0);
            yield return new WaitForSeconds(.2f);
            SpawnOnStart();
        }


        private void SpawnOnStart()
        {
            if (!_hasSpawned) return;
            Debug.Log("EventHurd");
            Debug.Log("SpawnOnStart");
            var point = GetEnemySpawnPoint();
            SpawnViolent(point);
            point = GetEnemySpawnPoint();
            SpawnTranquil(point);
            point = GetEnemySpawnPoint();
            SpawnHorror(point);
        }

        private EnemySpawnPoint GetEnemySpawnPoint()
        {
            var points = EnemySpawnPoints.Instance.ActiveEnemySpawnPoints;
            if (points.Count == 0)
            {
                Debug.LogWarning("[EnemySpawnManager] OnNetworkSpawn: no spawn points");
            }

            int idx = Random.Range(0, points.Count);

            return points[idx];
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
   
            SpawnAttemptTimer.TimerUpdate(Time.deltaTime);
            if (SpawnAttemptTimer.IsComplete)
            {
                HandleEnemySpawnChance();
                SpawnAttemptTimer.Reset();
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
      
        }


        private void HandleEnemySpawnChance()
        {
            // get the base spawn chance 
            HandleTranquilAttempt();
            HandleViolentAttempt();
            HandleHorrorAttempt();
            // if can been sapwned or not
            _totalAttempts++;
        }

        private void HandleHorrorAttempt()
        {
            float randomChance = Random.Range(0f, 100f);
            if (randomChance <= currentHorrorSpawnChance)
            {
                
                SpawnHorror(GetEnemySpawnPoint());
                _currentHorrorAttempts = 0;
                _HorrorsSpawned++;
            }
            else
            {
                _currentHorrorAttempts++;
            }
        }

        private void HandleViolentAttempt()
        {
            float randomChance = Random.Range(0f, 100f);
            if (randomChance <= currentViolentSpawnChance)
            {
                
                SpawnViolent(GetEnemySpawnPoint());
                _currentViolentAttempts = 0;
                _violentsSpawned++;
            }
            else
            {
                _currentViolentAttempts++;
            }
        }

        private void HandleTranquilAttempt()
        {
            float randomChance = Random.Range(0f, 100f);
            if (randomChance <= currentTranquilSpawnChance)
            {
                
                SpawnTranquil(GetEnemySpawnPoint());

                //Reset _tranquilAttempts
                _currentTranquilAttempts = 0;

                //increment _tranquilsSpawned
                _tranquilsSpawned++;
            }
            else
            {
                //IncreaseTranquilAttempts
                _currentTranquilAttempts++;
            }
        }

        //old spawn system
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

        public void DespawnAllEnemies()
        {
            if (!IsServer) return;

            foreach (var enemy in _aliveEnemies)
            {
                if (enemy != null && enemy.IsSpawned)
                {
                    enemy.Despawn(true);
                }
            }

            _aliveEnemies.Clear();

            foreach (var relatedObj in _aliveEnemiesRelatedObjs)
            {
                if (relatedObj != null && relatedObj.IsSpawned)
                {
                    relatedObj.Despawn(true);
                }
            }


            // Despawn unheld sellable items (dead beetles, brute pieces)
            SellableItemManager.Instance?.DespawnUnheldItems();
        }

        public void DespawnAllEnemies(OnEnterHubEvent evt)
        {
            if (!IsServer) return;

            foreach (var enemy in _aliveEnemies)
            {
                if (enemy != null && enemy.IsSpawned)
                {
                    enemy.Despawn(true);
                }
            }

            _aliveEnemies.Clear();

            foreach (var relatedObj in _aliveEnemiesRelatedObjs)
            {
                if (relatedObj != null && relatedObj.IsSpawned)
                {
                    relatedObj.Despawn(true);
                }
            }


            // Despawn unheld sellable items (dead beetles, brute pieces)
            SellableItemManager.Instance?.DespawnUnheldItems();
        }

        public void RegisterEnemyRelatedObject(NetworkObject netObject)
        {
            if (!IsServer || netObject == null) return;
            _aliveEnemiesRelatedObjs.Add(netObject);
        }

        public void UnregisterEnemyRelatedObject(NetworkObject netObject)
        {
            if (!IsServer || netObject == null) return;
            _aliveEnemiesRelatedObjs.Remove(netObject);
        }
        /// <summary>
        /// Unregisters and despawns a dead enemy.
        /// Called when an enemy dies to properly clean it up from the tracking system.
        /// </summary>
        public void UnregisterDeadEnemy(NetworkObject enemy)
        {
            if (!IsServer) return;

            if (enemy != null && _aliveEnemies.Contains(enemy))
            {
                _aliveEnemies.Remove(enemy);
                Debug.Log($"[EnemySpawnManager] Unregistered dead enemy, {_aliveEnemies.Count} enemies remaining");
            }

            if (enemy != null && enemy.IsSpawned)
            {
                enemy.Despawn(true);
            }
        }
    }

    public struct DungeonCompleteEvent : IEvent
    {
        
    }
}