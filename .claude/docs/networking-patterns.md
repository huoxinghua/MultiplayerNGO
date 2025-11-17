# Unity Netcode for GameObjects - Standard Patterns

**Version:** 1.0
**Last Updated:** 2025-11-07
**For:** MultiplayerNGO Project

This document outlines the standardized networking patterns to use throughout the codebase. Following these patterns ensures consistency, prevents common bugs, and makes code easier to maintain.

---

## Table of Contents

1. [Authority Patterns](#authority-patterns)
2. [RPC Patterns](#rpc-patterns)
3. [NetworkVariable Patterns](#networkvariable-patterns)
4. [Singleton Patterns](#singleton-patterns)
5. [Spawning Patterns](#spawning-patterns)
6. [State Synchronization](#state-synchronization)
7. [Validation Patterns](#validation-patterns)
8. [Common Mistakes to Avoid](#common-mistakes-to-avoid)

---

## Authority Patterns

### Pattern 1: Server Authority for Game Logic

**Rule:** All gameplay logic that affects state should execute on the server only.

**Correct:**
```csharp
public class EnemyHealth : NetworkBehaviour
{
    private NetworkVariable<float> health = new NetworkVariable<float>(100f);

    // Public method that anyone can call
    public void TakeDamage(float damage)
    {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(float damage)
    {
        // Only executes on server
        health.Value -= damage;

        if (health.Value <= 0)
        {
            Die();
        }
    }
}
```

**Incorrect (Double execution on host):**
```csharp
public void TakeDamage(float damage)
{
    if (!IsServer)
    {
        TakeDamageServerRpc(damage);
        return;
    }
    // This will execute twice on host!
    health.Value -= damage;
}
```

---

### Pattern 2: Client Authority for Local Input

**Rule:** Client processes input locally, sends commands to server for validation.

**Correct:**
```csharp
public class PlayerController : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return;  // Only process input for local player

        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        if (input != Vector2.zero)
        {
            MoveServerRpc(input);
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 input)
    {
        // Server validates and applies movement
        if (input.magnitude > 1f)
            input = input.normalized;  // Prevent cheating

        transform.position += new Vector3(input.x, 0, input.y) * moveSpeed * Time.deltaTime;
    }
}
```

---

### Pattern 3: Ownership-Based Authority

**Rule:** Objects owned by a client can have client-side prediction with server reconciliation.

**Use when:**
- Player movement (owned by player)
- Player-held items (ownership transfers)

**Don't use for:**
- World objects (server owns)
- Enemy AI (server owns)
- Shared resources (server owns)

```csharp
public class PlayerMovement : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return;

        // Client-side prediction: move immediately
        Vector3 movement = GetInputMovement();
        transform.position += movement;

        // Send to server for validation
        MoveServerRpc(transform.position);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 newPosition)
    {
        // Server validates
        if (Vector3.Distance(transform.position, newPosition) > maxMoveDistance)
        {
            // Cheating detected, force reconciliation
            ReconcilePositionClientRpc(transform.position);
            return;
        }

        // Accept movement
        transform.position = newPosition;
    }

    [ClientRpc]
    private void ReconcilePositionClientRpc(Vector3 serverPosition)
    {
        if (IsOwner)
            transform.position = serverPosition;  // Snap to server position
    }
}
```

---

## RPC Patterns

### Pattern 4: ServerRpc Standard Flow

**Rule:** Use ServerRpc for client → server communication

```csharp
// Naming: [Action]ServerRpc
[ServerRpc(RequireOwnership = false)]  // Only disable if ANY client can call
private void ActionNameServerRpc(paramType param, ServerRpcParams rpcParams = default)
{
    // 1. Validate input
    if (!ValidateAction(param, rpcParams.Receive.SenderClientId))
    {
        Debug.LogWarning($"Invalid RPC from {rpcParams.Receive.SenderClientId}");
        return;
    }

    // 2. Execute server logic
    DoServerLogic(param);

    // 3. Sync to clients if needed
    SyncActionClientRpc(param);
}
```

**When to use RequireOwnership = false:**
- Any client can damage any enemy (combat)
- Any client can interact with world objects (doors, buttons)
- Server-owned objects receiving input from any client

**When to use RequireOwnership = true (default):**
- Player modifying their own state (inventory, health)
- Player picking up items
- Player using abilities

---

### Pattern 5: ClientRpc Standard Flow

**Rule:** Use ClientRpc for server → clients communication

```csharp
// Naming: [Action]ClientRpc
[ClientRpc]
private void ActionNameClientRpc(paramType param, ClientRpcParams clientRpcParams = default)
{
    // This runs on ALL clients (including host)

    // If server should skip:
    if (IsServer) return;

    // Client-side visual/audio effects
    PlayEffect(param);
}
```

**Targeting Specific Clients:**
```csharp
private void NotifyPlayer(ulong clientId, string message)
{
    ClientRpcParams clientRpcParams = new ClientRpcParams
    {
        Send = new ClientRpcSendParams
        {
            TargetClientIds = new ulong[] { clientId }
        }
    };

    ShowMessageClientRpc(message, clientRpcParams);
}

[ClientRpc]
private void ShowMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
{
    UI.ShowMessage(message);
}
```

---

### Pattern 6: RPC Error Handling

**Rule:** Always validate RPC parameters and handle errors gracefully

```csharp
[ServerRpc(RequireOwnership = false)]
private void PickupItemServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
{
    // 1. Validate reference
    if (!itemRef.TryGet(out NetworkObject itemObject))
    {
        Debug.LogError($"Client {rpcParams.Receive.SenderClientId} sent invalid item reference");
        return;
    }

    // 2. Validate item state
    BaseItem item = itemObject.GetComponent<BaseItem>();
    if (item == null || item.IsPickedUp)
    {
        Debug.LogWarning($"Item {itemObject.name} is not available for pickup");
        return;
    }

    // 3. Validate distance
    if (!NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient client))
    {
        Debug.LogError($"Unknown client {rpcParams.Receive.SenderClientId}");
        return;
    }

    float distance = Vector3.Distance(client.PlayerObject.transform.position, itemObject.transform.position);
    if (distance > maxPickupDistance)
    {
        Debug.LogWarning($"Client {rpcParams.Receive.SenderClientId} too far from item: {distance}m");
        return;
    }

    // 4. Execute pickup
    GrantPickup(client.PlayerObject, itemObject);
}
```

---

## NetworkVariable Patterns

### Pattern 7: NetworkVariable Declaration

**Rule:** Use NetworkVariable for synchronized state

```csharp
public class PlayerHealth : NetworkBehaviour
{
    // Use NetworkVariable for state that must sync
    private NetworkVariable<float> health = new NetworkVariable<float>(
        100f,  // Default value
        NetworkVariableReadPermission.Everyone,  // Who can read
        NetworkVariableWritePermission.Server    // Who can write (usually Server)
    );

    // Public read-only access
    public float Health => health.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to changes
        health.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe to prevent memory leaks
        health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        // React to changes on ALL clients
        Debug.Log($"Health changed from {oldValue} to {newValue}");
        UpdateHealthUI(newValue);

        if (newValue <= 0)
            PlayDeathAnimation();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        // Only server can modify
        health.Value = Mathf.Max(0, health.Value - damage);
    }
}
```

---

### Pattern 8: NetworkList Usage

**Rule:** Use NetworkList for synchronized collections

```csharp
public class PlayerInventory : NetworkBehaviour
{
    // NetworkList for dynamic collections
    private NetworkList<NetworkObjectReference> items;

    private void Awake()
    {
        // Initialize in Awake (before OnNetworkSpawn)
        items = new NetworkList<NetworkObjectReference>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to collection changes
        items.OnListChanged += OnInventoryChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe
        items.OnListChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<NetworkObjectReference>.EventType.Add:
                Debug.Log($"Item added at index {changeEvent.Index}");
                UpdateInventoryUI();
                break;

            case NetworkListEvent<NetworkObjectReference>.EventType.Remove:
                Debug.Log($"Item removed at index {changeEvent.Index}");
                UpdateInventoryUI();
                break;

            case NetworkListEvent<NetworkObjectReference>.EventType.Value:
                Debug.Log($"Item changed at index {changeEvent.Index}");
                UpdateInventoryUI();
                break;
        }
    }

    [ServerRpc]
    public void AddItemServerRpc(NetworkObjectReference itemRef)
    {
        if (items.Count >= maxInventorySize)
        {
            Debug.LogWarning("Inventory full");
            return;
        }

        items.Add(itemRef);
    }
}
```

---

### Pattern 9: Custom NetworkVariable Serialization

**Rule:** Use INetworkSerializable for custom types

```csharp
public struct ItemData : INetworkSerializable
{
    public int itemId;
    public int quantity;
    public float durability;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemId);
        serializer.SerializeValue(ref quantity);
        serializer.SerializeValue(ref durability);
    }
}

public class Inventory : NetworkBehaviour
{
    private NetworkVariable<ItemData> equippedItem = new NetworkVariable<ItemData>();

    [ServerRpc]
    private void EquipItemServerRpc(int itemId)
    {
        equippedItem.Value = new ItemData
        {
            itemId = itemId,
            quantity = 1,
            durability = 100f
        };
    }
}
```

---

## Singleton Patterns

### Pattern 10: NetworkSingleton Base Class

**Rule:** Use NetworkSingleton<T> for networked singleton managers

**Create this base class:**
```csharp
using Unity.Netcode;
using UnityEngine;

public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple instances of {typeof(T).Name} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
```

**Usage:**
```csharp
public class GameManager : NetworkSingleton<GameManager>
{
    private NetworkVariable<int> score = new NetworkVariable<int>(0);

    public int Score => score.Value;

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int points)
    {
        score.Value += points;
    }
}

// Accessing from any script:
GameManager.Instance.AddScoreServerRpc(10);
```

---

### Pattern 11: Non-Networked Singleton (Local State Only)

**Rule:** Use regular Singleton<T> ONLY for client-local state (UI, audio settings, etc.)

**Never use for:**
- Game state (money, score, progress)
- Player data
- World state
- Anything that affects gameplay

**OK to use for:**
- Audio manager (local settings)
- Input manager (local mapping)
- UI manager (local state)
- Settings manager (local preferences)

```csharp
public class AudioManager : Singleton<AudioManager>
{
    // Local audio settings - not synced
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public float SFXVolume { get; set; } = 0.8f;

    public void PlaySound(AudioClip clip)
    {
        // Plays locally only
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, SFXVolume);
    }
}
```

---

## Spawning Patterns

### Pattern 12: Server-Only Spawning

**Rule:** Only server spawns NetworkObjects

```csharp
public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private void Start()
    {
        if (!IsServer) return;  // Critical: only server spawns

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        // 1. Instantiate
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // 2. Get NetworkObject component
        NetworkObject networkObject = enemyObject.GetComponent<NetworkObject>();

        // 3. Spawn on network
        networkObject.Spawn();  // Automatically replicates to all clients

        Debug.Log($"Enemy spawned with NetworkObjectId: {networkObject.NetworkObjectId}");
    }
}
```

---

### Pattern 13: Spawning with Ownership

**Rule:** Spawn objects and assign ownership when needed

```csharp
public class ItemPickup : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void PickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Create held item visual
        GameObject heldItem = Instantiate(heldItemPrefab);
        NetworkObject heldNetObj = heldItem.GetComponent<NetworkObject>();

        // Spawn and assign ownership to picker
        heldNetObj.SpawnWithOwnership(clientId);

        // Now client has authority over this object
        NotifyPickupClientRpc(heldNetObj, clientId);
    }

    [ClientRpc]
    private void NotifyPickupClientRpc(NetworkObjectReference itemRef, ulong ownerId)
    {
        if (NetworkManager.Singleton.LocalClientId == ownerId)
        {
            // Local player picked up item
            AttachToHand(itemRef);
        }
    }
}
```

---

### Pattern 14: Pooling NetworkObjects

**Rule:** Use object pooling for frequently spawned objects

```csharp
public class ProjectilePool : NetworkSingleton<ProjectilePool>
{
    [SerializeField] private GameObject projectilePrefab;
    private Queue<NetworkObject> pool = new Queue<NetworkObject>();

    public void SpawnProjectile(Vector3 position, Vector3 direction)
    {
        if (!IsServer) return;

        NetworkObject projectile;

        if (pool.Count > 0)
        {
            // Reuse from pool
            projectile = pool.Dequeue();
            projectile.gameObject.SetActive(true);
            projectile.transform.position = position;
        }
        else
        {
            // Create new
            GameObject obj = Instantiate(projectilePrefab, position, Quaternion.identity);
            projectile = obj.GetComponent<NetworkObject>();
            projectile.Spawn();
        }

        // Initialize projectile
        projectile.GetComponent<Projectile>().Launch(direction);
    }

    public void ReturnToPool(NetworkObject projectile)
    {
        if (!IsServer) return;

        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
    }
}
```

---

## State Synchronization

### Pattern 15: State Machine Network Sync

**Rule:** Sync state machines using NetworkVariable<enum>

```csharp
public enum EnemyState
{
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Dead
}

public class EnemyAI : NetworkBehaviour
{
    private NetworkVariable<EnemyState> currentState = new NetworkVariable<EnemyState>(EnemyState.Idle);

    private IEnemyState[] states;
    private IEnemyState activeState;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialize states
        states = new IEnemyState[]
        {
            new IdleState(this),
            new PatrollingState(this),
            new ChasingState(this),
            new AttackingState(this),
            new DeadState(this)
        };

        // Subscribe to state changes
        currentState.OnValueChanged += OnStateChanged;

        // Initialize to current state
        OnStateChanged(EnemyState.Idle, currentState.Value);
    }

    private void Update()
    {
        if (!IsServer) return;  // Only server updates logic

        // Server-side state machine logic
        activeState?.Update();
    }

    private void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        // Runs on ALL clients when state changes
        Debug.Log($"Enemy state changed: {oldState} -> {newState}");

        // Exit old state
        activeState?.Exit();

        // Enter new state
        activeState = states[(int)newState];
        activeState?.Enter();

        // Update animations (on all clients)
        UpdateAnimation(newState);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TransitionToServerRpc(EnemyState newState)
    {
        if (currentState.Value != newState)
        {
            currentState.Value = newState;
        }
    }

    private void UpdateAnimation(EnemyState state)
    {
        // Visual updates happen on all clients
        animator.SetInteger("State", (int)state);
    }
}
```

---

## Validation Patterns

### Pattern 16: Server-Side Validation

**Rule:** Never trust client input, always validate on server

```csharp
public class PlayerActions : NetworkBehaviour
{
    [SerializeField] private float maxInteractionDistance = 3f;
    [SerializeField] private float actionCooldown = 1f;
    private float lastActionTime;

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc(NetworkObjectReference targetRef, ServerRpcParams rpcParams = default)
    {
        // 1. Validate cooldown
        if (Time.time - lastActionTime < actionCooldown)
        {
            Debug.LogWarning($"Client {rpcParams.Receive.SenderClientId} spamming interactions");
            return;
        }

        // 2. Validate target exists
        if (!targetRef.TryGet(out NetworkObject targetObject))
        {
            Debug.LogError("Invalid target reference");
            return;
        }

        // 3. Validate player exists
        if (!NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient client))
        {
            Debug.LogError($"Unknown client {rpcParams.Receive.SenderClientId}");
            return;
        }

        // 4. Validate distance
        Vector3 playerPos = client.PlayerObject.transform.position;
        Vector3 targetPos = targetObject.transform.position;
        float distance = Vector3.Distance(playerPos, targetPos);

        if (distance > maxInteractionDistance)
        {
            Debug.LogWarning($"Client {rpcParams.Receive.SenderClientId} too far from target: {distance}m > {maxInteractionDistance}m");
            return;
        }

        // 5. Execute interaction
        lastActionTime = Time.time;
        IInteractable interactable = targetObject.GetComponent<IInteractable>();
        interactable?.Interact(client.PlayerObject);
    }
}
```

---

## Common Mistakes to Avoid

### Mistake 1: Double Execution on Host

**Wrong:**
```csharp
public void DealDamage(float damage)
{
    if (!IsServer)
    {
        DealDamageServerRpc(damage);
        return;
    }
    // This runs on host when called directly
    ApplyDamage(damage);
}

[ServerRpc]
private void DealDamageServerRpc(float damage)
{
    // This ALSO runs on host
    ApplyDamage(damage);  // DOUBLE DAMAGE!
}
```

**Correct:**
```csharp
public void DealDamage(float damage)
{
    // Always call ServerRpc
    DealDamageServerRpc(damage);
}

[ServerRpc(RequireOwnership = false)]
private void DealDamageServerRpc(float damage)
{
    // Only executed once, on server
    ApplyDamage(damage);
}
```

---

### Mistake 2: Modifying NetworkVariable on Client

**Wrong:**
```csharp
private NetworkVariable<int> score = new NetworkVariable<int>();

private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        score.Value += 10;  // ERROR: Only server can modify!
    }
}
```

**Correct:**
```csharp
private NetworkVariable<int> score = new NetworkVariable<int>();

private void Update()
{
    if (!IsOwner) return;

    if (Input.GetKeyDown(KeyCode.Space))
    {
        AddScoreServerRpc(10);
    }
}

[ServerRpc]
private void AddScoreServerRpc(int points)
{
    score.Value += points;  // Correct: Server modifies
}
```

---

### Mistake 3: Spawning on Clients

**Wrong:**
```csharp
private void Start()
{
    // Runs on all clients!
    GameObject enemy = Instantiate(enemyPrefab);
    enemy.GetComponent<NetworkObject>().Spawn();  // ERROR: Client can't spawn!
}
```

**Correct:**
```csharp
private void Start()
{
    if (!IsServer) return;  // Only server spawns

    GameObject enemy = Instantiate(enemyPrefab);
    enemy.GetComponent<NetworkObject>().Spawn();
}
```

---

### Mistake 4: Not Unsubscribing from Events

**Wrong:**
```csharp
public override void OnNetworkSpawn()
{
    health.OnValueChanged += OnHealthChanged;
    // Missing unsubscribe = memory leak!
}
```

**Correct:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    health.OnValueChanged += OnHealthChanged;
}

public override void OnNetworkDespawn()
{
    base.OnNetworkDespawn();
    health.OnValueChanged -= OnHealthChanged;  // Always unsubscribe!
}
```

---

### Mistake 5: Using GameObject.Find in Network Code

**Wrong:**
```csharp
private void Start()
{
    GameObject player = GameObject.Find("Player");  // Which player?!
}
```

**Correct:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    if (IsOwner)
    {
        // This is MY player
        SetupLocalPlayer();
    }
}
```

---

## NetworkObjectReference Best Practices

**Added:** 2025-11-07

### What is NetworkObjectReference?

`NetworkObjectReference` is a lightweight struct (8 bytes) that stores a reference to a NetworkObject. It's used to pass object references efficiently over the network.

**What's transmitted:** Just the `NetworkObjectId` (ulong)
**What's NOT transmitted:** The entire GameObject, components, mesh data, etc.

---

### Pattern 1: Passing Object References in RPCs

**❌ WRONG - Passing string/manual ID:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(string corpseName, ulong parentId)
{
    // Expensive scene search!
    var ragdolls = FindObjectsOfType<Ragdoll>();
    foreach (var rag in ragdolls)
    {
        if (rag.ParentId == parentId) { ... }  // O(n) search
    }
}
```

**Problems:**
- `FindObjectsOfType` is very expensive (searches entire scene hierarchy)
- String parameter wastes bandwidth (unused in this case)
- O(n) loop search instead of O(1) lookup

---

**✅ CORRECT - Using NetworkObjectReference:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(NetworkObjectReference ragdollRef)
{
    // Efficient direct lookup
    if (ragdollRef.TryGet(out NetworkObject ragdollNetObj))
    {
        Destroy(ragdollNetObj.gameObject);
    }
    else
    {
        Debug.LogWarning("Failed to resolve NetworkObjectReference");
    }
}

// Caller:
NetworkObject ragdoll = GetComponent<NetworkObject>();
DestroyCorpseClientRpc(ragdoll);  // Implicit conversion
```

**Benefits:**
- ✅ Only 8 bytes transmitted (ulong NetworkObjectId)
- ✅ O(1) dictionary lookup in SpawnManager.SpawnedObjects
- ✅ No scene search
- ✅ Built-in to NGO

---

### Pattern 2: Storing Object References in NetworkVariables

```csharp
public class BruteStateMachine : NetworkBehaviour
{
    // Store reference to target player
    private readonly NetworkVariable<NetworkObjectReference> _playerTargetRef
        = new NetworkVariable<NetworkObjectReference>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            _playerTargetRef.OnValueChanged += OnPlayerTargetRefChanged;
        }
    }

    private void OnPlayerTargetRefChanged(NetworkObjectReference previous, NetworkObjectReference current)
    {
        if (current.TryGet(out NetworkObject playerObj))
        {
            _currentTargetPlayer = playerObj.gameObject;
            UpdateTargetVisuals();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetPlayerServerRpc(NetworkObjectReference playerRef)
    {
        _playerTargetRef.Value = playerRef;  // Sync to all clients
    }
}
```

---

### Pattern 3: Using NetworkList for Collections

```csharp
public class BeetleHealth : NetworkBehaviour
{
    // Store list of hostile players
    private NetworkList<NetworkObjectReference> _hostilePlayers;

    private void Awake()
    {
        _hostilePlayers = new NetworkList<NetworkObjectReference>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            _hostilePlayers.OnListChanged += OnHostilePlayersChanged;
        }
    }

    private void OnHostilePlayersChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
    {
        Debug.Log($"Hostile players changed: {changeEvent.Type}");
        UpdateHostileUI();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddHostilePlayerServerRpc(NetworkObjectReference playerRef)
    {
        if (!_hostilePlayers.Contains(playerRef))
        {
            _hostilePlayers.Add(playerRef);
        }
    }
}
```

---

### Requirements for NetworkObjectReference

For `NetworkObjectReference` to work correctly:

1. ✅ Object must have a `NetworkObject` component
2. ✅ Object must be spawned on the network (via `NetworkObject.Spawn()`)
3. ✅ Object must exist on both server and all clients

If any requirement is false, `TryGet()` will return `false`.

---

### Converting Between Types

```csharp
// GameObject → NetworkObjectReference
GameObject myObject = ...;
NetworkObject netObj = myObject.GetComponent<NetworkObject>();
NetworkObjectReference objRef = netObj;  // Implicit conversion

// NetworkObjectReference → GameObject
NetworkObjectReference objRef = ...;
if (objRef.TryGet(out NetworkObject netObj))
{
    GameObject myObject = netObj.gameObject;
    // Use myObject
}
```

---

### When to Use NetworkObjectReference

| Use Case | Use NetworkObjectReference? |
|----------|---------------------------|
| Passing player/enemy reference in RPC | ✅ YES |
| Storing target reference in NetworkVariable | ✅ YES |
| Inventory system (list of items) | ✅ YES |
| Combat system (attacker/victim references) | ✅ YES |
| AI targeting (which player to chase) | ✅ YES |
| References to non-networked objects | ❌ NO - Won't work |
| References to scene objects not spawned | ❌ NO - Use scene path |

---

## When to Use NetworkVariable vs ServerRpc

**Added:** 2025-11-07

### Decision Flowchart

```
Does the value need to be synchronized across all clients?
│
├─ YES → Continue
│   │
│   ├─ Does it change frequently (multiple times per second)?
│   │   │
│   │   ├─ YES → Use NetworkTransform (for position/rotation)
│   │   │        OR optimize updates (dirty checking)
│   │   │
│   │   └─ NO → Continue
│   │
│   ├─ Do late-joiners need to see the current value?
│   │   │
│   │   ├─ YES → Use NetworkVariable ✅
│   │   │
│   │   └─ NO → Consider ClientRpc (one-time events)
│   │
│   └─ Is it a collection/list?
│       │
│       ├─ YES → Use NetworkList ✅
│       │
│       └─ NO → Use NetworkVariable ✅
│
└─ NO → Use local variable (not networked)
```

---

### NetworkVariable vs ServerRpc Comparison

| Aspect | NetworkVariable | ServerRpc |
|--------|----------------|-----------|
| **Purpose** | Synchronize persistent state | Execute logic on server |
| **Automatic sync** | ✅ Yes | ❌ No |
| **Late-joiner sync** | ✅ Yes (automatic) | ❌ No |
| **Bandwidth** | ✅ Efficient (delta compression) | ⚠️ Every call costs bandwidth |
| **OnValueChanged** | ✅ Built-in callbacks | ❌ Manual ClientRpc needed |
| **Use for** | Health, money, state, inventory | Commands, actions, validation |

---

### Examples by Use Case

#### State Synchronization → Use NetworkVariable

```csharp
// Player health (all clients need to see it, late-joiners too)
private NetworkVariable<float> _health = new NetworkVariable<float>(100f);

// Enemy AI state (clients render correct animation)
private NetworkVariable<int> _currentStateIndex = new NetworkVariable<int>(0);

// Door open/closed (persistent state)
private NetworkVariable<bool> _isOpen = new NetworkVariable<bool>(false);
```

---

#### Actions/Commands → Use ServerRpc

```csharp
// Player performs action (one-time command)
[ServerRpc]
private void AttackServerRpc(NetworkObjectReference targetRef) { ... }

// Item pickup request (validation needed)
[ServerRpc]
private void RequestPickupServerRpc(NetworkObjectReference itemRef) { ... }

// Spawn request (server-only logic)
[ServerRpc]
private void SpawnEnemyServerRpc(Vector3 position) { ... }
```

---

#### One-time Events → Use ClientRpc

```csharp
// Play sound effect on all clients (doesn't need persistence)
[ClientRpc]
private void PlayExplosionEffectClientRpc(Vector3 position) { ... }

// Show UI notification (one-time message)
[ClientRpc]
private void ShowNotificationClientRpc(string message) { ... }
```

---

### The Golden Rule Revisited

> **"If it changes during gameplay and needs to look the same on all clients, it needs a NetworkVariable."**

**Use NetworkVariable for:**
- Health/stamina/resources
- Entity states (alive/dead/stunned)
- Money/currency/score
- Inventory contents
- Quest progress
- Animation states
- Door/switch states
- Any UI-displayed value

**Use ServerRpc for:**
- Player actions (attack, use item, interact)
- Validation requests (can I pickup?, can I buy?)
- Spawn requests
- State change requests (request to open door)

**Use ClientRpc for:**
- Visual effects (explosions, particles)
- Sound effects
- UI notifications
- Camera shake
- One-time events

---

## Anti-Patterns to Avoid

**Added:** 2025-11-07

These patterns have been found in this codebase and should be refactored.

---

### Anti-Pattern #1: FindObjectsOfType in RPCs

**❌ BAD:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(string corpseName, ulong parentId)
{
    var ragdolls = FindObjectsOfType<Ragdoll>();  // VERY EXPENSIVE!
    foreach (var rag in ragdolls)
    {
        if (rag.ParentId == parentId) { ... }
    }
}
```

**Why it's bad:**
- `FindObjectsOfType` searches entire scene hierarchy (extremely slow)
- Called on EVERY client every time
- O(n) performance where n = number of objects in scene

**✅ GOOD:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(NetworkObjectReference ragdollRef)
{
    if (ragdollRef.TryGet(out NetworkObject ragdollNetObj))
    {
        Destroy(ragdollNetObj.gameObject);  // O(1) dictionary lookup
    }
}
```

---

### Anti-Pattern #2: Unused RPC Parameters

**❌ BAD:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(string corpseName, ulong parentId)
{
    // corpseName is NEVER USED!
    var ragdolls = FindObjectsOfType<Ragdoll>();
    foreach (var rag in ragdolls)
    {
        if (rag.ParentId == parentId) { ... }  // Only uses parentId
    }
}
```

**Why it's bad:**
- Wastes bandwidth (strings are expensive to serialize)
- Confuses readers (why is it there?)
- Suggests code smell (unclear intent)

**✅ GOOD:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(NetworkObjectReference ragdollRef)
{
    // Only send what's needed
    if (ragdollRef.TryGet(out NetworkObject ragdollNetObj))
    {
        Destroy(ragdollNetObj.gameObject);
    }
}
```

---

### Anti-Pattern #3: Plain C# Model Classes in MVC Pattern

**❌ BAD:**
```csharp
// Model class - can't have NetworkVariables!
public class BaseballBatModel
{
    public bool IsInHand = false;  // NOT synced!
    public GameObject Owner;       // NOT synced!
    float damage = 10;            // NOT synced!
}

// Controller tries to use it
public class BaseballBatController : NetworkBehaviour
{
    private BaseballBatModel model;  // Model is local only!
}
```

**Why it's bad:**
- NetworkVariables MUST be in NetworkBehaviour
- Plain C# classes can't sync over network
- Results in duplicate code (NetworkVariables in Controller + Model data)
- Doesn't fit Unity's component architecture

**✅ GOOD - Unity Component Pattern:**
```csharp
public class BaseballBatItem : NetworkBehaviour
{
    // Data layer (replaces Model)
    NetworkVariable<bool> _isInHand = new NetworkVariable<bool>();
    [SerializeField] private BaseballBatSO _itemData;  // ScriptableObject for static data

    // Logic layer (replaces Controller)
    [ServerRpc]
    void RequestHitServerRpc() { ... }

    // View layer (delegate to Unity components)
    private void UpdateVisuals() {
        _animator.SetBool("InHand", _isInHand.Value);
    }
}
```

---

### Anti-Pattern #4: Not Using NetworkVariable for Game-Critical State

**❌ BAD:**
```csharp
public class WalletBankton : Singleton<WalletBankton>  // Not even NetworkBehaviour!
{
    public int TotalMoney { get; private set; } = 100;  // Local only!
}
```

**Why it's bad:**
- Money isn't synced across clients
- Economy completely broken in multiplayer
- Each client has different money values

**✅ GOOD:**
```csharp
public class WalletBankton : NetworkSingleton<WalletBankton>
{
    public NetworkVariable<int> TotalMoney = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
}
```

---

### Anti-Pattern #5: Manual ServerRpc Pattern with IsServer Check

**❌ BAD (causes double execution on host):**
```csharp
public void OnHit(float damage)
{
    if (!IsServer)
    {
        OnHitServerRpc(damage);
        return;
    }
    ApplyDamage(damage);  // Executes on host
}

[ServerRpc(RequireOwnership = false)]
private void OnHitServerRpc(float damage)
{
    ApplyDamage(damage);  // ALSO executes on host = DOUBLE EXECUTION!
}
```

**Why it's bad:**
- Host executes logic twice (once in OnHit, once in ServerRpc)
- Results in double damage, double events, etc.
- Hard to debug

**✅ GOOD:**
```csharp
public void OnHit(float damage)
{
    OnHitServerRpc(damage);  // Always call ServerRpc
}

[ServerRpc(RequireOwnership = false)]
private void OnHitServerRpc(float damage)
{
    ApplyDamage(damage);  // Only executes once on server
}
```

---

### Anti-Pattern #6: Coroutines Waiting for Network State

**❌ BAD:**
```csharp
private IEnumerator WaitForNetworkSpawn()
{
    yield return new WaitUntil(() => NetworkObject.IsSpawned);
    yield return new WaitForSeconds(0.2f);  // Arbitrary delay!
    DoSomething();
}
```

**Why it's bad:**
- Race conditions
- Arbitrary delays don't guarantee correctness
- Fragile (breaks with network latency changes)

**✅ GOOD:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    DoSomething();  // Guaranteed to run when spawned
}
```

---

### Anti-Pattern #7: Inconsistent NetworkVariable Usage

**❌ BAD (inconsistent across codebase):**
```csharp
// Some files use NetworkVariable
public class SwingDoors : NetworkBehaviour
{
    private NetworkVariable<bool> _isOpen;  // ✅
}

// Other files don't
public class PlayerHealth : NetworkBehaviour
{
    float _currentHealth;  // ❌ NOT synced!
    bool _isDead;          // ❌ NOT synced!
}
```

**Why it's bad:**
- Inconsistent patterns confuse developers
- Some systems sync, others don't
- Hard to debug multiplayer issues

**✅ GOOD:**
```csharp
// Apply NetworkVariable consistently
public class PlayerHealth : NetworkBehaviour
{
    NetworkVariable<float> _currentHealth = new NetworkVariable<float>(100f);
    NetworkVariable<bool> _isDead = new NetworkVariable<bool>(false);
}
```

---

## Quick Reference

| Task | Pattern | Method |
|------|---------|--------|
| Client → Server | ServerRpc | `[ServerRpc]` |
| Server → Client | ClientRpc | `[ClientRpc]` |
| Sync state | NetworkVariable | `NetworkVariable<T>` |
| Sync collection | NetworkList | `NetworkList<T>` |
| Spawn object | Server only | `networkObject.Spawn()` |
| Server-only logic | Authority check | `if (!IsServer) return;` |
| Owner-only logic | Ownership check | `if (!IsOwner) return;` |
| Validate input | Server validation | Inside ServerRpc |
| Network singleton | NetworkSingleton<T> | Custom base class |

---

**Last Updated:** 2025-11-07

For more details, see:
- `.claude/docs/authority-model.md` - Network ownership rules
- `.claude/docs/audit-report.md` - Examples of what NOT to do
- `.claude/templates/` - Code templates using these patterns
