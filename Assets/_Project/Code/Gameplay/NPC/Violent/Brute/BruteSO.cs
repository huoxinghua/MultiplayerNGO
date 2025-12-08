using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    [CreateAssetMenu(fileName = "BruteSO", menuName = "Scriptable Objects/BruteSO")]
    public class BruteSO : BaseEnemySO
    {
        [field: Header("Brute Movement")]
        [field: SerializeField] public float AlertWalkSpeed { get; private set; } = 3f;
        [field: SerializeField] public float HurtWalkSpeed { get; private set; } = 15f;
        [field: SerializeField] public float MinWanderDistance { get; private set; } = 15f;
        [field: SerializeField] public float MaxWanderDistance { get; private set; } = 25f;

        [field: Header("Brute Idle")]
        [field: SerializeField] public float MinIdleTime { get; private set; } = 7f;
        [field: SerializeField] public float MaxIdleTime { get; private set; } = 10f;
        public float RandomIdleTime => Random.Range(MinIdleTime, MaxIdleTime);
        [field: Header("Brute Attack")]
        [field: SerializeField] public float AttackDistance { get; private set; } = 2f;
        [field: Header("Brute Alert")]
        [field: SerializeField] public float InstantAggroDistance { get; private set; } = 4f;
        [field: SerializeField] public int TimesHeardBeforeAgro { get; private set; } = 2;
        [field: SerializeField] public float HearingCooldown { get; private set; } = 3f;
        [field: SerializeField] public float LoseInterestTimeInvestigate { get; private set; } = 20f;
        [field: SerializeField] public float LoseInterestTimeChase { get; private set; } = 8f;
        [field: SerializeField] public float LoseInterestDistanceChase { get; private set; } = 40f;
        [field: Header("Scared To Delete")]
        //no idea whats this here
        [field: SerializeField] public float LandingCooldown { get; private set; } = 0f;
    }
}

