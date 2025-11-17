# Migration Guide: IK Synchronization Refactor

**Target:** Fix IK hand position desyncs and TPS baseball bat issue
**Difficulty:** High
**Time Estimate:** 6-8 hours
**Phase:** 3 (Network Synchronization)

---

## Overview

This guide walks through fixing the critical IK synchronization issues, including the confirmed "TPS Baseball bat hand position is still fucked up" problem (commit `60cb67e0`).

**What You'll Fix:**
- PlayerIKController not networked
- IK target positions not synchronized
- DOTween animation timing desyncs
- TPS baseball bat hand position bug
- Line 116 bug in PlayerAnimation.cs

---

## Prerequisites

- [ ] Phase 1 complete (Cleanup & Critical Fixes)
- [ ] Phase 2 complete (Singleton Architecture)
- [ ] Tested multiplayer environment ready
- [ ] Git repo clean (commit current work)
- [ ] Read [ik-animation-guide.md](../ik-animation-guide.md) first

---

## Quick Win: Fix Line 116 Bug First!

**Before starting the full refactor, fix this quick bug:**

### Step 0: Fix PlayerAnimation.cs Line 116

**File:** `Assets\_Project\Code\Art\AnimationScripts\Animations\PlayerAnimation.cs`

**Find (Line 116):**
```csharp
if(netAnim.Animator.GetBool(hCrouch) != true)
    tpsIKController.Interactable.PlayIKWalk(1f, true);  // ⚠️ Wrong!
```

**Replace with:**
```csharp
if(netAnim.Animator.GetBool(hCrouch) != true)
    tpsIKController.Interactable.PlayIKWalk(1f, false);  // ✓ Correct
```

**Test:**
- [ ] Start host and client
- [ ] Walk (not run) with baseball bat
- [ ] Verify walk animation (not run) plays on TPS view

**Commit:** "Fix: TPS IK walk/run parameter bug (Line 116)"

---

## Phase 1: Convert PlayerIKController to NetworkBehaviour

### Step 1.1: Backup Current Implementation

```bash
# Create backup
cp "Assets/_Project/Code/Art/AnimationScripts/IK/PlayerIKController.cs" "Assets/_Project/Code/Art/AnimationScripts/IK/PlayerIKController.cs.backup"

# Or Git commit
git add "Assets/_Project/Code/Art/AnimationScripts/IK/PlayerIKController.cs"
git commit -m "Backup before IK refactor"
```

### Step 1.2: Add NetworkBehaviour Inheritance

**File:** `PlayerIKController.cs`

**Change Line 6:**
```csharp
// FROM:
using UnityEngine;

public class PlayerIKController : MonoBehaviour

// TO:
using UnityEngine;
using Unity.Netcode;  // ← Add this

public class PlayerIKController : NetworkBehaviour  // ← Changed
```

### Step 1.3: Add NetworkVariables for IK Targets

**Add after Line 8 (after class declaration):**

```csharp
public class PlayerIKController : NetworkBehaviour
{
    // ============================================================
    // NETWORKED IK POSITIONS
    // ============================================================

    /// <summary>
    /// Right hand position (synced across network)
    /// </summary>
    private NetworkVariable<Vector3> netHandRPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Right hand rotation (synced across network)
    /// </summary>
    private NetworkVariable<Quaternion> netHandRRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Left hand position (synced across network)
    /// </summary>
    private NetworkVariable<Vector3> netHandLPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Left hand rotation (synced across network)
    /// </summary>
    private NetworkVariable<Quaternion> netHandLRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // ============================================================
    // EXISTING CODE (keep as-is)
    // ============================================================

    // ... rest of your existing fields ...
```

### Step 1.4: Add Update Method to Sync Positions

**Add new method before OnAnimatorIK():**

```csharp
/// <summary>
/// Sync IK positions to network (owner only)
/// </summary>
private void Update()
{
    if (!IsOwner) return;  // Only owner syncs

    // Sync right hand
    if (handR != null)
    {
        netHandRPosition.Value = handR.position;
        netHandRRotation.Value = handR.rotation;
    }

    // Sync left hand
    if (handL != null)
    {
        netHandLPosition.Value = handL.position;
        netHandLRotation.Value = handL.rotation;
    }
}
```

### Step 1.5: Update OnAnimatorIK to Use Network Data

**Replace the entire OnAnimatorIK() method:**

```csharp
private void OnAnimatorIK()
{
    if (!IkActive) return;

    // RIGHT HAND
    if (handR != null)
    {
        if (IsOwner)
        {
            // Owner: Use local transforms (lowest latency)
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, handR.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, handR.rotation);

            // Apply fingers
            if (shouldApplyFingers)
            {
                ApplyFinger(handR, FingerRotations, HumanBodyBones.RightIndexProximal);
            }
        }
        else
        {
            // Non-owner: Use synced network values
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, netHandRPosition.Value);
            animator.SetIKRotation(AvatarIKGoal.RightHand, netHandRRotation.Value);

            // Note: Fingers will be slightly off on non-owners (acceptable tradeoff)
            // Syncing individual finger bones would be too expensive
        }
    }
    else
    {
        // No hand target, disable IK
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
    }

    // LEFT HAND (same pattern)
    if (handL != null)
    {
        if (IsOwner)
        {
            // Owner: Use local transforms
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, handL.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, handL.rotation);

            if (shouldApplyFingers)
            {
                ApplyFinger(handL, FingerRotations, HumanBodyBones.LeftIndexProximal);
            }
        }
        else
        {
            // Non-owner: Use synced network values
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, netHandLPosition.Value);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, netHandLRotation.Value);
        }
    }
    else
    {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
    }
}
```

### Step 1.6: Test Phase 1

- [ ] **Compile:** Fix any errors
- [ ] **Start Host:** Pick up baseball bat
- [ ] **Start Client:** Join game
- [ ] **Verify:** Client sees baseball bat in host's hand (TPS view)
- [ ] **Host:** Walk around
- [ ] **Client:** Verify bat stays in correct hand position
- [ ] **Check Console:** Look for errors

**Expected Result:** Hand positions should now sync! Baseball bat should be in correct position on TPS view.

**If Issues:** Check "Common Issues" section below

**Commit:** "Phase 1: Convert PlayerIKController to NetworkBehaviour"

---

## Phase 2: Fix Animation Timing (Optional But Recommended)

This phase ensures DOTween animations start at the same time on all clients.

### Step 2.1: Convert IKInteractable to NetworkBehaviour

**File:** `IKInteractable.cs`

**Change Line 6:**
```csharp
// FROM:
public class IKInteractable : MonoBehaviour

// TO:
using Unity.Netcode;

public class IKInteractable : NetworkBehaviour  // ← Changed
```

### Step 2.2: Add Animation State NetworkVariables

**Add after class declaration:**

```csharp
public class IKInteractable : NetworkBehaviour
{
    // ============================================================
    // NETWORKED ANIMATION STATE
    // ============================================================

    public enum IKAnimState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Interact = 3
    }

    private NetworkVariable<IKAnimState> currentAnimState = new NetworkVariable<IKAnimState>(
        IKAnimState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> animationTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Drift correction
    private float localAnimTime = 0f;
    private const float DRIFT_CORRECTION_THRESHOLD = 0.1f;

    // ============================================================
    // EXISTING CODE
    // ============================================================

    // ... rest of your code ...
```

### Step 2.3: Add Lifecycle Methods

**Add after NetworkVariable declarations:**

```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    // Subscribe to state changes
    currentAnimState.OnValueChanged += OnAnimStateChanged;
}

public override void OnNetworkDespawn()
{
    base.OnNetworkDespawn();

    // Unsubscribe
    currentAnimState.OnValueChanged -= OnAnimStateChanged;
}

private void OnAnimStateChanged(IKAnimState oldState, IKAnimState newState)
{
    Debug.Log($"IK Animation state changed: {oldState} → {newState}");

    // Reset local time when state changes
    localAnimTime = 0f;
}
```

### Step 2.4: Add Update Method for Time Sync

```csharp
private void Update()
{
    // Server updates animation time
    if (IsServer && currentAnimState.Value != IKAnimState.Idle)
    {
        animationTime.Value += Time.deltaTime;
    }

    // Clients: Drift correction
    if (!IsServer)
    {
        float drift = Mathf.Abs(localAnimTime - animationTime.Value);

        if (drift > DRIFT_CORRECTION_THRESHOLD)
        {
            // Snap to server time
            localAnimTime = animationTime.Value;
            Debug.LogWarning($"IK animation drift corrected: {drift:F3}s");
        }
        else
        {
            // Normal time progression
            localAnimTime += Time.deltaTime;
        }
    }
}
```

### Step 2.5: Update PlayIK Methods to Use RPCs

**Replace PlayIKWalk method:**

```csharp
public void PlayIKWalk(float percent, bool isRunning)
{
    // Call the ServerRPC version
    if (IsOwner)
    {
        PlayIKWalkServerRpc(percent, isRunning);
    }
}

[ServerRpc]
private void PlayIKWalkServerRpc(float percent, bool isRunning)
{
    // Update state
    currentAnimState.Value = isRunning ? IKAnimState.Run : IKAnimState.Walk;
    animationTime.Value = 0f;  // Reset animation timer

    // Broadcast to all clients
    PlayIKWalkClientRpc(percent, isRunning);
}

[ClientRpc]
private void PlayIKWalkClientRpc(float percent, bool isRunning)
{
    // Reset local time
    localAnimTime = 0f;

    // Play animation locally
    PlayIKWalkLocal(percent, isRunning);
}

/// <summary>
/// Original PlayIKWalk logic (now called locally on all clients)
/// </summary>
private void PlayIKWalkLocal(float percent, bool isRunning)
{
    // ... MOVE ALL YOUR EXISTING PlayIKWalk CODE HERE ...
    // (The DOTween animation code)

    // Example from your code:
    if (!isRunning)
    {
        // Walk animation
        transform.DOKill();
        if (isFPS)
        {
            transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikIdle.fpsWaypoints[0], isFPS), 1f);
            transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikIdle.fpsRotations[0], isFPS), 1f);
        }
        else
        {
            transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikIdle.tpsWaypoints[0], isFPS), 1f);
            transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikIdle.tpsRotations[0], isFPS), 1f);
        }
    }
    else
    {
        // Run animation
        // ... rest of your code ...
    }
}
```

**Repeat for other methods:**
- `PlayIKIdle()` → `PlayIKIdleServerRpc()` → `PlayIKIdleClientRpc()` → `PlayIKIdleLocal()`
- `PlayIKRun()` → (similar pattern)
- `PlayIKInteract()` → (similar pattern)

### Step 2.6: Test Phase 2

- [ ] **Start Host:** Walk with item
- [ ] **Start Client:** Observe host
- [ ] **Verify:** Animations start simultaneously
- [ ] **Check Timing:** Use stopwatch, verify sync within 50ms
- [ ] **Test State Changes:** Idle → Walk → Run → Idle
- [ ] **All clients:** Verify smooth transitions

**Commit:** "Phase 2: Sync IK animation timing"

---

## Phase 3: Update PlayerAnimation.cs

### Step 3.1: Update IK Controller Calls

**File:** `PlayerAnimation.cs`

Since `PlayIKWalk` now uses RPCs internally, update the calling code:

**Find (around Line 55-82):**
```csharp
void UpdateIKMovement(float currentSpeed, float maxSpeed, bool isRunning)
{
    if (!IsOwner) return;

    float percent = Mathf.Clamp01(currentSpeed / maxSpeed);

    if (IsOwner)
    {
        fpsIKController.Interactable.PlayIKWalk(percent, isRunning);
    }

    UpdateIKMovementServerRPC(currentSpeed, maxSpeed, isRunning);
}
```

**Replace with:**
```csharp
void UpdateIKMovement(float currentSpeed, float maxSpeed, bool isRunning)
{
    if (!IsOwner) return;

    float percent = Mathf.Clamp01(currentSpeed / maxSpeed);

    // FPS controller (owner's view)
    fpsIKController.Interactable.PlayIKWalk(percent, isRunning);

    // TPS controller will be updated via RPC in the IKInteractable
    // No need to call UpdateIKMovementServerRPC anymore
}
```

**OR** if you want to keep the dual-controller architecture:

```csharp
void UpdateIKMovement(float currentSpeed, float maxSpeed, bool isRunning)
{
    if (!IsOwner) return;

    float percent = Mathf.Clamp01(currentSpeed / maxSpeed);

    // Owner updates FPS controller locally
    fpsIKController.Interactable.PlayIKWalkLocal(percent, isRunning);

    // Sync to TPS controller on other clients
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
    if (!IsOwner)
    {
        float percent = Mathf.Clamp01(currentSpeed / maxSpeed);
        tpsIKController.Interactable.PlayIKWalkClientRpc(percent, isRunning);
    }
}
```

### Step 3.2: Remove Redundant Crouch Bug

**Find the bug you fixed in Step 0 (Line 116):**
```csharp
if(netAnim.Animator.GetBool(hCrouch) != true)
    tpsIKController.Interactable.PlayIKWalk(1f, false);  // Already fixed
```

**Verify it's still correct after refactor.**

### Step 3.3: Test Phase 3

- [ ] **Full gameplay test:** Walk, run, crouch, jump with various items
- [ ] **Both views:** Verify FPS (owner) and TPS (others) both work
- [ ] **Item switching:** Swap between items, verify IK updates
- [ ] **Multiple clients:** 3+ players all with items

**Commit:** "Phase 3: Update PlayerAnimation for networked IK"

---

## Common Issues & Solutions

### Issue 1: "NetworkVariable can only be written to by the owner"

**Symptom:** Error when trying to update NetworkVariable

**Cause:** NetworkVariables have `WritePermission.Owner` but code runs on non-owner

**Solution:**
```csharp
// In Update():
if (!IsOwner) return;  // ← Add this check!

netHandRPosition.Value = handR.position;
```

---

### Issue 2: Hands still desynced slightly

**Symptom:** Hands are closer but still not perfect

**Cause:** Network lag, interpolation

**Solution:** Adjust NetworkVariable settings in NetworkManager:
- Increase tick rate (default 30Hz → 60Hz)
- Enable interpolation
- Reduce `DRIFT_CORRECTION_THRESHOLD` in Step 2.4

---

### Issue 3: Animations jitter/stutter

**Symptom:** DOTween animations look choppy

**Cause:** Animation timing drift

**Solution:** Ensure Step 2 (Animation Timing) is complete:
```csharp
// Make sure this is in Update():
if (!IsServer)
{
    if (Mathf.Abs(localAnimTime - animationTime.Value) > DRIFT_CORRECTION_THRESHOLD)
    {
        localAnimTime = animationTime.Value;  // Snap
    }
}
```

---

### Issue 4: Fingers not synchronized

**Symptom:** Finger poses different on each client

**Cause:** Fingers not synced (intentional - too expensive)

**Acceptable:** This is a known tradeoff. Syncing all finger bones would require 10+ NetworkVariables per hand.

**Solution (if critical):**
Add NetworkVariables for finger rotations:
```csharp
private NetworkVariable<Quaternion> indexFingerRotation = new NetworkVariable<Quaternion>();
// ... repeat for all fingers
```

**Not recommended** - significant bandwidth cost.

---

### Issue 5: Late-join clients see wrong hand positions

**Symptom:** Client joins, sees hands in T-pose or wrong position

**Cause:** NetworkVariables not initialized before OnAnimatorIK runs

**Solution:**
```csharp
private void OnAnimatorIK()
{
    if (!IsOwner && netHandRPosition.Value == Vector3.zero)
    {
        // Network data not ready yet
        return;
    }

    // ... rest of IK code
}
```

---

## Performance Optimization

### Reduce Update Frequency

Don't need to sync every frame:

```csharp
private float syncTimer = 0f;
private const float SYNC_INTERVAL = 0.033f;  // 30Hz instead of 60Hz

private void Update()
{
    if (!IsOwner) return;

    syncTimer += Time.deltaTime;
    if (syncTimer >= SYNC_INTERVAL)
    {
        syncTimer = 0f;

        // Update NetworkVariables
        if (handR != null)
        {
            netHandRPosition.Value = handR.position;
            netHandRRotation.Value = handR.rotation;
        }
        // ...
    }
}
```

**Result:** 2× bandwidth reduction, still smooth

---

## Testing Checklist

### Phase 1 Tests
- [ ] Host picks up item, client sees correct position
- [ ] Host walks, client sees hands move correctly
- [ ] Host runs, client sees running hand animation
- [ ] Item switching works on both clients

### Phase 2 Tests
- [ ] Animations start simultaneously (< 50ms difference)
- [ ] No drift over 60 seconds of continuous animation
- [ ] State transitions smooth on all clients

### Phase 3 Tests
- [ ] FPS view (owner) still works perfectly
- [ ] TPS view (others) now works correctly
- [ ] Baseball bat TPS issue FIXED
- [ ] All items (flashlight, bat, etc.) work

### Stress Tests
- [ ] 4 players all with items, all running
- [ ] Rapid item switching
- [ ] Late-join client
- [ ] Disconnect/reconnect
- [ ] Check FPS, bandwidth

---

## Rollback Plan

If issues occur:

```bash
# Restore Phase 1 backup
cp "Assets/_Project/Code/Art/AnimationScripts/IK/PlayerIKController.cs.backup" "Assets/_Project/Code/Art/AnimationScripts/IK/PlayerIKController.cs"

# Or Git revert
git log --oneline  # Find commit before IK refactor
git revert <commit-hash>
```

---

## Success Criteria

**IK refactor is complete when:**

- [ ] ✅ PlayerIKController is a NetworkBehaviour
- [ ] ✅ Hand positions sync across all clients
- [ ] ✅ TPS baseball bat issue FIXED (can see correct hand placement)
- [ ] ✅ Animations synchronized (timing drift < 100ms)
- [ ] ✅ Line 116 bug fixed (walk vs run)
- [ ] ✅ All tests pass
- [ ] ✅ FPS (owner view) still feels responsive
- [ ] ✅ TPS (others view) looks correct
- [ ] ✅ Bandwidth acceptable (< 5KB/s per player)

---

## Estimated Time

- **Step 0 (Line 116 fix):** 5 minutes
- **Phase 1 (NetworkBehaviour):** 2-3 hours
- **Phase 2 (Animation timing):** 2-3 hours
- **Phase 3 (PlayerAnimation updates):** 1-2 hours
- **Testing:** 1-2 hours

**Total:** 6-10 hours (depending on issues encountered)

---

## Next Steps

After completing this guide:

1. Update other IK interactions (weapon swapping, reloading, etc.)
2. Consider foot IK for walking on slopes
3. Add look-at IK for player heads
4. Optimize further if needed

---

**Last Updated:** 2025-11-07

The TPS baseball bat issue will be **completely fixed** after completing this migration! 🎯
