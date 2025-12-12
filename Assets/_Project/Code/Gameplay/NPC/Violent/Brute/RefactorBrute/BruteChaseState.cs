using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteChaseState : BruteBaseState
    {
        private const float ATTACK_CHECK_INTERVAL = 0.1f;

        public BruteChaseState(BruteStateMachine stateController) : base(stateController)
        {
            this.StateController = stateController;
        }

        private Timer _chaseTimer;
        private Timer _attackCheckTimer;
        public override void OnEnter()
        {
            _chaseTimer = new Timer(BruteSO.LoseInterestTimeChase);
            _chaseTimer.Start();
            _attackCheckTimer = new Timer(ATTACK_CHECK_INTERVAL);
            _attackCheckTimer.Start();
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
            if (StateController.LastHeardPlayer == null || _chaseTimer.IsComplete ||
                Vector3.Distance(StateController.LastHeardPlayer.transform.position, StateController.transform.position) >= BruteSO.LoseInterestDistanceChase)
            {
                StateController.TransitionTo(StateController.IdleState);
            }
        }
        public override void StateFixedUpdate()
        {
            if (StateController.LastHeardPlayer == null)
            {
                StateController.TransitionTo(StateController.IdleState);
                return;
            }

            Agent.SetDestination(StateController.LastHeardPlayer.transform.position);

            // Throttle attack distance checks to 10/sec instead of 50/sec
            _attackCheckTimer.TimerUpdate(Time.fixedDeltaTime);
            if (_attackCheckTimer.IsComplete)
            {
                _attackCheckTimer.Reset();
                foreach (PlayerList player in PlayerList.AllPlayers)
                {
                    if (Vector3.Distance(player.transform.position, StateController.transform.position) < BruteSO.AttackDistance)
                    {
                        StateController.OnAttack(player.gameObject);
                    }
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
