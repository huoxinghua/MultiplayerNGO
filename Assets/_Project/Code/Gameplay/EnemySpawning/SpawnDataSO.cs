using UnityEngine;

[CreateAssetMenu(fileName = "SpawnDataSO", menuName = "Enemy/SpawnDataSO")]
public class SpawnDataSO : ScriptableObject
{
    [field: SerializeField] public float BaseMaxTimeBetweenSpawns {  get; private set; }
    [field: SerializeField] public float BaseMinTimeBetweenSpawns { get; private set; }
    public float BaseRandomTimeBetweenSpawns => Random.Range(BaseMinTimeBetweenSpawns, BaseMaxTimeBetweenSpawns);

    [field: SerializeField] public float TranquilRandWeight { get; private set; }
    [field: SerializeField] public float ViolentRandWeight { get; private set; }
    [field: SerializeField] public float HorrorWeight { get; private set; }
}
