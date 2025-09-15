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


    BeetleState _beetleState;
    //temp var
    bool doMove = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        _beetleState = GetComponent<BeetleState>();
    }
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
            if(NavMesh.SamplePosition(beetleTransform.position + temp, out NavMeshHit hit, MaxWanderDistance * 3f,NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
            return Vector3.zero;
            }
    }
    public void MoveToPosition(Vector3 position)
    {
        agent.SetDestination(position);
        
    }
    public void StartIdle()
    {
        MoveToPosition(beetleTransform.position);
    }
    // Update is called once per frame
    void Update()
    {
       
        if(_beetleState.GetCurrentState() == BeetleStates.MovePosition)
        {
            if (Vector3.Distance(beetleTransform.position, agent.destination) < stopDistance)
            {
               _beetleState.TransitionToState(BeetleStates.Idle);
            }
        } 
        
        /*if (doMove)
        {
            MoveToPosition();
        }*/
    }
}
