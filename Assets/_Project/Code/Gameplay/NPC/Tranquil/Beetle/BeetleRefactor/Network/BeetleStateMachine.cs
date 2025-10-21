using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

namespace _Project.Code.Core.GamePlay.AI.NetWork
{

public class BeetleStateMachine : BaseStateController
{
    protected BeetleBaseState CurrentState;
    [field: SerializeField] public BeetleSO BeetleSO { get; private set; }
    public BeetleAnimation Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    //states
    public BeetleIdleState IdleState { get; private set; }
    public BeetleWanderState WanderState { get; private set; }
    public BeetleFollowState FollowState { get; private set; }
    public BeetleRunState RunState { get; private set; }

    public GameObject PlayerToRunFrom { get; private set; }
    public GameObject PlayerToFollow { get; private set; }

    public Timer FollowCooldown { get; private set; }
    public bool IsFirstFollow { get; set; }
    [SerializeField] private Ragdoll _ragdollScript;
    [SerializeField] private BeetleDead BeetleDeadScript;
 



        public void Awake()
    {
        IsFirstFollow = true;
        FollowCooldown = new Timer(BeetleSO.FollowCooldown);
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<BeetleAnimation>();
        IdleState = new BeetleIdleState(this);
        WanderState = new BeetleWanderState(this);
        FollowState = new BeetleFollowState(this);
        RunState = new BeetleRunState(this);
    }
    public void Start()
    {

        if (!IsServer) return;
        Debug.Log("is server start");
        TransitionTo(WanderState);
    }


        void Update()
    {
        if (!IsServer) return;
        FollowCooldown.TimerUpdate(Time.deltaTime);
        CurrentState?.StateUpdate();
    }
    void FixedUpdate()
    {
        if (!IsServer) return;
        CurrentState?.StateFixedUpdate();
    }
    public void TransitionTo(BeetleBaseState newState)
    {
        if (newState == CurrentState) return;
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }
    public void HandleFollowPlayer(GameObject playerToFollow)
    {
        PlayerToFollow = playerToFollow;
        CurrentState.OnSpotPlayer(false);
    }
    public void HandleRunFromPlayer(GameObject playerToRunFrom)
    {
        PlayerToRunFrom = playerToRunFrom;
        CurrentState.OnSpotPlayer(true);
    }
    public void HandleKnockedOut()
    {
        BeetleDeadScript.enabled = true;
        transform.GetChild(1).parent = null;
        _ragdollScript.EnableRagdoll();
        Destroy(transform.GetChild(0).gameObject);
        Destroy(gameObject);
    }
    /*
    public void HandleDeath()
    {
        BeetleDeadScript.enabled = true;
        transform.GetChild(1).parent = null;
        _ragdollScript.EnableRagdoll();
        Destroy(transform.GetChild(0).gameObject);
        Destroy(gameObject);
    }*/
    public void HandleHitByPlayer(GameObject player)
    {
        Debug.Log("HandleHitByPlayer");
        PlayerToRunFrom = player;
        CurrentState.OnHitByPlayer();
    }
    #region network
    [ClientRpc]
    private void PlayDeathClientRpc()
    {
        Debug.Log("PlayDeathClientRpc");
        BeetleDeadScript.enabled = true;
        transform.GetChild(1).parent = null;
        _ragdollScript.EnableRagdoll();
        Destroy(transform.GetChild(0).gameObject);
    }

    public void HandleDeath()
    {
        if (!IsServer) return;
        Debug.Log("PlayDeathClientRpc");
        PlayDeathClientRpc();
            // Destroy(gameObject, 2f);
        StartCoroutine(DespawnAfterSeconds(GetComponent<NetworkObject>(), 2f));
    }

    private IEnumerator DespawnAfterSeconds(NetworkObject netObj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
    }
        #endregion
    }
}