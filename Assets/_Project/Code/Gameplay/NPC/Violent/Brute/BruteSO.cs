using UnityEngine;
[CreateAssetMenu(fileName = "BruteSO", menuName = "Scriptable Objects/BruteSO")]
public class BruteSO : BaseEnemySO
{
    public float AlertWalkSpeed;
    public float MinWanderDistance;
    public float MaxWanderDistance;
    public float MinIdleTime;
    public float MaxIdleTime;
    public float HurtWalkSpeed;
    public float AttackDistance;
    public float WalkHearingDistance;
    public float RunHearingDistance;
    public float LandingHearingDistance;
    public float InstantAggroDistance;
    public float HearingCooldown;
    public float LoseInterestTimeInvestigate;
    public float LoseInterestTimeChase;
}
