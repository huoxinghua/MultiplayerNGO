using UnityEngine;
using UnityEngine.AI;

public class BeetleMove : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] float MaxWanderDistance;
    [SerializeField] float MinWanderDistance;
    [SerializeField] Transform beetleTransform;
    [SerializeField] Vector3 PointToMoveTo;
    [SerializeField] float stopDistance;

    //temp var
    bool doMove = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       PointToMoveTo = GetNextPosition();
        doMove = true;
    }
    public Vector3 GetNextPosition()
    {
        Vector3 nextPos = Vector3.zero;
        
            Vector3 temp = new Vector3(Random.Range(MinWanderDistance,MaxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(MinWanderDistance, MaxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(MinWanderDistance, MaxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
       // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
            if(NavMesh.SamplePosition(beetleTransform.position + temp, out NavMeshHit hit, MaxWanderDistance * 100,NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
            return Vector3.zero;
            }
    }
    public void MoveToPosition()
    {
        agent.SetDestination(PointToMoveTo);
    }
    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(beetleTransform.position,PointToMoveTo) < stopDistance)
        {
            PointToMoveTo = GetNextPosition();
        }
        if (doMove)
        {
            MoveToPosition();
        }
    }
}
