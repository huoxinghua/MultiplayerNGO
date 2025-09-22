using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BruteMovement : MonoBehaviour
{
    Transform _heartTransform;
    [SerializeField] BruteSO bruteSO;
    [SerializeField] NavMeshAgent agent;
    private float _minWanderDistance => bruteSO.MinWanderDistance;
    private float _maxWanderDistance => bruteSO.MaxWanderDistance;
    private float _walkSpeed => bruteSO.WalkSpeed;
    private float _runSpeed => bruteSO.RunSpeed;
    private float _minIdleTime;
    private float _maxIdleTime;
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
    public void OnStartIdle()
    {
        StartCoroutine(IdleTime());
    }
    public void OnStartWander()
    {

    }
    IEnumerator IdleTime()
    {
        yield return new WaitForSeconds(Random.Range(_minIdleTime, _maxIdleTime));
    }
    // Update is called once per frame
    void Update()
    {

    }
}
