using UnityEngine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteWanderState : BruteBaseState
    {
    
        public BruteWanderState(BruteStateMachine stateController) : base(stateController)
        {

        }

        public override void OnEnter()
        {
            Animator.PlayNormal();
            Agent.speed = BruteSO.WalkSpeed;
            Agent.updatePosition = false;
            WanderTo();

        }
        public override void OnExit()
        {
            Agent.updatePosition = true;
        }

        public override void StateUpdate()
        {
            var worldVel = Agent.desiredVelocity;
            var localVel = StateController.transform.InverseTransformDirection(worldVel);
            Animator.PlayWalk(localVel.magnitude, Agent.speed);

        }
        public override void StateFixedUpdate()
        {
            if(Vector3.Distance(StateController.gameObject.transform.position,Agent.destination) <= BruteSO.StoppingDist)
            {
                StateController.TransitionTo(StateController.IdleState);
            }
            
        }
        public override void OnHearPlayer()
        {
            StateController.TransitionTo(StateController.BruteHeardPlayerState);
        }

        public override void OnStateAnimatorMove()
        {
            var delta = Animator.GetAnimator().deltaPosition;
            StateController.transform.position += delta;               // capsule follows the clip
            Agent.nextPosition = StateController.transform.position;  // keep agent and capsule in sync
            StateController.transform.rotation = Animator.GetAnimator().rootRotation;
            Agent.nextPosition = StateController.transform.position;
        }
        #region Path Finding

        private void WanderTo()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float randomDist = Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance);

                Vector3 wanderOffset = new Vector3(randomDir.x, 0f, randomDir.y) * randomDist;
                Vector3 targetPoint = StateController.HeartPosition.position + wanderOffset;

                if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, BruteSO.MaxWanderDistance, NavMesh.AllAreas))
                {
                    Agent.SetDestination(hit.position);
                    return;
                }
            }
            // No valid point found after 4 attempts - stay put
        }

        #endregion

    }
}
