using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Utilities.StateMachine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollBaseState : BaseState
    {
        protected DollStateMachine StateController;
        protected DollAnimation Animator;
        protected NavMeshAgent Agent;
        // protected DollSO DollSO; (Needed)
        public DollBaseState(DollStateMachine stateMachine)
        {
            this.StateController = stateMachine;
            Animator = stateMachine.Animator;
            Agent = stateMachine.Agent;
            //  DollSO = stateController.DollSO;
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
