using UnityEngine;

public class BruteAnimationEventController : MonoBehaviour
{
    [SerializeField] private BruteStateMachine _stateMachine;
    public void OnFootStep()
    {
        AudioManager.Instance.PlayByKey3D("BruteFootStep", transform.position);
    }
    public void OnAttackNoise()
    {
        AudioManager.Instance.PlayByKey3D("BruteAttack", transform.position);
    }
    public void OnAttackConnect()
    {
        _stateMachine.OnAttackConnects();   
    }
    public void OnAttackEnd()
    {
        _stateMachine.OnAttackEnd();
    }
}
