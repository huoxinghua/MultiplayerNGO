using UnityEngine;

[CreateAssetMenu(fileName = "SpawnDataSO", menuName = "Enemy/SpawnDataSO")]
public class SpawnDataSO : ScriptableObject
{
    [field: SerializeField] public float BaseMaxTimeBetweenSpawns;
    [field: SerializeField] public float BaseMinTimeBetweenSpawns;
    public float BaseRandomTimeBetweenSpawns => Random.Range(BaseMinTimeBetweenSpawns, BaseMaxTimeBetweenSpawns);
}
