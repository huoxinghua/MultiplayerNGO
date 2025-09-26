using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BruteMovement : MonoBehaviour
{
    Transform _heartTransform;
    [SerializeField] BruteSO bruteSO;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] BruteStateController stateController;

    [SerializeField] BruteAnimation _bruteAnimation;
    private float _minWanderDistance => bruteSO.MinWanderDistance;
    private float _maxWanderDistance => bruteSO.MaxWanderDistance;
    private float _walkSpeed => bruteSO.WalkSpeed;
    private float _hurtWalkSpeed => bruteSO.HurtWalkSpeed;
    private float _alertWalkSpeed => bruteSO.AlertWalkSpeed;
    private float _runSpeed => bruteSO.RunSpeed;
    private float _minIdleTime => bruteSO.MinIdleTime;
    private float _maxIdleTime => bruteSO.MaxIdleTime;
    private float _stoppingDistance => bruteSO.StoppingDist;
    private float _loseInterestTimeInvestigate => bruteSO.LoseInterestTimeInvestigate;
    private float _timeSinceHeardPlayer = 0;
    private float _loseInterestTimeChase => bruteSO.LoseInterestTimeChase;
    private float tempSpeedHold;
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
        agent.speed = _hurtWalkSpeed;
    }
    public void OnEnterAlertState()
    {

    }
    public void OnEnterUnawareState()
    {

    }
    public void OnStartChase()
    {
        agent.speed = _runSpeed;
        StopAllCoroutines();
    }
    public void OnStopChase()
    {
        Debug.Log("?");
        if (stateController.GetAttentionState() == BruteAttentionStates.Unaware)
        {
            agent.speed = _walkSpeed;
        }
        if (stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            agent.speed = _hurtWalkSpeed;
        }
        if (stateController.GetAttentionState() == BruteAttentionStates.Alert)
        {
            agent.speed = _alertWalkSpeed;
        }
    }
    //get next wander position in unaware state
    public Vector3 GetNextPosition()
    {
        Vector3 nextPos = Vector3.zero;

        Vector3 temp = new Vector3(Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
        // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
        if (NavMesh.SamplePosition(_heartTransform.position + temp, out NavMeshHit hit, _maxWanderDistance * 3f, NavMesh.AllAreas))
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
    public Vector3 GetNextHurtPosition()
    {
        Vector3 nextPos = Vector3.zero;

        Vector3 temp = new Vector3(Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1), Random.Range(_minWanderDistance, _maxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
        // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
        if (NavMesh.SamplePosition(transform.position + temp, out NavMeshHit hit, _maxWanderDistance * 3f, NavMesh.AllAreas))
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
    public void OnStartIdle()
    {
        agent.SetDestination(transform.position);
        StartCoroutine(IdleTime());
    }
    public void OnStartWander()
    {

        if (stateController.GetAttentionState() == BruteAttentionStates.Unaware)
        {
            Vector3 newPos = GetNextPosition();
            if (newPos == Vector3.zero)
            {
                OnStartWander();
                return;
            }
            agent.SetDestination(newPos);
            agent.speed = _walkSpeed;
        }
        else if (stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            Vector3 newPos = GetNextHurtPosition();
            if (newPos == Vector3.zero)
            {
                OnStartWander();
                return;
            }
            agent.SetDestination(newPos);
            agent.speed = _hurtWalkSpeed;
        }
        //get the actual walking distance. Must implement still
        //float pathLength = GetPathLength(agent, agent.destination);
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
        yield return new WaitForSeconds(randTime);
        stateController.TransitionToBehaviourState(BruteBehaviourStates.Wander);
    }

    public void OnInvestigate(GameObject positionToInvestigate)
    {
        StopAllCoroutines();
        agent.SetDestination(positionToInvestigate.transform.position);
        agent.speed = _alertWalkSpeed;
        _timeSinceHeardPlayer = 0;
    }
    public void OnHearInChase()
    {
        _timeSinceHeardPlayer = 0;
    }
    public void StopForAttack()
    {
        agent.velocity = Vector3.zero;
        tempSpeedHold = agent.speed;
        agent.speed = 0;
        Debug.Log("Stop");
    }
    public void ResumeAfterAttack()
    {
        Debug.Log("Resume");
        agent.speed = tempSpeedHold;
    }
    public void OnDeathKO()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (stateController.GetAttentionState() == BruteAttentionStates.Dead) return;
        if (stateController.GetAttentionState() == BruteAttentionStates.KnockedOut) return;
        if (stateController.GetAttentionState() == BruteAttentionStates.Unaware
            || stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            if (stateController.GetBehaviourState() == BruteBehaviourStates.Wander
                && Vector3.Distance(transform.position, agent.destination) < _stoppingDistance)
            {
                stateController.TransitionToBehaviourState(BruteBehaviourStates.Idle);
            }
        }
        if (stateController.GetAttentionState() == BruteAttentionStates.Alert
            && stateController.GetBehaviourState() == BruteBehaviourStates.Chase)
        {
            agent.SetDestination(stateController.PlayerToChase.transform.position);
        }
        if (stateController.GetAttentionState() == BruteAttentionStates.Alert)
        {
            _timeSinceHeardPlayer += Time.deltaTime;
            if (stateController.GetBehaviourState() == BruteBehaviourStates.Investigate
                && _timeSinceHeardPlayer >= _loseInterestTimeInvestigate)
            {
                stateController.TransitionToAttentionState(BruteAttentionStates.Unaware);
                stateController.TransitionToBehaviourState(BruteBehaviourStates.Idle);
            }
            else if (stateController.GetBehaviourState() == BruteBehaviourStates.Chase
                && _timeSinceHeardPlayer >= _loseInterestTimeChase)
            {
                stateController.TransitionToAttentionState(BruteAttentionStates.Unaware);
                stateController.TransitionToBehaviourState(BruteBehaviourStates.Idle);
            }
        }


        //animation stuff

        if (stateController.GetAttentionState() == BruteAttentionStates.Alert)
        {
            if (stateController.GetBehaviourState() == BruteBehaviourStates.Investigate)
            {
                _bruteAnimation.PlayWalk(agent.velocity.magnitude, agent.speed);
            }
            else if (stateController.GetBehaviourState() == BruteBehaviourStates.Chase)
            {
                _bruteAnimation.PlayRun(agent.velocity.magnitude, agent.speed);
            }

        }
        if (stateController.GetAttentionState() == BruteAttentionStates.Unaware || stateController.GetAttentionState() == BruteAttentionStates.Hurt)
        {
            _bruteAnimation.PlayWalk(agent.velocity.magnitude, agent.speed); ;
        }




    }
}
