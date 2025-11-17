# MultiplayerNGO Refactoring Plan

**Project:** Unity 6 + Netcode for GameObjects Multiplayer Game
**Timeline:** 8-12 weeks for 2-3 developers
**Status:** Planning Phase Complete, Execution Starting
**Last Updated:** 2025-11-07

---

## Executive Summary

This Unity NGO project requires a comprehensive refactoring to address critical networking issues, code duplication, inconsistent patterns, and architectural problems. The codebase shows evidence of rapid iteration with multiple refactoring attempts but lacks consistency in network patterns and architecture.

**Key Problems:**
- Critical network synchronization bugs (enemy spawning, player inventory, economy)
- Duplicate scripts across multiple folders
- 3 different singleton patterns used inconsistently
- Non-networked singletons managing game-critical state
- Complex item pickup system with race conditions
- Enemy AI only runs on server (clients don't see behavior)
- 100+ lines of commented-out code
- ServiceLocator exists but is completely unused

**Estimated Effort:** 8-12 weeks with 2-3 developers working collaboratively

---

## Project Statistics

- **Total Scripts:** 208 (176 in _Project + 32 in Network)
- **NetworkBehaviour Scripts:** 22
- **ServerRpc Implementations:** ~30
- **ClientRpc Implementations:** ~15
- **Singletons:** 9 total (5 regular, 4 network)
- **Duplicate Scripts:** 4+ confirmed sets
- **Lines of Commented Code:** 100+
- **Files with IsServer checks:** 17

---

## Phased Implementation Plan

### Phase 1: Foundation & Cleanup (Week 1-2)

**Goal:** Remove technical debt, establish baseline stability

#### Team Member A: Deduplication & Cleanup
- [ ] Verify no unique code exists in `Assets/Network` folder
- [ ] Delete entire `Assets/Network` folder
- [ ] Delete `Assets/_Project/Code/Gameplay/MVCItems` folder (abandoned MVC attempt, duplicate baseball bat)
- [ ] Remove all commented-out code (100+ lines across codebase)
- [ ] Fix typo: Rename `PlayeNameTag.cs` → `PlayerNameTag.cs` (2 files)
- [ ] Fix NetworkRelay.cs inefficiencies:
  - Remove unused `string corpseName` parameter from `DestroyCorpseClientRpc` and `SetInPlayerHandClientRpc`
  - Replace `FindObjectsOfType<Ragdoll>()` with `NetworkObjectReference` for efficient ragdoll lookup
  - File: `Assets\_Project\Code\Network\ObjectManager\NetworkRelay.cs:18, 42`
- [ ] Rename folders:
  - `RefactorInventory` → `Inventory`
  - `RefactorBrute` → `Brute`
  - `NewItemSystem` → `Items` or merge with existing system
- [ ] Clean up folder structure in `Assets/_Project/Code/Gameplay`

#### Team Member B: Critical Network Fixes
- [ ] Convert `EnemySpawnManager` to `NetworkBehaviour`
  - File: `Assets/_Project/Code/Gameplay/EnemySpawning/EnemySpawnManager.cs`
  - Ensure spawning only happens on server
  - Sync spawn positions to all clients
- [ ] Add network checks to `BruteHealth.OnHit()`
  - File: `Assets\_Project\Code\Gameplay\NPC\Violent\Brute\BruteHealth.cs:31-35`
  - Implement ServerRpc pattern like `BeetleHealth`
- [ ] Fix double-execution bug in `BeetleHealth.OnHit()`
  - File: `Assets\_Project\Code\Gameplay\NPC\Tranquil\Beetle\BeetleHealth.cs:78-91`
  - Host calling logic twice (once in OnHit, once in ServerRpc)
- [ ] Audit all `RequireOwnership = false` usage
  - Document why it's needed for each case
  - Remove where not necessary

#### Team Member C: Documentation & Testing
- [ ] Document current network architecture
- [ ] Create network testing checklist
- [ ] Set up network testing environment (2+ builds for local testing)
- [ ] Complete `.claude/docs/audit-report.md`
- [ ] Begin `.claude/docs/testing-guide.md`

**Deliverables:**
- Clean codebase with no duplicates
- Basic network stability
- Testing infrastructure ready

**Breaking Changes:** ⚠️ Low risk - mostly cleanup

---

### Phase 2: Singleton & Service Architecture (Week 3-4)

**Goal:** Standardize patterns, create proper service layer

#### NetworkSingleton Pattern (REBUILD REQUIRED)

Create a new base class to replace 3 different singleton implementations:

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
        {
            Instance = null;
        }
    }
}
```

#### Migration Tasks

- [ ] Create `NetworkSingleton<T>` base class in `Assets/_Project/Code/Core/Patterns/`
- [ ] Migrate `PlayerListManager` to inherit from `NetworkSingleton<PlayerListManager>`
  - File: `Assets\_Project\Code\Network\PlayerList\PlayerListManager.cs`
- [ ] Migrate `NetworkRelay` to `NetworkSingleton<NetworkRelay>`
  - File: `Assets\_Project\Code\Network\NetworkRelay.cs`
- [ ] Migrate `SteamLobbyManager` to `NetworkSingleton<SteamLobbyManager>`
  - File: `Assets\_Project\Code\Network\SteamLobby\SteamLobbyManager.cs`

#### WalletBankton Conversion (CRITICAL)

- [ ] Convert `WalletBankton` to `NetworkSingleton<WalletBankton>`
  - File: `Assets\_Project\Code\Utilities\Singletons\WalletBankton.cs`
- [ ] Replace `int TotalMoney` with `NetworkVariable<int> TotalMoney`
- [ ] Replace `float CurrentResearchProgress` with `NetworkVariable<float>`
- [ ] Update all AddSubMoney/AddSubResearch to use ServerRpc
- [ ] Add OnValueChanged callbacks to publish EventBus updates
- [ ] Test money synchronization across multiple clients

#### CurrentPlayers Evaluation

- [ ] Analyze all references to `CurrentPlayers` singleton
- [ ] Determine if it can be deprecated in favor of `NetworkManager.Singleton.ConnectedClientsList`
- [ ] If keeping: Convert to `NetworkSingleton` with `NetworkList<NetworkObjectReference>`
- [ ] If removing: Update enemy AI to use NetworkManager directly

#### ServiceLocator Decision

**Option A:** Fully implement ServiceLocator
- Convert all singletons to services
- Register services in GameInitializer
- Use dependency injection pattern

**Option B:** Remove ServiceLocator (RECOMMENDED)
- Less complexity for networking
- NetworkSingleton pattern is simpler and Unity-idiomatic
- Delete `Assets/_Project/Code/Utilities/ServiceLocator/` folder

- [ ] **Decision:** Discuss with team and choose Option A or B
- [ ] Execute chosen option

**Deliverables:**
- Unified singleton pattern across codebase
- Networked game-critical state (money, research)
- Clear architecture decision (ServiceLocator vs NetworkSingleton)

**Breaking Changes:** ⚠️⚠️ Medium - All singleton access patterns change

---

### Phase 3: Network Synchronization (Week 5-7)

**Goal:** Fix all desync issues, implement proper authority model

#### Item Pickup System Rebuild (CRITICAL)

Current system in `BaseInventoryItem.cs:165-228` is too complex:
- 60+ lines with nested coroutines
- Race conditions between spawn and ownership transfer
- Multiple `WaitFor` coroutines chained together
- Unclear authority flow

**New simplified flow:**
1. Client calls `RequestPickupServerRpc(playerId)`
2. Server validates (range check, available slot check)
3. Server grants pickup, assigns ownership
4. Server calls `SyncPickupClientRpc()` to update all clients
5. Item state synchronized via NetworkVariables

Tasks:
- [ ] Design new pickup flow (see `migration-guides/item-pickup-refactor.md`)
- [ ] Backup current `BaseInventoryItem.cs`
- [ ] Rewrite `PickupItem()` method with simplified logic
- [ ] Remove coroutine chains (`WaitForHeld`, `AssignHeldVisualDelayed`)
- [ ] Add proper validation in ServerRpc
- [ ] Test with 2+ clients picking up items simultaneously
- [ ] Update `PlayerInventory.cs` NetworkList initialization (remove placeholder hack)

#### Enemy State Synchronization (REBUILD REQUIRED)

Current state machines only run on server - clients see frozen enemies.

**Files to modify:**
- `Assets\_Project\Code\Gameplay\NPC\Tranquil\Beetle\BeetleStateMachine.cs`
- `Assets\_Project\Code\Gameplay\NPC\Violent\Brute\BruteStateMachine.cs`

Tasks:
- [ ] Add `NetworkVariable<int> CurrentStateIndex` to state machines
- [ ] Create `OnStateChanged` callback to trigger animations on clients
- [ ] Sync state transitions via `SetStateServerRpc()`
- [ ] Sync critical transforms (position, rotation) via NetworkTransform
- [ ] Ensure animations play on all clients
- [ ] Test enemy behavior visibility from non-host client

#### Level Generation Sync Fix

File: `Assets\_Project\Code\Gameplay\DungeonGenerator\LevelNetworkSync.cs`

Current issues:
- Arbitrary 0.2s delay (Line 72) - race condition prone
- Manual waiting for NetworkManager.Singleton

Tasks:
- [ ] Remove `yield return new WaitForSeconds(0.2f)`
- [ ] Use `NetworkManager.OnServerStarted` callback instead of `WaitUntil`
- [ ] Ensure dungeon generation completes before player spawning
- [ ] Fix `SendDungeonDataClientRpc` - remove unused `ulong id` parameter
- [ ] Use `ClientRpcParams` if targeting specific client needed
- [ ] Test level sync with late-joining clients

#### IK Animation Synchronization (HIGH PRIORITY)

**CONFIRMED BUG:** "TPS Baseball bat hand position is still fucked up" (Git commit 60cb67e0)

**File:** `Assets\_Project\Code\Art\AnimationScripts\IK\PlayerIKController.cs`

Current issues:
- PlayerIKController is MonoBehaviour, not NetworkBehaviour
- IK target transforms (handL, handR) not synchronized
- DOTween animations run client-side only (timing desyncs)
- Line 116 bug in PlayerAnimation.cs (quick fix: `true` → `false`)
- Dual IK controller architecture (FPS/TPS) can cause race conditions

**Quick Win (5 minutes):**
- [ ] Fix Line 116 in PlayerAnimation.cs: `PlayIKWalk(1f, true)` → `PlayIKWalk(1f, false)`
- [ ] Test: Verify walk animation (not run) plays correctly on TPS view

**Full Fix (6-8 hours):**
- [ ] Convert `PlayerIKController` to `NetworkBehaviour`
- [ ] Add NetworkVariables for hand positions (netHandRPosition, netHandRRotation, etc.)
- [ ] Update `OnAnimatorIK()` to use synced values for non-owners
- [ ] Convert `IKInteractable` to NetworkBehaviour (optional but recommended)
- [ ] Add NetworkVariables for animation state and timing
- [ ] Sync DOTween animation triggers via ServerRpc → ClientRpc
- [ ] Test with baseball bat, flashlight, and all other items
- [ ] Verify TPS view shows correct hand positions
- [ ] See: `.claude/docs/ik-animation-guide.md` for complete analysis
- [ ] See: `.claude/docs/migration-guides/ik-sync-refactor.md` for step-by-step guide

**Deliverables:**
- Reliable item pickup with no duplication
- Enemy AI behavior visible on all clients
- Level generation synchronized properly
- **IK hand positions synced across all clients (TPS baseball bat fixed!)**
- No more race conditions in initialization

**Breaking Changes:** ⚠️⚠️⚠️ High - Requires retesting all multiplayer gameplay

---

### Phase 4: Authority & Validation (Week 8-9)

**Goal:** Security, cheat prevention, clear ownership model

#### Authority Documentation

Create comprehensive documentation of network authority:

- [ ] Create `.claude/docs/authority-model.md`
- [ ] Document what runs on Server vs Client vs Both
- [ ] Create flowcharts for key systems:
  - Combat damage flow
  - Item pickup flow
  - Enemy spawning flow
  - Player movement flow
- [ ] Document ownership for each NetworkObject type
- [ ] Create "Network Authority Cheat Sheet" for team

#### RPC Pattern Standardization

**Old pattern (causes double execution on host):**
```csharp
public void OnHit(float damage)
{
    if (!IsServer)
    {
        OnHitServerRpc(damage);
        return;
    }
    ApplyDamage(damage);
}
```

**New pattern (cleaner, no double execution):**
```csharp
public void OnHit(float damage)
{
    OnHitServerRpc(damage);
}

[ServerRpc(RequireOwnership = false)]
private void OnHitServerRpc(float damage)
{
    ApplyDamage(damage);
}
```

Tasks:
- [ ] Update all combat scripts to use new pattern
- [ ] Document pattern in `.claude/docs/networking-patterns.md`
- [ ] Add to code templates

#### Validation & Cheat Prevention

- [ ] Add position validation to movement ServerRpcs (speed hack detection)
- [ ] Add range checks to item pickup (distance limits)
- [ ] Add cooldown validation to action ServerRpcs (rate limiting)
- [ ] Validate inventory operations (no item duplication)
- [ ] Add server-side health validation (no health hacking)

#### RequireOwnership Audit

- [ ] Review all 17+ instances of `RequireOwnership = false`
- [ ] Document why each one needs it
- [ ] Add comments explaining security implications
- [ ] Re-enable ownership where possible

**Deliverables:**
- Clear authority model documentation
- Standardized RPC patterns
- Cheat-resistant validation layer
- Security audit complete

**Breaking Changes:** ⚠️⚠️ Medium - Changes RPC call patterns

---

### Phase 5: Optimization & Polish (Week 10-12)

**Goal:** Performance, maintainability, developer experience

#### Performance Optimizations

- [ ] Replace `PollEveryFrame` in `NetworkBinder.cs` with `OnValueChanged` callbacks
- [ ] Audit all NetworkVariable usage - minimize sync frequency
- [ ] Audit all NetworkList usage - use sparse updates where possible
- [ ] Reduce RPC call frequency in high-traffic areas
- [ ] Add NetworkVariable read/write tracking (identify bottlenecks)
- [ ] Consider implementing client-side prediction for player movement
- [ ] Consider implementing interpolation for enemy movement

#### Code Quality

- [ ] Replace all `Debug.Log` with proper logging system
  - Create `GameLogger.cs` with levels (Info, Warning, Error)
  - Add network context to logs (IsServer, IsClient, ClientId)
  - Remove Chinese characters from logs
- [ ] Add XML documentation to all public APIs
- [ ] Add region markers to organize large files
- [ ] Run code formatter/linter on entire project
- [ ] Remove magic numbers (replace with named constants)

#### Developer Experience

- [ ] Create code snippets for common patterns in IDE
- [ ] Set up automated network testing (if possible)
- [ ] Create debugging tools:
  - Network state visualizer
  - RPC call logger
  - Authority debugger
- [ ] Update all `.claude` documentation with final patterns
- [ ] Create onboarding guide for new team members

**Deliverables:**
- Optimized network performance
- Clean, documented, maintainable code
- Excellent developer experience
- Production-ready codebase

**Breaking Changes:** ⚠️ Low - Mostly internal improvements

---

## Risk Assessment

### High Risk (Requires Complete Rebuild)

**Item Pickup System**
- Current complexity: 60+ lines with coroutines
- Risk: Breaking existing item interactions
- Mitigation: Thorough testing, backup old code, phase in gradually

**Enemy State Machines**
- Current: Only runs on server
- Risk: Breaking enemy AI behaviors
- Mitigation: Test each enemy type individually, keep old code commented during transition

**WalletBankton Money Sync**
- Current: Local only, no network sync
- Risk: Breaking economy, mission rewards
- Mitigation: Test all money-earning paths, backup saves before testing

### Medium Risk (Significant Refactor)

**Singleton Pattern Migration**
- Touches many files across project
- Risk: Breaking initialization order
- Mitigation: Migrate one singleton at a time, test after each

**Level Generation Sync**
- Currently has race conditions
- Risk: Clients spawning in wrong locations
- Mitigation: Test with various network latencies, late-join scenarios

### Low Risk (Incremental Fixes)

**Cleanup Tasks** (Phase 1)
- Removing duplicates, comments
- Risk: Minimal, just cleaner code
- Mitigation: Git commit before deletions

**Debug Log Replacement**
- Simple find-and-replace
- Risk: None
- Mitigation: None needed

---

## Success Metrics

### Must-Have (Phase 1-3)
- ✅ No duplicate scripts in codebase
- ✅ All game-critical state synchronized across clients
- ✅ Enemy AI behavior visible on all clients
- ✅ Item pickup works reliably without race conditions
- ✅ Money/economy synced properly
- ✅ Level generation works for all clients

### Should-Have (Phase 4)
- ✅ Zero commented-out code blocks
- ✅ Single consistent singleton pattern
- ✅ All RPCs have proper authority checks
- ✅ Validation layer prevents common cheats
- ✅ Clear authority documentation

### Nice-to-Have (Phase 5)
- ✅ Network testing suite in place
- ✅ Optimized RPC/NetworkVariable usage
- ✅ Developer debugging tools
- ✅ Comprehensive code documentation
- ✅ Onboarding guide for new team members

---

## Team Coordination

### Recommended Split for 2-3 Developers

**Developer 1: Network Systems Lead**
- NetworkSingleton pattern implementation
- Item pickup rebuild
- Enemy state sync
- Authority model documentation

**Developer 2: Code Quality & Architecture**
- Deduplication and cleanup
- Folder restructuring
- Documentation writing
- Testing infrastructure

**Developer 3: Gameplay Systems** (if available)
- Combat system fixes
- Enemy spawning
- Level generation sync
- Validation and security

### Communication

- Daily standups to coordinate
- Code reviews for all network changes
- Shared testing sessions (multiple clients running)
- Document decisions in `.claude/progress/` folder

---

## Testing Strategy

### Unit Testing
- Test NetworkVariable synchronization
- Test RPC call patterns
- Test authority checks

### Integration Testing
- Test item pickup flow end-to-end
- Test enemy spawning and AI
- Test level generation sync

### Multiplayer Testing
- 2 clients (local builds)
- Host + client configuration
- Late-join scenarios
- Network latency simulation

See `.claude/docs/testing-guide.md` for detailed testing procedures.

---

## Documentation Map

- `CLAUDE.md` - This comprehensive plan
- `docs/audit-report.md` - Detailed initial audit findings
- `docs/networking-patterns.md` - Standard patterns to follow
- `docs/architecture-overview.md` - Target architecture
- `docs/authority-model.md` - Network ownership documentation
- `docs/testing-guide.md` - Multiplayer testing procedures
- `docs/migration-guides/` - Step-by-step refactoring guides
  - `singletons-to-network.md` - Migrating singletons
  - `item-pickup-refactor.md` - Rebuilding item system
  - `state-machine-sync.md` - Syncing enemy AI
- `commands/` - Claude Code slash commands for auditing
- `templates/` - Code templates for common patterns
- `progress/` - Phase checklists for tracking

---

## Next Steps

1. ✅ Complete .claude folder structure
2. ✅ Write all documentation files
3. ⏭️ Begin Phase 1: Deduplication & Cleanup
4. ⏭️ Set up testing environment
5. ⏭️ Start network fixes

---

**Last Updated:** 2025-11-07
**Status:** Documentation Complete, Ready to Execute

For questions or clarifications, refer to the detailed documentation in `.claude/docs/` or consult the team during daily standups.
