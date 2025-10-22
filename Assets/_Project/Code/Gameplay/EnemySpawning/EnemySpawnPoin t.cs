using UnityEditor.Rendering;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform _spawnPos;
    [SerializeField] private float _safeDistance;
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
        EnemySpawnPoints.Instance.RemoveSpawnPoint(this);
    }
    public void DoSpawnEnemy(GameObject EnemyPrefab)
    {
        GameObject temp = Instantiate(EnemyPrefab, _spawnPos);
    }
}
