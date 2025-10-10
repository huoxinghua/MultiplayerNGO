    using UnityEngine;
using UnityEngine.AI;

public class BruteStateMachine : BaseStateController
{
    protected BruteBaseState currentState;
    protected BruteBaseState stateBeforeAttack;
    public BruteIdleState idleState { get; private set; }
    public BruteWanderState wanderState { get; private set; }
    public BruteHurtIdleState bruteHurtIdleState { get; private set; }
    public BruteHurtWander bruteHurtWander { get; private set; }
    public BruteAlertState bruteAlertState { get; private set; }
    public BruteChaseState bruteChaseState { get; private set; }
    public BruteAttackState BruteAttackState { get; private set; }
    public BruteHeardPlayerState BruteHeardPlayerState { get; private set; }
    public BruteDeadState BruteDeadState { get; private set; }
    public BruteHitState BruteHitState { get; private set; }
    public BruteAnimation animator { get; private set; }
    public NavMeshAgent agent { get; private set; }
    [SerializeField] public BruteSO BruteSO;
    public Transform HeartPosition { get; private set; }
    [SerializeField] GameObject _heartPrefab;
    private GameObject _spawnedHeart;
    public GameObject lastHeardPlayer { get; private set; }
    public GameObject PlayerToAttack { get; private set; }
    public int TimesAlerted = 0;
    public void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<BruteAnimation>();
        HandleHeartSpawn();
        idleState = new BruteIdleState(this);
        wanderState = new BruteWanderState(this);
        bruteHurtIdleState = new BruteHurtIdleState(this);
        bruteHurtWander = new BruteHurtWander(this);
        bruteAlertState = new BruteAlertState(this);
        bruteChaseState = new BruteChaseState(this);
        BruteAttackState = new BruteAttackState(this);
        BruteHeardPlayerState = new BruteHeardPlayerState(this);
        BruteDeadState = new BruteDeadState(this);
        BruteHitState = new BruteHitState(this);
       
    }
    public void HandleHeartSpawn()
    {
        _spawnedHeart = Instantiate(_heartPrefab, transform);
        _spawnedHeart.GetComponent<BruteHeart>()?.SetStateController(this);
        _spawnedHeart.transform.SetParent(null);
        HeartPosition = transform;
    }
    public void Start()
    {
        TransitionTo(wanderState);
    }
    void Update()
    {
        currentState?.StateUpdate();
    }
    void FixedUpdate()
    {
        currentState?.StateFixedUpdate();
    }
    public void OnHearPlayer(GameObject playerObj)
    {

        if (playerObj == null){ Debug.Log("LeavingEarly"); return; }
      //  Debug.Log(playerObj.name);
        lastHeardPlayer = playerObj;
        if (Vector3.Distance(playerObj.transform.position,transform.position) <= BruteSO.InstantAggroDistance)
        {
            if(currentState == bruteChaseState)
            {
                currentState.OnHearPlayer();
            }
            else
            {
                TransitionTo(bruteChaseState);
            }
            return;
        }
        else
        {
            currentState?.OnHearPlayer();
        }
    }
    public void HandleDefendHeart(GameObject attackingPlayer)
    {
        lastHeardPlayer = attackingPlayer;
        TransitionTo(bruteChaseState);
    }
    public void OnAttack(GameObject playerToAttack) 
    {
        stateBeforeAttack = currentState;
        PlayerToAttack = playerToAttack;
        TransitionTo(BruteAttackState);
    }
    public void OnDeath()
    {
        //enter death state
        //TransitionTo();
    }
    public void OnHeartDestroyed()
    {
        TransitionTo(bruteHurtIdleState);
    }
    public void OnAttackEnd()
    {
        TransitionTo(stateBeforeAttack);
    }
    public void OnAttackConnects()
    {
        if(Vector3.Distance(transform.position,PlayerToAttack.transform.position) <= BruteSO.AttackDistance)
        {
            PlayerToAttack.GetComponent<IPlayerHealth>().TakeDamage(BruteSO.Damage);
        }
    }
    public virtual void TransitionTo(BruteBaseState newState)
    {
        if (newState == currentState) return;
        currentState?.OnExit();
        Debug.Log($"Last State: {currentState?.ToString()} nextState: {newState.ToString()}");
        currentState = newState;
        currentState.OnEnter();
    }
}
