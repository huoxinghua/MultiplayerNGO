using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
public enum BeetleStates
{
    Idle,
    MovePosition,
    RunAway,
    FollowPlayer,
    KnockedOut,
    Dead
}
public class BeetleState : MonoBehaviour
{
    BeetleStates _currentState;
    BeetleMove beetleMoveScript;
    BeetleLineOfSight beetleLineOfSight;
    [SerializeField] float _minIdleTime;
    [SerializeField] float _maxIdleTime;
    [SerializeField] float _minFollowTime;
    [SerializeField] float _maxFollowTime;
    [SerializeField] float _followCooldown;
    [SerializeField] BeetleAnimation _beetleAnimation;
    [SerializeField] Ragdoll _ragdollScript;
    bool _onFollowCooldown;
    bool _isFollowing;
    public void Awake()
    {
       // _currentState = BeetleStates.MovePosition;
        beetleMoveScript = GetComponent<BeetleMove>();
        beetleLineOfSight = GetComponent<BeetleLineOfSight>();
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
        if (_currentState == newState || _currentState == BeetleStates.Dead) return;

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
            case BeetleStates.KnockedOut:
                OnKnockOut();
                break;
            case BeetleStates.Dead:
                OnDeath();
                beetleMoveScript.OnDeath();
                beetleLineOfSight.OnDeath();
                break;
        }
    }
    public bool IsEnemyDead()
    {
        if (_currentState == BeetleStates.Dead) return true;
        else return false;
    }
    public bool IsEnemyKnockedout()
    {
        if(_currentState == BeetleStates.KnockedOut)return true;
        else return false;
    }
    void OnDeath()
    {
        StopAllCoroutines();
        _ragdollScript.EnableRagdoll();
    }
    void OnKnockOut()
    {
        StopAllCoroutines();
        _ragdollScript.EnableRagdoll();
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
        float randTime = Random.Range(_minIdleTime, _maxIdleTime);
        _beetleAnimation.PlayRandomIdle(0, 1);
        Debug.Log(randTime);
        float timeDelayAnim = Random.Range(1,randTime-4);

        Debug.Log(timeDelayAnim);
        yield return new WaitForSeconds(timeDelayAnim);
        Debug.Log(randTime - timeDelayAnim);
        randTime -= timeDelayAnim;
        _beetleAnimation.PlayRandomIdle(1, 0);
        Debug.Log(randTime);
        yield return new WaitForSeconds(randTime);
        if(_currentState == BeetleStates.Idle)
        {
            Debug.Log("StartMoving");
            TransitionToState(BeetleStates.MovePosition);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  

    // Update is called once per frame
    void Update()
    {
  
    }
}
