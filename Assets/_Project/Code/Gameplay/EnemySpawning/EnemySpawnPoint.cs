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
        private void OnEnable()
        {
            EnemySpawnPoints.Instance.AddSpawnPoint(this);
        }
        private void OnDisable()
        {
            EnemySpawnPoints.Instance?.RemoveSpawnPoint(this);
        }

        public Transform GetSpawnPoint()
        {
            return _spawnPos;
        }
    }
}
