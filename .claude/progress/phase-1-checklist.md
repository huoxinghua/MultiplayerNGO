# Phase 1: Foundation & Cleanup - Checklist

**Duration:** Week 1-2
**Status:** ⏸️ Not Started
**Team:** 2-3 developers

---

## Team Member A: Deduplication & Cleanup

### Deduplication Tasks

- [ ] **Verify Assets/Network folder contents**
  - [ ] Check `Assets/Network/Scripts/PlayerController/` for unique code
  - [ ] Check `Assets/Network/NetWorkUT/` for unique code
  - [ ] Document any unique implementations
  - [ ] Create backup if needed

- [ ] **Delete Assets/Network folder**
  - [ ] Confirmed no unique code exists
  - [ ] Delete entire `Assets/Network` directory
  - [ ] Verify Unity doesn't show errors after deletion
  - [ ] Commit deletion to Git

### Code Cleanup Tasks

- [ ] **Remove commented-out code**
  - [ ] `PlayerHealth.cs` Lines 40-57
  - [ ] `BaseInventoryItem.cs` Lines 65-70, 106-113, 281-327
  - [ ] `SwingDoors.cs` Lines 26-37, 80-104
  - [ ] `BruteHealth.cs` Lines 67-82
  - [ ] Search project for `//` blocks >10 lines
  - [ ] Remove all found commented code
  - [ ] Commit cleanup

- [ ] **Fix naming typos**
  - [ ] Rename `PlayeNameTag.cs` → `PlayerNameTag.cs`
  - [ ] Update class name inside file
  - [ ] Update all references to class
  - [ ] Commit rename

- [ ] **Rename refactor folders**
  - [ ] `RefactorInventory` → `Inventory`
  - [ ] `RefactorBrute` → `Brute` (if exists)
  - [ ] `NewItemSystem` → `Items` (or merge with existing)
  - [ ] Update all script references
  - [ ] Commit renames

- [ ] **Clean up folder structure**
  - [ ] Review `Assets/_Project/Code/Gameplay/` organization
  - [ ] Consolidate item systems if multiple exist
  - [ ] Document final structure in notes
  - [ ] Commit structure changes

---

## Team Member B: Critical Network Fixes

### Enemy Spawning Fix

- [ ] **Convert EnemySpawnManager to NetworkBehaviour**
  - [ ] File: `Assets\_Project\Code\Gameplay\EnemySpawning\EnemySpawnManager.cs`
  - [ ] Change inheritance: `MonoBehaviour` → `NetworkBehaviour`
  - [ ] Add `if (!IsServer) return;` to spawning methods
  - [ ] Ensure `NetworkObject.Spawn()` called after Instantiate
  - [ ] Test: Spawn enemies as host
  - [ ] Test: Client sees same enemies
  - [ ] Commit fix

### BruteHealth Network Fix

- [ ] **Add network checks to BruteHealth.OnHit()**
  - [ ] File: `Assets\_Project\Code\Gameplay\NPC\Violent\Brute\BruteHealth.cs:31-35`
  - [ ] Create `OnHitServerRpc` method
  - [ ] Add validation (range, damage bounds)
  - [ ] Move damage logic to ServerRpc
  - [ ] Test: Host damages brute
  - [ ] Test: Client damages brute
  - [ ] Verify health syncs on both
  - [ ] Commit fix

### BeetleHealth Double-Execution Fix

- [ ] **Fix double-execution bug in BeetleHealth**
  - [ ] File: `Assets\_Project\Code\Gameplay\NPC\Tranquil\Beetle\BeetleHealth.cs:78-91`
  - [ ] Remove direct `ApplyHit` call from `OnHit`
  - [ ] Always call `OnHitServerRpc`
  - [ ] Move logic inside ServerRpc only
  - [ ] Test as host: Verify damage applied once
  - [ ] Test as client: Verify damage syncs
  - [ ] Commit fix

### RequireOwnership Audit

- [ ] **Audit all RequireOwnership = false usage**
  - [ ] Search codebase for `RequireOwnership = false`
  - [ ] Create spreadsheet of all instances (17+ expected)
  - [ ] For each instance:
    - [ ] Determine if truly needed
    - [ ] Add comment explaining why
    - [ ] Consider re-enabling if possible
  - [ ] Document findings in notes
  - [ ] Commit documentation

---

## Team Member C: Documentation & Testing

### Documentation Tasks

- [ ] **Document current network architecture**
  - [ ] Create network diagram of current systems
  - [ ] Document player spawning flow
  - [ ] Document enemy spawning flow
  - [ ] Document item pickup flow (current, before refactor)
  - [ ] Save to `.claude/progress/current-architecture.md`

- [ ] **Complete audit report**
  - [ ] Review `.claude/docs/audit-report.md`
  - [ ] Add any additional findings during Phase 1
  - [ ] Update statistics
  - [ ] Commit updates

- [ ] **Begin testing guide**
  - [ ] Review `.claude/docs/testing-guide.md`
  - [ ] Add project-specific test cases
  - [ ] Document enemy types and their expected behavior
  - [ ] Commit additions

### Testing Infrastructure

- [ ] **Set up network testing environment**
  - [ ] Create development build
  - [ ] Test Host + Client connection
  - [ ] Document build process in notes
  - [ ] Create quick-start testing guide

- [ ] **Create network testing checklist**
  - [ ] Basic connection/disconnection
  - [ ] Player spawning
  - [ ] Enemy spawning (post-fix)
  - [ ] Basic combat
  - [ ] Save to `.claude/progress/test-checklist.md`

- [ ] **Run Phase 1 tests**
  - [ ] Test all Team B fixes
  - [ ] Document results
  - [ ] File bug reports for any issues
  - [ ] Update checklist

---

## Definition of Done

**Phase 1 is complete when:**

- [ ] ✅ No duplicate scripts exist in codebase
- [ ] ✅ No commented-out code blocks
- [ ] ✅ All typos fixed
- [ ] ✅ Folder names are professional
- [ ] ✅ EnemySpawnManager spawns only on server
- [ ] ✅ BruteHealth syncs damage correctly
- [ ] ✅ BeetleHealth has no double-execution
- [ ] ✅ RequireOwnership usage documented
- [ ] ✅ Testing infrastructure ready
- [ ] ✅ All tests pass
- [ ] ✅ All changes committed to Git
- [ ] ✅ Team standup: Confirm Phase 1 complete

---

## Notes

**Blockers:**
- (Document any blockers here)

**Decisions Made:**
- (Document key decisions here)

**Issues Found:**
- (Link to filed issues)

---

**Last Updated:** [DATE]
**Completed By:** [TEAM MEMBER NAMES]
