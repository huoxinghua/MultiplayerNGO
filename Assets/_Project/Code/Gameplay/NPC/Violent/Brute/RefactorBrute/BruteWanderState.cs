using UnityEngine;
using UnityEngine.AI;

public class BruteWanderState : BruteBaseState
{
    
    public BruteWanderState(BruteStateMachine stateController) : base(stateController)
    {

    }

    public override void OnEnter()
    {
        animator.PlayNormal();
        agent.speed = bruteSO.WalkSpeed;
        WanderTo();
        Debug.Log("Wander");
    }
    public override void OnExit()
    {

    }

    public override void StateUpdate()
    {
        
    }
    public override void StateFixedUpdate()
    {
        if(Vector3.Distance(stateController.gameObject.transform.position,agent.destination) <= agent.stoppingDistance)
        {
            stateController.TransitionTo(stateController.idleState);
        }
        animator.PlayWalk(agent.velocity.magnitude, agent.speed);
    }
    public override void OnHearPlayer()
    {
        stateController.TransitionTo(stateController.BruteHeardPlayerState);
    }


    #region Path Finding
    private Vector3 GetRandomPosition()
    {
        int RandNegativeX = (Random.Range(0, 2) * 2 - 1);
        int RandNegativeY = (Random.Range(0, 2) * 2 - 1);
        int RandNegativeZ = (Random.Range(0, 2) * 2 - 1);
        return new Vector3(Random.Range(bruteSO.MinWanderDistance, bruteSO.MaxWanderDistance)
            * (RandNegativeX), Random.Range(bruteSO.MinWanderDistance, bruteSO.MaxWanderDistance) *
            (RandNegativeY), Random.Range(bruteSO.MinWanderDistance, bruteSO.MaxWanderDistance) * (RandNegativeZ));
    }
    private Vector3 GetNextPosition()
    {
        Vector3 nextPos = Vector3.zero;

        Vector3 temp = GetRandomPosition();
        // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
        if (NavMesh.SamplePosition(stateController.HeartPosition.position + temp, out NavMeshHit hit, bruteSO.MaxWanderDistance * 3f, NavMesh.AllAreas))
        {
            if (GetPathLength(agent, hit.position) == -1)
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
        agent.SetDestination(newPos);
    }
    #endregion

}
