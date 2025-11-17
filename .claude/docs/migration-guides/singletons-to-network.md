# Migration Guide: Singletons to NetworkSingleton

**Target:** Convert existing singleton patterns to unified NetworkSingleton<T> pattern
**Difficulty:** Medium
**Time Estimate:** 2-4 hours
**Phase:** 2

---

## Overview

This guide walks through converting the 3 different singleton patterns currently used in the codebase to a single, consistent NetworkSingleton<T> pattern.

**Current State:**
- 3 different singleton implementations
- Mix of networked and non-networked singletons
- Inconsistent initialization and cleanup

**Target State:**
- Single NetworkSingleton<T> base class
- Consistent pattern across all networked managers
- Proper cleanup and lifecycle management

---

## Step 1: Create NetworkSingleton Base Class

**File:** `Assets\_Project\Code\Core\Patterns\NetworkSingleton.cs`

Create this new file:

```csharp
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Base class for networked singleton managers.
/// Ensures only one instance exists and persists across scenes.
/// </summary>
/// <typeparam name="T">The singleton type (must inherit from NetworkBehaviour)</typeparam>
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    /// <summary>
    /// Global access point for the singleton instance.
    /// </summary>
    public static T Instance { get; private set; }

    /// <summary>
    /// Whether this singleton should persist between scene loads.
    /// Override to return false if you want it destroyed on scene change.
    /// </summary>
    protected virtual bool PersistBetweenScenes => true;

    protected virtual void Awake()
    {
        // Check if instance already exists
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[{typeof(T).Name}] Multiple instances detected. Destroying duplicate on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // Set instance
        Instance = this as T;

        // Persist between scenes if configured
        if (PersistBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        Debug.Log($"[{typeof(T).Name}] Singleton initialized");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // Clear instance reference if we're the active instance
        if (Instance == this)
        {
            Instance = null;
            Debug.Log($"[{typeof(T).Name}] Singleton destroyed");
        }
    }

    /// <summary>
    /// Called when the NetworkObject is spawned.
    /// Override this to add custom initialization logic.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[{typeof(T).Name}] Network spawned. IsServer={IsServer}, IsClient={IsClient}");
    }

    /// <summary>
    /// Called when the NetworkObject is despawned.
    /// Override this to add custom cleanup logic.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Debug.Log($"[{typeof(T).Name}] Network despawned");
    }
}
```

---

## Step 2: Migrate PlayerListManager

**File:** `Assets\_Project\Code\Network\PlayerList\PlayerListManager.cs`

### Before:
```csharp
public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // ... rest of code
}
```

### After:
```csharp
public class PlayerListManager : NetworkSingleton<PlayerListManager>
{
    // Remove Instance property - inherited from base
    // Remove Awake() - inherited from base (unless you need custom logic)

    // If you need custom Awake logic:
    protected override void Awake()
    {
        base.Awake();  // IMPORTANT: Call base first

        // Your custom initialization here
    }

    // ... rest of code unchanged
}
```

### Testing:
1. Start host
2. Verify console shows: `[PlayerListManager] Singleton initialized`
3. Start client
4. Verify no duplicate messages
5. Test accessing `PlayerListManager.Instance` from another script

---

## Step 3: Migrate NetworkRelay

**File:** `Assets\_Project\Code\Network\NetworkRelay.cs`

### Before:
```csharp
public class NetworkRelay : MonoBehaviour  // ⚠️ Not even NetworkBehaviour!
{
    public static NetworkRelay Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // ... rest of code
}
```

### After:
```csharp
public class NetworkRelay : NetworkSingleton<NetworkRelay>
{
    // Remove Instance property
    // Remove Awake() unless custom logic needed

    // If you had non-network code, you may need to adjust it
    // to work with NetworkBehaviour lifecycle

    // Example: If you had Start() doing network stuff:
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Move network initialization here
        if (IsServer)
        {
            InitializeRelay();
        }
    }

    // ... rest of code
}
```

### ⚠️ Important Notes:
- `NetworkRelay` was previously just `MonoBehaviour`, now it's `NetworkBehaviour`
- This means it MUST be on a GameObject with a `NetworkObject` component
- Update the prefab/scene object to include `NetworkObject`
- If it was doing network operations before, they may now work correctly!

---

## Step 4: Migrate SteamLobbyManager

**File:** `Assets\_Project\Code\Network\SteamLobby\SteamLobbyManager.cs`

### Before:
```csharp
private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

### After:
```csharp
public class SteamLobbyManager : NetworkSingleton<SteamLobbyManager>
{
    // Remove entire Awake() - base class handles it correctly
    // Remove Instance property

    // If you had custom initialization in Awake:
    protected override void Awake()
    {
        base.Awake();  // CRITICAL: Call base first

        // Your custom Steam initialization
        InitializeSteamworks();
    }

    // ... rest of code unchanged
}
```

---

## Step 5: Convert WalletBankton (Critical!)

**File:** `Assets\_Project\Code\Utilities\Singletons\WalletBankton.cs`

This is a CRITICAL conversion because WalletBankton manages game state.

### Before:
```csharp
public class WalletBankton : Singleton<WalletBankton>
{
    public int TotalMoney { get; private set; } = 100;
    public float CurrentResearchProgress { get; private set; } = 0;

    public void AddSubMoney(int amount)
    {
        TotalMoney += amount;
        EventBus.EventBus.Instance.Publish<WalletUpdate>(new WalletUpdate());
    }

    public void AddSubResearch(float amount)
    {
        CurrentResearchProgress += amount;
        EventBus.EventBus.Instance.Publish<ResearchProgressUpdate>(new ResearchProgressUpdate());
    }
}
```

### After:
```csharp
public class WalletBankton : NetworkSingleton<WalletBankton>
{
    // Convert to NetworkVariables
    private NetworkVariable<int> totalMoney = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> currentResearchProgress = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Public read-only access
    public int TotalMoney => totalMoney.Value;
    public float CurrentResearchProgress => currentResearchProgress.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to changes on ALL clients
        totalMoney.OnValueChanged += OnMoneyChanged;
        currentResearchProgress.OnValueChanged += OnResearchChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe to prevent memory leaks
        totalMoney.OnValueChanged -= OnMoneyChanged;
        currentResearchProgress.OnValueChanged -= OnResearchChanged;
    }

    private void OnMoneyChanged(int oldValue, int newValue)
    {
        // Publish event on all clients when money changes
        EventBus.EventBus.Instance.Publish<WalletUpdate>(new WalletUpdate());
        Debug.Log($"Money changed: {oldValue} -> {newValue}");
    }

    private void OnResearchChanged(float oldValue, float newValue)
    {
        // Publish event on all clients when research changes
        EventBus.EventBus.Instance.Publish<ResearchProgressUpdate>(new ResearchProgressUpdate());
        Debug.Log($"Research changed: {oldValue} -> {newValue}");
    }

    // Change to ServerRpc - only server can modify
    [ServerRpc(RequireOwnership = false)]
    public void AddSubMoneyServerRpc(int amount)
    {
        if (amount == 0) return;

        totalMoney.Value += amount;

        // OnValueChanged will automatically trigger event on all clients
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddSubResearchServerRpc(float amount)
    {
        if (amount == 0) return;

        currentResearchProgress.Value += amount;

        // OnValueChanged will automatically trigger event on all clients
    }
}
```

### Update All Callers:

**Before:**
```csharp
WalletBankton.Instance.AddSubMoney(50);
```

**After:**
```csharp
WalletBankton.Instance.AddSubMoneyServerRpc(50);
```

**Find and replace all calls:**
Use Global Find & Replace:
- Find: `WalletBankton.Instance.AddSubMoney(`
- Replace: `WalletBankton.Instance.AddSubMoneyServerRpc(`

- Find: `WalletBankton.Instance.AddSubResearch(`
- Replace: `WalletBankton.Instance.AddSubResearchServerRpc(`

---

## Step 6: Update Scene Setup

For each networked singleton:

1. **Ensure GameObject has NetworkObject component**
   - Select GameObject in scene
   - Add Component → NetworkObject (if not present)
   - Uncheck "Spawn With Observers" (these are global managers)

2. **Add to NetworkPrefabs list**
   - Open NetworkManager in scene
   - Add each singleton to "Network Prefabs List" (if not already there)

3. **Ensure proper spawn**
   - Singletons should be in the scene already (scene-spawned)
   - OR spawned during NetworkManager initialization

---

## Step 7: Testing Checklist

### Test 1: Singleton Initialization
- [ ] Start host
- [ ] Check console for "[ClassName] Singleton initialized" messages
- [ ] Verify no duplicate messages
- [ ] Start client
- [ ] Verify client sees singleton instances

### Test 2: Money Synchronization (Critical!)
- [ ] Start host
- [ ] Start client
- [ ] Host: Earn money (kill enemy, complete mission, etc.)
- [ ] Client: Verify money UI updates
- [ ] Client: Check `WalletBankton.Instance.TotalMoney` matches host
- [ ] Client: Earn money
- [ ] Host: Verify money UI updates

### Test 3: Scene Transitions
- [ ] Start host
- [ ] Verify singleton exists
- [ ] Load new scene
- [ ] Verify singleton still exists (DontDestroyOnLoad working)
- [ ] Verify no duplicates created

### Test 4: Network Spawn/Despawn
- [ ] Start host
- [ ] Stop host
- [ ] Check console for "Network despawned" messages
- [ ] Verify no errors

### Test 5: Multiple Clients
- [ ] Start host
- [ ] Start client 1
- [ ] Start client 2
- [ ] Host: Change money
- [ ] Verify all clients see update
- [ ] Client 1: Change money
- [ ] Verify host and client 2 see update

---

## Common Issues & Solutions

### Issue 1: "Instance is null" Error

**Symptom:** Other scripts can't access `ClassName.Instance`

**Cause:** Singleton Awake() not called yet, or object destroyed

**Solution:**
- Ensure script execution order (Edit → Project Settings → Script Execution Order)
- Or use lazy initialization:
```csharp
public static T GetInstance()
{
    if (Instance == null)
    {
        Instance = FindFirstObjectByType<T>();
    }
    return Instance;
}
```

---

### Issue 2: Duplicate Instances Created

**Symptom:** Console shows multiple "[ClassName] Singleton initialized" messages

**Cause:** Multiple GameObjects with the singleton script

**Solution:**
- Search scene for all instances: Hierarchy → Search for script name
- Delete duplicates
- Ensure only one instance in scene

---

### Issue 3: NetworkVariable Not Syncing

**Symptom:** Money changes on server but not on clients

**Cause:** NetworkObject not spawned, or script not NetworkBehaviour

**Solution:**
- Verify GameObject has NetworkObject component
- Verify NetworkObject is spawned (check NetworkManager UI)
- Verify script inherits from NetworkBehaviour
- Check console for network errors

---

### Issue 4: "Calling ServerRpc Before Spawn"

**Symptom:** Error when calling ServerRpc too early

**Cause:** Calling ServerRpc before NetworkObject is spawned

**Solution:**
```csharp
public void DoSomething()
{
    if (!IsSpawned)
    {
        Debug.LogWarning("Not spawned yet, deferring action");
        StartCoroutine(WaitForSpawnAndDoSomething());
        return;
    }

    DoSomethingServerRpc();
}

private IEnumerator WaitForSpawnAndDoSomething()
{
    yield return new WaitUntil(() => IsSpawned);
    DoSomethingServerRpc();
}
```

---

## Rollback Plan

If something goes wrong:

1. **Git Reset:**
   ```bash
   git checkout -- Assets/_Project/Code/Core/Patterns/
   git checkout -- Assets/_Project/Code/Network/
   git checkout -- Assets/_Project/Code/Utilities/Singletons/
   ```

2. **Keep Changes, Fix Issues:**
   - Comment out problematic code
   - Use old pattern temporarily
   - File bug report with details

---

## Verification Script

Create this script to verify all singletons are working:

**File:** `Assets/_Project/Code/Utilities/SingletonVerifier.cs`

```csharp
using Unity.Netcode;
using UnityEngine;

public class SingletonVerifier : MonoBehaviour
{
    [ContextMenu("Verify All Singletons")]
    private void VerifyAllSingletons()
    {
        Debug.Log("=== Singleton Verification ===");

        CheckSingleton<PlayerListManager>();
        CheckSingleton<NetworkRelay>();
        CheckSingleton<SteamLobbyManager>();
        CheckSingleton<WalletBankton>();

        Debug.Log("=== Verification Complete ===");
    }

    private void CheckSingleton<T>() where T : NetworkBehaviour
    {
        T instance = FindFirstObjectByType<T>();

        if (instance == null)
        {
            Debug.LogError($"[{typeof(T).Name}] NOT FOUND in scene!");
            return;
        }

        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError($"[{typeof(T).Name}] Missing NetworkObject component!");
            return;
        }

        Debug.Log($"[{typeof(T).Name}] ✓ Found, IsSpawned={netObj.IsSpawned}");
    }
}
```

---

## Next Steps

After completing this migration:

1. Update all code that accesses these singletons
2. Test thoroughly in multiplayer scenarios
3. Proceed to Phase 2 Item Pickup refactor
4. Document any issues encountered

---

**Estimated Time:** 2-4 hours
**Risk Level:** Medium
**Rollback Difficulty:** Easy (Git revert)

---

**Last Updated:** 2025-11-07
