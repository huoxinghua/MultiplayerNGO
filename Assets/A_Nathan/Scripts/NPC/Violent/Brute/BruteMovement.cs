using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BruteMovement : MonoBehaviour
{
    Transform _heartTransform;
    [SerializeField] BruteSO bruteSO;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] BruteStateController stateController;
    private float _minWanderDistance => bruteSO.MinWanderDistance;
    private float _maxWanderDistance => bruteSO.MaxWanderDistance;
    private float _walkSpeed => bruteSO.WalkSpeed;
    private float _hurtWalkSpeed => bruteSO.HurtWalkSpeed;
    private float _runSpeed => bruteSO.RunSpeed;
    private float _minIdleTime => bruteSO.MinIdleTime;
    private float _maxIdleTime => bruteSO.MaxIdleTime;
    private float _stoppingDistance => bruteSO.StoppingDist;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        agent.speed = _walkSpeed;
    }
    void Start()
    {

    }
    public void SetHeartTransform(Transform heart)
    {
        _heartTransform = heart;
    }
    public void OnEnterHurtState()
    {
        _heartTransform = null;
    }
    public void OnEnterAlertState()
    {

    }
    public void OnEnterUnawareState()
    {

    }
    //get next wander position in unaware state
    public Vector3 GetNextPosition()
    {
        Vector3 nextPos = Vector3.zero;

        Vector3 temp = new Vector3(Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
        // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
        if (NavMesh.SamplePosition(_heartTransform.position + temp, out NavMeshHit hit, _maxWanderDistance * 3f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public Vector3 GetNextHurtPosition()
    {
        Vector3 nextPos = Vector3.zero;

        Vector3 temp = new Vector3(Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
        // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
        if (NavMesh.SamplePosition(transform.position + temp, out NavMeshHit hit, _maxWanderDistance * 3f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public void OnStartIdle()
    {
        StartCoroutine(IdleTime());
    }
    public void OnStartWander()
    {
        
        if(stateController.GetAttentionState() == BruteAttentionStates.Unaware)
        {
            agent.SetDestination(GetNextPosition());
            agent.speed = _walkSpeed;
        }
        else if(stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            agent.SetDestination(GetNextHurtPosition());
            agent.speed = _hurtWalkSpeed;
        }
     //get the actual walking distance. Must implement still
        float pathLength = GetPathLength(agent, agent.destination);
        // agent.isStopped = false;
    }
    //Function to return the travel path of agent. Not the straight line dist
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
    IEnumerator IdleTime()
    {
        //agent.isStopped = true;
        float randTime = Random.Range(_minIdleTime, _maxIdleTime);
        Debug.Log(randTime);
        yield return new WaitForSeconds(randTime);
        stateController.TransitionToBehaviourState(BruteBehaviourStates.Wander);
    }
    // Update is called once per frame
    void Update()
    {
        if(stateController.GetAttentionState() == BruteAttentionStates.Unaware || stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            if(stateController.GetBehaviourState() == BruteBehaviourStates.Wander && Vector3.Distance(transform.position,agent.destination) < _stoppingDistance)
            { 
               stateController.TransitionToBehaviourState(BruteBehaviourStates.Idle);
            }
        } 
    }
}
