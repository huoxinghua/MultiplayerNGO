using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteChaseState : BruteBaseState
    {
        public BruteChaseState(BruteStateMachine stateController) : base(stateController)
        {
            this.StateController = stateController;
        }
        private Timer _chaseTimer;
        public override void OnEnter()
        {
            _chaseTimer = new Timer(BruteSO.LoseInterestTimeChase);
            _chaseTimer.Start();
            Animator.PlayAlert();
            Agent.speed = BruteSO.RunSpeed;
        }
        public override void OnExit()
        {
            StateController.TimesAlerted = 0;
            _chaseTimer = null;
        }

        public override void StateUpdate()
        {
            if (_chaseTimer.IsComplete || Vector3.Distance(StateController.LastHeardPlayer.transform.position,StateController.transform.position) >= BruteSO.LoseInterestDistanceChase)
            {
                StateController.TransitionTo(StateController.IdleState);
            }
        }
        public override void StateFixedUpdate()
        {
            Agent.SetDestination(StateController.LastHeardPlayer.transform.position);//need add check,make sure if player die not call anymore
            foreach (PlayerList player in PlayerList.AllPlayers)
            {
                if (Vector3.Distance(player.transform.position, StateController.transform.position) < BruteSO.AttackDistance)
                {
                    StateController.OnAttack(player.gameObject);
                }
            }
            Animator.PlayRun(Agent.velocity.magnitude, Agent.speed);
        }
        public override void OnHearPlayer()
        {
            _chaseTimer.Reset(BruteSO.LoseInterestTimeChase);
        }
    }
}
