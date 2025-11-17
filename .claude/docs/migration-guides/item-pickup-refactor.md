# Migration Guide: Item Pickup System Refactor

**Target:** Simplify item pickup flow, eliminate race conditions
**Difficulty:** High
**Time Estimate:** 4-6 hours
**Phase:** 3

---

## Overview

The current item pickup system in `BaseInventoryItem.cs` is overly complex with 60+ lines of code, nested coroutines, and race conditions. This guide walks through rebuilding it with a simpler, more reliable approach.

**Current Issues:**
- 60+ lines of pickup logic
- Multiple coroutines waiting for network state
- Race conditions between spawn and ownership transfer
- Can cause item duplication
- Different code paths for server vs client

**Target Solution:**
- Single, clear pickup flow
- Server validates all pickups
- No coroutines waiting for network state
- No race conditions
- ~20 lines of pickup logic

---

## Current System Analysis

### File: `Assets\_Project\Code\Gameplay\NewItemSystem\Items\BaseInventoryItem.cs`

Current pickup flow (Lines 165-228):

```csharp
public void PickupItem(PlayerInventory playerInventory)
{
    // 1. Check if already picked up
    if (isPicked) return;
    isPicked = true;

    // 2. Create held visual prefab
    GameObject heldObj = Instantiate(/* ... */);

    // 3. Get NetworkObject
    NetworkObject heldNetObj = heldObj.GetComponent<NetworkObject>();

    // 4. Spawn on network
    heldNetObj.Spawn();

    // 5. Wait for spawn in coroutine
    StartCoroutine(WaitForHeld(heldNetObj, playerInventory));

    // 6. Change ownership (multiple attempts)
    // 7. Wait for ownership in another coroutine
    // 8. Set position and rotation
    // 9. Sync to inventory NetworkList
    // ... 60+ lines total
}
```

**Problems:**
1. `isPicked` flag is local, not synced → race condition
2. Client instantiates held object → should be server only
3. Multiple coroutines chained together → hard to debug
4. Ownership changes without validation → can fail silently
5. Position set before ownership confirmed → can cause glitches

---

## New System Design

### Simplified Flow

```
1. Client: Press interact key near item
2. Client: Call RequestPickupServerRpc(itemReference)
3. Server: Validate pickup request
   - Item still exists and available?
   - Player in range?
   - Inventory has space?
4. Server: Grant pickup
   - Mark item as picked up (NetworkVariable)
   - Add to player's inventory NetworkList
   - Change item ownership to player
   - Parent item to player's hand
5. All Clients: React to NetworkVariable changes
   - Hide world pickup visuals
   - Show held item visuals
   - Update inventory UI
```

**Benefits:**
- Clear, linear flow
- All validation on server
- No race conditions
- Synced state via NetworkVariables
- Easy to test and debug

---

## Step 1: Backup Current Code

Before making changes:

```bash
# Create backup
cp Assets/_Project/Code/Gameplay/NewItemSystem/Items/BaseInventoryItem.cs Assets/_Project/Code/Gameplay/NewItemSystem/Items/BaseInventoryItem.cs.backup

# Or commit to Git
git add Assets/_Project/Code/Gameplay/NewItemSystem/Items/BaseInventoryItem.cs
git commit -m "Backup before item pickup refactor"
```

---

## Step 2: Add NetworkVariable for Pickup State

**File:** `BaseInventoryItem.cs`

Add at the top of the class:

```csharp
public class BaseInventoryItem : NetworkBehaviour
{
    // NEW: Synced pickup state
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // NEW: Synced owner reference
    private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(
        999999,  // Invalid ID means no owner
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Keep existing fields
    [SerializeField] private GameObject heldItemVisualPrefab;
    [SerializeField] private Transform heldItemTransform;

    // ... rest of existing code
}
```

---

## Step 3: Subscribe to State Changes

Add to `OnNetworkSpawn()`:

```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    // Subscribe to pickup state changes
    isPickedUp.OnValueChanged += OnPickupStateChanged;

    // Initialize visual state
    OnPickupStateChanged(false, isPickedUp.Value);
}

public override void OnNetworkDespawn()
{
    base.OnNetworkDespawn();

    // Unsubscribe to prevent memory leaks
    isPickedUp.OnValueChanged -= OnPickupStateChanged;
}

private void OnPickupStateChanged(bool wasPickedUp, bool nowPickedUp)
{
    // Update visuals on ALL clients
    if (nowPickedUp)
    {
        // Hide world pickup visual
        if (worldVisual != null)
            worldVisual.SetActive(false);

        // If we're the owner, show held visual
        if (IsOwner && ownerClientId.Value == NetworkManager.Singleton.LocalClientId)
        {
            ShowHeldVisual();
        }
    }
    else
    {
        // Show world pickup visual
        if (worldVisual != null)
            worldVisual.SetActive(true);

        // Hide held visual
        HideHeldVisual();
    }
}
```

---

## Step 4: Rewrite Pickup Logic

Replace the entire `PickupItem()` method:

```csharp
/// <summary>
/// Called by client when player attempts to pick up item.
/// </summary>
public void RequestPickup(PlayerInventory playerInventory)
{
    // Client requests pickup from server
    RequestPickupServerRpc(
        playerInventory.GetComponent<NetworkObject>(),
        NetworkObject
    );
}

[ServerRpc(RequireOwnership = false)]
private void RequestPickupServerRpc(
    NetworkObjectReference playerRef,
    NetworkObjectReference itemRef,
    ServerRpcParams rpcParams = default)
{
    // === VALIDATION PHASE ===

    // 1. Validate player exists
    if (!playerRef.TryGet(out NetworkObject playerNetObj))
    {
        Debug.LogError("Invalid player reference in pickup request");
        return;
    }

    PlayerInventory inventory = playerNetObj.GetComponent<PlayerInventory>();
    if (inventory == null)
    {
        Debug.LogError("Player has no inventory component");
        return;
    }

    // 2. Validate item exists
    if (!itemRef.TryGet(out NetworkObject itemNetObj))
    {
        Debug.LogError("Invalid item reference in pickup request");
        return;
    }

    // 3. Check if already picked up
    if (isPickedUp.Value)
    {
        Debug.LogWarning($"Item {itemNetObj.name} already picked up");
        return;
    }

    // 4. Check inventory space
    if (!inventory.HasSpace())
    {
        Debug.LogWarning($"Player {rpcParams.Receive.SenderClientId} inventory full");
        // Optionally notify player via ClientRpc
        NotifyInventoryFullClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { rpcParams.Receive.SenderClientId }
            }
        });
        return;
    }

    // 5. Validate distance
    float distance = Vector3.Distance(
        playerNetObj.transform.position,
        itemNetObj.transform.position
    );

    if (distance > maxPickupDistance)
    {
        Debug.LogWarning($"Player {rpcParams.Receive.SenderClientId} too far from item: {distance}m");
        return;
    }

    // === GRANT PHASE ===

    // All validations passed, grant pickup
    GrantPickup(inventory, playerNetObj, rpcParams.Receive.SenderClientId);
}

private void GrantPickup(PlayerInventory inventory, NetworkObject playerNetObj, ulong clientId)
{
    // 1. Mark as picked up (triggers OnPickupStateChanged on all clients)
    isPickedUp.Value = true;
    ownerClientId.Value = clientId;

    // 2. Add to player's inventory
    inventory.AddItemServerInternal(NetworkObject);

    // 3. Change ownership to player
    NetworkObject.ChangeOwnership(clientId);

    // 4. Parent to player's hand
    Transform handTransform = playerNetObj.GetComponent<PlayerController>()?.GetHandTransform();
    if (handTransform != null)
    {
        transform.SetParent(handTransform);
        transform.localPosition = heldItemLocalPosition;
        transform.localRotation = Quaternion.Euler(heldItemLocalRotation);
    }

    // 5. Notify all clients
    OnPickedUpClientRpc(clientId);

    Debug.Log($"Item {NetworkObject.name} picked up by client {clientId}");
}

[ClientRpc]
private void OnPickedUpClientRpc(ulong pickerId)
{
    // Play pickup sound/effects on all clients
    if (pickupSound != null)
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);

    // If we're the picker, show feedback
    if (pickerId == NetworkManager.Singleton.LocalClientId)
    {
        ShowPickupFeedback();
    }
}

[ClientRpc]
private void NotifyInventoryFullClientRpc(ClientRpcParams clientRpcParams = default)
{
    // Show UI message to player
    UIManager.Instance?.ShowMessage("Inventory Full!");
}
```

---

## Step 5: Add Helper Methods

```csharp
private void ShowHeldVisual()
{
    if (heldItemVisualPrefab != null && heldItemTransform == null)
    {
        // Instantiate held visual locally (client-side only)
        GameObject heldObj = Instantiate(heldItemVisualPrefab, transform);
        heldItemTransform = heldObj.transform;
        heldItemTransform.localPosition = Vector3.zero;
        heldItemTransform.localRotation = Quaternion.identity;
    }

    if (heldItemTransform != null)
        heldItemTransform.gameObject.SetActive(true);
}

private void HideHeldVisual()
{
    if (heldItemTransform != null)
    {
        heldItemTransform.gameObject.SetActive(false);
    }
}

private void ShowPickupFeedback()
{
    // Local UI feedback
    Debug.Log($"You picked up {itemName}");

    // Could add UI popup, sound, etc.
}
```

---

## Step 6: Update PlayerInventory

**File:** `Assets\_Project\Code\Gameplay\Player\RefactorInventory\PlayerInventory.cs`

### Remove Placeholder Initialization

**Before:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    if (IsOwner)
    {
        // BAD: Creating 5 placeholder entries
        for (int i = 0; i < 5; i++)
        {
            RequestAddToListServerRpc();
        }
    }
}
```

**After:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    // Subscribe to inventory changes
    InventoryNetworkRefs.OnListChanged += OnInventoryChanged;

    // Initialize UI
    UpdateInventoryUI();
}

public override void OnNetworkDespawn()
{
    base.OnNetworkDespawn();

    // Unsubscribe
    InventoryNetworkRefs.OnListChanged -= OnInventoryChanged;
}

private void OnInventoryChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
{
    // Update UI when inventory changes
    UpdateInventoryUI();

    Debug.Log($"Inventory changed: {changeEvent.Type} at index {changeEvent.Index}");
}
```

### Add Helper Methods

```csharp
/// <summary>
/// Check if inventory has space for new item.
/// </summary>
public bool HasSpace()
{
    return InventoryNetworkRefs.Count < maxInventorySize;
}

/// <summary>
/// Called by server to add item to inventory.
/// DO NOT call this directly - use item.RequestPickup() instead.
/// </summary>
public void AddItemServerInternal(NetworkObjectReference itemRef)
{
    if (!IsServer)
    {
        Debug.LogError("AddItemServerInternal should only be called on server");
        return;
    }

    if (!HasSpace())
    {
        Debug.LogWarning("No inventory space");
        return;
    }

    InventoryNetworkRefs.Add(itemRef);
}

private void UpdateInventoryUI()
{
    // Update inventory UI on clients
    // This will be called automatically when NetworkList changes

    for (int i = 0; i < InventoryNetworkRefs.Count; i++)
    {
        if (InventoryNetworkRefs[i].TryGet(out NetworkObject itemObj))
        {
            BaseInventoryItem item = itemObj.GetComponent<BaseInventoryItem>();
            // Update UI slot with item info
            UpdateInventorySlot(i, item);
        }
    }
}
```

---

## Step 7: Update Interaction System

**File:** Player interaction script (e.g., `PlayerController.cs` or `PlayerInteraction.cs`)

**Before:**
```csharp
if (Input.GetKeyDown(KeyCode.E))
{
    if (nearbyItem != null)
    {
        nearbyItem.PickupItem(inventory);
    }
}
```

**After:**
```csharp
if (Input.GetKeyDown(KeyCode.E))
{
    if (!IsOwner) return;  // Only local player

    if (nearbyItem != null)
    {
        nearbyItem.RequestPickup(inventory);
    }
}
```

---

## Step 8: Testing Checklist

### Test 1: Basic Pickup (Host)
- [ ] Start host
- [ ] Walk to item
- [ ] Press interact key
- [ ] Verify item disappears from world
- [ ] Verify item appears in inventory
- [ ] Verify item appears in hand

### Test 2: Basic Pickup (Client)
- [ ] Start host
- [ ] Start client
- [ ] Client walks to item
- [ ] Client presses interact key
- [ ] Verify item disappears on both host and client
- [ ] Verify item appears in client's inventory on both screens

### Test 3: Distance Validation
- [ ] Stand far from item (>3m)
- [ ] Try to pick up
- [ ] Verify pickup fails
- [ ] Check console for "too far" warning

### Test 4: Inventory Full
- [ ] Fill inventory (pick up max items)
- [ ] Try to pick up another item
- [ ] Verify pickup fails
- [ ] Verify "Inventory Full" message appears

### Test 5: Race Condition (Critical!)
- [ ] Start host and client
- [ ] Both players stand near same item
- [ ] Both press interact at same time
- [ ] Verify only ONE player gets item
- [ ] Verify no duplication
- [ ] Verify other player sees appropriate feedback

### Test 6: Rapid Pickups
- [ ] Place multiple items in a line
- [ ] Walk through picking up all items rapidly
- [ ] Verify all items picked up correctly
- [ ] Verify no items duplicated
- [ ] Verify inventory synced correctly

### Test 7: Late Join
- [ ] Host picks up items
- [ ] Client joins after
- [ ] Verify client sees items in host's inventory
- [ ] Verify items not in world for client

---

## Common Issues & Solutions

### Issue 1: Item Not Picked Up

**Symptom:** Player presses interact, nothing happens

**Debug Steps:**
1. Check console for error messages
2. Verify distance (add debug log in validation)
3. Verify inventory has space
4. Verify NetworkObject is spawned on item

**Solution:**
```csharp
// Add debug logging
[ServerRpc(RequireOwnership = false)]
private void RequestPickupServerRpc(/* ... */)
{
    Debug.Log($"Pickup request from {rpcParams.Receive.SenderClientId}");

    if (!playerRef.TryGet(out NetworkObject playerNetObj))
    {
        Debug.LogError("❌ Player reference invalid");
        return;
    }
    Debug.Log("✓ Player reference valid");

    // ... continue with other checks
}
```

---

### Issue 2: Item Duplicated

**Symptom:** Same item appears in multiple inventories

**Cause:** Race condition - validation not atomic

**Solution:**
Ensure `isPickedUp` check and set are close together:
```csharp
// Check and set in same method, on server only
if (isPickedUp.Value)
{
    return;  // Already picked up
}

// Immediately mark as picked up
isPickedUp.Value = true;

// Continue with pickup logic
```

---

### Issue 3: Item Visual Not Showing

**Symptom:** Item disappears from world but doesn't appear in hand

**Cause:** Hand transform not found, or parenting failed

**Solution:**
```csharp
// Add null checks and logging
Transform handTransform = playerNetObj.GetComponent<PlayerController>()?.GetHandTransform();
if (handTransform == null)
{
    Debug.LogError($"Player {clientId} has no hand transform!");
    // Fallback: just parent to player root
    transform.SetParent(playerNetObj.transform);
}
else
{
    transform.SetParent(handTransform);
    Debug.Log($"Item parented to hand: {handTransform.name}");
}
```

---

### Issue 4: Ownership Change Failed

**Symptom:** Console warning: "Trying to change ownership of an object but you are not the owner"

**Cause:** Calling ChangeOwnership from client, or on non-spawned object

**Solution:**
```csharp
// Only server can change ownership
if (!IsServer)
{
    Debug.LogError("ChangeOwnership must be called on server");
    return;
}

// Verify object is spawned
if (!NetworkObject.IsSpawned)
{
    Debug.LogError("Cannot change ownership of non-spawned object");
    return;
}

// Now safe to change ownership
NetworkObject.ChangeOwnership(clientId);
```

---

## Performance Comparison

### Before:
- 60+ lines of pickup logic
- 2-3 coroutines per pickup
- Multiple frame delays
- Inconsistent state during pickup

### After:
- ~20 lines of core pickup logic
- 0 coroutines
- Instant pickup (validated on server)
- Consistent state via NetworkVariables

---

## Rollback Plan

If issues occur:

```bash
# Restore backup
cp Assets/_Project/Code/Gameplay/NewItemSystem/Items/BaseInventoryItem.cs.backup Assets/_Project/Code/Gameplay/NewItemSystem/Items/BaseInventoryItem.cs

# Or Git revert
git checkout HEAD -- Assets/_Project/Code/Gameplay/NewItemSystem/Items/BaseInventoryItem.cs
```

---

## Next Steps

After completing this refactor:

1. Test extensively with 2+ clients
2. Update all item types (weapons, flashlight, etc.) to use new system
3. Proceed to Phase 3: Enemy State Machine sync
4. Document any additional issues

---

**Estimated Time:** 4-6 hours
**Risk Level:** High (core gameplay system)
**Rollback Difficulty:** Easy (backup exists)

---

**Last Updated:** 2025-11-07
