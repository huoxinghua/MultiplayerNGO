using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollHuntingState : DollBaseState
    {
        public DollHuntingState(DollStateMachine stateMachine, StateEnum stateEnum) : base(stateMachine, stateEnum)
        {
        
        }
        private Timer _dollFootstepTimer= new Timer(.25f);
        public override void OnEnter()
        {
            //choose new pose for next looked at (with animator likely)
            //set speed to hunting speed
            //unfreeze anything nessesary
            Agent.stoppingDistance = DollSO.StoppingDist;
            Agent.speed = DollSO.RunSpeed;
            Agent.isStopped = false;
            Animator.PlaySwitchPose();
            _dollFootstepTimer.Start();
            if (StateMachine.CurrentPlayerToHunt == null)
            {
                StateMachine.TransitionTo(StateEnum.WanderState);
                return;
            }
            Agent.SetDestination(StateMachine.CurrentPlayerToHunt.position);
        }

        public override void OnExit()
        {
        _dollFootstepTimer.Stop();
        }

        public override void StateFixedUpdate()
        {
            //update destination to be accurate to the currentHuntedPlayer
            if (StateMachine.CurrentPlayerToHunt == null)
            {
                StateMachine.TransitionTo(StateEnum.WanderState);
                return;
            }
            Agent.SetDestination(StateMachine.CurrentPlayerToHunt.position);
            _dollFootstepTimer.TimerUpdate(Time.deltaTime);
            if (_dollFootstepTimer.IsComplete)
            {
                EventBus.Instance.PublishGameplayEvent(new DollFootstepsEvent
                {
                    EventID = EventIDs.EnemyDollFootsteps, EventPosition = StateMachine.transform.position
                });
                _dollFootstepTimer.Reset();
            }
        }

        public override void StateUpdate()
        {
        
        }
        public override void StateLookedAt()
        {
            StateMachine.TransitionTo(StateEnum.LookedAtState);
        }
        
        public override void StateLookedAway(Transform playerToHunt)
        {
            // Already hunting, no need to do anything
        }

        public override void StateNoValidPlayer()
        {
            StateMachine.TransitionTo(StateEnum.WanderState);
        }

        public override void StateAttemptKill()
        {
            StateMachine.RequestKill(StateMachine.CurrentPlayerToHunt.parent.gameObject);
        }
    }
}
