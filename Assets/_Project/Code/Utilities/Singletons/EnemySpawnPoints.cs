using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.EnemySpawning;

namespace _Project.Code.Utilities.Singletons
{
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
}
