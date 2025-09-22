using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] BeetleLineOfSight _beetleLineOfSight;
    [SerializeField] float hostileCheckFrequency;
    [SerializeField] float fleeDistance = 10f;
    [SerializeField] float randomRunPointOffSet;
    [SerializeField] BeetleSO _beetleSO;
   // [SerializeField] LayerMask navMeshLayerMask;
    bool _followingPlayer = false;
    bool _runFromPlayer;
    Transform playerToFollow;
    Transform currentHostilePlayer;
    BeetleState _beetleState;
    //temp var
    bool doMove = false;
    public void OnDeath()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }
    public void OnKnockout()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        _beetleState = GetComponent<BeetleState>();
    }
    void Start()
    {
       PointToMoveTo = GetNextPosition();
        doMove = true;
        agent.speed = _beetleSO.WalkSpeed;
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
  

    public void RunAwayLogic(GameObject threat)
    {
        Vector3 directionAway = (transform.position - threat.transform.position).normalized;
        Vector3 randomOffset = new Vector3(Random.Range(-randomRunPointOffSet, randomRunPointOffSet), 0, Random.Range(-randomRunPointOffSet, randomRunPointOffSet));
        Vector3 rawFleePosition = transform.position + (directionAway * fleeDistance) + randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawFleePosition, out hit, fleeDistance * 2f, NavMesh.AllAreas))
        {
            GetComponent<NavMeshAgent>().SetDestination(hit.position);
        }
        else
        {
            Debug.Log("No valid NavMesh point found to flee to.");
        }
    }

    public void RunFromPlayer(Transform playerToRunFrom)
    {
        currentHostilePlayer = playerToRunFrom;
        _runFromPlayer = true;
        agent.speed = _beetleSO.RunSpeed;
        StartCoroutine(PeriodicCheckHostiles());
    }
    public void OnStopRunning()
    {
        currentHostilePlayer = null;
        _runFromPlayer = false;
        agent.speed = _beetleSO.WalkSpeed;
        _beetleState.TransitionToState(BeetleStates.Idle);
        StopCoroutine(PeriodicCheckHostiles());
    }
    IEnumerator PeriodicCheckHostiles()
    {
        while(true)
        {
            CheckForHostilePlayers();
            yield return new WaitForSeconds(hostileCheckFrequency);
        }
    }
    public void CheckForHostilePlayers()
    {
        if (!_beetleLineOfSight.CheckForHostiles())
        {
            OnStopRunning();
            //can stop running.
        }
        else
        {
            //keep running
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
    public void OnFollowPlayer()
    {
        _followingPlayer = true;
    }
    public void SetPlayerToFollow(Transform player)
    {
        playerToFollow = player;
    }
    public void OnStopFollow()
    {
        _followingPlayer = false;
        playerToFollow = null;
    }
    public void SetDestinationToPlayer()
    {
        agent.SetDestination(playerToFollow.position);
    }
    // Update is called once per frame
    void Update()
    {
        if (_beetleState.IsEnemyDead() || _beetleState.IsEnemyKnockedout()) return;
        if (_beetleState.GetCurrentState() == BeetleStates.MovePosition)
        {
            if (Vector3.Distance(beetleTransform.position, agent.destination) < stopDistance)
            {
               _beetleState.TransitionToState(BeetleStates.Idle);
            }
        } 
    }
    public void FixedUpdate()
    {
        if (_beetleState.IsEnemyDead() || _beetleState.IsEnemyKnockedout()) return;
        if (_followingPlayer)
        {
            SetDestinationToPlayer();
        }
        if (_runFromPlayer)
        {
            RunAwayLogic(currentHostilePlayer.gameObject);
        }
    }
}
