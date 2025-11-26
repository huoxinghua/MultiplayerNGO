using UnityEngine;

namespace _Project.Code.Gameplay.EnemySpawning
{
    [CreateAssetMenu(fileName = "BaseSpawnSO", menuName = "Spawn/BaseSpawnSO")]
    public class BaseSpawnSO : ScriptableObject
    {
        [field:SerializeField] public float BaseSpawnChance { get; private set; }
        [field:SerializeField] public float IncreaseByAttempts { get; private set; }
        [field: SerializeField] public int SpawnsBeforeMajorLoss { get; private set; }
        [field: SerializeField] public float MultAtZeroTime { get; private set; }
        
        [field: SerializeField] public float MultByTime { get; private set; }
        [field: SerializeField] public float SpeedOfMajorLoss { get; private set; }
        [field: SerializeField] public  float MinClampSpawn { get; private set; }
        
        
   
    }
}
