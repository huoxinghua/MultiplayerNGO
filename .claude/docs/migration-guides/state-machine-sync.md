# Migration Guide: Enemy State Machine Network Synchronization

**Target:** Sync enemy AI state machines across all clients
**Difficulty:** Medium-High
**Time Estimate:** 3-5 hours
**Phase:** 3

---

## Overview

Currently, enemy state machines only run on the server. Clients see frozen enemies in T-pose because they never receive state updates or animations. This guide walks through adding network synchronization to enemy state machines.

**Current Issues:**
- State machine Update() only runs on server
- Clients never initialize state machine
- Animations don't play on clients
- Enemies appear frozen from non-host perspectives

**Target Solution:**
- Server runs AI logic
- State synced via NetworkVariable
- All clients play appropriate animations
- Smooth, consistent enemy behavior

---

## Current System Analysis

### Files to Modify:
- `Assets\_Project\Code\Gameplay\NPC\Tranquil\Beetle\BeetleStateMachine.cs`
- `Assets\_Project\Code\Gameplay\NPC\Violent\Brute\BruteStateMachine.cs`

### Current Implementation (Beetle)

```csharp
public class BeetleStateMachine : NetworkBehaviour
{
    public BeetleStateIdle IdleState;
    public BeetleStateWander WanderState;
    public BeetleStateFollow FollowState;
    // ... other states

    private IState CurrentState;

    public void Start()
    {
        if (!IsServer) return;  // ⚠️ Clients never initialize!
        Debug.Log("is server start");
        TransitionTo(WanderState);
    }

    void Update()
    {
        if (!IsServer) return;  // ⚠️ Clients never update!
        FollowCooldown.TimerUpdate(Time.deltaTime);
        CurrentState?.StateUpdate();
    }

    public void TransitionTo(IState state)
    {
        CurrentState?.ExitState();
        CurrentState = state;
        CurrentState?.EnterState();
    }
}
```

**Problem:** Clients never know which state the enemy is in!

---

## New System Design

### Architecture

```
SERVER                              CLIENTS
┌─────────────────┐                ┌─────────────────┐
│ Run AI Logic    │                │ Display Only    │
│                 │                │                 │
│ Update()        │                │ No Update()     │
│  ├─ Pathfinding │                │                 │
│  ├─ Decisions   │                │                 │
│  └─ Transitions │                │                 │
│                 │                │                 │
│ TransitionTo()  │                │                 │
│  └─ Set         │───NetworkVar──►│ OnStateChanged()│
│     StateEnum   │                │  └─ Update Anim │
│                 │                │                 │
└─────────────────┘                └─────────────────┘
```

**Key Points:**
- Server: Runs logic, updates NetworkVariable
- Clients: React to NetworkVariable changes, update visuals
- Animations: Play on all clients based on synced state

---

## Step 1: Define State Enum

Create a new enum for beetle states.

**Add to:** `BeetleStateMachine.cs` (top of file)

```csharp
/// <summary>
/// Enemy state types for network synchronization.
/// </summary>
public enum BeetleStateType
{
    Idle = 0,
    Wander = 1,
    Follow = 2,
    Flee = 3,
    Dead = 4
}
```

---

## Step 2: Add NetworkVariable for State

**Add to class:**

```csharp
public class BeetleStateMachine : NetworkBehaviour
{
    // NEW: Synced state
    private NetworkVariable<BeetleStateType> netState = new NetworkVariable<BeetleStateType>(
        BeetleStateType.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Existing state objects
    public BeetleStateIdle IdleState;
    public BeetleStateWander WanderState;
    public BeetleStateFollow FollowState;
    public BeetleStateFlee FleeState;
    public BeetleStateDead DeadState;

    // Current state (server and clients maintain this locally)
    private IState CurrentState;

    // Reference to animator
    [SerializeField] private Animator animator;

    // ... rest of existing code
}
```

---

## Step 3: Subscribe to State Changes

**Replace Start() with OnNetworkSpawn():**

```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    // Subscribe to state changes on ALL clients
    netState.OnValueChanged += OnStateChanged;

    // Initialize to current state
    if (IsServer)
    {
        // Server: Start in Wander state
        TransitionTo(BeetleStateType.Wander);
    }
    else
    {
        // Clients: Initialize to synced state
        ApplyState(netState.Value);
    }

    Debug.Log($"Beetle state machine initialized. IsServer={IsServer}, State={netState.Value}");
}

public override void OnNetworkDespawn()
{
    base.OnNetworkDespawn();

    // Unsubscribe to prevent memory leaks
    netState.OnValueChanged -= OnStateChanged;
}

private void OnStateChanged(BeetleStateType oldState, BeetleStateType newState)
{
    Debug.Log($"Beetle state changed: {oldState} -> {newState} (IsServer={IsServer})");

    // Apply state on all clients (visual changes)
    ApplyState(newState);
}
```

---

## Step 4: Rewrite TransitionTo Method

**Replace existing TransitionTo():**

```csharp
/// <summary>
/// Transition to a new state. Only call this on the server!
/// </summary>
public void TransitionTo(BeetleStateType newState)
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

    // Set NetworkVariable (triggers OnStateChanged on all clients)
    netState.Value = newState;

    // Apply state locally on server
    ApplyState(newState);

    Debug.Log($"Server transitioning beetle to {newState}");
}

/// <summary>
/// Apply state locally (runs on server and clients).
/// Updates local state machine and animations.
/// </summary>
private void ApplyState(BeetleStateType stateType)
{
    // Exit current state
    CurrentState?.ExitState();

    // Map enum to state object
    CurrentState = stateType switch
    {
        BeetleStateType.Idle => IdleState,
        BeetleStateType.Wander => WanderState,
        BeetleStateType.Follow => FollowState,
        BeetleStateType.Flee => FleeState,
        BeetleStateType.Dead => DeadState,
        _ => IdleState
    };

    // Enter new state
    CurrentState?.EnterState();

    // Update animations on all clients
    UpdateAnimation(stateType);
}
```

---

## Step 5: Update Animation Method

```csharp
/// <summary>
/// Update animator based on state. Runs on all clients.
/// </summary>
private void UpdateAnimation(BeetleStateType stateType)
{
    if (animator == null)
    {
        Debug.LogWarning("Beetle has no animator!");
        return;
    }

    // Set animator parameters based on state
    switch (stateType)
    {
        case BeetleStateType.Idle:
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsDead", false);
            break;

        case BeetleStateType.Wander:
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsDead", false);
            break;

        case BeetleStateType.Follow:
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", true);  // Run when following
            animator.SetBool("IsDead", false);
            break;

        case BeetleStateType.Flee:
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", true);  // Run when fleeing
            animator.SetBool("IsDead", false);
            break;

        case BeetleStateType.Dead:
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");  // Play death animation
            break;
    }

    Debug.Log($"Beetle animation updated to {stateType}");
}
```

---

## Step 6: Update Individual State Scripts

Each state script (e.g., `BeetleStateWander.cs`) needs to call the new `TransitionTo` method.

### Example: BeetleStateWander.cs

**Before:**
```csharp
public void StateUpdate()
{
    if (CanSeePlayer())
    {
        stateMachine.TransitionTo(stateMachine.FollowState);
    }
}
```

**After:**
```csharp
public void StateUpdate()
{
    if (CanSeePlayer())
    {
        // Use enum instead of state object
        stateMachine.TransitionTo(BeetleStateType.Follow);
    }
}
```

### Update All State Transitions

Find and replace in all beetle state files:

**Find:**
- `stateMachine.TransitionTo(stateMachine.IdleState)`
- `stateMachine.TransitionTo(stateMachine.WanderState)`
- `stateMachine.TransitionTo(stateMachine.FollowState)`
- `stateMachine.TransitionTo(stateMachine.FleeState)`
- `stateMachine.TransitionTo(stateMachine.DeadState)`

**Replace with:**
- `stateMachine.TransitionTo(BeetleStateType.Idle)`
- `stateMachine.TransitionTo(BeetleStateType.Wander)`
- `stateMachine.TransitionTo(BeetleStateType.Follow)`
- `stateMachine.TransitionTo(BeetleStateType.Flee)`
- `stateMachine.TransitionTo(BeetleStateType.Dead)`

---

## Step 7: Repeat for Brute Enemy

### Create BruteStateType enum

```csharp
public enum BruteStateType
{
    Idle = 0,
    Patrol = 1,
    Chase = 2,
    Attack = 3,
    Dead = 4
}
```

### Apply same changes to BruteStateMachine.cs

Follow Steps 2-6 but with Brute-specific states:
- BruteStateIdle
- BruteStatePatrol
- BruteStateChase
- BruteStateAttack
- BruteStateDead

---

## Step 8: Testing Checklist

### Test 1: Host Perspective
- [ ] Start as host
- [ ] Spawn beetle enemy
- [ ] Verify beetle wanders around
- [ ] Get close to beetle
- [ ] Verify beetle follows you
- [ ] Run away
- [ ] Verify beetle returns to wandering
- [ ] Kill beetle
- [ ] Verify death animation plays

### Test 2: Client Perspective (Critical!)
- [ ] Start host
- [ ] Start client
- [ ] Client: Spawn enemy (or have it spawned)
- [ ] Client: Observe enemy from distance
- [ ] Verify enemy is wandering (not T-pose!)
- [ ] Verify animations play smoothly
- [ ] Client: Get close to enemy
- [ ] Verify enemy transitions to follow state
- [ ] Verify running animation plays
- [ ] Kill enemy
- [ ] Verify death animation plays on client

### Test 3: State Sync
- [ ] Start host and client
- [ ] Host: Stand near enemy (enemy follows host)
- [ ] Client: Observe enemy following host
- [ ] Verify client sees correct animation
- [ ] Host: Run away
- [ ] Verify client sees enemy stop following
- [ ] Verify state changes propagate correctly

### Test 4: Multiple Enemies
- [ ] Spawn 3+ enemies
- [ ] Verify all enemies wander independently
- [ ] Get one enemy to follow you
- [ ] Verify other enemies continue wandering
- [ ] Verify no state bleed between enemies

### Test 5: Late Join
- [ ] Host starts and spawns enemies
- [ ] Enemies wander/follow
- [ ] Client joins
- [ ] Verify client sees enemies in correct states
- [ ] Verify animations play correctly from start

---

## Common Issues & Solutions

### Issue 1: Animations Not Playing on Clients

**Symptom:** Client sees enemy frozen or in T-pose

**Debug Steps:**
1. Check if `OnStateChanged` is being called on client
2. Check if animator reference is set
3. Check if animator parameters exist

**Solution:**
```csharp
private void UpdateAnimation(BeetleStateType stateType)
{
    if (animator == null)
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Beetle has no Animator component!");
            return;
        }
    }

    Debug.Log($"[Client={IsClient}] Updating animation to {stateType}");

    // ... set animator parameters
}
```

---

### Issue 2: State Changes Not Syncing

**Symptom:** Server transitions state, client doesn't update

**Cause:** NetworkVariable not syncing, or OnValueChanged not subscribed

**Solution:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    // Ensure subscription happens
    Debug.Log($"Subscribing to state changes. IsServer={IsServer}");
    netState.OnValueChanged += OnStateChanged;

    // Force initial state application
    Debug.Log($"Initial state: {netState.Value}");
    ApplyState(netState.Value);
}
```

---

### Issue 3: Clients Running AI Logic

**Symptom:** Enemies behaving erratically, pathfinding on clients

**Cause:** Update() running AI logic on clients

**Solution:**
```csharp
void Update()
{
    // CRITICAL: Only server runs AI logic
    if (!IsServer) return;

    // Update timers
    FollowCooldown.TimerUpdate(Time.deltaTime);

    // Update current state AI
    CurrentState?.StateUpdate();
}
```

---

### Issue 4: Double Animation Triggers

**Symptom:** Animations play twice on server

**Cause:** ApplyState called twice on server (once in TransitionTo, once in OnStateChanged)

**Solution:**
```csharp
private void OnStateChanged(BeetleStateType oldState, BeetleStateType newState)
{
    if (IsServer)
    {
        // Server already applied state in TransitionTo
        return;
    }

    // Only clients apply state in callback
    ApplyState(newState);
}
```

**Or** (simpler):
```csharp
public void TransitionTo(BeetleStateType newState)
{
    if (!IsServer) return;

    if (netState.Value == newState) return;

    // Just set NetworkVariable - let OnStateChanged handle the rest
    netState.Value = newState;
}

private void OnStateChanged(BeetleStateType oldState, BeetleStateType newState)
{
    // Apply on ALL clients (including server/host)
    ApplyState(newState);
}
```

---

## Performance Considerations

### NetworkVariable Updates

**Good:**
- State changes are infrequent (every few seconds)
- Only sends 1 byte (enum value)
- Minimal bandwidth usage

**Bad (Don't Do This):**
```csharp
// DON'T sync position every frame via NetworkVariable
void Update()
{
    netPosition.Value = transform.position;  // BAD!
}
```

**Use NetworkTransform for position/rotation sync instead.**

---

## Debugging Tools

### Add Debug Visualization

```csharp
private void OnDrawGizmos()
{
    if (!Application.isPlaying) return;

    // Draw sphere colored by state
    Color stateColor = netState.Value switch
    {
        BeetleStateType.Idle => Color.gray,
        BeetleStateType.Wander => Color.green,
        BeetleStateType.Follow => Color.yellow,
        BeetleStateType.Flee => Color.red,
        BeetleStateType.Dead => Color.black,
        _ => Color.white
    };

    Gizmos.color = stateColor;
    Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.5f);

    // Draw text
    #if UNITY_EDITOR
    UnityEditor.Handles.Label(
        transform.position + Vector3.up * 3,
        $"{netState.Value}\n{(IsServer ? "SERVER" : "CLIENT")}"
    );
    #endif
}
```

### Add Console Commands

```csharp
[ContextMenu("Force Idle")]
private void ForceIdle()
{
    if (IsServer)
        TransitionTo(BeetleStateType.Idle);
}

[ContextMenu("Force Follow")]
private void ForceFollow()
{
    if (IsServer)
        TransitionTo(BeetleStateType.Follow);
}

[ContextMenu("Print Current State")]
private void PrintCurrentState()
{
    Debug.Log($"Current State: {netState.Value}, IsServer: {IsServer}");
}
```

---

## Optimization: State Prediction (Advanced)

For smoother behavior, clients can predict state changes:

```csharp
private BeetleStateType predictedState;

private void Update()
{
    if (IsServer)
    {
        // Server runs normal AI
        CurrentState?.StateUpdate();
    }
    else if (IsClient)
    {
        // Client predicts state based on observations
        PredictState();
    }
}

private void PredictState()
{
    // Simple prediction: if player is close, predict Follow state
    Transform nearestPlayer = FindNearestPlayer();
    if (nearestPlayer != null)
    {
        float distance = Vector3.Distance(transform.position, nearestPlayer.position);

        if (distance < 5f && netState.Value != BeetleStateType.Follow)
        {
            // Predict follow (but don't actually transition)
            predictedState = BeetleStateType.Follow;

            // Play animation early for responsiveness
            if (animator != null)
                animator.SetBool("IsRunning", true);
        }
    }

    // Server state always wins when it arrives
}

private void OnStateChanged(BeetleStateType oldState, BeetleStateType newState)
{
    // Server state arrived - use it
    predictedState = newState;
    ApplyState(newState);
}
```

**Note:** Only implement prediction if you need extra responsiveness. It adds complexity!

---

## Rollback Plan

```bash
# Backup current state
git add -A
git commit -m "Backup before state machine sync"

# If issues occur, revert
git revert HEAD
```

---

## Next Steps

After completing this migration:

1. Test thoroughly with 2+ clients
2. Verify all enemy types (Beetle, Brute, any others)
3. Proceed to Phase 4: Authority & Validation
4. Consider adding more sophisticated AI behaviors

---

**Estimated Time:** 3-5 hours
**Risk Level:** Medium-High
**Rollback Difficulty:** Easy (Git revert)

---

**Last Updated:** 2025-11-07
