using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle.BeetleRefactor
{
    public class BeetleIdleState : BeetleBaseState
    {
        public BeetleIdleState(BeetleStateMachine stateController) : base(stateController)
        {

        }
        private Timer _idleTimer;
        private Timer _randomAnimationTimer;
        private bool _hasPlayedRandomAnim;
        public override void OnEnter()
        {
            float randomIdleTime = BeetleSO.RandomIdleTime;
            float randomAnimationTime = Random.Range(1, randomIdleTime - 4);
            _hasPlayedRandomAnim = false;
            _idleTimer = new Timer(randomIdleTime);
            _randomAnimationTimer = new Timer(randomAnimationTime);
            _randomAnimationTimer.Start();
            _idleTimer.Start();
            Animator.PlayWalk(0, 10);
            Animator.PlayRandomIdle(0, 1);
            Agent.speed = 0;
        }
        public override void OnExit()
        {
            _idleTimer?.Stop();
            _randomAnimationTimer?.Stop();
            _idleTimer = null;
            _randomAnimationTimer = null;
        }

        public override void StateUpdate()
        {
            _idleTimer?.TimerUpdate(Time.deltaTime);
            _randomAnimationTimer?.TimerUpdate(Time.deltaTime);
            if(!_hasPlayedRandomAnim && _randomAnimationTimer.IsComplete)
            {
                Animator.PlayRandomIdle(1, 0);
                _hasPlayedRandomAnim = true;
            }
            if (_idleTimer.IsComplete)
            {
                StateController.TransitionTo(StateController.WanderState);
            }
        }
        public override void StateFixedUpdate()
        {

        }
        public override void OnSpotPlayer(bool isHostilePlayer)
        {
            if (isHostilePlayer)
            {
                StateController.TransitionTo(StateController.RunState);
            }
            else if (StateController.FollowCooldown.IsComplete || StateController.IsFirstFollow)
            {
                StateController.TransitionTo(StateController.FollowState);
            }
        }
        public override void OnHitByPlayer()
        {
            StateController.TransitionTo(StateController.RunState);
        }
    }
}
