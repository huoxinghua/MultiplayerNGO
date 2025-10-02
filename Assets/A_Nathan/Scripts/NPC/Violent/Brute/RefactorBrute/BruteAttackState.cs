using UnityEngine;


public class BruteAttackState : BruteBaseState
{
    public float rotationSpeed = 5f;
    public BruteAttackState(BruteStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        agent.speed = 0;
        animator.PlayAttack();
        agent.updateRotation = false;
    }
    public override void OnExit()
    {
        agent.updateRotation = true;
    }

    public override void StateUpdate()
    {
        Vector3 direction = (stateController.PlayerToAttack.transform.position - stateController.transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stateController.transform.rotation = Quaternion.RotateTowards(stateController.transform.rotation , targetRotation , rotationSpeed * Time.deltaTime * 100f);
        }
    }
    public override void StateFixedUpdate()
    {

    }
    public override void OnHearPlayer()
    {

    }
}
