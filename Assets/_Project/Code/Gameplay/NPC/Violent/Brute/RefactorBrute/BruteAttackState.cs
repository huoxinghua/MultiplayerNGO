using UnityEngine;


public class BruteAttackState : BruteBaseState
{
    //temp
    public float rotationSpeed = 5f;
    public BruteAttackState(BruteStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        Agent.speed = 0;
        Animator.PlayAttack();
        Agent.updateRotation = false;
    }
    public override void OnExit()
    {
        Agent.updateRotation = true;
    }

    public override void StateUpdate()
    {
        Vector3 direction = (StateController.PlayerToAttack.transform.position - StateController.transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            StateController.transform.rotation = Quaternion.RotateTowards(StateController.transform.rotation , targetRotation , rotationSpeed * Time.deltaTime * 100f);
        }
    }
    public override void StateFixedUpdate()
    {

    }
    public override void OnHearPlayer()
    {

    }
}
