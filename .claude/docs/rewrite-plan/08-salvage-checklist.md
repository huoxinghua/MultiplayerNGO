# Salvage Checklist - What to Keep from Old Codebase

**Purpose:** Don't throw away good work - identify what's worth keeping
**Timeline:** Day 0 (before starting rewrite)
**Responsible:** Project lead + team review

---

## How to Use This Checklist

For each category:
1. **KEEP** - Copy to new project, use as-is
2. **ADAPT** - Keep the concept, rewrite with networking
3. **DISCARD** - Fundamentally broken, rebuild from scratch

---

## Art & Assets (KEEP - High Value)

### 3D Models & Textures
- [ ] **Player models** - Copy to new project
- [ ] **Enemy models** (Beetle, Brute, etc.) - Copy to new project
- [ ] **Item models** (Baseball bat, Flashlight, Machete, etc.) - Copy to new project
- [ ] **Environment models** (Level geometry, props) - Copy to new project
- [ ] **Textures** - Copy to new project

**Status:** ✅ KEEP ALL - These are networking-agnostic

---

### Animations
- [ ] **Player animations** (Walk, Run, Idle, Attack, Death) - Copy to new project
- [ ] **Enemy animations** (Beetle, Brute behaviors) - Copy to new project
- [ ] **Item animations** (Swing animations, etc.) - Copy to new project

**Status:** ✅ KEEP ALL - Just need to trigger via NetworkVariable state changes

**Note:** IK rig data can be kept, but IK controller scripts need NetworkBehaviour rewrite

---

### Audio
- [ ] **Sound effects** - Copy to new project
- [ ] **Music** - Copy to new project
- [ ] **Audio clips** (footsteps, attacks, UI sounds) - Copy to new project

**Status:** ✅ KEEP ALL

**Note:** AudioManager.cs can likely be kept if it doesn't manage networked state

---

## ScriptableObjects (KEEP - High Value)

### Enemy Data
- [ ] **BeetleSO** - `_Project/Code/Gameplay/NPC/Tranquil/Beetle/BeetleSO.cs`
  - Contains: MaxHealth, MaxConsciousness, Speed, etc.
  - **Status:** ✅ KEEP - Static data, no networking needed

- [ ] **BruteSO** - Similar for Brute enemy
  - **Status:** ✅ KEEP

- [ ] **EnemyPrefabsSO** - `_Project/Code/Gameplay/EnemySpawning/EnemyPrefabsSO.cs`
  - Contains: Lists of enemy prefabs by type
  - **Status:** ✅ KEEP

- [ ] **SpawnDataSO** - `_Project/Code/Gameplay/EnemySpawning/SpawnDataSO.cs`
  - Contains: Spawn timing, weights, probabilities
  - **Status:** ✅ KEEP

---

### Item Data
- [ ] **BaseItemSO** - Base class for item stats
  - **Status:** ✅ KEEP - Static data

- [ ] **BaseballBatSO / FlashlightSO / MacheteSO** - Item configurations
  - Contains: Damage, range, durability, etc.
  - **Status:** ✅ KEEP - Just reference from new NetworkBehaviour items

**Action:** Copy all ScriptableObject .cs files and .asset files to new project

---

## UI Prefabs (ADAPT - Medium Value)

### UI Elements
- [ ] **HealthBar** - Visual only, but needs to read NetworkVariable
  - **Status:** ⚠️ ADAPT - Keep prefab, rewrite binding script

- [ ] **InventoryUI** - Visual layout is good
  - **Status:** ⚠️ ADAPT - Keep prefab, rewrite to read NetworkList

- [ ] **MoneyDisplay / ResearchProgressBar** - Visual only
  - **Status:** ⚠️ ADAPT - Keep prefab, rewrite to read NetworkVariable from WalletBankton

- [ ] **Menus** (Main menu, pause menu, etc.)
  - **Status:** ✅ KEEP - Likely not networked

**Action:** Keep all UI prefabs, but expect to rewrite the C# scripts that update them

---

## Prefabs (KEEP/ADAPT)

### Player Prefabs
- [ ] **Player prefab** - GameObject hierarchy is good
  - **Status:** ⚠️ ADAPT
  - Keep: Model, Animator, Camera setup, Colliders
  - Rewrite: All NetworkBehaviour scripts

---

### Enemy Prefabs
- [ ] **Beetle prefab** - GameObject hierarchy
  - **Status:** ⚠️ ADAPT
  - Keep: Model, Animator, NavMeshAgent setup, Colliders
  - Rewrite: All NetworkBehaviour scripts (AI, Health)

- [ ] **Brute prefab** - Similar
  - **Status:** ⚠️ ADAPT

**Action:** Copy prefabs, remove old scripts, add new NetworkBehaviour scripts

---

### Item Prefabs
- [ ] **Baseball bat prefab**
- [ ] **Flashlight prefab**
- [ ] **Machete prefab**
  - **Status:** ⚠️ ADAPT
  - Keep: Model, visual setup
  - Rewrite: NetworkBehaviour item script

**Note:** If duplicate exists in MVCItems/, use the NewItemSystem version

---

## Level Design (KEEP - High Value)

### Scenes
- [ ] **Level layouts** - Dungeon generation setup
  - **Status:** ✅ KEEP - Just need to ensure generation runs on server only

- [ ] **Spawn points** - Enemy spawn locations
  - **Status:** ✅ KEEP - Just use in new EnemySpawnManager

- [ ] **NavMesh** - For enemy pathfinding
  - **Status:** ✅ KEEP

**Action:** Keep scene files, but remove/replace NetworkBehaviour components

---

## Utility Scripts (CASE-BY-CASE)

### Timer Utility
- [ ] **Timer.cs** - `_Project/Code/Utilities/Utility/Timer.cs`
  - Simple countdown timer logic
  - **Status:** ✅ KEEP - Not networked, just utility

---

### EventBus
- [ ] **EventBus.cs** - `_Project/Code/Utilities/EventBus/EventBus.cs`
  - Local event publish/subscribe
  - **Status:** ✅ KEEP - Used correctly (local events)

**Important:** EventBus is LOCAL, not networked. Use NetworkVariable.OnValueChanged to trigger EventBus events.

---

### AudioManager
- [ ] **AudioManager.cs**
  - **Status:** ⚠️ REVIEW
  - If it's just `PlaySound(string key, Vector3 position)` → ✅ KEEP
  - If it tracks networked state → ❌ DISCARD, rebuild

**Action:** Read AudioManager.cs, check if it has any networked state

---

### ServiceLocator
- [ ] **ServiceLocator.cs** - `_Project/Code/Utilities/ServiceLocator/`
  - **Status:** ❌ DISCARD - Completely unused in project
  - **Recommendation:** Delete folder

---

## Game Logic Scripts (DISCARD & REBUILD)

### Player Scripts (❌ All need NetworkBehaviour rewrite)
- [ ] ~~PlayerHealth.cs~~ - No NetworkVariables
- [ ] ~~PlayerMovement.cs~~ - No server validation
- [ ] ~~PlayerInventory.cs~~ - NetworkList initialized wrong
- [ ] ~~PlayerIKController.cs~~ - Not a NetworkBehaviour
- [ ] ~~PlayerAnimation.cs~~ - Has bugs (Line 116)

**Status:** ❌ DISCARD ALL - Rebuild with proper NetworkBehaviour

---

### Enemy Scripts (❌ All need NetworkBehaviour rewrite)
- [ ] ~~BeetleHealth.cs~~ - No NetworkVariable for health
- [ ] ~~BeetleStateMachine.cs~~ - State not synced
- [ ] ~~BruteHealth.cs~~ - No network checks at all
- [ ] ~~BruteStateMachine.cs~~ - Partial network implementation

**Status:** ❌ DISCARD ALL - Rebuild with NetworkVariable<int> for state

---

### Item Scripts (❌ All need NetworkBehaviour rewrite)
- [ ] ~~BaseInventoryItem.cs~~ - Has some NetworkVariables but pickup logic is too complex (60+ lines, coroutines, race conditions)
- [ ] ~~BaseballBatItem.cs~~ - Inherits broken base class
- [ ] ~~FlashlightItem.cs~~ - Same issues
- [ ] ~~MacheteInventoryItem.cs~~ - Same issues

**Status:** ❌ DISCARD ALL - Rebuild with simplified ServerRpc pickup flow

---

### Spawning Scripts (❌ Completely broken)
- [ ] ~~EnemySpawnManager.cs~~ - MonoBehaviour, spawns on all clients
- [ ] ~~EnemySpawnPoints.cs~~ - May be salvageable if it's just data

**Status:** ❌ DISCARD - Critical issue #1 from audit

---

### Economy Scripts (❌ Not networked at all)
- [ ] ~~WalletBankton.cs~~ - Singleton<T>, not networked
- [ ] ~~CurrentPlayers.cs~~ - Singleton<T>, not networked

**Status:** ❌ DISCARD - Rebuild as NetworkSingleton with NetworkVariables

---

### Singleton Base Classes (⚠️ ADAPT)
- [ ] **Singleton.cs** - Regular singleton pattern
  - **Status:** ✅ KEEP - Can still use for non-networked singletons

- [ ] **NetworkSingleton.cs** - Doesn't exist yet!
  - **Status:** ⚠️ CREATE NEW

**Action:** Keep Singleton.cs, create new NetworkSingleton<T> base class

---

## MVC Folder (❌ DISCARD ENTIRELY)

- [ ] ~~MVCItems/BaseballBat/~~ - Abandoned attempt
- [ ] ~~MVCItems/Flashlight/~~ - Abandoned attempt
- [ ] ~~MVCItems/SampleJar/~~ - Abandoned attempt

**Status:** ❌ DELETE ENTIRE FOLDER - See `.claude/docs/architecture-overview.md` for why MVC doesn't fit Unity NGO

---

## Duplicate Files (❌ DELETE)

### Assets/Network/ Folder
- [ ] ~~Assets/Network/~~ - Entire folder is duplicates
  - PlayerMovement.cs (duplicate)
  - PlayerLook.cs (duplicate)
  - GroundCheck.cs (duplicate)
  - PlayeNameTag.cs (typo + duplicate)

**Status:** ❌ DELETE ENTIRE ASSETS/NETWORK FOLDER

---

## Commented Code (❌ DELETE)

**Status:** ❌ DELETE ALL - 100+ lines of commented code found

**Action:** Before deleting, skim to ensure no hidden gems, then remove all commented blocks

---

## Summary Table

| Category | Action | Effort | Value |
|----------|--------|--------|-------|
| **3D Models/Textures** | ✅ KEEP | Copy files | HIGH |
| **Animations** | ✅ KEEP | Copy files | HIGH |
| **Audio** | ✅ KEEP | Copy files | HIGH |
| **ScriptableObjects** | ✅ KEEP | Copy files | HIGH |
| **UI Prefabs** | ⚠️ ADAPT | Keep prefab, rewrite binding | MEDIUM |
| **Level Design** | ✅ KEEP | Copy scenes | HIGH |
| **EventBus** | ✅ KEEP | Copy script | MEDIUM |
| **Timer utility** | ✅ KEEP | Copy script | LOW |
| **Singleton.cs** | ✅ KEEP | Copy script | LOW |
| **Player scripts** | ❌ REBUILD | Rewrite from scratch | N/A |
| **Enemy scripts** | ❌ REBUILD | Rewrite from scratch | N/A |
| **Item scripts** | ❌ REBUILD | Rewrite from scratch | N/A |
| **Spawning** | ❌ REBUILD | Rewrite from scratch | N/A |
| **Economy** | ❌ REBUILD | Rewrite from scratch | N/A |
| **MVCItems/** | ❌ DELETE | Delete folder | N/A |
| **Assets/Network/** | ❌ DELETE | Delete folder | N/A |

---

## Action Plan

### Day 0 - Before Rewrite Starts

**Step 1: Create Clean Repository**
```bash
# Create new branch or new Unity project
git checkout -b rewrite-clean
# or create fresh Unity 6 project
```

**Step 2: Copy Assets (✅ KEEP items)**
- Copy Art/ folder (models, textures, animations, audio)
- Copy ScriptableObjects (all .cs and .asset files)
- Copy UI prefabs (GameObject hierarchies only)
- Copy Scenes (level layouts, NavMesh)
- Copy utility scripts (Timer.cs, EventBus.cs, Singleton.cs)

**Step 3: Prepare for Rebuild (❌ DISCARD items)**
- List all scripts that need NetworkBehaviour rewrite
- Assign to team members (Student A, B, C)
- Reference old code for game logic ONLY (don't copy networking bugs)

**Step 4: Document What Was Learned**
- What game mechanics work well? (keep those)
- What networking patterns failed? (avoid those)
- What architecture decisions were good? (ScriptableObjects, EventBus)

---

## Files to Reference (Not Copy)

When rebuilding, look at old scripts for:
- ✅ Game mechanics (how does attack work? what's the damage formula?)
- ✅ State machine states (what states does Beetle have?)
- ✅ UI layout (how should health bar look?)

But DON'T copy:
- ❌ Networking code (it's broken)
- ❌ Initialization logic (race conditions)
- ❌ RPC patterns (double-execution bugs)

---

## Estimation

**Time to salvage:** 2-4 hours
**Value saved:** ~40% of original work (all non-code assets)
**Time saved:** ~20 hours (not recreating art, audio, levels)

**Net benefit:** Massively worth it - keep the good, discard the broken

---

**Last Updated:** 2025-11-08
**Status:** Ready for Day 0 salvage operation
