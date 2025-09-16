using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BeetleStates
{
    Idle,
    MovePosition,
    RunAway,
    FollowPlayer
}
public class BeetleState : MonoBehaviour
{
    BeetleStates _currentState;
    BeetleMove beetleMoveScript;
    [SerializeField] float _minIdleTime;
    [SerializeField] float _maxIdleTime;
    [SerializeField] float _minFollowTime;
    [SerializeField] float _maxFollowTime;
    [SerializeField] float _followCooldown;
    bool _onFollowCooldown;
    bool _isFollowing;
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
                StopCoroutine(FollowTime());
                if (_isFollowing)
                {
                    StartCoroutine(FollowCooldown());
                    _isFollowing = false;
                }
                StopCoroutine(IdleTime());
                beetleMoveScript.OnStopFollow();
                Debug.Log("Start To Run");
                break;
            case BeetleStates.Idle:
                beetleMoveScript.StartIdle();
                StartCoroutine(IdleTime());
                break;
            case BeetleStates.FollowPlayer:
                StartCoroutine(FollowTime());
                break;
        }
    }
    IEnumerator FollowCooldown()
    {
        yield return new WaitForSeconds(_followCooldown);
        _onFollowCooldown = false;
    }
    IEnumerator FollowTime()
    {
        _onFollowCooldown = true;
        _isFollowing = true;
        beetleMoveScript.OnFollowPlayer();
        yield return new WaitForSeconds(Random.Range(_minFollowTime, _maxFollowTime));
        _isFollowing = false;
        beetleMoveScript.OnStopFollow();
        TransitionToState(BeetleStates.MovePosition);
        StartCoroutine(FollowCooldown());
    }
    public bool GetFollowCooldown()
    {
        return _onFollowCooldown;
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
