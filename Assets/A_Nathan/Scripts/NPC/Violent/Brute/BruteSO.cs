using UnityEngine;
[CreateAssetMenu(fileName = "BruteSO", menuName = "Scriptable Objects/BruteSO")]
public class BruteSO : BaseEnemySO
{
    public float MinWanderDistance;
    public float MaxWanderDistance;
    public float MinIdleTime;
    public float MaxIdleTime;
    public float HurtWalkSpeed;
    public float AttackDistance;
}
