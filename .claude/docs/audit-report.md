# MultiplayerNGO Comprehensive Audit Report

**Date:** 2025-11-07
**Auditor:** Claude Code
**Scope:** Complete codebase analysis for Unity 6 + Netcode for GameObjects project

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Structure Analysis](#project-structure-analysis)
3. [Netcode Implementation Issues](#netcode-implementation-issues)
4. [Services/Singletons Pattern](#servicessingletons-pattern)
5. [Common Issues and Anti-patterns](#common-issues-and-anti-patterns)
6. [Issues by Severity](#issues-by-severity)
7. [Patterns Observed](#patterns-observed)
8. [Statistics](#statistics)

---

## Executive Summary

Your project contains **176 C# scripts in Assets/_Project** and **32 scripts in Assets/Network**, with significant organizational and networking implementation issues. The codebase shows evidence of rapid iteration with multiple refactoring attempts but lacks consistency in network patterns and architecture.

### Critical Issues Found
- **4 categories** of game-breaking network bugs
- **9 singletons** with inconsistent patterns (3 different implementations)
- **4+ sets** of duplicate scripts
- **100+ lines** of commented-out code
- **22 NetworkBehaviour scripts** with varying quality

### Severity Breakdown
- **Critical:** 5 issues (project-breaking)
- **High:** 9 issues (causes desync)
- **Medium:** 5 issues (code quality)
- **Low:** 5 issues (quick wins)

---

## Project Structure Analysis

### Current Organization

**Assets/_Project/** - Main project structure:
```
_Project/
├── Code/
│   ├── Art/                    # Animation and visual scripts
│   ├── Core/
│   │   └── Patterns/          # Singleton, GameInitializer
│   ├── Gameplay/              # ⚠️ MIXED organization
│   │   ├── FirstPersonController/
│   │   ├── Player/RefactorInventory/  # ⚠️ "Refactor" in name
│   │   ├── MVCItems/
│   │   ├── NewItemSystem/      # ⚠️ "New" indicates old exists
│   │   ├── UsableItems/
│   │   ├── NPC/
│   │   ├── DungeonGenerator/
│   │   └── EnemySpawning/
│   ├── Network/               # Network-specific code
│   ├── Optimization/          # Performance code
│   ├── UI/                    # UI scripts
│   └── Utilities/             # Shared utilities
│       ├── EventBus/
│       ├── ServiceLocator/    # ⚠️ EXISTS BUT UNUSED
│       ├── Singletons/
│       └── StateMachine/
```

**Assets/Network/** - ⚠️ DUPLICATE/LEGACY folder:
```
Network/
├── Scripts/
│   └── PlayerController/      # ⚠️ DUPLICATES FirstPersonController
└── NetWorkUT/                 # Typo: "NetWork" instead of "Network"
    └── UI/
```

### Organizational Issues

#### CRITICAL - Script Duplication

**Duplicate PlayerMovement.cs:**
- `Assets/Network/Scripts/PlayerController/PlayerMovement.cs`
- `Assets/_Project/Code/Gameplay/FirstPersonController/PlayerMovement.cs`

**Duplicate PlayerLook.cs:**
- `Assets/Network/Scripts/PlayerController/PlayerLook.cs`
- `Assets/_Project/Code/Gameplay/FirstPersonController/PlayerLook.cs`

**Duplicate GroundCheck.cs:**
- `Assets/Network/Scripts/PlayerController/GroundCheck.cs`
- `Assets/_Project/Code/Gameplay/FirstPersonController/GroundCheck.cs`

**Duplicate PlayeNameTag.cs** (typo in name!):
- `Assets/Network/NetWorkUT/UI/PlayeNameTag.cs`
- `Assets/Network/Scripts/PlayerController/PlayeNameTag.cs`

**Impact:** Confusion about which scripts are active, potential for using wrong version

**Recommendation:** Delete entire `Assets/Network` folder after verifying no unique code

#### Refactoring Artifacts

**"Refactor" in folder names:**
```
Assets\_Project\Code\Gameplay\Player\RefactorInventory\
├── PlayerInventory.cs (current version)
└── OriginalGamePlaySave\
    └── PlayerInventory.cs (backup - should be removed)
```

**"New" in folder names:**
```
Assets\_Project\Code\Gameplay\NewItemSystem\
```

**Impact:** Indicates incomplete refactoring, confusing naming

**Recommendation:** Rename to proper names (e.g., `Inventory`, `Items`)

#### Mixed Patterns in Gameplay Folder

The `Gameplay/` folder contains multiple attempts at the same systems:
- **Player Controllers:** `FirstPersonController/` AND `Assets/Network/Scripts/PlayerController/`
- **Item Systems:** `MVCItems/`, `NewItemSystem/`, `UsableItems/`
- **Inventory:** `RefactorInventory/` with backup folder inside

**Impact:** Unclear which system is current, maintenance nightmare

**Recommendation:** Consolidate to single system per feature

---

## Netcode Implementation Issues

### NetworkBehaviour Implementations Found

**Total: 22 NetworkBehaviour scripts** spread across the project

### ISSUE #1: Inconsistent Server Authority Checks

#### Example 1: BeetleHealth.cs (Double Execution on Host)

**File:** `Assets\_Project\Code\Gameplay\NPC\Tranquil\Beetle\BeetleHealth.cs`
**Lines:** 78-91

```csharp
public void OnHit(GameObject attacker, float damage, float knockoutPower)
{
    Debug.Log("【beetleHealth】:onhit" +"IsServer："+IsServer+"IsHost：="+IsHost+ "IsClient:"+IsClient+ damage);
    if (!IsServer)
    {
        var attackerNetObj = attacker.GetComponent<NetworkObject>();
        if (attackerNetObj != null)
        {
            OnHitServerRpc(attackerNetObj, damage, knockoutPower);
        }
        return;
    }

    ApplyHit(attacker, damage, knockoutPower);
}
```

**PROBLEM:**
- Clients call `OnHitServerRpc` correctly
- **But host calls `ApplyHit` directly, bypassing ServerRpc**
- If another script calls `OnHit`, host will execute twice (once directly, once via ServerRpc)

**Impact:** Double damage on host, inconsistent behavior

**Fix:** Always call ServerRpc, execute logic inside RPC only

#### Example 2: BruteHealth.cs (No Network Checks)

**File:** `Assets\_Project\Code\Gameplay\NPC\Violent\Brute\BruteHealth.cs`
**Lines:** 31-35

```csharp
public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
{
    ChangeHealth(-damage);
    ChangeConsciousness(-knockoutPower);
}
```

**PROBLEM:**
- No `IsServer` check
- No ServerRpc call
- Will only execute on the client/host that calls it
- **Other clients will never see the damage**

**Impact:** Complete desync of enemy health

**Fix:** Implement ServerRpc pattern

---

### ISSUE #2: Missing NetworkVariable Synchronization

#### Example: EnemySpawnManager (Not Networked at All!)

**File:** `Assets\_Project\Code\Gameplay\EnemySpawning\EnemySpawnManager.cs`
**Lines:** 7-84

```csharp
public class EnemySpawnManager : MonoBehaviour  // ⚠️ Should be NetworkBehaviour!
{
    private void Start()
    {
        int randomSpawnPoint = Random.Range(0, EnemySpawnPoints.Instance.ActiveEnemySpawnPoints.Count);
        SpawnViolent(randomSpawnPoint);
        SpawnTranquil(randomSpawnPoint);
    }

    private void SpawnViolent(int spawnIndex)
    {
        GameObject newEnemy = Instantiate(/* ... */);
        // No NetworkObject.Spawn() call!
    }
}
```

**PROBLEM:**
- Runs on every client independently
- Random.Range gives different results on each client
- **Each client spawns its own set of enemies**
- Enemies not spawned as NetworkObjects
- Complete chaos in multiplayer

**Impact:** CRITICAL - Game unplayable in multiplayer

**Fix:**
1. Inherit from `NetworkBehaviour`
2. Only spawn on server (`if (!IsServer) return;`)
3. Call `networkObject.Spawn()` after Instantiate
4. Optionally sync spawn locations via NetworkVariable

---

### ISSUE #3: Race Conditions in Initialization

#### Example: LevelNetworkSync.cs (Magic Number Delays)

**File:** `Assets\_Project\Code\Gameplay\DungeonGenerator\LevelNetworkSync.cs`
**Lines:** 68-84

```csharp
private IEnumerator WaitForNetworkAndGenerate()
{
    yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening);
    yield return new WaitForSeconds(0.2f);  // ⚠️ MAGIC NUMBER - RACE CONDITION

    if (IsServer)
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        generator.Generate();
    }
}
```

**PROBLEM:**
- 0.2 second delay is arbitrary
- May not be enough time on slow machines/networks
- May be too much time (unnecessary wait)
- **No guarantee network is ready**

**Impact:** Intermittent level generation failures, late-join issues

**Fix:** Use proper NetworkManager callbacks instead of delays

**Better pattern:**
```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    if (IsServer)
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        generator.Generate();
    }
}
```

---

### ISSUE #4: Incorrect RPC Usage

#### Example: LevelNetworkSync.cs (Unused RPC Parameter)

**File:** `Assets\_Project\Code\Gameplay\DungeonGenerator\LevelNetworkSync.cs`
**Lines:** 143-151

```csharp
[ClientRpc]
private void SendDungeonDataClientRpc(string json, ulong id)
{
    Debug.Log($"[ClientRpc] Received call! IsServer={IsServer}, IsClient={IsClient}, length={json?.Length}");
    if (IsServer)
    {
        Debug.Log("server return");
        return;  // ⚠️ Server ignoring its own ClientRpc
    }
    // ... reconstruction logic using json
    // ⚠️ 'id' parameter never used!
}
```

**PROBLEMS:**
1. `ulong id` parameter is never used anywhere
2. Server explicitly skips ClientRpc (should use `ClientRpcParams` to exclude server instead)
3. ClientRpc is sent to all clients including server (wasteful)

**Impact:** Wasted bandwidth, confusing code

**Fix:**
- Remove unused `id` parameter
- Use `ClientRpcParams` to exclude server:
```csharp
[ClientRpc]
private void SendDungeonDataClientRpc(string json, ClientRpcParams clientRpcParams = default)
```

---

### ISSUE #5: Complex Item Pickup with Ownership Issues

#### BaseInventoryItem.cs - Overly Complex Pickup Logic

**File:** `Assets\_Project\Code\Gameplay\NewItemSystem\Items\BaseInventoryItem.cs`
**Lines:** 165-228 (60+ lines!)

**Current flow:**
1. Creates held visual prefab
2. Spawns as NetworkObject
3. Waits for spawn in coroutine
4. Changes ownership
5. Waits for ownership in another coroutine
6. Sets position and rotation
7. Syncs to inventory NetworkList
8. Different paths for Server vs Client

**PROBLEMS:**
1. Ownership changes happen without validation (Line 220)
2. Multiple coroutines waiting for network state (`WaitForHeld`, `AssignHeldVisualDelayed`)
3. `SetPositionAndRotation` called before ownership confirmed (Line 265)
4. **Race conditions** between item spawn and inventory sync
5. Can cause item duplication if multiple clients pick up simultaneously

**Impact:** HIGH - Items can be duplicated, pickup can fail intermittently

**Fix:** Complete rebuild with simpler flow (see migration guide)

**Simplified flow:**
```csharp
// Client requests pickup
[ServerRpc]
void RequestPickupServerRpc(ulong playerId)
{
    // Validate on server only
    if (!ValidatePickup(playerId)) return;

    // Grant pickup
    GrantPickup(playerId);

    // Sync to all clients
    SyncPickupClientRpc(playerId);
}
```

---

#### PlayerInventory.cs - Inefficient Initialization

**File:** `Assets\_Project\Code\Gameplay\Player\RefactorInventory\PlayerInventory.cs`

```csharp
public NetworkList<NetworkObjectReference> InventoryNetworkRefs = new NetworkList<NetworkObjectReference>();

public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();

    if (IsOwner)
    {
        // ⚠️ Creating 5 placeholder entries!
        for (int i = 0; i < 5; i++)
        {
            RequestAddToListServerRpc();
        }
    }
}
```

**PROBLEM:** NetworkList used as fixed-size array by pre-filling with placeholders. This is inefficient and unclear.

**Impact:** Wasted network bandwidth, confusing initialization

**Fix:** Use array if fixed size needed, or initialize properly on server

---

### ISSUE #6: State Machine Network Sync

#### BeetleStateMachine.cs (Server-Only State Updates)

**File:** `Assets\_Project\Code\Gameplay\NPC\Tranquil\Beetle\BeetleStateMachine.cs`
**Lines:** 50-62

```csharp
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
```

**PROBLEM:**
- State machine only runs on server
- Clients never transition states
- Animations don't play on clients
- **Clients see frozen/T-pose enemies**

**Impact:** HIGH - Enemy AI appears broken to non-host players

**Fix:** Add NetworkVariable for state, sync via ClientRpc

**Proper pattern:**
```csharp
private NetworkVariable<int> netCurrentState = new NetworkVariable<int>();

public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    if (IsClient)
    {
        netCurrentState.OnValueChanged += OnStateChanged;
    }
}

private void OnStateChanged(int oldState, int newState)
{
    // Update animations, visual state on clients
    PlayStateAnimation(newState);
}
```

---

## Services/Singletons Pattern

### Current Implementations

#### Singleton Pattern (Regular)

**File:** `Assets\_Project\Code\Core\Patterns\Singleton.cs`
**Lines:** 64-109

```csharp
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isApplicationQuitting;
    protected virtual bool PersistBetweenScenes => true;

    public static T Instance
    {
        get
        {
            if (_isApplicationQuitting) return null;
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject singletonObject = new();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";
                }
            }
            return _instance;
        }
    }
}
```

**Used by:**
1. `CurrentPlayers` - Tracks player GameObjects/Transforms
2. `WalletBankton` - Manages money and research progress ⚠️ CRITICAL
3. `EnemySpawnPoints` - Tracks spawn point locations
4. `CurrentLights` - Tracks light objects for optimization
5. `LayerMasks` - Holds layer mask references

#### NetworkSingleton Pattern #1 (PlayerListManager)

**File:** `Assets\_Project\Code\Network\PlayerList\PlayerListManager.cs`
**Lines:** 7-17

```csharp
public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;  // ⚠️ No null check, no DontDestroyOnLoad
    }
}
```

**PROBLEM:** Simple assignment, no protection against duplicates

#### NetworkSingleton Pattern #2 (NetworkRelay)

**File:** `Assets\_Project\Code\Network\NetworkRelay.cs`

```csharp
public class NetworkRelay : MonoBehaviour
{
    public static NetworkRelay Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
```

**PROBLEM:** Not even a NetworkBehaviour! Just MonoBehaviour with static instance

#### NetworkSingleton Pattern #3 (SteamLobbyManager) ✓ CORRECT

**File:** `Assets\_Project\Code\Network\SteamLobby\SteamLobbyManager.cs`
**Lines:** 35-44

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

**This is the CORRECT pattern** - but inconsistent with others!

### ServiceLocator Pattern

**Files:**
- `Assets\_Project\Code\Utilities\ServiceLocator\IService.cs`
- `Assets\_Project\Code\Utilities\ServiceLocator\MonoBehaviourService.cs`
- `Assets\_Project\Code\Utilities\ServiceLocator\ServiceLocator.cs`

**PROBLEM:** Code exists but is **COMPLETELY UNUSED**

**GameInitializer** references it but registers no services:

```csharp
void Start()
{
    ServiceLocator serviceLocator = ServiceLocator.Create(true);
    // ... no services registered!
}
```

**Impact:** Dead code, confusion about architecture

**Decision needed:** Fully implement OR remove entirely

---

### CRITICAL ISSUES: Non-Networked Singletons Used in Network Code

#### ISSUE #1: WalletBankton (Money Not Synced!)

**File:** `Assets\_Project\Code\Utilities\Singletons\WalletBankton.cs`
**Lines:** 7-33

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

**PROBLEM:**
- This is **game-critical state**
- Not networked at all!
- **Each client has their own separate money value**
- Missions give rewards locally
- Economy is completely broken in multiplayer

**Impact:** CRITICAL - Game economy doesn't work

**Fix:** Convert to NetworkBehaviour with NetworkVariables

```csharp
public class WalletBankton : NetworkSingleton<WalletBankton>
{
    public NetworkVariable<int> TotalMoney = new NetworkVariable<int>(100);
    public NetworkVariable<float> CurrentResearchProgress = new NetworkVariable<float>(0);

    [ServerRpc(RequireOwnership = false)]
    public void AddSubMoneyServerRpc(int amount)
    {
        TotalMoney.Value += amount;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TotalMoney.OnValueChanged += (oldValue, newValue) =>
        {
            EventBus.EventBus.Instance.Publish<WalletUpdate>(new WalletUpdate());
        };
    }
}
```

#### ISSUE #2: CurrentPlayers (Inconsistent Player List)

**File:** `Assets\_Project\Code\Utilities\Singletons\CurrentPlayers.cs`

```csharp
public class CurrentPlayers : Singleton<CurrentPlayers>
{
    public List<Transform> PlayerTransforms = new List<Transform>();
    public List<GameObject> PlayerGameObjects = new List<GameObject>();

    public void AddPlayer(GameObject player)
    {
        PlayerGameObjects.Add(player);
        PlayerTransforms.Add(player.transform);
    }
}
```

**PROBLEM:**
- Local list only, not synced
- If used by enemy AI for targeting, will be inconsistent
- Each client has different list of players

**Impact:** MEDIUM-HIGH - Depends on how enemy AI uses this

**Fix Options:**
1. Convert to NetworkBehaviour with NetworkList
2. Deprecate entirely, use `NetworkManager.Singleton.ConnectedClientsList`

---

## Common Issues and Anti-patterns

### Anti-Pattern #1: Commented-Out Code Everywhere

**Examples throughout codebase:**

**PlayerHealth.cs:** Lines 40-57 (dead player handling)
```csharp
// private void HandleDeadPlayer()
// {
//     // ... 17 lines of commented code
// }
```

**BaseInventoryItem.cs:** Lines 65-70, 106-113, 281-327 (60+ lines!)
```csharp
// public virtual void Use()
// {
//     // ... commented implementation
// }
```

**SwingDoors.cs:** Lines 26-37, 80-104
```csharp
// private void OpenDoor()
// {
//     // ... old implementation
// }
```

**BruteHealth.cs:** Lines 67-82
```csharp
// public void Die()
// {
//     // ... commented death logic
// }
```

**PROBLEM:** Makes code hard to read and maintain. Use version control instead!

**Impact:** MEDIUM - Reduces code clarity

**Fix:** Delete all commented code. It's in Git if you need it.

---

### Anti-Pattern #2: Magic Numbers

**LevelNetworkSync.cs:**
```csharp
yield return new WaitForSeconds(0.2f);  // Why 0.2? No comment explaining
```

**PlayerInventory.cs:**
```csharp
for (int i = 0; i < 5; i++)  // Why 5? Should be named constant
```

**Fix:** Replace with named constants

```csharp
private const float NETWORK_INITIALIZATION_DELAY = 0.2f;
private const int INVENTORY_SIZE = 5;
```

---

### Anti-Pattern #3: Inconsistent Naming

**PlayeNameTag.cs** - Typo: "Playe" instead of "Player"
- Appears in 2 files
- Unprofessional

**NetworkRelay.cs** - Too generic name
- What does it relay?
- Should be more specific

**BeetleHealth vs BruteHealth** - Different implementations
- Should follow same pattern
- Inherit from BaseEnemyHealth

---

### Anti-Pattern #4: Debug.Log Spam

**BeetleHealth.cs** has Chinese characters:
```csharp
Debug.Log("【beetleHealth】:onhit" +"IsServer："+IsServer+"IsHost：="+IsHost+ "IsClient:"+IsClient+ damage);
```

**Problems:**
- Mixed languages
- String concatenation (should use string interpolation)
- Too verbose
- Will spam console in production

**Fix:** Replace with proper logging system

```csharp
GameLogger.LogNetwork($"BeetleHealth.OnHit: damage={damage}", LogLevel.Debug);
```

---

### Anti-Pattern #5: RequireOwnership = false Everywhere

**Found in 17+ RPCs:**
```csharp
[ServerRpc(RequireOwnership = false)]
[ClientRpc(RequireOwnership = false)]
```

**PROBLEM:**
- Disabling ownership checks makes game vulnerable to cheating
- Any client can call any RPC
- Creates ambiguity about who can call what
- Should only disable when truly needed

**Impact:** MEDIUM - Security and clarity issue

**Fix:**
1. Re-enable where possible
2. Document why it's disabled for cases that need it
3. Add validation inside RPC

```csharp
[ServerRpc(RequireOwnership = false)]
private void DamageServerRpc(float damage, ServerRpcParams rpcParams = default)
{
    // Validate the sender
    var senderId = rpcParams.Receive.SenderClientId;
    if (!IsValidAttacker(senderId))
    {
        Debug.LogWarning($"Invalid damage RPC from client {senderId}");
        return;
    }

    ApplyDamage(damage);
}
```

---

### Anti-Pattern #6: Update Loops for Network Syncing

**NetworkBinder.cs:** Lines 48-51
```csharp
if (owner != null)
    owner.StartCoroutine(PollEveryFrame(PushToNetwork));
```

**PROBLEM:**
- Polling every frame to sync network state
- Inefficient - sends updates even when nothing changed

**Impact:** MEDIUM - Performance issue

**Fix:** Use OnValueChanged callbacks

```csharp
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    myNetworkVariable.OnValueChanged += OnValueChangedCallback;
}

private void OnValueChangedCallback(int oldValue, int newValue)
{
    // React to changes only
    UpdateVisuals(newValue);
}
```

---

### Anti-Pattern #7: Manual NetworkObject.Spawn() on Scene Objects

**SwingDoors.cs:** Line 23
```csharp
if (netObj != null)
{
    netObj.Spawn();  // ⚠️ DON'T DO THIS FOR SCENE OBJECTS!
}
```

**PROBLEM:**
- Scene-placed NetworkObjects are automatically spawned by NetworkManager
- Manually calling Spawn() will cause errors
- Shows misunderstanding of NetworkObject lifecycle

**Impact:** HIGH - Will cause runtime errors

**Fix:** Remove Spawn() call entirely for scene objects

---

## Issues by Severity

### CRITICAL (Project-Breaking Issues)

#### 1. EnemySpawnManager not networked
**File:** `Assets\_Project\Code\Gameplay\EnemySpawning\EnemySpawnManager.cs`
**Lines:** 7-84
**Impact:** Will spawn enemies on each client separately, complete chaos
**Fix:** Inherit from NetworkBehaviour, spawn only on server, sync spawn positions

#### 2. WalletBankton money not synced
**File:** `Assets\_Project\Code\Utilities\Singletons\WalletBankton.cs`
**Lines:** 6-33
**Impact:** Game economy won't work in multiplayer
**Fix:** Convert to NetworkBehaviour with NetworkVariable<int> for TotalMoney

#### 3. BruteHealth.OnHit has no network checks
**File:** `Assets\_Project\Code\Gameplay\NPC\Violent\Brute\BruteHealth.cs`
**Lines:** 31-35
**Impact:** Damage only applied locally, health desync
**Fix:** Add ServerRpc pattern like BeetleHealth

#### 4. Duplicate player controller scripts
**Files:** Multiple in Assets/Network and Assets/_Project
**Impact:** Will cause conflicts, unclear which is active
**Fix:** Delete entire Assets/Network folder after verifying no unique code

#### 5. IK system not networked (CRITICAL)
**File:** `Assets\_Project\Code\Art\AnimationScripts\IK\PlayerIKController.cs`
**Lines:** 6, 24-88
**Impact:** Hand positions completely desynced across clients, "TPS Baseball bat hand position is still fucked up" (confirmed git commit 60cb67e0)
**Fix:** Convert PlayerIKController to NetworkBehaviour, add NetworkVariables for hand positions
**See:** `.claude/docs/ik-animation-guide.md` for full analysis

---

### HIGH (Causes Desync Issues)

#### 6. IK target transforms not synchronized
**File:** `Assets\_Project\Code\Art\AnimationScripts\IK\IKInteractable.cs`
**Lines:** 11-12, 20-260
**Impact:** Each client has separate IK target instances, causes hand position desyncs
**Fix:** Add NetworkTransform to IK targets or sync via NetworkVariables
**See:** `.claude/docs/ik-animation-guide.md`

#### 7. DOTween IK animations run client-side only
**File:** `Assets\_Project\Code\Art\AnimationScripts\IK\IKInteractable.cs`
**Lines:** 68-214 (PlayIKIdle, PlayIKWalk, PlayIKRun, PlayIKInteract methods)
**Impact:** Animation timing desyncs between clients, positions drift over time
**Fix:** Sync animation state and timing via NetworkVariables
**See:** `.claude/docs/migration-guides/ik-sync-refactor.md`

#### 8. TPS Baseball Bat hand position bug
**File:** `Assets\_Project\Code\Art\AnimationScripts\Animations\PlayerAnimation.cs`
**Line:** 116
**Impact:** Line 116 bug - passing `true` instead of `false` for walk animation
**Fix:** Change `tpsIKController.Interactable.PlayIKWalk(1f, true);` to `false`
**Git Commit:** 60cb67e0 - "TPS Baseball bat hand position is still fucked up"

#### 9. Dual IK controller architecture issues
**File:** `Assets\_Project\Code\Art\AnimationScripts\Animations\PlayerAnimation.cs`
**Lines:** 19-21
**Impact:** Two IK controllers per player (FPS/TPS) can cause race conditions
**Fix:** Consider single IK controller with view-dependent offsets

#### 10. State machines only run on server
**Files:** BeetleStateMachine.cs, BruteStateMachine.cs
**Impact:** Clients see no enemy behavior, frozen enemies
**Fix:** Add NetworkVariable for state, sync animations via ClientRpc

#### 11. Item pickup ownership race conditions
**File:** `BaseInventoryItem.cs` Lines 165-228
**Impact:** Items can be duplicated
**Fix:** Simplify pickup flow, use single ServerRpc with validation

#### 12. LevelNetworkSync timing issues
**File:** `LevelNetworkSync.cs` Line 72
**Impact:** Dungeon generation can fail intermittently
**Fix:** Remove arbitrary delays, use NetworkManager callbacks properly

#### 13. CurrentPlayers singleton not networked
**File:** `Assets\_Project\Code\Utilities\Singletons\CurrentPlayers.cs`
**Impact:** Enemy AI targeting will be inconsistent
**Fix:** Make it NetworkBehaviour or use NetworkManager's ConnectedClientsList

#### 14. NetworkVariable usage is inconsistent (DISCOVERED 2025-11-07)
**Files:** PlayerHealth.cs, BeetleHealth.cs, BruteHealth.cs, WalletBankton.cs, State machines, IK controllers
**Impact:** ~75% of required state synchronization is missing - health desyncs, money not synced, AI state not synced, IK positions wrong
**Current:** 8 NetworkVariable declarations found
**Expected:** 35-44 NetworkVariables needed for proper synchronization
**Details:**
- ✅ Items system uses NetworkVariables correctly (IsPickedUp, IsInHand, FlashOnNetworkVariable)
- ✅ Doors use NetworkVariables (_isOpen in SwingDoors.cs)
- ✅ Level sync uses NetworkVariables (syncedSeed)
- ❌ PlayerHealth._currentHealth and _isDead are plain floats/bools
- ❌ BeetleHealth/BruteHealth health values not networked
- ❌ WalletBankton.TotalMoney and CurrentResearchProgress not networked
- ❌ State machine current state not synced
- ❌ PlayerIKController hand positions not synced
**Fix:** Convert all game-critical state to NetworkVariables (see `.claude/docs/network-variable-audit.md`)
**Rule:** "If it changes during gameplay and needs to look the same on all clients, it needs a NetworkVariable"

---

### MEDIUM (Code Quality Issues)

#### 15. Mixed singleton patterns
**Files:** PlayerListManager, NetworkRelay, SteamLobbyManager
**Impact:** Inconsistent architecture, maintainability issues
**Fix:** Standardize on one NetworkSingleton<T> base class

#### 16. ServiceLocator unused
**File:** `Assets\_Project\Code\Utilities\ServiceLocator\ServiceLocator.cs`
**Impact:** Dead code, architectural confusion
**Fix:** Either use it or remove it

#### 17. NetworkRelay.cs inefficient patterns (DISCOVERED 2025-11-07)
**File:** `Assets\_Project\Code\Network\ObjectManager\NetworkRelay.cs`
**Lines:** 18, 42
**Impact:** Wasted bandwidth, poor performance
**Issues:**
- Unused `string corpseName` parameter in DestroyCorpseClientRpc and SetInPlayerHandClientRpc (wastes bandwidth)
- `FindObjectsOfType<Ragdoll>()` called on every client (very expensive O(n) operation)
- Manual loop search through all ragdolls instead of direct reference
- Not using `NetworkObjectReference` for ragdoll lookup
**Current code:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(string corpseName, ulong parentId)
{
    var ragdolls = FindObjectsOfType<Ragdoll>();  // ⚠️ VERY EXPENSIVE!
    foreach (var rag in ragdolls)
    {
        if (rag.ParentId == parentId) { ... }
    }
}
```
**Fix:** Remove unused string parameter, use `NetworkObjectReference` for efficient lookup
**Proper pattern:**
```csharp
[ClientRpc]
public void DestroyCorpseClientRpc(NetworkObjectReference ragdollRef)
{
    if (ragdollRef.TryGet(out NetworkObject ragdollNetObj))
    {
        Destroy(ragdollNetObj.gameObject);
    }
}
```

#### 18. Refactor artifacts in folder names
**Folders:** `RefactorInventory`, `RefactorBrute`, `NewItemSystem`
**Impact:** Unprofessional, confusing
**Fix:** Rename folders to proper names

#### 19. Commented code everywhere
**Impact:** Reduces code clarity
**Fix:** Remove all commented code (100+ lines)

---

### LOW (Quick Wins)

#### 20. Line 116 bug in PlayerAnimation.cs (QUICK FIX!)
**File:** `Assets\_Project\Code\Art\AnimationScripts\Animations\PlayerAnimation.cs`
**Line:** 116
**Impact:** Walk animation plays as run on TPS view
**Fix:** Change one boolean: `PlayIKWalk(1f, true)` → `PlayIKWalk(1f, false)` (5 minute fix)
**Estimated Time:** 5 minutes

#### 21. Typo in PlayeNameTag.cs
**Files:** 2 files affected
**Impact:** Unprofessional
**Fix:** Rename files and classes to PlayerNameTag

#### 22. Debug.Log spam
**Impact:** Console clutter, performance
**Fix:** Replace with proper logging system or remove

#### 23. RequireOwnership = false overuse
**Files:** 17+ RPCs
**Impact:** Security/clarity issue
**Fix:** Only disable when truly needed, document why

#### 24. Abandoned MVC implementation (DISCOVERED 2025-11-07)
**Folder:** `Assets\_Project\Code\Gameplay\MVCItems\`
**Files:** BaseballBat/, Flashlight/, SampleJar/ subdirectories
**Impact:** Dead code, architectural confusion, duplicate baseball bat implementations
**Issues:**
- Traditional MVC pattern doesn't fit Unity NGO architecture
- Model classes can't have NetworkVariables (must be in NetworkBehaviour)
- Duplicate baseball bat: MVCItems/BaseballBat vs NewItemSystem/BaseballBatItem.cs
- MVC was attempted but abandoned, code left in place
- NewItemSystem/ is the active implementation
**Explanation:** Unity's component system already provides separation of concerns:
- Data: NetworkVariables in NetworkBehaviour + ScriptableObjects
- Logic: NetworkBehaviour methods + ServerRpc/ClientRpc
- View: Renderer/Animator/AudioSource components
**Fix:** Delete entire `Assets/_Project/Code/Gameplay/MVCItems` folder
**See:** `.claude/docs/architecture-overview.md` for Unity pattern vs MVC analysis

---

## Patterns Observed

### GOOD Patterns ✓

1. **EventBus implementation** - Clean event system for loose coupling
   - File: `Assets\_Project\Code\Utilities\EventBus\EventBus.cs`
   - Used properly for WalletUpdate, ResearchProgressUpdate events

2. **State machine pattern** - Proper separation of enemy behavior states
   - Files: `BeetleStateMachine.cs`, `BruteStateMachine.cs`
   - Just needs network synchronization

3. **ScriptableObjects for data** - Good data-driven design
   - Used for item data, enemy stats

4. **NetworkObjectReference usage** - Efficient object references in multiplayer
   - Used correctly in inventory system, combat system, enemy AI targeting
   - Example: `NetworkList<NetworkObjectReference> InventoryNetworkRefs`

5. **Top-level folder organization** - _Project/Code structure is logical
   - Clear separation of concerns at high level

---

### BAD Patterns ✗

1. **If (!IsServer) ServerRpc; else DirectCall** - Causes double execution on host
   - Example: BeetleHealth.cs Line 78-91

2. **NetworkList as placeholder array** - Inefficient initialization
   - Example: PlayerInventory.cs NetworkList with 5 placeholder entries

3. **Coroutines waiting for network state** - Race condition prone
   - Example: BaseInventoryItem.cs WaitForHeld, AssignHeldVisualDelayed

4. **Manual NetworkObject.Spawn() calls on scene objects** - Anti-pattern
   - Example: SwingDoors.cs Line 23

5. **Singletons for network-critical state** - Should be NetworkBehaviours
   - Example: WalletBankton, CurrentPlayers

---

## Statistics

### Code Metrics
- **Total Scripts:** 208
  - Assets/_Project: 176
  - Assets/Network: 32
- **NetworkBehaviour Scripts:** 22
- **ServerRpc Count:** ~30
- **ClientRpc Count:** ~15
- **Total Lines of Code:** ~15,000+ (estimated)

### Singleton Metrics
- **Total Singletons:** 9
  - Regular Singleton<T>: 5
  - NetworkBehaviour singletons: 4
- **Different Patterns:** 3 (inconsistent)

### Quality Metrics
- **Duplicate Script Sets:** 4+ confirmed
- **Lines of Commented Code:** 100+
- **Files with Chinese Characters:** 2
- **Typos in Class Names:** 1 (PlayeNameTag)
- **Folders with "Refactor" in name:** 2
- **Folders with "New" in name:** 1

### Network Metrics
- **RequireOwnership = false:** 17+ instances
- **Race Conditions:** 5+ identified
- **Missing Authority Checks:** 3+ critical cases
- **Manual Spawn() Calls:** 2+ (incorrect usage)

---

## Recommendations Summary

### Immediate (Do First)
1. Delete Assets/Network folder
2. Fix EnemySpawnManager to use NetworkBehaviour
3. Convert WalletBankton to networked singleton
4. Add network checks to BruteHealth.OnHit
5. Remove all commented code

### Short-term (This Sprint)
6. Standardize singleton pattern with NetworkSingleton<T>
7. Fix item pickup flow (simplify BaseInventoryItem)
8. Sync enemy state machines
9. Rename refactor folders
10. Add proper logging system

### Long-term (Next Month)
11. Decide on ServiceLocator (implement or remove)
12. Create network authority documentation
13. Add network validation layer
14. Standardize RPC patterns
15. Performance audit

---

## Conclusion

This project is **salvageable** but requires **significant refactoring effort**. The core systems (EventBus, State Machines, MVC patterns) show good architectural thinking, but the networking implementation is incomplete and inconsistent.

**Estimated Effort:** 8-12 weeks for 2-3 developers

**Priority:** Focus on critical network bugs first (Phase 1-2), then architecture cleanup (Phase 3-4), then optimization (Phase 5).

**Risk:** Medium-High. Some systems require complete rebuilds (item pickup, enemy spawning), but most can be fixed incrementally.

**Recommendation:** Follow the phased plan in CLAUDE.md, test extensively at each phase, and maintain clear documentation of decisions.

---

**Report End**

For detailed refactoring steps, see:
- `.claude/CLAUDE.md` - Comprehensive plan
- `.claude/docs/migration-guides/` - Step-by-step guides
- `.claude/docs/networking-patterns.md` - Patterns to follow
