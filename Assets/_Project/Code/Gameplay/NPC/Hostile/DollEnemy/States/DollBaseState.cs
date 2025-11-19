using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Utilities.StateMachine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollBaseState : BaseState
    {
        protected DollStateMachine StateMachine;
        protected DollAnimation Animator;
        protected NavMeshAgent Agent;
        protected DollSO DollSO;
        public DollBaseState(DollStateMachine stateMachine)
        {
            this.StateMachine = stateMachine;
            Animator = stateMachine.Animator;
            Agent = stateMachine.Agent;
            DollSO = stateMachine.DollSO;
        }
        public override void OnEnter()
        {
        
        }

        public override void OnExit()
        {
        
        }

        public override void StateFixedUpdate()
        {
        
        }

        public override void StateUpdate()
        {
        
        }
    }
}
