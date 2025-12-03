using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteAlertState : BruteBaseState
    {
        private Timer _alertTimer = new Timer(0f);
        public BruteAlertState(BruteStateMachine stateController) : base(stateController)
        {
            this.StateController = stateController;
        }
        public override void OnEnter()
        {
            Animator.PlayAlert();
            Agent.speed = BruteSO.AlertWalkSpeed;
            _alertTimer.Reset(BruteSO.LoseInterestTimeInvestigate);
            Agent.SetDestination(StateController.LastHeardPlayer.transform.position);
        }
        public override void OnExit()
        {
            _alertTimer.Stop();
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
}
