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
            Debug.Log("Wander");
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
          //  Debug.Log($"Agent vel : {Agent.desiredVelocity}");
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
//            Debug.Log("OnStateAnimatorMove");
            var delta = Animator.GetAnimator().deltaPosition;
            StateController.transform.position += delta;               // capsule follows the clip
            Agent.nextPosition = StateController.transform.position;  // keep agent and capsule in sync
            StateController.transform.rotation = Animator.GetAnimator().rootRotation;
            Agent.nextPosition = StateController.transform.position;
        }
        #region Path Finding
        private Vector3 GetRandomPosition()
        {
            int RandNegativeX = (Random.Range(0, 2) * 2 - 1);
            int RandNegativeY = (Random.Range(0, 2) * 2 - 1);
            int RandNegativeZ = (Random.Range(0, 2) * 2 - 1);
            return new Vector3(Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance)
                               * (RandNegativeX), Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance) *
                                                  (RandNegativeY), Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance) * (RandNegativeZ));
        }
        private Vector3 GetNextPosition()
        {
            Vector3 nextPos = Vector3.zero;

            Vector3 temp = GetRandomPosition();
            // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
            if (NavMesh.SamplePosition(StateController.HeartPosition.position + temp, out NavMeshHit hit, BruteSO.MaxWanderDistance * 3f, NavMesh.AllAreas))
            {
                if (GetPathLength(Agent, hit.position) == -1)
                {
                    return Vector3.zero;
                }
                return hit.position;
            }
            else
            {
                return Vector3.zero;
            }
        }
        private float GetPathLength(NavMeshAgent navAgent, Vector3 targetPosition)
        {
            NavMeshPath path = new NavMeshPath();
            if (navAgent.CalculatePath(targetPosition, path))
            {
                float length = 0.0f;

                if (path.corners.Length < 2)
                    return 0;

                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }

                return length;
            }

            return -1f; // Invalid path
        }
        private void WanderTo()
        {
            Vector3 newPos = GetNextPosition();
            if (newPos == Vector3.zero)
            {
                WanderTo();
                return;
            }
            Agent.SetDestination(newPos);
        }
        #endregion

    }
}
