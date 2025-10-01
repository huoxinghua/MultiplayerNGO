using UnityEngine;
using UnityEngine.AI;

public class BruteWanderState : BruteBaseState
{
    public BruteWanderState(BruteStateMachine stateController) : base(stateController)
    {

    }
    Vector3 GetNextPosition()
    {
        Vector3 nextPos = Vector3.zero;

        Vector3 temp = new Vector3(Random.Range(bruteSO.MinWanderDistance, bruteSO.MaxWanderDistance)
            * (Random.Range(0, 2) * 2 - 1), Random.Range(bruteSO.MinWanderDistance, bruteSO.MaxWanderDistance) * 
            (Random.Range(0, 2) * 2 - 1), Random.Range(bruteSO.MinWanderDistance, bruteSO.MaxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
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
    float GetPathLength(NavMeshAgent navAgent, Vector3 targetPosition)
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
    void WanderTo()
    {
        Vector3 newPos = GetNextPosition();
        if (newPos == Vector3.zero)
        {
            WanderTo();
            return;
        }
        agent.SetDestination(newPos);
    } 
    public override void OnEnter()
    {
        agent.speed = bruteSO.WalkSpeed;
        WanderTo();
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
    }
    public override void OnHearPlayer()
    {
    }
}
