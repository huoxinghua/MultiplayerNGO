# Phase 2: Singleton & Service Architecture - Checklist

**Duration:** Week 3-4
**Status:** ⏸️ Not Started
**Team:** 2-3 developers
**Prerequisites:** Phase 1 complete

---

## Pre-Phase Verification

- [ ] All Phase 1 tasks completed
- [ ] All Phase 1 tests passing
- [ ] Git repo clean (all changes committed)
- [ ] Team briefed on Phase 2 goals

---

## NetworkSingleton Pattern Implementation

### Create Base Class

- [ ] **Create NetworkSingleton<T> base class**
  - [ ] File: `Assets\_Project\Code\Core\Patterns\NetworkSingleton.cs`
  - [ ] Implement according to migration guide
  - [ ] Add XML documentation
  - [ ] Test compilation
  - [ ] Commit base class

### Migrate PlayerListManager

- [ ] **Migrate PlayerListManager**
  - [ ] File: `Assets\_Project\Code\Network\PlayerList\PlayerListManager.cs`
  - [ ] Change inheritance to `NetworkSingleton<PlayerListManager>`
  - [ ] Remove custom Instance property
  - [ ] Remove custom Awake (unless needed)
  - [ ] Test: Start host
  - [ ] Test: Access `PlayerListManager.Instance`
  - [ ] Test: Start client, verify no duplicates
  - [ ] Commit migration

### Migrate NetworkRelay

- [ ] **Migrate NetworkRelay**
  - [ ] File: `Assets\_Project\Code\Network\NetworkRelay.cs`
  - [ ] Change inheritance: `MonoBehaviour` → `NetworkSingleton<NetworkRelay>`
  - [ ] Add `NetworkObject` component to GameObject
  - [ ] Adjust network initialization code
  - [ ] Test: Connection/relay still works
  - [ ] Test: No errors on spawn
  - [ ] Commit migration

### Migrate SteamLobbyManager

- [ ] **Migrate SteamLobbyManager**
  - [ ] File: `Assets\_Project\Code\Network\SteamLobby\SteamLobbyManager.cs`
  - [ ] Change inheritance to `NetworkSingleton<SteamLobbyManager>`
  - [ ] Remove custom Awake/Instance code
  - [ ] Add custom Awake if needed (call base.Awake first!)
  - [ ] Test: Lobby creation
  - [ ] Test: Lobby joining
  - [ ] Commit migration

---

## WalletBankton Conversion (CRITICAL!)

### Backup Current Implementation

- [ ] **Create backup**
  - [ ] Copy `WalletBankton.cs` to `WalletBankton.cs.backup`
  - [ ] OR commit current state to Git
  - [ ] Document current behavior in notes

### Convert to NetworkSingleton

- [ ] **Change inheritance**
  - [ ] File: `Assets\_Project\Code\Utilities\Singletons\WalletBankton.cs`
  - [ ] Change: `Singleton<WalletBankton>` → `NetworkSingleton<WalletBankton>`
  - [ ] Add `NetworkObject` component to GameObject

- [ ] **Convert TotalMoney to NetworkVariable**
  - [ ] Replace `int TotalMoney` property
  - [ ] Create `NetworkVariable<int> totalMoney`
  - [ ] Set permissions: Everyone read, Server write
  - [ ] Add public property: `public int TotalMoney => totalMoney.Value`
  - [ ] Test compilation

- [ ] **Convert CurrentResearchProgress to NetworkVariable**
  - [ ] Replace `float CurrentResearchProgress` property
  - [ ] Create `NetworkVariable<float> currentResearchProgress`
  - [ ] Set permissions: Everyone read, Server write
  - [ ] Add public property
  - [ ] Test compilation

- [ ] **Add lifecycle methods**
  - [ ] Override `OnNetworkSpawn()`
  - [ ] Subscribe to `totalMoney.OnValueChanged`
  - [ ] Subscribe to `currentResearchProgress.OnValueChanged`
  - [ ] Override `OnNetworkDespawn()`
  - [ ] Unsubscribe from all events
  - [ ] Test compilation

- [ ] **Create OnValueChanged callbacks**
  - [ ] Create `OnMoneyChanged(int old, int new)` method
  - [ ] Publish `WalletUpdate` event in callback
  - [ ] Create `OnResearchChanged(float old, float new)` method
  - [ ] Publish `ResearchProgressUpdate` event in callback

- [ ] **Convert AddSubMoney to ServerRpc**
  - [ ] Rename: `AddSubMoney()` → `AddSubMoneyServerRpc()`
  - [ ] Add `[ServerRpc(RequireOwnership = false)]` attribute
  - [ ] Change to modify `totalMoney.Value` instead of property
  - [ ] Remove manual event publish (OnValueChanged handles it)
  - [ ] Test compilation

- [ ] **Convert AddSubResearch to ServerRpc**
  - [ ] Rename: `AddSubResearch()` → `AddSubResearchServerRpc()`
  - [ ] Add `[ServerRpc(RequireOwnership = false)]` attribute
  - [ ] Change to modify `currentResearchProgress.Value`
  - [ ] Remove manual event publish
  - [ ] Test compilation

### Update All Callers

- [ ] **Find and replace all calls**
  - [ ] Search: `WalletBankton.Instance.AddSubMoney(`
  - [ ] Replace: `WalletBankton.Instance.AddSubMoneyServerRpc(`
  - [ ] Document number of replacements (should be many)
  - [ ] Search: `WalletBankton.Instance.AddSubResearch(`
  - [ ] Replace: `WalletBankton.Instance.AddSubResearchServerRpc(`
  - [ ] Test compilation

### Update Scene Setup

- [ ] **Add NetworkObject components**
  - [ ] Find WalletBankton GameObject in scene
  - [ ] Add NetworkObject component (if not present)
  - [ ] Configure: Uncheck "Spawn With Observers"
  - [ ] Verify in NetworkManager prefab list

### Test WalletBankton Thoroughly

- [ ] **Test money synchronization**
  - [ ] Start host, note initial money ($100)
  - [ ] Start client, verify shows $100
  - [ ] Host earns $50
  - [ ] Verify host UI updates
  - [ ] Verify client UI updates to $150
  - [ ] Client spends $25
  - [ ] Verify client UI updates to $125
  - [ ] Verify host UI updates to $125
  - [ ] **CRITICAL: Values must match!**

- [ ] **Test research synchronization**
  - [ ] Repeat above tests with research progress
  - [ ] Verify synchronization
  - [ ] Verify progress bar updates on all clients

- [ ] **Test late-join**
  - [ ] Host earns money
  - [ ] Client joins
  - [ ] Verify client sees correct money value

- [ ] **Commit WalletBankton conversion**
  - [ ] Only commit after ALL tests pass
  - [ ] Detailed commit message

---

## CurrentPlayers Evaluation

### Analysis

- [ ] **Analyze CurrentPlayers usage**
  - [ ] Search codebase for `CurrentPlayers.Instance`
  - [ ] Document all usage locations
  - [ ] Determine: Used by networked code?
  - [ ] Determine: Could use NetworkManager instead?

### Decision

- [ ] **Choose path:**
  - [ ] **Option A:** Convert to NetworkSingleton with NetworkList
    - [ ] If used by enemy AI or network code
    - [ ] Follow WalletBankton pattern
  - [ ] **Option B:** Deprecate and replace with NetworkManager
    - [ ] If simple list of connected players needed
    - [ ] Replace all usages with `NetworkManager.ConnectedClientsList`
    - [ ] Delete CurrentPlayers class
  - [ ] Document decision in notes

- [ ] **Execute chosen path**
  - [ ] Implement changes
  - [ ] Update all references
  - [ ] Test thoroughly
  - [ ] Commit changes

---

## ServiceLocator Decision

### Evaluation

- [ ] **Review ServiceLocator implementation**
  - [ ] Read `ServiceLocator.cs` code
  - [ ] Check if any services registered
  - [ ] Check if used anywhere
  - [ ] Compare with NetworkSingleton pattern

### Decision

- [ ] **Make decision:**
  - [ ] **Option A:** Fully implement ServiceLocator
    - [ ] More complex, but flexible
    - [ ] Document in architecture docs
    - [ ] Create examples
  - [ ] **Option B:** Remove ServiceLocator (RECOMMENDED)
    - [ ] Simpler, NetworkSingleton is sufficient
    - [ ] Less overhead
    - [ ] Delete ServiceLocator folder
  - [ ] Document decision in notes

- [ ] **Execute decision**
  - [ ] If keeping: Implement and document
  - [ ] If removing: Delete folder
  - [ ] Update GameInitializer if needed
  - [ ] Commit changes

---

## Scene Setup & Verification

### Update NetworkManager

- [ ] **Verify all singletons in network prefabs**
  - [ ] Open NetworkManager in scene
  - [ ] Check "Network Prefabs List"
  - [ ] Add any missing singleton prefabs
  - [ ] Test spawning

### Verify DontDestroyOnLoad

- [ ] **Test scene transitions**
  - [ ] Start host
  - [ ] Verify singletons exist
  - [ ] Load new scene
  - [ ] Verify singletons still exist
  - [ ] Verify no duplicates
  - [ ] Test with client too

---

## Testing

### Run Phase 2 Tests

- [ ] **Test 1: Singleton initialization**
  - [ ] Start host
  - [ ] Check console for initialization messages
  - [ ] Verify no duplicates
  - [ ] Start client
  - [ ] Verify client can access singletons

- [ ] **Test 2: Money synchronization (Extended)**
  - [ ] Run all money tests from checklist
  - [ ] Test with 3 clients
  - [ ] Test rapid changes
  - [ ] Test late-join
  - [ ] **All must pass!**

- [ ] **Test 3: Scene transitions**
  - [ ] Start host
  - [ ] Verify singletons
  - [ ] Change scene
  - [ ] Verify persistence
  - [ ] Stop and restart
  - [ ] Verify clean initialization

- [ ] **Test 4: Multiple clients stress test**
  - [ ] Start host + 3 clients
  - [ ] All clients modify money
  - [ ] Verify all stay in sync
  - [ ] Check console for errors

### Document Results

- [ ] **Create test report**
  - [ ] File: `.claude/progress/phase-2-test-report.md`
  - [ ] Document all test results
  - [ ] Note any failures
  - [ ] Include screenshots if helpful

---

## Definition of Done

**Phase 2 is complete when:**

- [ ] ✅ NetworkSingleton<T> base class created
- [ ] ✅ All singleton managers migrated
- [ ] ✅ WalletBankton syncs money across all clients
- [ ] ✅ Research progress syncs correctly
- [ ] ✅ CurrentPlayers decision executed
- [ ] ✅ ServiceLocator decision executed
- [ ] ✅ All tests pass
- [ ] ✅ No console errors
- [ ] ✅ All changes committed
- [ ] ✅ Team standup: Confirm Phase 2 complete
- [ ] ✅ Ready to start Phase 3

---

## Notes

**Blockers:**
- (Document any blockers here)

**Decisions Made:**
- CurrentPlayers: [Option A or B]
- ServiceLocator: [Keep or Remove]

**Issues Found:**
- (Link to filed issues)

**Time Spent:**
- Total: ___ hours
- Per developer: ___ hours

---

**Last Updated:** [DATE]
**Completed By:** [TEAM MEMBER NAMES]
