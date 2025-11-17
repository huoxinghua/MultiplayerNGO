using Unity.Netcode;
using UnityEngine;

/// <summary>
/// State types for this enemy.
/// Add/remove states as needed for your enemy.
/// </summary>
public enum TemplateEnemyState
{
    Idle = 0,
    Patrol = 1,
    Chase = 2,
    Attack = 3,
    Flee = 4,
    Dead = 5
}

/// <summary>
/// Template for a networked enemy AI with state machine synchronization.
/// Replace "TemplateEnemy" with your enemy name (e.g., Beetle, Brute).
/// </summary>
public class TemplateEnemyAI : NetworkBehaviour
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    [Header("AI Properties")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("State Objects")]
    [SerializeField] private TemplateIdleState idleState;
    [SerializeField] private TemplatePatrolState patrolState;
    [SerializeField] private TemplateChaseState chaseState;
    [SerializeField] private TemplateAttackState attackState;
    [SerializeField] private TemplateFleeState fleeState;
    [SerializeField] private TemplateDeadState deadState;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private UnityEngine.AI.NavMeshAgent navAgent;

    // ============================================================
    // NETWORKED STATE
    // ============================================================

    /// <summary>
    /// Current AI state, synced across network.
    /// </summary>
    private NetworkVariable<TemplateEnemyState> netState = new NetworkVariable<TemplateEnemyState>(
        TemplateEnemyState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Target player (if any).
    /// </summary>
    private NetworkVariable<ulong> targetClientId = new NetworkVariable<ulong>(
        999999,  // Invalid ID = no target
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ============================================================
    // LOCAL STATE
    // ============================================================

    private IEnemyState currentState;
    private IEnemyState[] states;
    private Transform currentTarget;

    // ============================================================
    // PROPERTIES
    // ============================================================

    public TemplateEnemyState CurrentState => netState.Value;
    public Transform Target => currentTarget;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float MoveSpeed => moveSpeed;
    public float ChaseSpeed => chaseSpeed;
    public UnityEngine.AI.NavMeshAgent NavAgent => navAgent;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        // Initialize state array
        states = new IEnemyState[]
        {
            idleState,
            patrolState,
            chaseState,
            attackState,
            fleeState,
            deadState
        };

        // Initialize each state
        foreach (var state in states)
        {
            if (state != null)
                state.Initialize(this);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to state changes on ALL clients
        netState.OnValueChanged += OnStateChanged;
        targetClientId.OnValueChanged += OnTargetChanged;

        // Initialize to current state
        if (IsServer)
        {
            // Server: Start in Patrol state
            TransitionTo(TemplateEnemyState.Patrol);
        }
        else
        {
            // Clients: Apply synced state
            ApplyState(netState.Value);
        }

        Debug.Log($"Enemy AI initialized. IsServer={IsServer}, State={netState.Value}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe
        netState.OnValueChanged -= OnStateChanged;
        targetClientId.OnValueChanged -= OnTargetChanged;
    }

    private void Update()
    {
        // CRITICAL: Only server runs AI logic
        if (!IsServer) return;

        // Update current state AI
        currentState?.Update();
    }

    // ============================================================
    // STATE MANAGEMENT
    // ============================================================

    /// <summary>
    /// Transition to a new state. Only call on server!
    /// </summary>
    public void TransitionTo(TemplateEnemyState newState)
    {
        if (!IsServer)
        {
            Debug.LogError("TransitionTo should only be called on server!");
            return;
        }

        if (netState.Value == newState)
        {
            return;  // Already in this state
        }

        Debug.Log($"Server transitioning to {newState}");

        // Set NetworkVariable (triggers OnStateChanged on all clients)
        netState.Value = newState;
    }

    private void OnStateChanged(TemplateEnemyState oldState, TemplateEnemyState newState)
    {
        Debug.Log($"State changed: {oldState} -> {newState} (IsServer={IsServer})");

        // Apply state on all clients (server included)
        ApplyState(newState);
    }

    /// <summary>
    /// Apply state locally. Runs on server and clients.
    /// </summary>
    private void ApplyState(TemplateEnemyState stateType)
    {
        // Exit current state
        currentState?.Exit();

        // Get new state object
        currentState = states[(int)stateType];

        // Enter new state
        currentState?.Enter();

        // Update animations on all clients
        UpdateAnimation(stateType);
    }

    // ============================================================
    // TARGET MANAGEMENT
    // ============================================================

    /// <summary>
    /// Set target player. Only call on server!
    /// </summary>
    public void SetTarget(NetworkObject targetNetObj)
    {
        if (!IsServer) return;

        if (targetNetObj != null)
        {
            targetClientId.Value = targetNetObj.OwnerClientId;
        }
        else
        {
            targetClientId.Value = 999999;  // Clear target
        }
    }

    /// <summary>
    /// Clear target. Only call on server!
    /// </summary>
    public void ClearTarget()
    {
        if (!IsServer) return;

        targetClientId.Value = 999999;
    }

    private void OnTargetChanged(ulong oldId, ulong newId)
    {
        // Update local target reference on all clients
        if (newId == 999999)
        {
            currentTarget = null;
            Debug.Log("Target cleared");
        }
        else
        {
            if (NetworkManager.ConnectedClients.TryGetValue(newId, out NetworkClient client))
            {
                currentTarget = client.PlayerObject.transform;
                Debug.Log($"Target set to client {newId}");
            }
        }
    }

    // ============================================================
    // PERCEPTION (Server-Only)
    // ============================================================

    /// <summary>
    /// Find nearest player within detection range.
    /// Only call on server!
    /// </summary>
    public Transform FindNearestPlayer()
    {
        if (!IsServer) return null;

        Transform nearest = null;
        float nearestDist = detectionRange;

        foreach (var client in NetworkManager.ConnectedClients.Values)
        {
            if (client.PlayerObject == null) continue;

            Transform playerTransform = client.PlayerObject.transform;
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = playerTransform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Check if player is in line of sight.
    /// Only call on server!
    /// </summary>
    public bool CanSeePlayer(Transform player)
    {
        if (!IsServer) return false;
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > detectionRange) return false;

        // Raycast to check line of sight
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, distance))
        {
            if (hit.transform == player)
                return true;
        }

        return false;
    }

    // ============================================================
    // ANIMATION
    // ============================================================

    /// <summary>
    /// Update animator based on state. Runs on all clients.
    /// </summary>
    private void UpdateAnimation(TemplateEnemyState stateType)
    {
        if (animator == null)
        {
            Debug.LogWarning("Enemy has no animator!");
            return;
        }

        // Set animator parameters based on state
        switch (stateType)
        {
            case TemplateEnemyState.Idle:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", false);
                animator.SetBool("IsDead", false);
                break;

            case TemplateEnemyState.Patrol:
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", false);
                break;

            case TemplateEnemyState.Chase:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsAttacking", false);
                break;

            case TemplateEnemyState.Attack:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", true);
                animator.SetTrigger("Attack");
                break;

            case TemplateEnemyState.Flee:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsAttacking", false);
                break;

            case TemplateEnemyState.Dead:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", false);
                animator.SetBool("IsDead", true);
                animator.SetTrigger("Die");
                break;
        }

        Debug.Log($"Animation updated to {stateType}");
    }

    // ============================================================
    // DEBUG
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw line to target
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, currentTarget.position + Vector3.up);
        }
    }
}

// ============================================================
// STATE INTERFACE
// ============================================================

/// <summary>
/// Interface for enemy AI states.
/// </summary>
public interface IEnemyState
{
    void Initialize(TemplateEnemyAI ai);
    void Enter();
    void Update();
    void Exit();
}

// ============================================================
// EXAMPLE STATE IMPLEMENTATION
// ============================================================

[System.Serializable]
public class TemplatePatrolState : IEnemyState
{
    private TemplateEnemyAI ai;
    private Vector3 patrolTarget;
    private float patrolRadius = 10f;

    public void Initialize(TemplateEnemyAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        Debug.Log("Entered Patrol state");

        // Set random patrol point
        SetNewPatrolPoint();
    }

    public void Update()
    {
        // Check for nearby players
        Transform player = ai.FindNearestPlayer();
        if (player != null && ai.CanSeePlayer(player))
        {
            ai.SetTarget(player.GetComponent<NetworkObject>());
            ai.TransitionTo(TemplateEnemyState.Chase);
            return;
        }

        // Move to patrol point
        if (ai.NavAgent.enabled && !ai.NavAgent.pathPending)
        {
            if (ai.NavAgent.remainingDistance <= ai.NavAgent.stoppingDistance)
            {
                // Reached patrol point, set new one
                SetNewPatrolPoint();
            }
        }
    }

    public void Exit()
    {
        Debug.Log("Exited Patrol state");
    }

    private void SetNewPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += ai.transform.position;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDir, out hit, patrolRadius, 1))
        {
            patrolTarget = hit.position;
            ai.NavAgent.SetDestination(patrolTarget);
        }
    }
}

// TODO: Implement other states (Idle, Chase, Attack, Flee, Dead)
// Follow the same pattern as TemplatePatrolState
