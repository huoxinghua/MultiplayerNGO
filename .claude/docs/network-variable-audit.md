# NetworkVariable Usage Audit

**Date:** 2025-11-07
**Project:** Unity 6 + Netcode for GameObjects
**Status:** INCOMPLETE - Missing 75% of required NetworkVariables

---

## Executive Summary

The project currently uses **NetworkVariable in only 8 locations** but needs **35-44 NetworkVariables** for proper multiplayer synchronization. This missing synchronization causes:

- Health desyncs (players/enemies appear alive on some clients, dead on others)
- Money not syncing (economy broken in multiplayer)
- Enemy AI state not syncing (enemies appear frozen on non-host clients)
- IK hand positions wrong (confirmed bug: "TPS Baseball bat hand position is still fucked up")

**The team understands NetworkVariable** (evidenced by correct usage in items system) but has **inconsistently applied it** across the codebase.

---

## Current NetworkVariable Usage (8 instances)

### ✅ Systems Using NetworkVariable Correctly

| File | NetworkVariable | Purpose | Status |
|------|----------------|---------|--------|
| SwingDoors.cs | `_isOpen` | Door open/closed state | ✅ Working |
| BaseInventoryItem.cs | `IsPickedUp` | Item in inventory | ✅ Working |
| BaseInventoryItem.cs | `IsInHand` | Item held in hand | ✅ Working |
| FlashlightItem.cs | `FlashOnNetworkVariable` | Flashlight on/off | ✅ Working |
| PlayerInventory.cs | `NetworkCurrentIndex` | Current inventory slot | ✅ Working |
| PlayerInventory.cs | `InventoryNetworkRefs` (NetworkList) | Inventory items list | ✅ Working |
| BruteStateMachine.cs | `_playerTargetRef` | Enemy target player | ✅ Working |
| MayNetworkSync.cs | `syncedSeed` | Level generation seed | ✅ Working |
| MannualGenetate.cs | `syncedSeed` | Level generation seed | ✅ Working |

**Total:** 8 NetworkVariables (9 counting NetworkList separately)

**What's working:**
- ✅ Items system (pickup, in-hand states, flashlight)
- ✅ Doors/Interactables
- ✅ Inventory management
- ✅ Level generation sync

---

## Missing NetworkVariable Usage (27-36 instances needed)

### ❌ Systems NOT Using NetworkVariable (Critical State Desynced)

#### Player Systems (Missing 10-12 NetworkVariables)

**PlayerHealth.cs** - Lines 16-18
```csharp
// CURRENT (WRONG):
float _currentHealth;  // ❌ NOT synced!
[SerializeField] float _maxHealth;
private bool _isDead;  // ❌ NOT synced!

// SHOULD BE:
NetworkVariable<float> _currentHealth = new NetworkVariable<float>();
NetworkVariable<float> _maxHealth = new NetworkVariable<float>();
NetworkVariable<bool> _isDead = new NetworkVariable<bool>(false);
```

**Impact:** Players appear dead on some clients, alive on others. Health UI shows different values.

---

**PlayerStamina.cs** (if exists)
```csharp
// NEEDED:
NetworkVariable<float> _currentStamina;
NetworkVariable<bool> _isSprinting;
```

---

**PlayerIKController.cs** - Hand positions (CRITICAL!)
```csharp
// CURRENT (WRONG):
private Transform handL;  // ❌ Local only!
private Transform handR;  // ❌ Local only!

// SHOULD BE:
NetworkVariable<Vector3> _netHandRPosition = new NetworkVariable<Vector3>();
NetworkVariable<Quaternion> _netHandRRotation = new NetworkVariable<Quaternion>();
NetworkVariable<Vector3> _netHandLPosition = new NetworkVariable<Vector3>();
NetworkVariable<Quaternion> _netHandLRotation = new NetworkVariable<Quaternion>();
```

**Impact:** "TPS Baseball bat hand position is still fucked up" (Git commit 60cb67e0)
**Fix:** See `.claude/docs/ik-animation-guide.md`

---

**PlayerAnimation.cs** (maybe)
```csharp
// OPTIONAL:
NetworkVariable<int> _currentAnimationState;
```

---

#### Enemy Systems (Missing 8-10 NetworkVariables)

**BeetleHealth.cs** - Lines 16-19
```csharp
// CURRENT (WRONG):
private float _currentHealth;  // ❌ NOT synced!
private float _currentConsciousness;  // ❌ NOT synced!
public List<GameObject> HostilePlayers = new List<GameObject>();  // ❌ NOT synced!

// SHOULD BE:
NetworkVariable<float> _currentHealth = new NetworkVariable<float>();
NetworkVariable<float> _currentConsciousness = new NetworkVariable<float>();
NetworkList<NetworkObjectReference> _hostilePlayers = new NetworkList<NetworkObjectReference>();
```

**Impact:** Beetle health desyncs, clients see different health values, hostile players list not shared.

---

**BeetleStateMachine.cs**
```csharp
// NEEDED:
NetworkVariable<int> _currentStateIndex = new NetworkVariable<int>(0);
// 0 = Idle, 1 = MovePosition, 2 = RunAway, etc.
```

**Impact:** Clients don't see beetle AI behavior - beetles appear frozen.

---

**BruteHealth.cs** - Same as BeetleHealth
```csharp
// NEEDED:
NetworkVariable<float> _currentHealth;
NetworkVariable<float> _currentConsciousness;
NetworkVariable<bool> _isDead;
```

---

**BruteStateMachine.cs**
```csharp
// CURRENT (PARTIAL):
private readonly NetworkVariable<NetworkObjectReference> _playerTargetRef;  // ✅ Already exists!

// NEEDED (ADD):
NetworkVariable<int> _currentStateIndex = new NetworkVariable<int>(0);
```

**Impact:** Brute AI state doesn't sync, clients see frozen brutes.

---

#### Game State / Economy (Missing 3-4 NetworkVariables)

**WalletBankton.cs** - Lines 8-10 (CRITICAL!)
```csharp
// CURRENT (WRONG - NOT EVEN NETWORKED):
public class WalletBankton : Singleton<WalletBankton>  // ❌ Not a NetworkBehaviour!
{
    public int TotalMoney { get; private set; } = 100;  // ❌ NOT synced!
    public float CurrentResearchProgress { get; private set; } = 0;  // ❌ NOT synced!
    public float ResearchQuota { get; private set; } = 250;  // ❌ NOT synced!
}

// SHOULD BE:
public class WalletBankton : NetworkSingleton<WalletBankton>
{
    NetworkVariable<int> TotalMoney = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    NetworkVariable<float> CurrentResearchProgress = new NetworkVariable<float>();
    NetworkVariable<float> ResearchQuota = new NetworkVariable<float>(250f);
}
```

**Impact:** **GAME-BREAKING** - Money and research progress are completely local, not synced. Economy doesn't work in multiplayer.

---

#### Items (Missing 4-6 NetworkVariables)

**BaseballBatItem.cs**
```csharp
// NEEDED (if durability system exists):
NetworkVariable<float> _durability;
```

---

**MacheteInventoryItem.cs**
```csharp
// NEEDED:
NetworkVariable<float> _durability;
NetworkVariable<bool> _isBloodied;  // Cosmetic state
```

---

**Other items** (if ammo/durability systems exist)
```csharp
// EXAMPLES:
NetworkVariable<int> _ammoCount;
NetworkVariable<bool> _isBroken;
```

---

#### World / Interactables (Missing 2-4 NetworkVariables)

**DeliveryVan.cs** (if networked delivery system exists)
```csharp
// NEEDED:
NetworkVariable<bool> _hasArrived;
NetworkVariable<Vector3> _currentPosition;
```

---

**Other interactables**
```csharp
// EXAMPLES:
NetworkVariable<bool> _isActivated;
NetworkVariable<float> _activationProgress;
```

---

## Summary Table

| Category | Current | Needed | Total Expected | Missing |
|----------|---------|--------|----------------|---------|
| **Player Systems** | 1 | 10-12 | 11-13 | 10-12 |
| **Enemy Systems** | 1 | 8-10 | 9-11 | 8-10 |
| **Game State** | 0 | 3-4 | 3-4 | 3-4 |
| **Items** | 3 | 4-6 | 7-9 | 4-6 |
| **World** | 3 | 2-4 | 5-7 | 2-4 |
| **TOTAL** | **8** | **27-36** | **35-44** | **27-36** |

**Missing:** ~75% of required NetworkVariables

---

## The Golden Rule

> **"If it changes during gameplay and needs to look the same on all clients, it needs a NetworkVariable."**

### Examples of What Needs NetworkVariables

✅ **YES** - Needs NetworkVariable:
- Health values
- Money/currency
- Entity states (alive/dead, stunned, etc.)
- AI state machine current state
- Position of non-physics objects
- Animation states (if not driven by NetworkTransform)
- IK target positions
- Item durability
- Door open/closed
- Quest progress
- Any UI-displayed value that should match across clients

❌ **NO** - Doesn't need NetworkVariable:
- Local UI state (menu open/closed)
- Camera position (each client has own camera)
- Local sound effects triggered by synced events
- Cached references to other components
- Temporary calculation variables
- Cosmetic-only particle effects

---

## Migration Priority

### Phase 1: Critical (Week 1-2)
1. **WalletBankton** - Money and research (GAME-BREAKING)
2. **PlayerHealth** - Player health and death state
3. **BeetleHealth / BruteHealth** - Enemy health values

### Phase 2: High Priority (Week 3-4)
4. **BeetleStateMachine / BruteStateMachine** - AI state sync
5. **PlayerIKController** - Hand positions (confirmed bug)

### Phase 3: Medium Priority (Week 5-6)
6. **Item durability** - If system exists
7. **Additional game state** - Quest progress, mission state, etc.

### Phase 4: Polish (Week 7+)
8. **Cosmetic states** - Bloodied weapons, etc.
9. **Optimization** - Reduce sync frequency where possible

---

## Code Patterns

### Basic NetworkVariable Pattern

```csharp
public class ExampleNetworkedScript : NetworkBehaviour
{
    // Declare NetworkVariable
    private NetworkVariable<float> _health = new NetworkVariable<float>(
        100f,  // Initial/default value
        NetworkVariableReadPermission.Everyone,  // All clients can read
        NetworkVariableWritePermission.Server    // Only server can write
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to changes
        if (IsClient)
        {
            _health.OnValueChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        // Update UI, play effects, etc.
        Debug.Log($"Health changed from {oldValue} to {newValue}");
        UpdateHealthUI(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        // Only server modifies the NetworkVariable
        _health.Value -= damage;

        if (_health.Value <= 0)
        {
            HandleDeath();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _health.OnValueChanged -= OnHealthChanged;
        }
        base.OnNetworkDespawn();
    }
}
```

### NetworkVariable with Custom Permissions (Owner-writable)

```csharp
// Player can update their own stamina
private NetworkVariable<float> _stamina = new NetworkVariable<float>(
    100f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner  // Only the owning client can write
);
```

### NetworkList for Collections

```csharp
// For lists of references (e.g., hostile players)
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
    // Handle list changes
    Debug.Log($"Hostile players list changed: {changeEvent.Type}");
}
```

---

## Performance Considerations

### Sync Frequency

NetworkVariables automatically handle:
- Delta compression (only sends changes)
- Throttling (doesn't spam network)
- Late-joiner sync (new clients get current values)

### Optimization Tips

1. **Don't sync every frame** - Use dirty checking:
```csharp
private float _localHealth;

void Update()
{
    if (IsServer)
    {
        // Only update NetworkVariable when changed
        if (Mathf.Abs(_localHealth - _health.Value) > 0.01f)
        {
            _health.Value = _localHealth;
        }
    }
}
```

2. **Combine related values into structs**:
```csharp
struct HealthData : INetworkSerializable
{
    public float currentHealth;
    public float maxHealth;
    public bool isDead;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref currentHealth);
        serializer.SerializeValue(ref maxHealth);
        serializer.SerializeValue(ref isDead);
    }
}

private NetworkVariable<HealthData> _healthData = new NetworkVariable<HealthData>();
```

3. **Use appropriate read/write permissions** - Don't make everything server-writable if owner can manage it

---

## Testing Checklist

After adding NetworkVariables:

- [ ] Test with 2+ clients (not just host)
- [ ] Verify value syncs on spawn
- [ ] Verify value syncs on change
- [ ] Test late-joiner (client joins mid-game)
- [ ] Check OnValueChanged callbacks fire
- [ ] Verify UI updates correctly
- [ ] Test network disconnect/reconnect
- [ ] Check for unnecessary updates (bandwidth)

---

## Common Mistakes to Avoid

### ❌ Mistake #1: Modifying on Client
```csharp
void Update()
{
    if (IsOwner)
    {
        _health.Value -= damage;  // ❌ WRONG! Even owner can't write if permission is Server!
    }
}
```

**Fix:** Use ServerRpc to request changes:
```csharp
void Update()
{
    if (IsOwner && shouldTakeDamage)
    {
        TakeDamageServerRpc(damage);
    }
}

[ServerRpc]
void TakeDamageServerRpc(float damage)
{
    _health.Value -= damage;  // ✅ Correct!
}
```

### ❌ Mistake #2: Forgetting OnNetworkSpawn
```csharp
private void Start()
{
    _health.OnValueChanged += OnHealthChanged;  // ❌ Might not be spawned yet!
}
```

**Fix:** Use OnNetworkSpawn:
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    if (IsClient)
    {
        _health.OnValueChanged += OnHealthChanged;  // ✅ Correct!
    }
}
```

### ❌ Mistake #3: Not Unsubscribing
```csharp
// Missing unsubscribe = memory leak
```

**Fix:** Always unsubscribe:
```csharp
public override void OnNetworkDespawn()
{
    if (IsClient)
    {
        _health.OnValueChanged -= OnHealthChanged;  // ✅ Correct!
    }
    base.OnNetworkDespawn();
}
```

---

## Next Steps

1. **Read this audit** - Understand scope of missing NetworkVariables
2. **Review Phase 2-3 in CLAUDE.md** - Follow the migration plan
3. **Start with WalletBankton** - Critical path item
4. **Test after each conversion** - Don't batch too many changes
5. **See `.claude/templates/`** - Use code templates for consistency

---

## References

- Main plan: `.claude/CLAUDE.md`
- Detailed audit: `.claude/docs/audit-report.md` (Issues #14, #2, #5-9)
- IK specific: `.claude/docs/ik-animation-guide.md`
- Patterns: `.claude/docs/networking-patterns.md`

---

**Last Updated:** 2025-11-07
**Status:** Audit Complete - Ready for Migration
