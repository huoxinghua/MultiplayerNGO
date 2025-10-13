using UnityEngine;
[CreateAssetMenu(fileName = "BeetleSO", menuName = "Scriptable Objects/BeetleSO")]
public class BeetleSO : BaseEnemySO
{
    [field: Header("Beetle Idle")]
    [field: SerializeField] public float MinIdleTime { get; private set; }
    [field: SerializeField] public float MaxIdleTime { get; private set; }
    public float RandomIdleTime => Random.Range(MinIdleTime, MaxIdleTime);

    [field: Header("Beetle Wander")]
    [field: SerializeField] public float MinWanderDist { get; private set; }
    [field: SerializeField] public float MaxWanderDist { get; private set; }
    public float RandomWanderDist => Random.Range(MinWanderDist, MaxWanderDist) * (Random.Range(0, 2) * 2 - 1);

    [field: Header("Beetle Follow")]
    [field: SerializeField] public float MinFollowTime { get; private set; }
    [field: SerializeField] public float MaxFollowTime { get; private set; }
    public float RandomFollowTime => Random.Range(MinFollowTime, MaxFollowTime);
}
