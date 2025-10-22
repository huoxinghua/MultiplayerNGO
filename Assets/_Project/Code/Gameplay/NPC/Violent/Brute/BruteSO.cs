using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    [CreateAssetMenu(fileName = "BruteSO", menuName = "Scriptable Objects/BruteSO")]
    public class BruteSO : BaseEnemySO
    {
        [field: Header("Brute Movement")]
        [field: SerializeField] public float AlertWalkSpeed { get; private set; }
        [field: SerializeField] public float HurtWalkSpeed { get; private set; }
        [field: SerializeField] public float MinWanderDistance { get; private set; }
        [field: SerializeField] public float MaxWanderDistance { get; private set; }

        [field: Header("Brute Idle")]
        [field: SerializeField] public float MinIdleTime { get; private set; }
        [field: SerializeField] public float MaxIdleTime { get; private set; }
        public float RandomIdleTime => Random.Range(MinIdleTime, MaxIdleTime);
        [field: Header("Brute Attack")]
        [field: SerializeField] public float AttackDistance { get; private set; }
        [field: Header("Brute Alert")]
        [field: SerializeField] public float InstantAggroDistance { get; private set; }
        [field: SerializeField] public int TimesHeardBeforeAgro { get; private set; }
        [field: SerializeField] public float HearingCooldown { get; private set; }
        [field: SerializeField] public float LoseInterestTimeInvestigate { get; private set; }
        [field: SerializeField] public float LoseInterestTimeChase { get; private set; }
        [field: SerializeField] public float LoseInterestDistanceChase { get; private set; }
        [field: Header("Scared To Delete")]
        //no idea whats this here
        [field: SerializeField] public float LandingCooldown { get; private set; }


 

        //for a reference. Not part of BruteSO
        //private float _randomT;
        //public float RandomT
        //{
        //    get
        //    {
        //        return _randomT;
        //    }
        //    private set
        //    {

        //        _randomT = value;
        //    }
        //}
    }
}

/*//[Range(0,100)]    */