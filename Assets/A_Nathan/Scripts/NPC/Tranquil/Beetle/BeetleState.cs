using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
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
    [SerializeField] GameObject _beetleSkel;
    [SerializeField] BeetleDead _beetleDead;
    [SerializeField] float _minNoiseTime;
    [SerializeField] float _maxNoiseTime;
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
                beetleMoveScript.OnWander();
                break;
            case BeetleStates.RunAway:
                StopCoroutine(FollowTime());
                StopCoroutine(RandomNoises());
                AudioManager.Instance.PlayByKey3D("BeetleSqueak", transform.position);
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
                StartCoroutine(RandomNoises());
                break;
            case BeetleStates.FollowPlayer:
                StartCoroutine(FollowTime());
                break;
            case BeetleStates.KnockedOut:
                StopCoroutine(RandomNoises());
                beetleMoveScript.OnKnockout();
                OnKnockOut();
                break;
            case BeetleStates.Dead:

                StopCoroutine(RandomNoises());
               
                beetleMoveScript.OnDeath();
                beetleLineOfSight.OnDeath();
                OnDeath();
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
    IEnumerator RandomNoises()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minNoiseTime,_maxNoiseTime));
            int index = Random.Range(0, 3);
            switch (index)
            {
                case 0:
                    AudioManager.Instance.PlayByKey3D("BeetleBugNoise1", transform.position);
                    break;
                case 1:
                    AudioManager.Instance.PlayByKey3D("BeetleBugNoise2", transform.position);
                    break;
                case 2:
                    AudioManager.Instance.PlayByKey3D("BeetleBugNoise3", transform.position);
                    break;

            }
        }
    }
    void OnDeath()
    {
        StopAllCoroutines();
        _ragdollScript.EnableRagdoll();
        _beetleSkel.transform.parent = null;
        _beetleDead.enabled = true;
        Destroy(gameObject);
    }
    void OnKnockOut()
    {
        StopAllCoroutines();
        _ragdollScript.EnableRagdoll();
        _beetleSkel.transform.parent = null;
        _beetleDead.enabled = true;
        Destroy(gameObject);
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
