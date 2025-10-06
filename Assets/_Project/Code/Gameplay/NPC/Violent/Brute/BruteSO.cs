using UnityEngine;
[CreateAssetMenu(fileName = "BruteSO", menuName = "Scriptable Objects/BruteSO")]
public class BruteSO : BaseEnemySO
{
    [field: Header("Name")]
    [field: SerializeField] public float AlertWalkSpeed { get; private set; }
    [field: SerializeField] public float MinWanderDistance { get; private set; }
    [field: SerializeField] public float MaxWanderDistance { get; private set; }
    [field: SerializeField] public float MinIdleTime { get; private set; }
    [field: SerializeField] public float MaxIdleTime { get; private set; }
    [field: SerializeField] public float HurtWalkSpeed { get; private set; }
    [field: SerializeField] public float AttackDistance { get; private set; }
    [field: SerializeField] public float WalkHearingDistance { get; private set; }
    [field: Header("Values")]
    public float RunHearingDistance;
    public float LandingHearingDistance;
    public float InstantAggroDistance;
    public float HearingCooldown;
    public float LoseInterestTimeInvestigate;
    public float LoseInterestTimeChase;
    public float LandingCooldown;

    public float RandomIdleTime => Random.Range(MinIdleTime, MaxIdleTime);
    private float _randomT;
    public float RandomT
    {
        get
        {
            return _randomT;
        }
        private set
        {

            _randomT = value;
        }
    }
}

/*//[Range(0,100)]    */