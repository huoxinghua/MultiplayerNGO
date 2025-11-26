using UnityEngine;

namespace _Project.Code.Gameplay.EnemySpawning
{
    [CreateAssetMenu(fileName = "SpawnDataSO", menuName = "Enemy/SpawnDataSO")]
    public class SpawnDataSO : ScriptableObject
    {
        [field: Header("New Values")]
        [field: SerializeField] public int SpawnAttemptRate {  get; private set; }
        [field: Header("Old Values")]
        [field: SerializeField] public float BaseMaxTimeBetweenSpawns {  get; private set; }
        [field: SerializeField] public float BaseMinTimeBetweenSpawns { get; private set; }
        [field: SerializeField]  public int MinSpawnPerWave { get; private set; }
        [field: SerializeField]    public int MaxSpawnPerWave { get; private set; }
        [field: SerializeField]    public int MaxTotalEnemies { get; private set; }
        public float BaseRandomTimeBetweenSpawns => Random.Range(BaseMinTimeBetweenSpawns, BaseMaxTimeBetweenSpawns);

        [field: SerializeField] public float TranquilRandWeight { get; private set; }
        [field: SerializeField] public float ViolentRandWeight { get; private set; }
        [field: SerializeField] public float HorrorWeight { get; private set; }
    }
}
