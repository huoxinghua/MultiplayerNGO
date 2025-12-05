using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteHurtIdleState : BruteBaseState
    {
        private const float ATTACK_CHECK_INTERVAL = 0.1f;

        private Timer _idleTimer;
        private Timer _attackCheckTimer;

        public BruteHurtIdleState(BruteStateMachine stateController) : base(stateController)
        {
        }

        public override void OnEnter()
        {
            _idleTimer = new Timer(BruteSO.RandomIdleTime);
            _idleTimer.Start();
            _attackCheckTimer = new Timer(ATTACK_CHECK_INTERVAL);
            _attackCheckTimer.Start();
            Animator.PlayInjured();
            Agent.SetDestination(StateController.gameObject.transform.position);
        }
        public override void OnExit()
        {
            _idleTimer.Stop();
            _idleTimer = null;
        }

        public override void StateUpdate()
        {
            _idleTimer.TimerUpdate(Time.deltaTime);
            if (_idleTimer.IsComplete)
            {
                StateController.TransitionTo(StateController.WanderState);
            }
        }
        public override void StateFixedUpdate()
        {
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
        }
        public override void OnHearPlayer()
        {

        }
    }
}
