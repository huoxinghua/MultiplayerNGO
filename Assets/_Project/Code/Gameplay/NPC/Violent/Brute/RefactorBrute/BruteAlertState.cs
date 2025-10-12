using UnityEngine;

public class BruteAlertState : BruteBaseState
{
    private Timer _alertTimer = new Timer(0f);
    public BruteAlertState(BruteStateMachine stateController) : base(stateController)
    {
        this.StateController = stateController;
    }
    public override void OnEnter()
    {
        Debug.Log("Alert not chase " + BruteSO.AlertWalkSpeed);
        Animator.PlayAlert();
        Agent.speed = BruteSO.AlertWalkSpeed;
        _alertTimer.Reset(BruteSO.LoseInterestTimeInvestigate);
        Agent.SetDestination(StateController.LastHeardPlayer.transform.position);
    }
    public override void OnExit()
    {
        _alertTimer.Stop();
        Debug.Log("Left Alert");
    }

    public override void StateUpdate()
    {
        _alertTimer.TimerUpdate(Time.deltaTime);
        if (_alertTimer.IsDone)
        {
            StateController.TimesAlerted = 0;
            StateController.TransitionTo(StateController.IdleState);
        }
        
    }
    public override void StateFixedUpdate()
    {
        Animator.PlayWalk(Agent.velocity.magnitude, Agent.speed);
    }
    public override void OnHearPlayer()
    {
        StateController.TransitionTo(StateController.BruteHeardPlayerState);
    }
}
