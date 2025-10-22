

using _Project.Code.Core.Patterns;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoints : Singleton<EnemySpawnPoints>
{
    public List<EnemySpawnPoint> ActiveEnemySpawnPoints = new List<EnemySpawnPoint>();
    public void AddSpawnPoint(EnemySpawnPoint enemySpawnPoint)
    {
        ActiveEnemySpawnPoints.Add(enemySpawnPoint);
    }
    public void RemoveSpawnPoint(EnemySpawnPoint enemySpawnPoint)
    {
        ActiveEnemySpawnPoints.Remove(enemySpawnPoint);
    }
    public void ClearSpawnPoints()
    {
        ActiveEnemySpawnPoints.Clear();
    }
}
