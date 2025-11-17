# Network Authority Model

**Version:** 1.0
**Last Updated:** 2025-11-07
**Project:** MultiplayerNGO - Unity Netcode for GameObjects

This document defines the network authority model for the project. It specifies which operations execute on the server vs client, who owns which NetworkObjects, and what data flows in which direction.

---

## Table of Contents

1. [Authority Types](#authority-types)
2. [Authority by System](#authority-by-system)
3. [Data Flow Diagrams](#data-flow-diagrams)
4. [Decision Flowcharts](#decision-flowcharts)
5. [Authority Quick Reference](#authority-quick-reference)

---

## Authority Types

### Server Authority
**Definition:** Only the server can execute this logic or modify this state.

**Used for:**
- Game logic that affects all players
- World state (enemies, items, environment)
- Validation and anti-cheat
- Economy and progression
- Spawning/despawning objects

**Example:**
```csharp
[ServerRpc(RequireOwnership = false)]
private void DealDamageServerRpc(float damage)
{
    // Only executes on server
    health.Value -= damage;
}
```

---

### Client Authority (Owner)
**Definition:** The client that owns a NetworkObject can execute logic or modify state locally.

**Used for:**
- Local player input
- Camera control
- UI interactions (local)
- Client-side prediction

**Example:**
```csharp
private void Update()
{
    if (!IsOwner) return;  // Only owner processes input

    Vector2 input = GetInput();
    MoveServerRpc(input);
}
```

---

### Shared Authority
**Definition:** Client predicts locally, server validates, reconciliation if needed.

**Used for:**
- Player movement
- Player camera look
- Fast-paced interactions

**Example:**
```csharp
private void Update()
{
    if (!IsOwner) return;

    // Predict locally
    transform.position += GetMovement();

    // Send to server for validation
    SyncPositionServerRpc(transform.position);
}

[ServerRpc]
private void SyncPositionServerRpc(Vector3 position)
{
    if (IsValidPosition(position))
        transform.position = position;
    else
        ReconcileClientRpc(transform.position);  // Force correction
}
```

---

## Authority by System

### Player System

#### Player Movement
**Authority:** Shared (Client predicts, Server validates)

**Flow:**
```
1. Client: Reads input
2. Client: Applies movement locally (prediction)
3. Client: Sends input to server via ServerRpc
4. Server: Validates input
5. Server: Applies movement
6. Server: If client position differs too much, corrects via ClientRpc
7. All Clients: Receive updated position via NetworkTransform
```

**Code Pattern:**
```csharp
// Client-side
private void Update()
{
    if (!IsOwner) return;

    Vector2 input = new Vector2(
        Input.GetAxis("Horizontal"),
        Input.GetAxis("Vertical")
    );

    // Predict locally
    Vector3 movement = new Vector3(input.x, 0, input.y) * speed * Time.deltaTime;
    transform.position += movement;

    // Send to server
    if (input != Vector2.zero)
        MoveServerRpc(input);
}

// Server-side
[ServerRpc]
private void MoveServerRpc(Vector2 input)
{
    // Validate input
    if (input.magnitude > 1f)
        input = input.normalized;  // Prevent speed hacking

    // Apply on server (NetworkTransform syncs automatically)
    Vector3 movement = new Vector3(input.x, 0, input.y) * speed * Time.deltaTime;
    transform.position += movement;
}
```

---

#### Player Health
**Authority:** Server

**Flow:**
```
1. Client: Player takes damage (collision, enemy attack, etc.)
2. Client: Calls TakeDamageServerRpc
3. Server: Validates damage source and amount
4. Server: Modifies health NetworkVariable
5. All Clients: health.OnValueChanged triggers UI update
```

**Code Pattern:**
```csharp
private NetworkVariable<float> health = new NetworkVariable<float>(100f);

public void TakeDamage(float damage, GameObject source)
{
    TakeDamageServerRpc(damage, source.GetComponent<NetworkObject>());
}

[ServerRpc(RequireOwnership = false)]
private void TakeDamageServerRpc(float damage, NetworkObjectReference sourceRef)
{
    // Validate source exists
    if (!sourceRef.TryGet(out NetworkObject source))
        return;

    // Validate damage is reasonable
    if (damage < 0 || damage > 1000)
        return;

    // Apply damage
    health.Value = Mathf.Max(0, health.Value - damage);

    if (health.Value <= 0)
        Die();
}
```

**Ownership:** Server (NetworkVariableWritePermission.Server)

---

#### Player Inventory
**Authority:** Server for modifications, Owner for display

**Flow:**
```
1. Client (Owner): Attempts to pick up item
2. Client: Calls PickupItemServerRpc
3. Server: Validates pickup (distance, inventory space)
4. Server: Adds item to inventory NetworkList
5. All Clients: NetworkList.OnListChanged triggers UI update
6. Client (Owner): Shows item in inventory UI
```

**Code Pattern:**
```csharp
private NetworkList<NetworkObjectReference> inventory;

[ServerRpc]
private void PickupItemServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
{
    // Validate item exists
    if (!itemRef.TryGet(out NetworkObject itemObject))
        return;

    // Validate inventory space
    if (inventory.Count >= maxInventorySize)
        return;

    // Validate distance
    if (!IsInRange(itemObject, rpcParams.Receive.SenderClientId))
        return;

    // Add to inventory
    inventory.Add(itemRef);

    // Change item ownership to player
    itemObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    itemObject.GetComponent<BaseItem>().OnPickedUp();
}
```

**Ownership:** Server for inventory list, Owner for held items

---

### Enemy System

#### Enemy Spawning
**Authority:** Server ONLY

**Flow:**
```
1. Server: Decides to spawn enemy (spawn trigger, wave system, etc.)
2. Server: Instantiates enemy GameObject
3. Server: Calls networkObject.Spawn()
4. All Clients: Receive enemy spawn notification
5. All Clients: Enemy appears in their world
```

**Code Pattern:**
```csharp
public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private void Start()
    {
        if (!IsServer) return;  // CRITICAL: Server only!

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        // Instantiate
        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        // Spawn on network
        NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
        networkObject.Spawn();  // Replicates to all clients

        Debug.Log($"Enemy spawned: {networkObject.NetworkObjectId}");
    }
}
```

**Ownership:** Server (never changes)

---

#### Enemy AI
**Authority:** Server for logic, Clients for visual

**Flow:**
```
1. Server: Runs AI logic (pathfinding, state machine, decisions)
2. Server: Updates state NetworkVariable when state changes
3. All Clients: state.OnValueChanged triggers animation/effects
4. All Clients: NetworkTransform syncs position automatically
```

**Code Pattern:**
```csharp
public enum EnemyState { Idle, Patrolling, Chasing, Attacking }

public class EnemyAI : NetworkBehaviour
{
    private NetworkVariable<EnemyState> currentState =
        new NetworkVariable<EnemyState>(EnemyState.Idle);

    private StateMachine stateMachine;

    private void Update()
    {
        if (!IsServer) return;  // AI logic runs on server only

        stateMachine.Update();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // All clients subscribe to state changes
        currentState.OnValueChanged += OnStateChanged;

        // Initialize to current state
        OnStateChanged(EnemyState.Idle, currentState.Value);
    }

    private void OnStateChanged(EnemyState oldState, EnemyState newState)
    {
        // Visual updates happen on all clients
        animator.SetInteger("State", (int)newState);

        Debug.Log($"Enemy state: {oldState} -> {newState}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyPlayerSpottedServerRpc(NetworkObjectReference playerRef)
    {
        // Only server can change state
        currentState.Value = EnemyState.Chasing;
        SetTarget(playerRef);
    }
}
```

**Ownership:** Server (logic), All Clients (visuals)

---

#### Enemy Health
**Authority:** Server

**Flow:**
```
1. Client: Weapon hits enemy
2. Client: Calls DealDamageServerRpc on enemy
3. Server: Validates hit (distance, line of sight, cooldown)
4. Server: Applies damage to enemy health NetworkVariable
5. All Clients: health.OnValueChanged updates health bar
6. Server: If health <= 0, despawn enemy
```

**Code Pattern:**
```csharp
private NetworkVariable<float> health = new NetworkVariable<float>(100f);

[ServerRpc(RequireOwnership = false)]
public void TakeDamageServerRpc(float damage, NetworkObjectReference attackerRef, ServerRpcParams rpcParams = default)
{
    // Validate attacker exists
    if (!attackerRef.TryGet(out NetworkObject attacker))
        return;

    // Validate damage is reasonable
    if (damage <= 0 || damage > 500)
        return;

    // Validate attacker is in range
    float distance = Vector3.Distance(transform.position, attacker.transform.position);
    if (distance > maxAttackRange)
        return;

    // Apply damage
    health.Value -= damage;

    // Check for death
    if (health.Value <= 0)
    {
        Die();
    }
}

private void Die()
{
    // Server-only death logic
    PlayDeathAnimationClientRpc();

    // Wait for animation, then despawn
    StartCoroutine(DespawnAfterDelay(2f));
}

[ClientRpc]
private void PlayDeathAnimationClientRpc()
{
    animator.SetTrigger("Die");
}
```

**Ownership:** Server

---

### Item System

#### Item Pickup
**Authority:** Server

**Flow:**
```
1. Client: Player presses interact near item
2. Client: Calls PickupItemServerRpc on item
3. Server: Validates pickup:
   - Item still exists?
   - Player in range?
   - Inventory has space?
4. Server: Grants pickup:
   - Adds to player's inventory NetworkList
   - Changes item ownership to player
   - Disables item collider
5. All Clients: See item disappear or attach to player
```

**Code Pattern:**
```csharp
public class ItemPickup : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void PickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId;

        // Validate player exists
        if (!NetworkManager.ConnectedClients.TryGetValue(playerId, out NetworkClient client))
            return;

        NetworkObject player = client.PlayerObject;

        // Validate distance
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > maxPickupDistance)
            return;

        // Validate inventory space
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (!inventory.HasSpace())
            return;

        // Grant pickup
        inventory.AddItemServerRpc(NetworkObject);

        // Change ownership to player
        NetworkObject.ChangeOwnership(playerId);

        // Notify all clients
        OnPickedUpClientRpc(playerId);
    }

    [ClientRpc]
    private void OnPickedUpClientRpc(ulong pickerId)
    {
        // Disable visuals on all clients
        gameObject.SetActive(false);

        // If local player picked up, show UI
        if (pickerId == NetworkManager.Singleton.LocalClientId)
        {
            ShowPickupFeedback();
        }
    }
}
```

**Ownership:** Server initially, transfers to Player on pickup

---

#### Item Usage
**Authority:** Owner (player) requests, Server validates

**Flow:**
```
1. Client (Owner): Player uses item (clicks, presses key)
2. Client: Plays local animation (prediction)
3. Client: Calls UseItemServerRpc
4. Server: Validates usage:
   - Item still equipped?
   - Item has durability?
   - Item not on cooldown?
5. Server: Applies item effect
6. Server: Updates item state (durability, cooldown)
7. All Clients: See effect (via ClientRpc or NetworkVariable)
```

**Code Pattern:**
```csharp
public class UsableItem : NetworkBehaviour
{
    private NetworkVariable<float> durability = new NetworkVariable<float>(100f);
    private float lastUseTime;

    public void Use()
    {
        if (!IsOwner) return;

        // Local prediction (play animation immediately)
        PlayUseAnimation();

        // Request server validation
        UseItemServerRpc();
    }

    [ServerRpc]
    private void UseItemServerRpc()
    {
        // Validate cooldown
        if (Time.time - lastUseTime < useCooldown)
            return;

        // Validate durability
        if (durability.Value <= 0)
            return;

        // Apply effect
        ApplyItemEffect();

        // Update state
        durability.Value -= durabilityDrainPerUse;
        lastUseTime = Time.time;

        // Notify clients
        OnItemUsedClientRpc();
    }

    [ClientRpc]
    private void OnItemUsedClientRpc()
    {
        // Non-owner clients see the effect
        if (!IsOwner)
        {
            PlayUseAnimation();
        }

        // All clients see/hear effects
        PlaySound();
        PlayVFX();
    }
}
```

**Ownership:** Player (Owner)

---

### World System

#### Level Generation
**Authority:** Server generates, All clients receive

**Flow:**
```
1. Server: Generates level procedurally
2. Server: Serializes level data (JSON, binary, etc.)
3. Server: Sends level data to all clients via ClientRpc
4. Clients: Deserialize and reconstruct level locally
5. All Clients: Level appears identical
```

**Code Pattern:**
```csharp
public class LevelNetworkSync : NetworkBehaviour
{
    private DungeonGenerator generator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // Server generates level
            generator.Generate();

            // Serialize level data
            string levelData = generator.SerializeToJSON();

            // Send to all clients
            SyncLevelClientRpc(levelData);
        }
    }

    [ClientRpc]
    private void SyncLevelClientRpc(string levelData)
    {
        if (IsServer) return;  // Server already has level

        // Clients reconstruct level from data
        generator.DeserializeFromJSON(levelData);
        generator.Reconstruct();

        Debug.Log("Level synchronized");
    }
}
```

**Ownership:** Server generates, All clients have copy

---

#### Interactive Objects (Doors, Buttons, etc.)
**Authority:** Server for state, Clients can request interaction

**Flow:**
```
1. Client: Player interacts with object
2. Client: Calls InteractServerRpc
3. Server: Validates interaction (distance, cooldown, state)
4. Server: Toggles object state (NetworkVariable)
5. All Clients: state.OnValueChanged updates visual (door opens, button presses)
```

**Code Pattern:**
```csharp
public class Door : NetworkBehaviour
{
    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isOpen.OnValueChanged += OnDoorStateChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        // Validate player is close enough
        if (!NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient client))
            return;

        float distance = Vector3.Distance(transform.position, client.PlayerObject.transform.position);
        if (distance > maxInteractDistance)
            return;

        // Toggle state
        isOpen.Value = !isOpen.Value;
    }

    private void OnDoorStateChanged(bool wasOpen, bool nowOpen)
    {
        // Update visuals on all clients
        if (nowOpen)
            PlayOpenAnimation();
        else
            PlayCloseAnimation();
    }
}
```

**Ownership:** Server

---

### Economy System

#### Money / Currency
**Authority:** Server

**Flow:**
```
1. Server: Player earns money (mission complete, enemy killed, etc.)
2. Server: Updates money NetworkVariable
3. All Clients: money.OnValueChanged updates UI
```

**Code Pattern:**
```csharp
public class WalletManager : NetworkSingleton<WalletManager>
{
    private NetworkVariable<int> totalMoney = new NetworkVariable<int>(100);

    public int Money => totalMoney.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        totalMoney.OnValueChanged += OnMoneyChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMoneyServerRpc(int amount)
    {
        if (amount < 0) return;  // Validate

        totalMoney.Value += amount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpendMoneyServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        if (amount < 0 || amount > totalMoney.Value) return;  // Validate

        totalMoney.Value -= amount;
    }

    private void OnMoneyChanged(int oldMoney, int newMoney)
    {
        // Publish event for UI
        EventBus.Instance.Publish(new WalletUpdateEvent { Money = newMoney });

        Debug.Log($"Money: {oldMoney} -> {newMoney}");
    }
}
```

**Ownership:** Server (never changes)

---

## Data Flow Diagrams

### Combat Flow

```
┌─────────────┐
│  Client A   │
│  (Attacker) │
└──────┬──────┘
       │
       │ 1. Weapon swing (local animation)
       │
       │ 2. SwingWeaponServerRpc(targetRef)
       │
       ▼
┌─────────────────────┐
│      SERVER         │
│                     │
│  3. Validate:       │
│     - Range         │
│     - Cooldown      │
│     - Target exists │
│                     │
│  4. Calculate damage│
│                     │
│  5. enemy.Health -= │
└──────┬──────────────┘
       │
       │ 6. health.OnValueChanged
       │
   ┌───┴────┬─────────────┐
   │        │             │
   ▼        ▼             ▼
┌────────┐ ┌────────┐  ┌────────┐
│Client A│ │Client B│  │Client C│
│        │ │        │  │        │
│ Update │ │ Update │  │ Update │
│Enemy HP│ │Enemy HP│  │Enemy HP│
└────────┘ └────────┘  └────────┘
```

---

### Item Pickup Flow

```
┌─────────────┐
│  Client A   │
│  (Player)   │
└──────┬──────┘
       │
       │ 1. Press interact key
       │
       │ 2. PickupItemServerRpc()
       │
       ▼
┌─────────────────────┐
│      SERVER         │
│                     │
│  3. Validate:       │
│     - Item exists   │
│     - In range      │
│     - Has space     │
│                     │
│  4. Grant pickup:   │
│     - Add to inv    │
│     - Change owner  │
│                     │
│  5. OnPickedUpRpc() │
└──────┬──────────────┘
       │
   ┌───┴────┬─────────────┐
   │        │             │
   ▼        ▼             ▼
┌────────┐ ┌────────┐  ┌────────┐
│Client A│ │Client B│  │Client C│
│        │ │        │  │        │
│ Show   │ │ Item   │  │ Item   │
│in inv  │ │disappea│  │disappea│
└────────┘ └────────┘  └────────┘
```

---

## Decision Flowcharts

### "Should this run on Server or Client?"

```
START
  │
  ▼
┌──────────────────────────────────────┐
│ Does it affect game state?           │
│ (health, position, inventory, etc.)  │
└────┬──────────────────────────┬──────┘
     │                          │
   YES                         NO
     │                          │
     ▼                          ▼
┌─────────┐              ┌────────────┐
│ SERVER  │              │Is it visual│
│AUTHORITY│              │or audio?   │
└─────────┘              └─────┬──────┘
                               │
                        ┌──────┴──────┐
                        │             │
                       YES           NO
                        │             │
                        ▼             ▼
                  ┌──────────┐  ┌──────────┐
                  │ CLIENT   │  │Does it   │
                  │ LOCAL    │  │read state│
                  └──────────┘  │only?     │
                                └────┬─────┘
                                     │
                              ┌──────┴──────┐
                              │             │
                             YES           NO
                              │             │
                              ▼             ▼
                        ┌──────────┐  ┌──────────┐
                        │ BOTH     │  │ SERVER   │
                        │ CLIENTS  │  │ AUTHORITY│
                        └──────────┘  └──────────┘
```

---

### "Who should own this NetworkObject?"

```
START
  │
  ▼
┌──────────────────────────────────────┐
│ Is it a player-controlled object?    │
└────┬──────────────────────────┬──────┘
     │                          │
   YES                         NO
     │                          │
     ▼                          ▼
┌─────────────┐         ┌──────────────────┐
│ OWNED BY    │         │Is it spawned by  │
│ PLAYER      │         │a specific player?│
└─────────────┘         └────────┬─────────┘
                                 │
                          ┌──────┴──────┐
                          │             │
                         YES           NO
                          │             │
                          ▼             ▼
                   ┌──────────┐  ┌──────────┐
                   │ OWNED BY │  │ OWNED BY │
                   │ SPAWNER  │  │ SERVER   │
                   └──────────┘  └──────────┘

Examples:
- Player Character → OWNED BY PLAYER
- Held Item → OWNED BY PLAYER (transfer on pickup)
- Projectile → OWNED BY SPAWNER (player who shot it)
- Enemy → OWNED BY SERVER
- World Object → OWNED BY SERVER
- Pickup Item → OWNED BY SERVER (transfer on pickup)
```

---

## Authority Quick Reference

### System Authority Table

| System      | Component     | Authority | Owner           | Can Modify                                          |     |
| ----------- | ------------- | --------- | --------------- | --------------------------------------------------- | --- |
| **Player**  | Movement      | Shared    | Player          | Player (predict), Server (validate)                 |     |
|             | Health        | Server    | Server          | Server only                                         |     |
|             | Inventory     | Server    | Server          | Server only (Player requests via RPC)               |     |
|             | Input         | Client    | Player          | Player only                                         |     |
|             | Camera        | Client    | Player          | Player only                                         |     |
| **Enemy**   | Spawning      | Server    | Server          | Server only                                         |     |
|             | AI Logic      | Server    | Server          | Server only                                         |     |
|             | AI State      | Server    | Server          | Server only (synced to clients)                     |     |
|             | Health        | Server    | Server          | Server only (Any client can request damage via RPC) |     |
|             | Animations    | Client    | Server          | All clients (reacts to state)                       |     |
| **Item**    | Pickup        | Server    | Server → Player | Server only                                         |     |
|             | Usage         | Owner     | Player          | Player requests, Server validates                   |     |
|             | Durability    | Server    | Player          | Server only                                         |     |
|             | Spawning      | Server    | Server          | Server only                                         |     |
| **World**   | Level Gen     | Server    | Server          | Server generates, Clients receive                   |     |
|             | Doors/Buttons | Server    | Server          | Server only (Any client can request via RPC)        |     |
|             | Hazards       | Server    | Server          | Server only                                         |     |
| **Economy** | Money         | Server    | Server          | Server only                                         |     |
|             | Research      | Server    | Server          | Server only                                         |     |
|             | Shop          | Server    | Server          | Server validates purchases                          |     |
| **UI**      | Display       | Client    | Local           | Each client independently                           |     |
|             | Input         | Client    | Local           | Each client independently                           |     |
| **Audio**   | Settings      | Client    | Local           | Each client independently                           |     |
|             | Playback      | Client    | Local           | Triggered by network events                         |     |

---

### RPC Permission Guide

| Scenario                    | RequireOwnership | Why                                       |
| --------------------------- | ---------------- | ----------------------------------------- |
| Player modifying own health | `true` (default) | Only player should control own health     |
| Player shooting enemy       | `false`          | Any player can damage any enemy           |
| Player picking up item      | `true`           | Only player should pick up for themselves |
| Player opening door         | `false`          | Any player can open any door              |
| Enemy attacking player      | `false`          | Enemy (server-owned) damages player       |
| Server updating game state  | N/A (ClientRpc)  | Server broadcasts to all                  |

---

### Validation Checklist

When implementing a ServerRpc, always validate:

- [ ] **Sender exists** - Is the calling client valid?
- [ ] **Object exists** - Do all NetworkObjectReferences resolve?
- [ ] **Range check** - Is the action within reasonable distance?
- [ ] **Cooldown check** - Has enough time passed since last action?
- [ ] **State check** - Is the action valid in current state?
- [ ] **Value bounds** - Are numeric values within reasonable ranges?
- [ ] **Authorization** - Is this client allowed to perform this action?

**Example:**
```csharp
[ServerRpc(RequireOwnership = false)]
private void DamageServerRpc(float damage, NetworkObjectReference targetRef, ServerRpcParams rpcParams = default)
{
    // ✓ Sender exists
    if (!NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient sender))
        return;

    // ✓ Object exists
    if (!targetRef.TryGet(out NetworkObject target))
        return;

    // ✓ Range check
    if (Vector3.Distance(sender.PlayerObject.transform.position, target.transform.position) > maxRange)
        return;

    // ✓ Cooldown check
    if (Time.time - lastAttackTime < attackCooldown)
        return;

    // ✓ Value bounds
    if (damage < 0 || damage > maxDamage)
        return;

    // All checks passed, apply damage
    target.GetComponent<Health>().TakeDamage(damage);
    lastAttackTime = Time.time;
}
```

---

## Common Authority Mistakes

### ❌ Mistake 1: Client Modifying NetworkVariable

```csharp
// WRONG - Only server can modify NetworkVariable
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        score.Value += 10;  // ERROR!
    }
}
```

**✓ Correct:**
```csharp
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
    score.Value += points;  // Correct
}
```

---

### ❌ Mistake 2: Server-Only Logic Running on Clients

```csharp
// WRONG - Spawning on all clients
private void Start()
{
    GameObject enemy = Instantiate(enemyPrefab);
    enemy.GetComponent<NetworkObject>().Spawn();  // ERROR!
}
```

**✓ Correct:**
```csharp
private void Start()
{
    if (!IsServer) return;  // Correct

    GameObject enemy = Instantiate(enemyPrefab);
    enemy.GetComponent<NetworkObject>().Spawn();
}
```

---

### ❌ Mistake 3: Not Validating ServerRpc Input

```csharp
// WRONG - Trusting client input
[ServerRpc(RequireOwnership = false)]
private void TeleportServerRpc(Vector3 position)
{
    transform.position = position;  // DANGER: Client can teleport anywhere!
}
```

**✓ Correct:**
```csharp
[ServerRpc(RequireOwnership = false)]
private void TeleportServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
{
    // Validate position is reachable
    if (!IsReachablePosition(position, rpcParams.Receive.SenderClientId))
    {
        Debug.LogWarning($"Client {rpcParams.Receive.SenderClientId} attempted invalid teleport");
        return;
    }

    transform.position = position;
}
```

---

## Conclusion

This authority model ensures:
- **Security** - Server validates all state changes
- **Consistency** - All clients see the same game state
- **Performance** - Client-side prediction for responsiveness
- **Clarity** - Clear rules for who controls what

**Golden Rules:**
1. **Server is the source of truth**
2. **Clients send input, not commands**
3. **Always validate ServerRpc parameters**
4. **Use NetworkVariables for synchronized state**
5. **Use RPCs for events and commands**

---

**Last Updated:** 2025-11-07

For implementation examples, see:
- `.claude/docs/networking-patterns.md` - Code patterns
- `.claude/templates/` - Code templates
- `.claude/docs/audit-report.md` - Anti-patterns to avoid
