using _Project.Code.Network.RegisterNetObj;
using _Project.Code.Utilities.Singletons;
using UnityEngine;

namespace _Project.Code.Gameplay.EnemySpawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        //keep this sc to monoBehaviour is important
        [SerializeField] private Transform _spawnPos;
        [SerializeField] private float _safeDistance;
        public float SafeDistance => _safeDistance;
        public bool CanSpawnEnemy()
        {
            bool canSpawn = true;
            foreach(Transform playerTransforms in CurrentPlayers.Instance.PlayerTransforms)
            {
                if (Vector3.Distance(transform.position, playerTransforms.position) <=  _safeDistance)
                {
                    canSpawn = false;
                }
            }
        
            return canSpawn;
        }
        private void OnEnable()
        {
            EnemySpawnPoints.Instance.AddSpawnPoint(this);
        }
        private void OnDisable()
        {
            EnemySpawnPoints.Instance?.RemoveSpawnPoint(this);
        }
        /*
        public void DoSpawnEnemy(GameObject EnemyPrefab)
        {
            Debug.Log("[EnemySpawnPoint]do spawn enemy");
            NetworkPrefabRuntimeRegistry.EnsurePrefabRegistered(EnemyPrefab);
            GameObject temp = Instantiate(EnemyPrefab, _spawnPos);
            temp.GetComponent<NetworkObject>().Spawn();
            temp.transform.parent = null;
        }
        */

        public Transform GetSpawnPoint()
        {
            return _spawnPos;
        }
    }
}
