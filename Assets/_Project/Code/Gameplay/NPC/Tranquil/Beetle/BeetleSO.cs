using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    [CreateAssetMenu(fileName = "BeetleSO", menuName = "Scriptable Objects/BeetleSO")]
    public class BeetleSO : BaseEnemySO
    {
        [field: Header("Beetle Idle")]
        [field: SerializeField] public float MinIdleTime { get; private set; } = 6f;
        [field: SerializeField] public float MaxIdleTime { get; private set; } = 14f;
        public float RandomIdleTime => Random.Range(MinIdleTime, MaxIdleTime);

        [field: Header("Beetle Wander")]
        [field: SerializeField] public float MinWanderDist { get; private set; } = 10f;
        [field: SerializeField] public float MaxWanderDist { get; private set; } = 25f;
        public float RandomWanderDist => Random.Range(MinWanderDist, MaxWanderDist) * (Random.Range(0, 2) * 2 - 1);

        [field: Header("Beetle Follow")]
        [field: SerializeField] public float MinFollowTime { get; private set; } = 10f;
        [field: SerializeField] public float MaxFollowTime { get; private set; } = 45f;
        public float RandomFollowTime => Random.Range(MinFollowTime, MaxFollowTime);
        [field: SerializeField] public float FollowCooldown { get; private set; } = 30f;
        [field: Header("Beetle Run")]
        [field: SerializeField] public float MaxRunPointOffset { get; private set; } = 10f;
        public float RandomRunOffset => Random.Range(-MaxRunPointOffset, MaxRunPointOffset);
        [field: SerializeField] public float FleeDistance { get; private set; } = 29.1f;
        [field: SerializeField] public float StopRunDistance { get; private set; } = 20f;
    }
}
