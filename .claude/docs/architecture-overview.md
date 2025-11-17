# MultiplayerNGO - Target Architecture Overview

**Version:** 1.0
**Last Updated:** 2025-11-07
**Status:** Target Architecture (Post-Refactor)

This document describes the target architecture for the MultiplayerNGO project after completing the refactoring plan. It outlines the high-level structure, key components, and design principles.

---

## Table of Contents

1. [Architecture Principles](#architecture-principles)
2. [System Overview](#system-overview)
3. [Layer Architecture](#layer-architecture)
4. [Core Systems](#core-systems)
5. [Network Architecture](#network-architecture)
6. [Data Flow](#data-flow)
7. [Folder Structure](#folder-structure)

---

## Architecture Principles

### 1. Server Authority
- All game logic executes on the server
- Clients send input, server validates and executes
- Server broadcasts state changes to clients

### 2. Separation of Concerns
- Clear separation between logic, presentation, and data
- Network code isolated from gameplay logic where possible
- UI reacts to state changes, doesn't modify state directly

### 3. Event-Driven Communication
- Systems communicate via EventBus
- Loose coupling between components
- Easy to add/remove features without breaking dependencies

### 4. Predictable Initialization
- Clear initialization order
- No race conditions in startup
- Proper use of NetworkBehaviour lifecycle

### 5. Single Responsibility
- Each class has one clear purpose
- Small, focused scripts
- Composition over inheritance

---

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      Game Manager Layer                      │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ GameManager │  │ WalletManager│  │ ProgressManager  │   │
│  │ (Singleton) │  │  (Networked) │  │   (Networked)    │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Network Services Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ NetworkRelay │  │ LobbyManager │  │ PlayerListMgr    │  │
│  │  (Singleton) │  │  (Singleton) │  │   (Singleton)    │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Gameplay Systems Layer                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Player       │  │ Enemy        │  │ Item             │  │
│  │ System       │  │ System       │  │ System           │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Combat       │  │ Inventory    │  │ Level            │  │
│  │ System       │  │ System       │  │ Generation       │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer (Client)               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ UI           │  │ Audio        │  │ Visual           │  │
│  │ System       │  │ System       │  │ Effects          │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                         Utility Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ EventBus     │  │ StateMachine │  │ ObjectPool       │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Layer Architecture

### Layer 1: Game Manager Layer
**Purpose:** High-level game state management, networked singletons for game-critical state

**Components:**
- `GameManager` - Overall game state, scene management
- `WalletManager` - Networked money and economy
- `ProgressManager` - Networked research and progression

**Rules:**
- Always NetworkSingleton<T>
- Manage game-wide state
- Communicate via EventBus
- No direct gameplay logic

---

### Layer 2: Network Services Layer
**Purpose:** Network connection, lobby, player management

**Components:**
- `NetworkRelay` - Steam/Unity relay connection management
- `SteamLobbyManager` - Steam lobby creation and joining
- `PlayerListManager` - Track connected players

**Rules:**
- NetworkSingleton<T> pattern
- Handle connection lifecycle
- Player spawning and despawning
- No gameplay logic

---

### Layer 3: Gameplay Systems Layer
**Purpose:** Core gameplay logic (networked)

#### Player System
**Components:**
- `PlayerController` - Movement, input handling
- `PlayerHealth` - Health and damage
- `PlayerInventory` - Item management
- `PlayerAnimator` - Animation controller

**Authority:** Owner (client) for input, Server for state changes

#### Enemy System
**Components:**
- `EnemySpawner` - Spawns enemies (server-only)
- `EnemyAI` - State machine for behavior (server-only logic, synced state)
- `EnemyHealth` - Health and damage (server authority)
- `EnemyAnimator` - Synced animations (client-side)

**Authority:** Server for all logic

#### Item System
**Components:**
- `BaseItem` - Base class for all items
- `ItemPickup` - World pickup logic
- `UsableItem` - Items that can be used (weapons, flashlight, etc.)
- `ItemData` - ScriptableObject for item definitions

**Authority:** Server for pickup/drop, Owner for usage

#### Combat System
**Components:**
- `DamageSource` - Applies damage (weapons, hazards)
- `IDamageable` - Interface for things that can take damage
- `CombatValidator` - Server-side validation

**Authority:** Server for all damage calculations

#### Inventory System
**Components:**
- `InventorySlot` - Single inventory slot
- `InventoryUI` - Client-side inventory display
- `InventorySync` - NetworkList synchronization

**Authority:** Server for modifications, Client for display

#### Level Generation System
**Components:**
- `DungeonGenerator` - Procedural level generation (server-only)
- `LevelNetworkSync` - Syncs level data to clients
- `NavMeshSync` - Syncs navigation mesh

**Authority:** Server generates, clients receive

---

### Layer 4: Presentation Layer (Client-Only)
**Purpose:** Visual and audio feedback, UI

**Components:**
- `UIManager` - Local UI management
- `AudioManager` - Local audio playback
- `VFXManager` - Visual effects spawning

**Rules:**
- No NetworkBehaviour (client-local only)
- Reacts to EventBus events
- Never modifies game state
- Uses regular Singleton<T>

---

### Layer 5: Utility Layer
**Purpose:** Shared utilities and patterns

**Components:**
- `EventBus` - Event system for loose coupling
- `StateMachine` - Generic state machine
- `ObjectPool` - Object pooling for performance
- `ServiceLocator` - Optional dependency injection (TBD)

**Rules:**
- No game logic
- Reusable across projects
- Well-documented
- Unit tested

---

## Core Systems

### Network Singleton System

**Base Class:**
```csharp
public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
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
            Instance = null;
    }
}
```

**Networked Singletons:**
- `GameManager`
- `WalletManager`
- `ProgressManager`
- `NetworkRelay`
- `SteamLobbyManager`
- `PlayerListManager`
- `EnemySpawnManager`

---

### Event System

**EventBus Pattern:**
```csharp
// Define events
public class WalletUpdateEvent { }
public class PlayerDeathEvent { public ulong PlayerId; }
public class EnemySpawnedEvent { public NetworkObjectReference EnemyRef; }

// Publish
EventBus.Instance.Publish(new WalletUpdateEvent());

// Subscribe
EventBus.Instance.Subscribe<WalletUpdateEvent>(OnWalletUpdated);

// Unsubscribe
EventBus.Instance.Unsubscribe<WalletUpdateEvent>(OnWalletUpdated);
```

**Key Events:**
- `WalletUpdateEvent` - Money changed
- `ResearchProgressUpdateEvent` - Research progress changed
- `PlayerSpawnedEvent` - Player joined
- `PlayerDiedEvent` - Player died
- `EnemySpawnedEvent` - Enemy spawned
- `EnemyDiedEvent` - Enemy killed
- `ItemPickedUpEvent` - Item picked up
- `LevelGeneratedEvent` - Level generation complete

---

### State Machine System

**Generic State Machine:**
```csharp
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

public class StateMachine
{
    private IState currentState;

    public void TransitionTo(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
```

**Networked State Machine:**
```csharp
public class EnemyAI : NetworkBehaviour
{
    private NetworkVariable<EnemyStateType> netState;
    private StateMachine stateMachine;

    private void Update()
    {
        if (IsServer)
            stateMachine.Update();
    }

    [ServerRpc]
    public void TransitionToServerRpc(EnemyStateType newState)
    {
        netState.Value = newState;
    }

    private void OnStateChanged(EnemyStateType oldState, EnemyStateType newState)
    {
        // Update local state machine
        stateMachine.TransitionTo(GetState(newState));
    }
}
```

---

## Network Architecture

### Client-Server Model

```
┌────────────────┐         ┌────────────────┐         ┌────────────────┐
│                │         │                │         │                │
│  Client 1      │◄───────►│     Server     │◄───────►│  Client 2      │
│  (Player)      │         │   (Host/DS)    │         │  (Player)      │
│                │         │                │         │                │
└────────────────┘         └────────────────┘         └────────────────┘
       │                          │                          │
       │                          │                          │
       ▼                          ▼                          ▼
  ┌─────────┐              ┌─────────┐                ┌─────────┐
  │ Input   │              │ Game    │                │ Input   │
  │ Send    │──ServerRpc──►│ Logic   │◄──ServerRpc────│ Send    │
  └─────────┘              │ Execute │                └─────────┘
       ▲                   └─────────┘                     ▲
       │                        │                          │
       │                        │                          │
       │                   ┌────▼─────┐                    │
       │                   │ Validate │                    │
       │                   │  & Sync  │                    │
       │                   └────┬─────┘                    │
       │                        │                          │
       └────────ClientRpc───────┴──────ClientRpc──────────┘
            (State Updates)              (State Updates)
```

### Authority Model

**Server Authority:**
- Enemy spawning
- Enemy AI logic
- Damage calculations
- Item pickup validation
- Economy (money, research)
- Level generation

**Client Authority:**
- Local input
- Camera control
- UI state
- Audio settings
- Visual effects (local)

**Shared Authority (Owner):**
- Player movement (client prediction)
- Player animation (synced)
- Held item usage (client request, server validates)

---

### Network Communication Flow

**Example: Player Deals Damage to Enemy**

```
1. Client A: Player swings weapon
   └─► Local animation starts (prediction)

2. Client A → Server: "I attacked at position X with weapon Y"
   └─► SwingWeaponServerRpc(position, weaponId)

3. Server: Validates attack
   ├─► Check: Is player in range?
   ├─► Check: Is weapon on cooldown?
   ├─► Check: Does enemy exist?
   └─► Calculate damage

4. Server: Applies damage to enemy
   └─► enemy.Health.Value -= damage

5. Server → All Clients: Enemy health changed
   └─► NetworkVariable.OnValueChanged

6. All Clients: Update enemy health UI/VFX
   └─► PlayDamageEffect()
```

---

## Data Flow

### Game State Data Flow

```
┌──────────────────────────────────────────────────────────────┐
│                         SERVER                               │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │ NetworkVariable<GameState>                         │    │
│  │  - Money                                           │    │
│  │  - Research Progress                               │    │
│  │  - Current Level                                   │    │
│  └────────────────┬───────────────────────────────────┘    │
│                   │ OnValueChanged                         │
│                   │                                        │
└───────────────────┼────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
┌───────────────┐       ┌───────────────┐
│   CLIENT 1    │       │   CLIENT 2    │
│               │       │               │
│  Update UI    │       │  Update UI    │
│  Publish      │       │  Publish      │
│  Event        │       │  Event        │
└───────────────┘       └───────────────┘
```

### Player Input Data Flow

```
┌───────────────┐
│   CLIENT 1    │
│  (Player)     │
│               │
│  Input.GetKey │
│  Move Vector  │
└───────┬───────┘
        │
        │ MoveServerRpc(vector)
        │
        ▼
┌───────────────────────┐
│      SERVER           │
│                       │
│  Validate Input       │
│  Apply Movement       │
│  Physics Checks       │
│                       │
│  transform.position   │
│  (NetworkTransform    │
│   auto-syncs)         │
└───────┬───────────────┘
        │
        │ Auto-sync via NetworkTransform
        │
    ┌───┴────┐
    │        │
    ▼        ▼
┌────────┐ ┌────────┐
│CLIENT 1│ │CLIENT 2│
│ Update │ │ Update │
│Position│ │Position│
└────────┘ └────────┘
```

---

## Folder Structure

### Target Folder Organization

```
Assets/
├── _Project/
│   ├── Code/
│   │   ├── Core/
│   │   │   ├── Patterns/
│   │   │   │   ├── Singleton.cs
│   │   │   │   └── NetworkSingleton.cs
│   │   │   └── GameInitializer.cs
│   │   │
│   │   ├── Network/
│   │   │   ├── NetworkRelay.cs
│   │   │   ├── SteamLobby/
│   │   │   │   └── SteamLobbyManager.cs
│   │   │   └── PlayerList/
│   │   │       └── PlayerListManager.cs
│   │   │
│   │   ├── Gameplay/
│   │   │   ├── Player/
│   │   │   │   ├── PlayerController.cs
│   │   │   │   ├── PlayerHealth.cs
│   │   │   │   ├── PlayerInventory.cs
│   │   │   │   └── PlayerAnimator.cs
│   │   │   │
│   │   │   ├── Enemies/
│   │   │   │   ├── EnemySpawner.cs
│   │   │   │   ├── Beetle/
│   │   │   │   │   ├── BeetleAI.cs
│   │   │   │   │   ├── BeetleHealth.cs
│   │   │   │   │   └── BeetleAnimator.cs
│   │   │   │   └── Brute/
│   │   │   │       ├── BruteAI.cs
│   │   │   │       ├── BruteHealth.cs
│   │   │   │       └── BruteAnimator.cs
│   │   │   │
│   │   │   ├── Items/
│   │   │   │   ├── BaseItem.cs
│   │   │   │   ├── ItemPickup.cs
│   │   │   │   ├── UsableItem.cs
│   │   │   │   ├── Weapons/
│   │   │   │   │   ├── BaseballBat/
│   │   │   │   │   └── Flashlight/
│   │   │   │   └── Data/
│   │   │   │       └── (ScriptableObjects)
│   │   │   │
│   │   │   ├── Combat/
│   │   │   │   ├── IDamageable.cs
│   │   │   │   ├── DamageSource.cs
│   │   │   │   └── CombatValidator.cs
│   │   │   │
│   │   │   ├── Level/
│   │   │   │   ├── DungeonGenerator.cs
│   │   │   │   ├── LevelNetworkSync.cs
│   │   │   │   └── NavMeshSync.cs
│   │   │   │
│   │   │   └── Managers/
│   │   │       ├── GameManager.cs
│   │   │       ├── WalletManager.cs
│   │   │       └── ProgressManager.cs
│   │   │
│   │   ├── UI/
│   │   │   ├── UIManager.cs
│   │   │   ├── InventoryUI.cs
│   │   │   ├── HealthUI.cs
│   │   │   └── WalletUI.cs
│   │   │
│   │   ├── Audio/
│   │   │   └── AudioManager.cs
│   │   │
│   │   └── Utilities/
│   │       ├── EventBus/
│   │       │   ├── EventBus.cs
│   │       │   └── Events.cs
│   │       ├── StateMachine/
│   │       │   ├── IState.cs
│   │       │   └── StateMachine.cs
│   │       └── ObjectPool/
│   │           └── ObjectPool.cs
│   │
│   ├── Prefabs/
│   │   ├── Player.prefab
│   │   ├── Enemies/
│   │   ├── Items/
│   │   └── UI/
│   │
│   ├── ScriptableObjects/
│   │   ├── Items/
│   │   ├── Enemies/
│   │   └── Game/
│   │
│   └── Scenes/
│       ├── MainMenu.unity
│       ├── Lobby.unity
│       └── Game.unity
│
└── Plugins/
    └── (Third-party assets)
```

### Folder Rules

**Code Organization:**
- One script per file
- File name matches class name
- Group related scripts in folders
- Keep folder depth reasonable (max 4 levels)

**Naming Conventions:**
- PascalCase for classes, methods, properties
- camelCase for private fields
- UPPER_CASE for constants
- Prefix interfaces with I (IState, IDamageable)
- Suffix behaviours with logical name (Controller, Manager, Handler)

---

## Design Patterns Used

### Singleton Pattern
**When:** Game managers, network services
**Implementation:** `NetworkSingleton<T>` for networked, `Singleton<T>` for local

### Observer Pattern
**When:** Event-driven communication
**Implementation:** EventBus for loose coupling

### State Pattern
**When:** AI behavior, player states
**Implementation:** IState interface + StateMachine

### Command Pattern
**When:** Player input, action validation
**Implementation:** ServerRpcs as commands

### Object Pool Pattern
**When:** Frequently spawned objects (projectiles, VFX)
**Implementation:** ObjectPool utility

### MVC Pattern (Optional)
**When:** Complex UI or gameplay systems
**Implementation:** Model (data), View (presentation), Controller (logic)

---

## Initialization Flow

### Game Startup Sequence

```
1. Unity Scene Load
   └─► GameInitializer.Awake()
       ├─► Initialize EventBus
       ├─► Initialize Audio/UI managers
       └─► Wait for network ready

2. Network Connection
   └─► NetworkManager.StartHost() or StartClient()
       └─► OnNetworkSpawn() called on all NetworkBehaviours

3. Singleton Initialization
   └─► NetworkSingletons spawn (in order):
       ├─► NetworkRelay
       ├─► SteamLobbyManager
       ├─► PlayerListManager
       ├─► GameManager
       ├─► WalletManager
       └─► ProgressManager

4. Player Spawn
   └─► NetworkManager spawns player prefab
       └─► PlayerController.OnNetworkSpawn()
           ├─► Initialize input
           ├─► Setup camera (if owner)
           └─► Register with PlayerListManager

5. Level Load
   └─► Server: DungeonGenerator.Generate()
       └─► LevelNetworkSync syncs to clients
           └─► Clients: Reconstruct level
               └─► Publish LevelGeneratedEvent

6. Gameplay Ready
   └─► All systems initialized
   └─► EventBus publishes GameReadyEvent
```

---

## Performance Considerations

### Network Optimization

**Reduce RPC Calls:**
- Batch multiple state changes
- Use NetworkVariable for frequent updates
- Avoid RPCs in Update loop

**Minimize Bandwidth:**
- Compress data where possible
- Use appropriate NetworkVariable settings
- Pool NetworkObjects instead of spawning/despawning

**Prediction & Interpolation:**
- Client-side prediction for player movement
- Interpolation for remote player movement
- Server reconciliation for corrections

### Memory Optimization

**Object Pooling:**
- Projectiles
- VFX
- Audio sources
- UI elements

**Async Loading:**
- Level generation in chunks
- Asset loading on demand

---

## Testing Strategy

### Unit Tests
- Utility functions
- State machines
- Event system

### Integration Tests
- Player spawn flow
- Item pickup flow
- Combat damage flow

### Network Tests
- Host + 2+ clients
- Late-join scenarios
- Disconnect/reconnect

See `.claude/docs/testing-guide.md` for details.

---

## Unity Component Pattern vs MVC

**Added:** 2025-11-07

### Why Traditional MVC Doesn't Fit Unity NGO

The project initially attempted to use the Model-View-Controller (MVC) pattern for items (see `Assets/_Project/Code/Gameplay/MVCItems/`), but this approach has been abandoned. Here's why:

---

### The Problem with MVC in Unity Netcode

#### Traditional MVC Architecture

```
┌─────────┐     ┌────────────┐     ┌──────┐
│  Model  │────>│ Controller │────>│ View │
│ (Data)  │     │  (Logic)   │     │ (UI) │
└─────────┘     └────────────┘     └──────┘
```

#### The Critical Flaw

**NetworkVariables MUST be in NetworkBehaviour classes!**

```csharp
// ❌ THIS DOESN'T WORK:
public class BaseballBatModel  // Plain C# class
{
    public bool IsInHand = false;  // Can't be NetworkVariable!
    public GameObject Owner;       // Can't sync over network!
    float damage = 10;
}

public class BaseballBatController : NetworkBehaviour
{
    private BaseballBatModel model;  // Model is local only!
    // Now you need DUPLICATE NetworkVariables here for syncing!
}
```

**Problems:**
1. Model can't have NetworkVariables (not a NetworkBehaviour)
2. Controller needs duplicate NetworkVariables to sync Model data
3. Two sources of truth (Model local data + Controller NetworkVariables)
4. Manual synchronization between Model and NetworkVariables
5. Defeats the purpose of MVC (separation becomes duplication)

---

### Unity's Component-Based Pattern (RECOMMENDED)

Unity already provides excellent separation of concerns through its component system:

#### Architecture Diagram

```
┌──────────────────────────────────────────────────────────┐
│                    NetworkBehaviour                       │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐ │
│  │ DATA LAYER (replaces Model)                         │ │
│  │                                                      │ │
│  │  NetworkVariable<float> _health                     │ │
│  │  NetworkVariable<bool> _isInHand                    │ │
│  │  [SerializeField] BaseballBatSO _itemData          │ │
│  │                                                      │ │
│  │  ScriptableObject = Static/config data             │ │
│  │  NetworkVariable = Dynamic/synced data             │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐ │
│  │ LOGIC LAYER (replaces Controller)                   │ │
│  │                                                      │ │
│  │  [ServerRpc]                                        │ │
│  │  void RequestHitServerRpc() { ... }                │ │
│  │                                                      │ │
│  │  private void ApplyDamage(float damage) { ... }    │ │
│  │                                                      │ │
│  │  public void Use() { ... }                         │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐ │
│  │ VIEW INTEGRATION (replaces View)                    │ │
│  │                                                      │ │
│  │  private void UpdateVisuals() {                    │ │
│  │      _animator.SetBool("InHand", _isInHand.Value); │ │
│  │      _renderer.enabled = _isVisible.Value;         │ │
│  │  }                                                  │ │
│  └─────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
         │
         │ Delegates to separate Unity components
         ▼
┌─────────────────────────────────┐
│     Presentation Components      │
│                                  │
│  ┌────────┐  ┌────────────┐    │
│  │Animator│  │AudioSource│     │
│  └────────┘  └────────────┘    │
│  ┌────────┐  ┌────────────┐    │
│  │Renderer│  │ParticleSys │    │
│  └────────┘  └────────────┘    │
└─────────────────────────────────┘
```

---

### Comparison Table

| Aspect | Traditional MVC | Unity Component Pattern |
|--------|----------------|------------------------|
| **Data Storage** | Plain Model class | NetworkVariables in NetworkBehaviour |
| **Network Sync** | Manual (duplicate in Controller) | Automatic (NetworkVariable) |
| **Configuration** | Hard-coded in Model | ScriptableObject |
| **Logic** | Controller methods | NetworkBehaviour methods + RPCs |
| **Presentation** | View class | Separate Unity components (Animator, Renderer, etc.) |
| **Separation** | Strict class separation | Logical separation within NetworkBehaviour |
| **Learning Curve** | Familiar to web devs | Familiar to Unity devs |
| **Unity Integration** | Fights against Unity | Works with Unity |
| **Networking** | Doesn't fit NGO | Perfect fit for NGO |

---

### Code Example Comparison

#### ❌ MVC Attempt (Found in Project - Abandoned)

```csharp
// BaseballBatModel.cs - Plain C# class
public class BaseballBatModel
{
    public bool IsInHand = false;  // NOT synced
    float damage = 10;             // NOT synced
    public GameObject Owner;       // NOT synced
}

// BaseballBatController.cs - NetworkBehaviour
public class BaseballBatController : NetworkBehaviour
{
    private BaseballBatModel model;  // Local only!

    // Now need to add NetworkVariables to actually sync...
    // This defeats the purpose of having a separate Model!
}

// BaseballBatView.cs - MonoBehaviour
public class BaseballBatView : MonoBehaviour
{
    public void DisplayHeld(Transform position) { ... }
}
```

**Result:** Duplicate code, manual sync, confusing architecture

---

#### ✅ Unity Component Pattern (Recommended)

```csharp
// BaseballBatItem.cs - Single NetworkBehaviour
public class BaseballBatItem : NetworkBehaviour
{
    // === DATA LAYER ===
    // Dynamic synced data
    NetworkVariable<bool> _isInHand = new NetworkVariable<bool>();
    NetworkVariable<float> _durability = new NetworkVariable<float>(100f);

    // Static config data (ScriptableObject)
    [SerializeField] private BaseballBatSO _itemData;

    // Local references
    private Animator _animator;
    private Renderer _renderer;

    // === INITIALIZATION ===
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            // Subscribe to state changes
            _isInHand.OnValueChanged += OnIsInHandChanged;
            _durability.OnValueChanged += OnDurabilityChanged;
        }
    }

    // === LOGIC LAYER ===
    [ServerRpc(RequireOwnership = false)]
    public void RequestHitServerRpc(NetworkObjectReference targetRef)
    {
        if (targetRef.TryGet(out NetworkObject target))
        {
            var hitable = target.GetComponent<IHitable>();
            hitable?.OnHit(gameObject, _itemData.damage, _itemData.knockoutPower);

            // Apply durability loss
            _durability.Value -= _itemData.durabilityLossPerHit;
        }
    }

    [ServerRpc]
    public void RequestPickupServerRpc()
    {
        _isInHand.Value = true;
    }

    // === VIEW INTEGRATION ===
    private void OnIsInHandChanged(bool oldValue, bool newValue)
    {
        // Update visuals when state changes
        _renderer.enabled = !newValue;  // Hide world model when in hand
        _animator?.SetBool("InHand", newValue);
    }

    private void OnDurabilityChanged(float oldValue, float newValue)
    {
        // Update durability UI, visual wear-and-tear, etc.
        UpdateDurabilityUI(newValue / 100f);
    }

    private void UpdateDurabilityUI(float percentage)
    {
        // Delegate to UI system via EventBus or direct reference
        EventBus.Instance.Publish(new ItemDurabilityChangedEvent
        {
            item = this,
            durabilityPercent = percentage
        });
    }
}

// BaseballBatSO.cs - ScriptableObject for static data
[CreateAssetMenu(fileName = "BaseballBat", menuName = "Items/Melee/BaseballBat")]
public class BaseballBatSO : ScriptableObject
{
    public float damage = 10f;
    public float knockoutPower = 5f;
    public float attackRange = 1.5f;
    public float durabilityLossPerHit = 5f;
}
```

---

### Separation of Concerns in Unity Pattern

| Concern | How Unity Pattern Handles It |
|---------|------------------------------|
| **Static Data** | ScriptableObjects (damage values, prefab refs) |
| **Dynamic Data** | NetworkVariables (health, state, inventory) |
| **Game Logic** | NetworkBehaviour methods + ServerRpc/ClientRpc |
| **Presentation** | Separate Unity components (Animator, Renderer, AudioSource) |
| **Communication** | EventBus for cross-system messaging |
| **Validation** | Server-side logic in ServerRpc methods |

---

### Industry Standard - What Successful Unity Multiplayer Games Use

**Unity Multiplayer Frameworks:**
- Mirror (formerly UNET) - Component-based
- Photon PUN/Fusion - Component-based
- FishNet - Component-based
- Netcode for GameObjects - Component-based

**Successful Unity Multiplayer Games:**
- Among Us - Component-based
- Fall Guys - Component-based
- Rust - Component-based (ECS variant)
- Valheim - Component-based

**Nobody uses strict MVC** in Unity multiplayer because:
1. NetworkVariables must be in NetworkBehaviour
2. Unity's component system already provides separation
3. Fighting against the framework causes more problems than it solves

---

### Migration from MVC to Unity Pattern

**Current state:**
- `Assets/_Project/Code/Gameplay/MVCItems/` contains abandoned MVC attempt
- Duplicate baseball bat implementations:
  - `MVCItems/BaseballBat/` (old, MVC)
  - `NewItemSystem/BaseballBatItem.cs` (new, component-based)

**Refactoring plan:**
1. ✅ Use `NewItemSystem/BaseballBatItem.cs` as the active implementation
2. ✅ Delete entire `MVCItems/` folder (Phase 1 cleanup)
3. ✅ Document Unity Component Pattern as the standard (this document)
4. ✅ Update all new items to follow Unity Component Pattern
5. ✅ Use ScriptableObjects for item configuration data

---

### Recommended Pattern Template

```csharp
public class [ItemName]Item : NetworkBehaviour
{
    // === DATA ===
    [Header("Configuration")]
    [SerializeField] private [ItemName]SO _itemData;

    [Header("Network State")]
    private NetworkVariable<bool> _isInHand = new NetworkVariable<bool>();
    // Add more NetworkVariables as needed

    [Header("References")]
    private Animator _animator;
    private Renderer _renderer;
    // Add more component references as needed

    // === INITIALIZATION ===
    private void Awake()
    {
        // Cache component references
        _animator = GetComponentInChildren<Animator>();
        _renderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            // Subscribe to NetworkVariable changes
            _isInHand.OnValueChanged += OnIsInHandChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            // Unsubscribe to prevent memory leaks
            _isInHand.OnValueChanged -= OnIsInHandChanged;
        }
        base.OnNetworkDespawn();
    }

    // === GAME LOGIC (ServerRpc for authoritative actions) ===
    [ServerRpc(RequireOwnership = false)]
    public void RequestActionServerRpc()
    {
        // Validate and execute action
        // Modify NetworkVariables (auto-syncs to clients)
    }

    // === VIEW UPDATES (React to state changes) ===
    private void OnIsInHandChanged(bool oldValue, bool newValue)
    {
        // Update visuals when state changes
        _renderer.enabled = !newValue;
        _animator?.SetBool("InHand", newValue);
    }
}
```

---

### Key Takeaways

1. **Don't fight Unity's architecture** - Use components, not MVC classes
2. **NetworkVariables must be in NetworkBehaviour** - Can't be in plain Model classes
3. **ScriptableObjects for static data** - Damage values, prefab references, etc.
4. **NetworkVariables for dynamic data** - Health, state, inventory, etc.
5. **Separation happens logically** - Not through strict class hierarchies
6. **EventBus for cross-system communication** - Maintain loose coupling
7. **Follow the framework** - Unity NGO is designed for component-based architecture

---

## Future Enhancements

### Phase 6+ (Post-Refactor)

**Potential Additions:**
- Save/load system (networked)
- Matchmaking system
- Statistics tracking
- Achievement system
- Voice chat integration
- Anti-cheat improvements
- Performance profiling tools
- Network traffic analyzer

---

## Conclusion

This architecture provides:
- **Clear separation of concerns**
- **Predictable initialization**
- **Scalable network design**
- **Maintainable codebase**
- **Testable components**

Follow this architecture during refactoring to achieve a robust, multiplayer-ready Unity game.

---

**Last Updated:** 2025-11-07

For implementation details, see:
- `.claude/CLAUDE.md` - Refactoring plan
- `.claude/docs/networking-patterns.md` - Code patterns
- `.claude/docs/authority-model.md` - Network ownership
- `.claude/docs/migration-guides/` - Step-by-step guides
