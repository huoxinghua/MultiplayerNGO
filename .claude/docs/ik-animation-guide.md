# IK Animation Guide for Unity Netcode

**Version:** 1.0
**Last Updated:** 2025-11-07
**Project:** MultiplayerNGO - Unity Netcode for GameObjects

This guide addresses the critical IK (Inverse Kinematics) synchronization issues in the project, including the confirmed "TPS Baseball bat hand position" problem.

---

## Table of Contents

1. [Current Implementation](#current-implementation)
2. [Critical Issues Found](#critical-issues-found)
3. [Root Cause Analysis](#root-cause-analysis)
4. [Solution Approaches](#solution-approaches)
5. [Recommended Solution](#recommended-solution)
6. [Performance Considerations](#performance-considerations)
7. [Testing Requirements](#testing-requirements)

---

## Current Implementation

### Architecture Overview

The project uses a dual IK controller system:

```
Player GameObject
├── FPS IK Controller (PlayerIKController)  ← For owner's first-person view
├── TPS IK Controller (PlayerIKController)  ← For other clients' third-person view
└── PlayerAnimation (NetworkBehaviour)      ← Coordinates IK updates
```

### Key Components

#### 1. PlayerIKController.cs (MonoBehaviour - ⚠️ NOT NETWORKED)

**File:** `Assets\_Project\Code\Art\AnimationScripts\IK\PlayerIKController.cs`

```csharp
public class PlayerIKController : MonoBehaviour  // ⚠️ Should be NetworkBehaviour!
{
    private Transform handL;  // ⚠️ Local only, not synced
    private Transform handR;  // ⚠️ Local only, not synced

    private void OnAnimatorIK()  // Runs every frame on ALL clients
    {
        if (IkActive && handR != null)
        {
            // Sets IK from LOCAL transform data
            animator.SetIKPosition(AvatarIKGoal.RightHand, handR.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, handR.rotation);
            // NO NETWORK SYNCHRONIZATION!
        }
        // ... same for left hand and fingers
    }
}
```

**Purpose:** Reads IK target positions and applies them to the Unity Animator's IK system.

**Problem:** Runs independently on each client with different data.

---

#### 2. IKInteractable.cs (MonoBehaviour)

**File:** `Assets\_Project\Code\Art\AnimationScripts\IK\IKInteractable.cs`

```csharp
public class IKInteractable : MonoBehaviour
{
    public Transform handL;  // ⚠️ Local transform, not networked
    public Transform handR;  // ⚠️ Local transform, not networked

    public void PlayIKWalk(float percent, bool isRunning)
    {
        // DOTween animates LOCAL transforms
        transform.DOLocalMove(targetPosition, duration);  // ⚠️ Not synced!
        transform.DOLocalRotate(targetRotation, duration); // ⚠️ Not synced!
    }
}
```

**Purpose:** Animates IK target transforms using DOTween based on player state (idle/walk/run/interact).

**Problem:** DOTween animations run independently on each client, can desync.

---

#### 3. PlayerAnimation.cs (NetworkBehaviour)

**File:** `Assets\_Project\Code\Art\AnimationScripts\Animations\PlayerAnimation.cs`

```csharp
public class PlayerAnimation : NetworkBehaviour
{
    private PlayerIKController fpsIKController;
    private PlayerIKController tpsIKController;

    void UpdateIKMovement(float currentSpeed, float maxSpeed, bool isRunning)
    {
        if (!IsOwner) return;  // Owner only

        // Update FPS IK for owner
        fpsIKController.Interactable.PlayIKWalk(percent, isRunning);

        // Sync to other clients via network
        UpdateIKMovementServerRPC(currentSpeed, maxSpeed, isRunning);
    }

    [ServerRpc]
    private void UpdateIKMovementServerRPC(float currentSpeed, float maxSpeed, bool isRunning)
    {
        DistributeMovementAnimClientRPC(currentSpeed, maxSpeed, isRunning);
    }

    [ClientRpc]
    void DistributeMovementAnimClientRPC(float currentSpeed, float maxSpeed, bool isRunning)
    {
        if (!IsOwner)  // Non-owners update TPS IK
        {
            tpsIKController.Interactable.PlayIKWalk(percent, isRunning);
        }
    }
}
```

**Purpose:** Coordinates IK updates and syncs animation state across network.

**Problem:** Syncs **animation state** (walk/run) but NOT **IK target positions**.

---

### Data Flow Diagram

```
OWNER CLIENT (First-Person View):
─────────────────────────────────────
Player Input
    ↓
UpdateIKMovement()
    ↓
fpsIKController.Interactable.PlayIKWalk()
    ↓
DOTween animates handL/handR (LOCAL)
    ↓
OnAnimatorIK() reads handL/handR position
    ↓
Owner sees CORRECT hand placement ✓


OTHER CLIENTS (Third-Person View):
───────────────────────────────────────
ServerRPC → ClientRPC
    ↓
tpsIKController.Interactable.PlayIKWalk()
    ↓
DOTween animates handL/handR (DIFFERENT INSTANCE!)
    ↓
OnAnimatorIK() reads handL/handR position (DIFFERENT DATA!)
    ↓
Other players see DESYNCED hand placement ✗
```

---

## Critical Issues Found

### Issue #1: PlayerIKController Not Networked

**Severity:** ⛔ CRITICAL
**File:** `PlayerIKController.cs` Line 6
**Impact:** Complete IK desync across clients

**Problem:**
```csharp
public class PlayerIKController : MonoBehaviour  // ⚠️ Should be NetworkBehaviour
```

- IK controller is not a NetworkBehaviour
- `OnAnimatorIK()` runs independently on each client
- No way to sync IK target data across network
- Each client computes IK from its own local data

**Result:** Owner sees correct hand placement, other players see wrong positions.

---

### Issue #2: TPS Baseball Bat Hand Position Desync

**Severity:** 🔴 HIGH (CONFIRMED BY GIT HISTORY)
**File:** `PlayerAnimation.cs` Lines 55-123
**Git Commit:** `60cb67e0` - "TPS Baseball bat hand position is still fucked up"

**Problem:**

1. **Line 116 Bug:**
```csharp
if(netAnim.Animator.GetBool(hCrouch) != true)
    tpsIKController.Interactable.PlayIKWalk(1f, true);  // ⚠️ Should be false!
```
The third parameter should be `false` when not running, but it's hardcoded to `true`.

2. **Owner vs Non-Owner Split:**
```csharp
if (IsOwner)  // FPS controller
{
    fpsIKController.Interactable.PlayIKWalk(percent, isRunning);
}
// Non-owners use TPS controller with SYNCED state but UNSYNCED positions
```

3. **Network Sync Incomplete:**
- Movement **state** (walk/run) is synced via RPC
- IK target **positions** are NOT synced
- TPS view shows animation trigger but wrong hand placement

**Result:** Baseball bat (and other items) show correct animation state but hands are in wrong positions on TPS view.

---

### Issue #3: IK Target Transforms Not Synchronized

**Severity:** ⛔ CRITICAL
**Files:** `IKInteractable.cs` Lines 11-12, `BaseInventoryItem.cs` Line 186
**Impact:** Core cause of all IK desyncs

**Problem Chain:**

1. **Item Pickup** (`BaseInventoryItem.cs` Line 186):
```csharp
_currentHeldVisual = Instantiate(_heldVisual, _heldVisualParent);
```
Each client instantiates its own copy of the held item visual.

2. **IK Target Assignment** (`IKInteractable.cs` Lines 20-22):
```csharp
public void PickupAnimation(PlayerIKController ikController, bool isFPS)
{
    ikController.IKPos(this, handL, handR, ikInteractSo);
}
```
IK controller points to local `handL` and `handR` transforms.

3. **No Network Sync:**
- `handL` and `handR` are child transforms of `_currentHeldVisual`
- No `NetworkTransform` on these objects
- No `NetworkVariable` syncing their positions
- Each client has separate instances with separate positions

**Result:** OnAnimatorIK() reads different positions on each client.

---

### Issue #4: DOTween Animations Run Client-Side Only

**Severity:** 🔴 HIGH
**File:** `IKInteractable.cs` Lines 68-214
**Impact:** Timing desyncs, position drift

**Problem:**

All IK animations use DOTween:
```csharp
public void PlayIKWalk(float percent, bool isRunning)
{
    // These modify LOCAL transforms, not networked!
    transform.DOLocalMove(targetPosition, duration);
    transform.DOLocalRotate(targetRotation, duration);
}
```

**Issues:**
1. **No Sync:** DOTween animations are local, not replicated
2. **Timing:** Can start at different times on different clients (network latency)
3. **Drift:** Over time, positions can drift apart
4. **Interruptions:** If animation interrupted on one client but not others, permanent desync

**Example Scenario:**
```
Time 0.0s:
- Host triggers walk animation
- ServerRPC sent to clients

Time 0.05s (50ms latency):
- Client receives RPC
- Client starts DOTween animation

Time 1.0s:
- Host: handR at position (0.1, 0.5, 0.3) (animation 100% complete)
- Client: handR at position (0.09, 0.48, 0.29) (animation 95% complete)
- DESYNC: 5% difference due to timing
```

---

### Issue #5: Dual IK Controller Architecture Issues

**Severity:** 🟡 MEDIUM
**Files:** `PlayerAnimation.cs` Lines 19-21
**Impact:** Complexity, potential race conditions

**Problem:**

Each player has TWO `PlayerIKController` instances:
```csharp
private PlayerIKController fpsIKController;  // Owner's view
private PlayerIKController tpsIKController;  // Others' view
```

**Issues:**
1. **Shared IKInteractable:** Both controllers can point to same `IKInteractable`, causing potential conflicts
2. **Duplicate Logic:** Same IK computation happening twice
3. **Confusion:** Unclear which controller is active at any time
4. **Bug-Prone:** Line 116 bug shows how easy it is to mix up FPS/TPS logic

**Better Approach:** Single IK controller with view-dependent offsets.

---

## Root Cause Analysis

### The Fundamental Problem

**Unity's Animator IK system is client-local.** It was designed for single-player games where:
- OnAnimatorIK() runs on one machine
- All IK targets are in the same scene
- No synchronization needed

**In multiplayer:**
- OnAnimatorIK() runs on EVERY client
- Each client has its own copy of the scene
- IK targets need to be synchronized

### Why Current Implementation Fails

```
┌─────────────────────────────────────────────────────────────┐
│                    OWNER CLIENT                             │
│                                                             │
│  Item Pickup → Spawn _currentHeldVisual (Instance A)       │
│    ↓                                                        │
│  Get handL, handR from Instance A                          │
│    ↓                                                        │
│  DOTween animates Instance A transforms                    │
│    ↓                                                        │
│  OnAnimatorIK() reads Instance A positions                 │
│    ↓                                                        │
│  ✓ CORRECT HAND PLACEMENT                                  │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    OTHER CLIENT                             │
│                                                             │
│  Item Pickup Sync → Spawn _currentHeldVisual (Instance B)  │
│    ↓                                                        │
│  Get handL, handR from Instance B (DIFFERENT INSTANCE!)    │
│    ↓                                                        │
│  ClientRPC triggers DOTween on Instance B                  │
│    ↓                                                        │
│  (50ms latency, animation starts late)                     │
│    ↓                                                        │
│  OnAnimatorIK() reads Instance B positions                 │
│    ↓                                                        │
│  ✗ DESYNCED HAND PLACEMENT                                 │
└─────────────────────────────────────────────────────────────┘
```

### Why DOTween Makes It Worse

DOTween is a **local animation library**. It has no knowledge of Unity Netcode:
- No automatic synchronization
- No network callbacks
- Animations run based on local `Time.time`
- If started at slightly different times, will be out of sync

---

## Solution Approaches

### Approach A: Network the IK Targets (NetworkTransform)

**Concept:** Add `NetworkTransform` to the IK target objects (handL, handR).

#### Implementation:
```csharp
// On held item prefab hierarchy:
HeldItemVisual (NetworkObject)
├── handL (Transform + NetworkTransform)
└── handR (Transform + NetworkTransform)
```

**Pros:**
- ✓ Simple to implement
- ✓ Unity handles sync automatically
- ✓ Smooth interpolation
- ✓ Works with existing DOTween code

**Cons:**
- ✗ High bandwidth (2 transforms × 2 hands × 60Hz = 240 updates/sec)
- ✗ DOTween still runs on all clients (redundant computation)
- ✗ Still need to sync DOTween timing for exact matching

**Best For:** Quick fix, low player count (<4 players)

---

### Approach B: Sync DOTween State (NetworkVariables)

**Concept:** Keep transforms local, sync the animation state instead.

#### Implementation:
```csharp
public class IKInteractable : NetworkBehaviour  // Make it networked
{
    private NetworkVariable<float> animationTime = new NetworkVariable<float>();
    private NetworkVariable<IKAnimState> currentAnim = new NetworkVariable<IKAnimState>();

    [ServerRpc]
    public void PlayIKWalkServerRpc(float percent, bool isRunning)
    {
        currentAnim.Value = isRunning ? IKAnimState.Run : IKAnimState.Walk;
        animationTime.Value = 0f;  // Reset animation
        PlayIKWalkClientRpc(percent, isRunning);
    }

    [ClientRpc]
    private void PlayIKWalkClientRpc(float percent, bool isRunning)
    {
        // All clients play same animation at same time
        PlayIKWalkLocal(percent, isRunning);
    }

    private void Update()
    {
        if (IsServer)
        {
            animationTime.Value += Time.deltaTime;
        }

        // All clients evaluate DOTween at same time point
        EvaluateAnimationAtTime(animationTime.Value);
    }
}
```

**Pros:**
- ✓ Lower bandwidth (1 float + 1 enum vs 2 full transforms)
- ✓ Perfect sync (all clients at same animation time)
- ✓ Works with DOTween

**Cons:**
- ✗ More complex implementation
- ✗ Requires custom DOTween evaluation
- ✗ Need to handle animation blending manually

**Best For:** Optimized solution, medium player count

---

### Approach C: Procedural IK (Server-Authoritative)

**Concept:** Calculate IK positions procedurally based on synced state, no animations.

#### Implementation:
```csharp
public class PlayerIKController : NetworkBehaviour
{
    private NetworkVariable<PlayerState> playerState = new NetworkVariable<PlayerState>();

    private void OnAnimatorIK()
    {
        // Calculate IK positions based on current state
        Vector3 handRPos = CalculateHandPosition(
            playerState.Value.movementSpeed,
            playerState.Value.isRunning,
            playerState.Value.currentItem
        );

        animator.SetIKPosition(AvatarIKGoal.RightHand, handRPos);
    }

    private Vector3 CalculateHandPosition(float speed, bool running, ItemType item)
    {
        // Procedural calculation based on state
        // No animations, just math
        return basePosition + GetItemOffset(item) + GetMovementSway(speed, running);
    }
}
```

**Pros:**
- ✓ Lowest bandwidth (just player state)
- ✓ Perfect sync (deterministic calculation)
- ✓ No DOTween needed
- ✓ Simplest architecture

**Cons:**
- ✗ Less artistic control (no animation curves)
- ✗ Requires rewriting all IK logic
- ✗ May look less polished than hand-animated

**Best For:** Large player counts, competitive games where bandwidth matters

---

## Recommended Solution

### Hybrid Approach: NetworkBehaviour + Synced Animation Triggers

**Best balance of quality, performance, and implementation effort.**

#### Phase 1: Convert PlayerIKController to NetworkBehaviour

```csharp
public class PlayerIKController : NetworkBehaviour  // ← Changed
{
    // Add network variables for IK target positions
    private NetworkVariable<Vector3> handRPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> handRRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> handLPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> handLRotation = new NetworkVariable<Quaternion>();

    private Transform handL;
    private Transform handR;

    private void Update()
    {
        if (IsOwner)
        {
            // Owner: Read from local transforms, push to network
            handRPosition.Value = handR.position;
            handRRotation.Value = handR.rotation;
            handLPosition.Value = handL.position;
            handLRotation.Value = handL.rotation;
        }
    }

    private void OnAnimatorIK()
    {
        if (IkActive)
        {
            if (IsOwner)
            {
                // Owner: Use local transforms (lowest latency)
                animator.SetIKPosition(AvatarIKGoal.RightHand, handR.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, handR.rotation);
            }
            else
            {
                // Non-owners: Use synced network values
                animator.SetIKPosition(AvatarIKGoal.RightHand, handRPosition.Value);
                animator.SetIKRotation(AvatarIKGoal.RightHand, handRRotation.Value);
            }

            // Same for left hand and fingers...
        }
    }
}
```

#### Phase 2: Sync Animation Timing

```csharp
public class IKInteractable : NetworkBehaviour  // ← Changed
{
    private NetworkVariable<float> syncedAnimTime = new NetworkVariable<float>();
    private NetworkVariable<IKState> currentState = new NetworkVariable<IKState>();

    private float localAnimTime = 0f;
    private Tween currentTween;

    [ServerRpc]
    public void PlayIKWalkServerRpc(float percent, bool isRunning)
    {
        currentState.Value = isRunning ? IKState.Run : IKState.Walk;
        syncedAnimTime.Value = 0f;

        PlayIKWalkClientRpc(percent, isRunning);
    }

    [ClientRpc]
    private void PlayIKWalkClientRpc(float percent, bool isRunning)
    {
        // Start animation on all clients simultaneously
        localAnimTime = 0f;
        PlayIKWalkLocal(percent, isRunning);
    }

    private void Update()
    {
        if (IsServer && currentState.Value != IKState.Idle)
        {
            syncedAnimTime.Value += Time.deltaTime;
        }

        // Clients correct drift periodically
        if (!IsServer && Mathf.Abs(localAnimTime - syncedAnimTime.Value) > 0.1f)
        {
            localAnimTime = syncedAnimTime.Value;  // Snap to server time
        }
        else
        {
            localAnimTime += Time.deltaTime;
        }
    }
}
```

#### Why This Works:

1. **Owner:** Uses local transforms (zero latency, perfect feel)
2. **Network Variables:** Sync IK positions to other clients
3. **Animation Timing:** Synced via RPC + time tracking
4. **Drift Correction:** Periodic snap to server time prevents desyncs
5. **Bandwidth:** ~40 bytes/frame (4 NetworkVariables × 10 bytes)

---

## Performance Considerations

### Bandwidth Analysis

| Approach | Data Per Frame | @ 60Hz | Notes |
|----------|---------------|--------|-------|
| **NetworkTransform** | 48 bytes (2 transforms) | 2.8 KB/s | High, but manageable |
| **NetworkVariable Positions** | 40 bytes (4 vec3/quat) | 2.4 KB/s | Recommended |
| **Synced State Only** | 8 bytes (enum + float) | 0.48 KB/s | Best, but less accurate |
| **No Sync (Current)** | 0 bytes | 0 KB/s | FREE but BROKEN |

### Update Frequency Options

You don't need to sync every frame. Consider:

```csharp
private float syncTimer = 0f;
private const float SYNC_INTERVAL = 0.05f;  // 20Hz instead of 60Hz

private void Update()
{
    if (!IsOwner) return;

    syncTimer += Time.deltaTime;
    if (syncTimer >= SYNC_INTERVAL)
    {
        syncTimer = 0f;

        // Update network variables
        handRPosition.Value = handR.position;
        // ...
    }
}
```

**20Hz sync:** 0.8 KB/s (3× reduction)
**Still smooth:** NetworkVariable interpolates between updates

---

## Testing Requirements

### Test 1: Basic IK Sync
- [ ] Host picks up baseball bat
- [ ] Client joins
- [ ] Client sees bat in correct hand position
- [ ] Host swings bat
- [ ] Client sees correct swing animation

### Test 2: Movement State Transitions
- [ ] Idle → Walk: Hands move to walk position on all clients
- [ ] Walk → Run: Hands move to run position on all clients
- [ ] Run → Crouch: Hands adjust for crouch
- [ ] Verify timing matches (use stopwatch)

### Test 3: Item Switching
- [ ] Pick up flashlight
- [ ] Verify hand positions
- [ ] Switch to baseball bat
- [ ] Verify hand positions change
- [ ] Both clients see same positions

### Test 4: Late Join
- [ ] Host picks up item, starts moving
- [ ] Client joins mid-animation
- [ ] Verify client sees correct current state
- [ ] Verify animation continues smoothly

### Test 5: Network Stress
- [ ] 4 players all holding different items
- [ ] All running in different directions
- [ ] Verify all IK stays synced
- [ ] Check frame rate, bandwidth

### Test 6: Edge Cases
- [ ] Drop item while animation playing
- [ ] Pick up item while running
- [ ] Rapid item switching
- [ ] Disconnect/reconnect

---

## Next Steps

1. **Read migration guide:** `.claude/docs/migration-guides/ik-sync-refactor.md`
2. **Use code template:** `.claude/templates/networked-ik-controller.cs`
3. **Follow testing checklist** above
4. **Fix Line 116 bug** in PlayerAnimation.cs first (quick win)
5. **Phase implementation:** Do Phase 1 first, test, then Phase 2

---

## Related Documents

- **Migration Guide:** [ik-sync-refactor.md](migration-guides/ik-sync-refactor.md)
- **Code Template:** [networked-ik-controller.cs](../templates/networked-ik-controller.cs)
- **Authority Model:** [authority-model.md](authority-model.md)
- **Main Plan:** [CLAUDE.md](../CLAUDE.md)

---

**Last Updated:** 2025-11-07

This IK issue is **fixable** with the recommended hybrid approach. The TPS baseball bat issue will be resolved once IK positions are properly synchronized across the network. 🎯
