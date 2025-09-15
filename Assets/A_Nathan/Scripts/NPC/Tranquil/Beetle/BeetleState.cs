using System.Collections;
using UnityEngine;
public enum BeetleStates
{
    Idle,
    MovePosition,
    RunAway
}
public class BeetleState : MonoBehaviour
{
    BeetleStates _currentState;
    BeetleMove beetleMoveScript;
    [SerializeField] float _minIdleTime;
    [SerializeField] float _maxIdleTime;
    public void Awake()
    {
       // _currentState = BeetleStates.MovePosition;
        beetleMoveScript = GetComponent<BeetleMove>();
    }
    void Start()
    {
        TransitionToState(BeetleStates.MovePosition);
    }
    public BeetleStates GetCurrentState()
    {
        return _currentState;
    }
    public void SetCurrentState(BeetleStates state)
    {
        _currentState=state;
    }
 
    public void TransitionToState(BeetleStates newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        OnEnterState(newState);
    }
    public void OnEnterState(BeetleStates state)
    {
        switch (state)
        {
            case BeetleStates.MovePosition:
                beetleMoveScript.MoveToPosition(beetleMoveScript.GetNextPosition());
                break;
            case BeetleStates.RunAway:
                Debug.Log("Start To Run");
                break;
            case BeetleStates.Idle:
                beetleMoveScript.StartIdle();
                StartCoroutine(IdleTime());
                break;
        }
    }
    IEnumerator IdleTime()
    {
        yield return new WaitForSeconds(Random.Range(_minIdleTime, _maxIdleTime));
        if(_currentState == BeetleStates.Idle)
        {
            TransitionToState(BeetleStates.MovePosition);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  

    // Update is called once per frame
    void Update()
    {
  
    }
}
